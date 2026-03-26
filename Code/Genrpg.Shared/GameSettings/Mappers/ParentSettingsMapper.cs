using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.GameSettings.Mappers
{
    public class ParentSettingsMapper<TParent, TChild, TDto> : IGameSettingsMapper
      where TParent : ParentSettings<TChild>, new()
      where TChild : ChildSettings, new()
      where TDto : ParentSettingsDto<TParent, TChild>, new()
    {
        public virtual Version GetMinClientVersion() { return VersionConstants.MinVersion; }
        public virtual Version GetMaxClientVersion() { return VersionConstants.MaxVersion; }
        public virtual Type GetClientType() { return typeof(TDto); }
        public virtual bool SendToClient() { return true; }
        [IgnoreMember] public virtual Type HelperKey => typeof(TParent);


        public virtual ITopLevelSettings MapToDto(ITopLevelSettings settings, bool simplify)
        {
            if (settings is TParent tparent)
            {
                TDto api = new TDto()
                {
                    Parent = tparent,
                    Children = tparent.GetData().ToList(),
                    Id = tparent.Id,
                    SaveTime = tparent.SaveTime,
                };
                if (simplify)
                {
                    tparent.SaveTime = DateTime.MinValue;
                }

                if (simplify)
                {
                    List<TChild> removeList = new List<TChild>();
                    foreach (TChild child in tparent.GetData())
                    {
                        child.SaveTime = DateTime.MinValue;
                        child.Id = null;
                        child.ParentId = null;

                        if (child is IId ichild && ichild.IdKey < 1)
                        {
                            removeList.Add(child);
                        }
                    }

                    if (removeList.Count > 0)
                    {
                        api.Children = api.Children.Except(removeList).ToList();
                    }
                }

                return api;
            }
            return settings;
        }
    }
}


