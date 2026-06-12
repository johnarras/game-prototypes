using OxDb.DataUtils.Entities.Core;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace OxDb.DataUtils.Services.Importing
{
    public interface IImportService : IInjectable
    {
        T ImportLine<T>(EditorGameState gs, int row, string[] headers, string[] rowWords, T curr = null, bool firstColumnHasData = false) where T : class, new();
        void ConvertImportWordsToContainer(object import, List<IIdName> children, SmallIdLongCollection cont);
        void AddEffectList<TImport, TParent, TChild, TEffect>(EditorGameState gs, int row, string headerWord, long entityTypeId, List<TEffect> effects, string data) where TEffect : IEffect, new()
            where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new();


        long GetOrAddMissingEntity<TParent, TChild>(EditorGameState gs, string name) where TParent : ParentSettings<TChild>, new() where TChild : ChildSettings, IIndexedGameItem, new();

        Task CleanOldObjects<T>(List<T> newObjects) where T : ChildSettings, IIndexedGameItem;
        void WriteCSVRow(StringBuilder sb, object obj, string forcedHeaderName = null);
        void WriteCSVHeader(StringBuilder sb, Type type);

        string WriteCSVSettings(ITopLevelSettings settings);
    }

    public class ImportService : IImportService
    {

        private IReflectionService _reflectionService = null;
        private IFullRepositoryService _repoService = null;

        public T ImportLine<T>(EditorGameState gs, int row, string[] headers, string[] rowWords, T curr = null, bool firstColumnHasData = false) where T : class, new()
        {
            if (curr == null)
            {
                curr = new T();
            }

            PropertyInfo[] allProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int i = (firstColumnHasData ? 0 : 1); i < headers.Length && i < rowWords.Length; i++)
            {
                string header = StrUtils.NormalizeWord(headers[i]);

                PropertyInfo prop = allProperties.FirstOrDefault(x => StrUtils.IsLowercaseEqual(x.Name, header));

                if (prop == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(rowWords[i]))
                {
                    continue;
                }

                if (prop.PropertyType == typeof(DateTime))
                {
                    DateTimeConverter timeConverter = new DateTimeConverter();
                    prop.SetValue(curr, timeConverter.ConvertFromString(rowWords[i]));
                }
                else
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(prop.PropertyType);

                    if (converter != null)
                    {
                        try
                        {
                            object value = converter.ConvertFromString(rowWords[i]);
                            if (value is string str)
                            {
                                if (str == "")
                                {
                                    value = null;
                                }
                            }
                            prop.SetValue(curr, value);
                        }
                        catch (Exception ex)
                        {
                            bool didFindName = false;
                            List<IIdName> dropdownList = _reflectionService.GetDropdownList(prop, curr);

                            if (prop.PropertyType.IsPrimitive)
                            {
                                if (dropdownList.Count > 0)
                                {
                                    string lowerDataName = StrUtils.NormalizeWord(rowWords[i]);

                                    List<string> namesToCheck = new List<string>();

                                    namesToCheck.Add(lowerDataName);

                                    if (lowerDataName.Length >= 4 && lowerDataName.LastIndexOf("type") == lowerDataName.Length - 4)
                                    {
                                        namesToCheck.Add(lowerDataName.Substring(0, lowerDataName.LastIndexOf("type")));
                                    }


                                    foreach (string currNameToCheck in namesToCheck)
                                    {
                                        foreach (IIdName iidname in dropdownList)
                                        {
                                            string lowerObjName = StrUtils.NormalizeWord(iidname.Name);

                                            if (lowerObjName != null &&
                                                lowerObjName.Length >= currNameToCheck.Length &&
                                                currNameToCheck == lowerObjName.Substring(0, currNameToCheck.Length))
                                            {
                                                prop.SetValue(curr, iidname.IdKey);
                                                didFindName = true;
                                                break;
                                            }

                                            if (iidname is INameId ind)
                                            {
                                                string lowerObjNameId = StrUtils.NormalizeWord(ind.NameId);

                                                if (lowerObjNameId != null &&
                                                    lowerObjNameId.Length >= currNameToCheck.Length &&
                                                    currNameToCheck == currNameToCheck.Substring(0, lowerObjNameId.Length))
                                                {
                                                    prop.SetValue(curr, iidname.IdKey);
                                                    didFindName = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!didFindName)
                            {
                                throw new Exception(ex.Message + $" Bad Import for {typeof(T).Name} Row: {row} Header: {header} Data: {rowWords[i]}");
                            }
                        }
                    }
                }
            }
            return curr;

        }



        public void ConvertImportWordsToContainer(object import, List<IIdName> children, SmallIdLongCollection cont)
        {

            PropertyInfo[] props = import.GetType().GetProperties();

            for (int p = 0; p < props.Length; p++)
            {
                IIdName matchingStat = children.FirstOrDefault(x => StrUtils.IsLowercaseEqual(StrUtils.NormalizeWord(x.Name), props[p].Name));

                if (matchingStat != null)
                {
                    int value = _reflectionService.GetObjectInt(import, props[p].Name);
                    if (value != 0)
                    {
                        cont.Add(matchingStat.IdKey, value);
                    }
                }
            }
        }

        public void AddEffectList<TImport, TParent, TChild, TEffect>(EditorGameState gs, int mainRow, string headerWord, long entityTypeId, List<TEffect> effects, string data)
            where TParent : ParentSettings<TChild>
            where TChild : ChildSettings, IIdName, new()
            where TEffect : IEffect, new()
        {

            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            List<string> rows = StrUtils.CommaSemiColonSplit(data);

            if (rows.Count < 1)
            {
                return;
            }

            IReadOnlyList<TChild> children = gs.data.Get<TParent>(null).GetData();

            foreach (string row in rows)
            {
                string trimmedRow = row.Trim();
                string mergedLowerRow = StrUtils.NormalizeWord(trimmedRow);
                if (string.IsNullOrEmpty(trimmedRow))
                {
                    continue;
                }

                string[] words = trimmedRow.Split(' ');

                if (words.Length < 1)
                {
                    continue;
                }

                for (int w = 0; w < words.Length; w++)
                {
                    words[w] = StrUtils.NormalizeWord(words[w]);
                }

                if (words.Length < 1)
                {
                    continue;
                }

                TChild child = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == words[0]);

                if (child == null)
                {
                    child = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == mergedLowerRow);
                }

                if (child == null)
                {
                    throw new Exception($"Bad Import for {typeof(TImport).Name} Row: {mainRow} Header: {headerWord} Data: {data} Subitem: {row} Word: {words[0]} No {typeof(TChild).Name}  matches");
                }

                long quantity = 1;

                if (words.Length > 1)
                {
                    if (Int64.TryParse(words[1], out long qty))
                    {
                        quantity = qty;
                    }
                }

                quantity = Math.Max(1, quantity);

                effects.Add(new TEffect()
                {
                    EntityTypeId = entityTypeId,
                    EntityId = child.IdKey,
                    Quantity = quantity,
                });

            }
        }

        public async Task CleanOldObjects<T>(List<T> newObjects) where T : ChildSettings, IIndexedGameItem
        {

            if (newObjects.Count < 1)
            {
                return;
            }

            List<T> oldObjects = await _repoService.Search<T>(x => x.ParentId == GameDataConstants.DefaultFilename);


            foreach (T oldObject in oldObjects)
            {
                if (!newObjects.Any(x => x.IdKey == oldObject.IdKey))
                {
                    await _repoService.Delete(oldObject);
                }
            }
        }

        public long GetOrAddMissingEntity<TParent, TChild>(EditorGameState gs, string name) where TParent : ParentSettings<TChild>, new() where TChild : ChildSettings, IIndexedGameItem, new()
        {
            TParent parent = gs.data.Get<TParent>(null);

            if (parent == null)
            {
                return 0;
            }

            List<TChild> children = parent.GetData().ToList();

            string normalizedName = StrUtils.NormalizeWord(name);


            TChild currChild = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == normalizedName);

            if (currChild != null)
            {
                return currChild.IdKey;
            }
            long newId = (children.Count > 0 ? children.Max(x => x.IdKey) : 0) + 1;


            string assetName = StrUtils.GetAlNumSubstring(name);
            currChild = new TChild()
            {
                IdKey = newId,
                Name = name,

            };
            currChild.Icon = assetName;
            currChild.Art = assetName;

            children.Add(currChild);

            gs.LookedAtObjects.Add(currChild);
            parent.SetData(children);

            return newId;

        }

        public void WriteCSVHeader(StringBuilder sb, Type type)
        {
            List<Type> genericListsToAdd = new List<Type>();
            sb.Append("header " + type.Name.ToLower() + ",");
            PropertyInfo[] props = type.GetProperties();
            bool didWriteProperty = false;
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo prop = props[i];

                if (_reflectionService.IsEnumerableType(prop.PropertyType))
                {

                    if (_reflectionService.IsGenericList(prop.PropertyType))
                    {
                        if (!genericListsToAdd.Contains(prop.PropertyType))
                        {
                            genericListsToAdd.Add(prop.PropertyType);
                        }
                    }
                }
                else
                {
                    if (didWriteProperty)
                    {
                        sb.Append(",");
                    }
                    sb.Append(StrUtils.WriteCSVString(prop.Name));
                    didWriteProperty = true;
                }
            }
            sb.Append('\n');

            foreach (Type enType in genericListsToAdd)
            {
                Type underlyingType = _reflectionService.GetUnderlyingType(enType);

                WriteCSVHeader(sb, underlyingType);
            }
        }


        public void WriteCSVRow(StringBuilder sb, object obj, string forcedHeaderName = null)
        {

            if (obj == null)
            {
                return;
            }

            List<PropertyInfo> listProperties = new List<PropertyInfo>();


            sb.Append(obj.GetType().Name.ToLower() + ",");
            bool didWriteProperty = false;
            PropertyInfo[] props = obj.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo prop = props[i];
                object val = _reflectionService.GetObjectValue(obj, prop);

                if (_reflectionService.IsEnumerableType(prop.PropertyType))
                {
                    if (val is IEnumerable enumerable)
                    {
                        listProperties.Add(prop);
                    }
                }
                else
                {
                    if (didWriteProperty)
                    {
                        sb.Append(",");
                    }
                    sb.Append(StrUtils.WriteCSVString(val));
                    didWriteProperty = true;
                }
            }
            sb.Append('\n');

            foreach (PropertyInfo prop in listProperties)
            {
                continue;
                object val = _reflectionService.GetObjectValue(obj, prop);

                if (_reflectionService.IsEnumerableType(prop.PropertyType))
                {
                    if (val is IEnumerable enumerable)
                    {
                        Type underlyingType = _reflectionService.GetUnderlyingType(prop.PropertyType);


                        string headerName = underlyingType.Name.ToLower();
                        foreach (object item in enumerable)
                        {
                            WriteCSVRow(sb, item, headerName);
                        }
                    }
                }
            }
        }

        public string WriteCSVSettings(ITopLevelSettings settings)
        {
            StringBuilder sb = new StringBuilder();
            WriteCSVHeader(sb, settings.GetType());
            WriteCSVRow(sb, settings);

            List<IGameSettings> children = settings.GetChildren();

            if (children.Count > 0)
            {
                WriteCSVHeader(sb, children[0].GetType());

                foreach (IGameSettings child in children)
                {
                    WriteCSVRow(sb, child);
                }
            }
            return sb.ToString();
        }
    }
}


