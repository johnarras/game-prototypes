using MessagePack;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crafting.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crafting.PlayerData.Crafting
{
    [MessagePackObject]
    public class CraftingStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }

        [Key(3)] public int CraftingSkillPoints { get; set; }

        [Key(4)] public int GatheringSkillPoints { get; set; }

        public void AddSkillPoints(int skillCategory, int amount)
        {
            if (skillCategory == CraftingConstants.GatheringSkill)
            {
                GatheringSkillPoints += amount;
            }
            else if (skillCategory == CraftingConstants.CraftingSkill)
            {
                CraftingSkillPoints += amount;
            }
        }

        public int GetSkillPoints(int skillCategory)
        {
            if (skillCategory == CraftingConstants.CraftingSkill)
            {
                return CraftingSkillPoints;
            }
            else if (skillCategory == CraftingConstants.GatheringSkill)
            {
                return GatheringSkillPoints;
            }
            return 0;
        }

        public int Get(int skillCategory)
        {
            if (skillCategory == CraftingConstants.CraftingSkill)
            {

                return 1 + CraftingSkillPoints / CraftingConstants.CraftingSkillPointsPerLevel;
            }
            else if (skillCategory == CraftingConstants.GatheringSkill)
            {

                return 1 + GatheringSkillPoints / CraftingConstants.GatheringSkillPointsPerLevel;
            }
            return 0;
        }
    }
    [MessagePackObject]
    public class CraftingData : OwnerIdObjectList<CraftingStatus>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class CraftingDto : OwnerDtoList<CraftingData, CraftingStatus>
    {
        [Key(0)] public override List<CraftingStatus> Children { get; set; }
        [Key(1)] public override CraftingData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }
    public class CrafterDataLoader : OwnerIdDataLoader<CraftingData, CraftingStatus> { }


    public class CrafterDataMapper : OwnerDataMapper<CraftingData, CraftingStatus, CraftingDto> { }
}


