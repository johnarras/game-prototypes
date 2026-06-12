using MongoDB.Bson;
using MongoDB.Driver;
using OxDb.DataUtils.Entities.Copying;
using OxDb.DataUtils.Entities.Core;
using OxDb.ServerCore.AzureImpl.DataStores.Mongo.PolymorphicNoSQL;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.ServerGame.PlayerData.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Serialization.Utils;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.DataUtils.Services.EditorData
{

    public interface IEditorDataService : IInjectable
    {
        Task CopyFromGitToDb(EditorGameState gs, CancellationToken token);
        Task<FullGameDataCopy> LoadDataFromGit(EditorGameState gs, CancellationToken token);
        Task SaveFullDataToDb(EditorGameState gs, FullGameDataCopy fullCopy, string env, bool deleteExistingData, CancellationToken token);
        void InitSerialization();
        void WriteGameDataListToGit(List<IGameSettings> list);
        void WriteAllGameDataToGit(EditorGameState gs);
        void WriteGameDataToClient(List<IGameSettings> gameSettings);


        Task LoadEditorUserData(EditorGameState gs, string userId);
        Task SaveEditorUserData(EditorGameState gs);
        Task DeleteEditorUserData(EditorGameState gs);
    }

    public class EditorDataService : IEditorDataService
    {
        private ITextSerializer _textSerializer = null;
        private IServerGameDataService _gameDataService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;
        private IReflectionService _reflectionService = null;

        public async Task<FullGameDataCopy> LoadDataFromGit(EditorGameState gs, CancellationToken token)
        {

            ITextSerializer serializer = gs.loc.Get<ITextSerializer>();

            FullGameDataCopy dataCopy = new FullGameDataCopy();

            List<Type> settingsTypes = _reflectionService.GetTypesImplementing(typeof(IGameSettings));

            string mainDirName = GetCodeFolderPath() + GitDataOffsetPath;

            if (!Directory.Exists(mainDirName))
            {
                Directory.CreateDirectory(mainDirName);
            }

            string[] fullDirectoryNames = Directory.GetDirectories(mainDirName);

            List<string> directoryNames = new List<string>();

            foreach (string fullName in fullDirectoryNames)
            {
                directoryNames.Add(fullName.Replace(mainDirName + "\\", ""));
            }

            foreach (string subDirName in directoryNames)
            {
                Type currType = settingsTypes.FirstOrDefault(x => StrUtils.IsLowercaseEqual(x.Name, subDirName));

                if (currType == null)
                {
                    continue;
                }

                try
                {
                    string fullDirectoryName = Path.Combine(mainDirName, subDirName);

                    if (!Directory.Exists(fullDirectoryName))
                    {
                        continue;
                    }

                    string[] fileNames = Directory.GetFiles(fullDirectoryName);

                    List<string> allFiles = new List<string>();

                    foreach (string file in fileNames)
                    {
                        allFiles.Add(File.ReadAllText(Path.Combine(fullDirectoryName, file)));
                    }

                    foreach (string fileData in allFiles)
                    {
                        dataCopy.Data.Add((IGameSettings)serializer.DeserializeWithType(fileData, currType));
                    }
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "EditorData.LoadDataFromGit");
                }
            }
            return dataCopy;
        }

        public async Task SaveFullDataToDb(EditorGameState gs, FullGameDataCopy fullCopy, string env, bool deleteExistingData, CancellationToken token)
        {
            try
            {
                FullRepositoryService repoService = (FullRepositoryService)gs.loc.Get<IRepositoryService>();

                PolymorphicMongoRepository polyRepo = (PolymorphicMongoRepository)repoService.FindRepo(typeof(CoreCurrencyTypeSettings));

                IMongoCollection<BsonDocument> collection = polyRepo.GetSettingsCollection();

                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Empty;
                await collection.DeleteManyAsync(filter);

                List<IGameSettings> dataList = fullCopy.Data;

                // This will overload Cosmos serverless...soo put breakpoints here to slow down the saving
                // to avoid 429 errors
                List<Task> saveTasks = new List<Task>();
                for (int i = 0; i < dataList.Count; i++)
                {
                    saveTasks.Add(repoService.Save(dataList[i]));

                    if (i % 20 == 9 || i == dataList.Count - 1)
                    {
                        await Task.WhenAll(saveTasks);
                        saveTasks = new List<Task>();
                        await Task.Delay(250);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "EditorData.FullDataToDb");
            }
        }

        public void InitSerialization()
        {
            SerializationInitializer.Init(GetCodeFolderPath(), _reflectionService, GetType().Assembly);
        }

        public void WriteGameDataListToGit(List<IGameSettings> list)
        {
            string dirName = GetGitDataPath();

            DateTime saveTime = DateTime.UtcNow;
            foreach (IGameSettings settings in list)
            {
                WriteGameDataText(dirName, settings, saveTime);
            }
        }

        private string GetCodeFolderPath() { return AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\..\\..\\"; }

        private string GetGitDataPath()
        {
            string dirName = GetCodeFolderPath() + GitDataOffsetPath;

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            return dirName;
        }


        const string GitDataOffsetPath = "..\\GameData";
        public void WriteAllGameDataToGit(EditorGameState gs)
        {
            string dirName = GetGitDataPath();

            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName, true);
            }
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            foreach (string file in Directory.GetFiles(dirName))
            {
                File.Delete(file);
            }

            List<IGameSettings> allSettings = gs.data.AllSettings().Cast<IGameSettings>().ToList();

            DateTime saveTime = DateTime.UtcNow;
            foreach (ITopLevelSettings topLevel in allSettings)
            {
                WriteGameDataText(dirName, topLevel, saveTime);

                topLevel.SetInternalIds();
                List<IGameSettings> children = topLevel.GetChildren();
                foreach (IGameSettings child in children)
                {
                    WriteGameDataText(dirName, child, saveTime);
                }

                RemoveDeletedFiles(dirName, children);
            }
            RemoveDeletedFiles(dirName, allSettings);
        }

        public void WriteGameDataToClient(List<IGameSettings> gameSettings)
        {
            string dirName = GetCodeFolderPath() + "..\\Code\\GenrpgClient\\Assets\\Resources\\BakedGameData";

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            Dictionary<Type, IGameSettingsMapper> mapperDict = _gameDataService.GetAllMappers();

            DateTime saveTime = DateTime.UtcNow;


            List<ITopLevelSettings> finalSettings = new List<ITopLevelSettings>();
            foreach (IGameSettings gameSetting in gameSettings)
            {
                if (gameSetting is ITopLevelSettings topLevelSettings)
                {
                    if (EditorGameState.UpdateSaveTime)
                    {
                        topLevelSettings.SaveTime = saveTime;
                    }
                    if (mapperDict.TryGetValue(topLevelSettings.GetType(), out IGameSettingsMapper mapper))
                    {
                        if (mapper.SendToClient())
                        {
                            finalSettings.Add(mapper.MapToDto(topLevelSettings, true));
                        }
                    }
                }
            }



            foreach (ITopLevelSettings settingsItem in finalSettings)
            {
                string txt = _textSerializer.PrettyPrint(settingsItem);
                string filename = settingsItem.GetType().Name + ".txt";

                File.WriteAllText(dirName + "\\" + filename, txt);
            }
        }


        private void RemoveDeletedFiles(string parentPath, List<IGameSettings> allSettings)
        {
            if (allSettings.Count < 1)
            {
                return;
            }

            foreach (IGameSettings settings in allSettings)
            {
                string subpath = StrUtils.NormalizeTypeName(settings.GetType());

                string fullDir = Path.Combine(parentPath, subpath);

                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }

                string[] fileNames = Directory.GetFiles(fullDir);

                foreach (string fileName in fileNames)
                {
                    IGameSettings matchingObject = allSettings.FirstOrDefault(x => x.GetType() == settings.GetType() && x.Id == settings.Id);

                    if (matchingObject == null)
                    {
                        string fullPath = Path.Combine(fullDir, fileName);
                        File.Delete(fullPath);
                    }
                }
            }
        }

        private void WriteGameDataText(string parentPath, object objectToSave, DateTime saveTime)
        {
            IStringId idObj = objectToSave as IStringId;

            if (idObj == null)
            {
                return;
            }

            BaseGameSettings baseSettings = idObj as BaseGameSettings;

            if (baseSettings != null)
            {

                if (baseSettings is IId iid && iid.IdKey == 0)
                {
                    return;
                }

                ITopLevelSettings topLevel = idObj as ITopLevelSettings;

                if (topLevel == null)
                {
                    baseSettings.SaveTime = DateTime.MinValue;
                }
                else
                {
                    if (EditorGameState.UpdateSaveTime)
                    {
                        baseSettings.SaveTime = saveTime;
                    }
                }
            }

            string subpath = StrUtils.NormalizeTypeName(objectToSave.GetType());

            string fullDir = Path.Combine(parentPath, subpath);

            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }


            string fullPath = Path.Combine(fullDir, idObj.Id);

            string txt = _textSerializer.PrettyPrint(idObj);
            File.WriteAllText(fullPath, txt);
        }

        public async Task LoadEditorUserData(EditorGameState gs, string userId)
        {

            gs.EditorUser.GameAccount = await _repoService.Load<GameAccount>(userId.ToString());

            List<CharacterStub> charStubs = await gs.loc.Get<IPlayerDataService>().LoadCharacterStubs(userId.ToString());

            foreach (CharacterStub stub in charStubs)
            {
                CoreCharacter coreChar = await _repoService.Load<CoreCharacter>(stub.Id);
                if (coreChar != null)
                {
                    Character ch = new Character(coreChar);

                    EditorCharacter ech = new EditorCharacter() { Character = ch, CoreCharacter = coreChar };
                    gs.EditorUser.Characters.Add(ech);
                    await gs.loc.Get<IPlayerDataService>().LoadAllPlayerData(gs.rand, gs.EditorUser.GameAccount.Id, new List<IUnitData>(), ch);
                    foreach (ITopLevelUnitData dataCont in ch.GetTopLevelData())
                    {
                        ech.Data.Add(new EditorUnitData() { Data = dataCont });
                    }
                }
            }
        }

        public async Task SaveEditorUserData(EditorGameState gs)
        {

            List<Task<bool>> tasks = new List<Task<bool>>();
            if (gs.LookedAtObjects.Contains(gs.EditorUser.GameAccount))
            {
                tasks.Add(_repoService.Save(gs.EditorUser.GameAccount));
            }
            if (gs.EditorUser.Characters != null)
            {
                foreach (EditorCharacter ech in gs.EditorUser.Characters)
                {
                    if (gs.LookedAtObjects.Contains(ech.CoreCharacter))
                    {
                        tasks.Add(_repoService.Save(ech.CoreCharacter));
                    }
                    foreach (IUnitData unitData in ech.Character.GetAllData())
                    {
                        if (gs.LookedAtObjects.Contains(unitData))
                        {
                            tasks.Add(_repoService.Save(unitData));
                        }
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task DeleteEditorUserData(EditorGameState gs)
        {

            List<Task<bool>> tasks = new List<Task<bool>>();
            tasks.Add(_repoService.Delete(gs.EditorUser.GameAccount));

            if (gs.EditorUser.Characters != null)
            {
                foreach (EditorCharacter ech in gs.EditorUser.Characters)
                {
                    tasks.Add(_repoService.Delete(ech.CoreCharacter));
                    foreach (IUnitData unitData in ech.Character.GetAllData())
                    {
                        tasks.Add(_repoService.Delete(unitData));
                    }
                }
            }

            await Task.WhenAll(tasks);

        }

        public async Task CopyFromGitToDb(EditorGameState gs, CancellationToken token)
        {

            try
            {
                FullGameDataCopy fullCopy = await LoadDataFromGit(gs, token);


                FullRepositoryService repoService = (FullRepositoryService)gs.loc.Get<IRepositoryService>();

                PolymorphicMongoRepository polyRepo = (PolymorphicMongoRepository)repoService.FindRepo(typeof(CoreCurrencyTypeSettings));

                IMongoCollection<BsonDocument> collection = polyRepo.GetSettingsCollection();


                List<List<WriteModel<BsonDocument>>> bulkOpsList = new List<List<WriteModel<BsonDocument>>>();

                List<WriteModel<BsonDocument>> bulkOps = new List<WriteModel<BsonDocument>>();

                bulkOpsList.Add(bulkOps);
                int currCount = 0;
                foreach (IGameSettings settings in fullCopy.Data)
                {
                    settings.Id = polyRepo.GetFullDocId(settings.GetType().Name, settings.Id);

                    // Parse the JSON string into a BsonDocument
                    BsonDocument doc = settings.ToBsonDocument();

                    if (doc.Contains("Id"))
                    {
                        BsonValue idValue = doc["Id"];
                        doc["_id"] = idValue;
                    }

                    InsertOneModel<BsonDocument> insertOp = new InsertOneModel<BsonDocument>(doc);
                    bulkOps.Add(insertOp);

                    if (bulkOps.Count >= 100)
                    {
                        bulkOps = new List<WriteModel<BsonDocument>>();
                        bulkOpsList.Add(bulkOps);
                    }
                }


                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Empty;
                await collection.DeleteManyAsync(filter);

                await Task.Delay(500);


                foreach (List<WriteModel<BsonDocument>> bulkOpSet in bulkOpsList)
                {

                    // Perform a bulk insert for efficiency
                    if (bulkOpSet.Count > 0)
                    {
                        BulkWriteOptions options = new BulkWriteOptions { IsOrdered = false };
                        await collection.BulkWriteAsync(bulkOpSet, options);

                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "CopyFromGitToDb");
            }
        }
    }
}
