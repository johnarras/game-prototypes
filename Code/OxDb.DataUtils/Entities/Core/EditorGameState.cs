using OxDb.DataUtils.Interfaces;
using OxDb.DataUtils.Services.EditorData;
using OxDb.DataUtils.Services.Setup;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.Core;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.DataUtils.Entities.Core
{
    public delegate Task OnEditorClickAction(EditorServer server, EditorGameState gs, IEditorDataService gameDataService, CancellationToken token);
    public class EditorGameState : ServerGameState
    {
        public static bool UpdateSaveTime = false;
        public static CancellationTokenSource CTS = new CancellationTokenSource();

        public EditorUser EditorUser { get; set; }
        public EditorGameData EditorGameData { get; set; }
        public IGameData data { get; set; }
        public MyRandom rand { get; set; } = new MyRandom();
        public EditorGameState(IServerConfig config, ILogService logService) : base(config, logService)
        {
            EditorUser = new EditorUser();
        }

        public List<object> LookedAtObjects = new List<object>();
    }

    public class EditorGameData : IShowChildListAsButton
    {
        public IGameData GameData { get; set; }

        public List<EditorSettingsList> Data { get; set; } = new List<EditorSettingsList>();

    }

    public interface IEditorScaffold
    {
        System.Collections.IEnumerable GetData();
    }

    public abstract class EditorSettingsList : IEditorScaffold
    {
        public string TypeName { get; set; }
        public virtual void SetData(List<BaseGameSettings> baseList) { }
        public abstract System.Collections.IEnumerable GetData();
    }

    public class TypedEditorSettingsList<T> : EditorSettingsList where T : BaseGameSettings, new()
    {
        // This list needs a concrete type as a parameter or it won't bind to the datagrid correctly...
        // either doesn't appear or doesn't have the id visible.
        public List<T> Data { get; set; } = new List<T>();

        public override void SetData(List<BaseGameSettings> baseList)
        {
            List<T> list = new List<T>();

            foreach (BaseGameSettings settings in baseList)
            {
                if (settings is T t)
                {
                    list.Add(t);
                }
            }
            Data = list;
        }

        public override System.Collections.IEnumerable GetData()
        {
            return Data;
        }
    }

    public class EditorUser
    {
        public GameAccount GameAccount { get; set; }
        public List<EditorCharacter> Characters { get; set; }

        public EditorUser()
        {
            Characters = new List<EditorCharacter>();
        }
    }

    public class EditorUnitData
    {
        public IUnitData Data { get; set; }

        public string Id
        {
            get
            {
                return Data != null ? Data.GetType().Name : "--";
            }
            set
            {

            }
        }
    }

    public class EditorCharacter
    {
        public Character Character { get; set; }
        public CoreCharacter CoreCharacter { get; set; }
        public List<EditorUnitData> Data { get; set; }

        public EditorCharacter()
        {
            Data = new List<EditorUnitData>();
        }
        public string Id
        {
            get
            {
                return Character != null ? Character.Id : "None";
            }
            set
            {

            }
        }

        public string Name
        {
            get
            {
                return Character != null ? Character.Name : "None";
            }
            set
            {

            }
        }
    }
}


