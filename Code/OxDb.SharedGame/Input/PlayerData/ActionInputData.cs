using MessagePack;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Input.Constants;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Input.PlayerData
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
                    Id = HashUtils.NewGuid(),
                };
                _data.Add(input);
            }
            return input;
        }

        public ActionInput SetInput(int actionIndex, long spellTypeId)
        {
            ActionInput input = GetInput(actionIndex);
            if (input == null)
            {
                return null;
            }

            if (input.SpellId != spellTypeId)
            {
                input.SpellId = spellTypeId;
                return input;
            }
            return null;
        }
    }
    [MessagePackObject]
    public class ActionInputDto : OwnerDtoList<ActionInputData, ActionInput>
    {
        [Key(0)] public override List<ActionInput> Children { get; set; }
        [Key(1)] public override ActionInputData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }

    public class ActionInputDataLoader : OwnerDataLoader<ActionInputData, ActionInput> { }

    public class ActionInputDataMapper : OwnerDataMapper<ActionInputData, ActionInput, ActionInputDto> { }

}


