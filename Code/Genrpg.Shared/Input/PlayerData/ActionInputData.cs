using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Input.PlayerData
{
    [MessagePackObject]
    public class ActionInput : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public int Index { get; set; }
        [Key(3)] public long SpellId { get; set; }
    }


    [MessagePackObject]
    public class ActionInputData : OwnerObjectList<ActionInput>
    {
        [Key(0)] public override string Id { get; set; }

        public ActionInput GetInput(int actionIndex)
        {
            if (!InputConstants.OkActionIndex(actionIndex))
            {
                return null;
            }

            ActionInput input = _data.FirstOrDefault(x => x.Index == actionIndex);
            if (input == null)
            {
                input = new ActionInput
                {
                    Index = actionIndex,
                    OwnerId = Id,
                    Id = HashUtils.NewUUId(),
                };
                _data.Add(input);
            }
            return input;
        }

        public void SetInput(int actionIndex, long spellTypeId, IRepositoryService repoService)
        {
            ActionInput input = GetInput(actionIndex);
            if (input == null)
            {
                return;
            }

            if (input.SpellId != spellTypeId)
            {
                input.SpellId = spellTypeId;  
                repoService.QueueSave(input);
            }
        }
    }
    public class ActionInputDto : OwnerDtoList<ActionInputData, ActionInput> { }

    public class ActionInputDataLoader : OwnerDataLoader<ActionInputData, ActionInput> { }

    public class ActionInputDataMapper : OwnerDataMapper<ActionInputData, ActionInput, ActionInputDto> { }

}
