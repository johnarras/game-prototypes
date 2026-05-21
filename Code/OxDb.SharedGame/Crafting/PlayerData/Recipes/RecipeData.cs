using MessagePack;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crafting.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crafting.PlayerData.Recipes
{
    [MessagePackObject]
    public class RecipeStatus : OwnerPlayerData, IId
    {

        const int LevelId = 1;
        const int MaxLevelId = 2;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public override string OwnerId { get; set; }

        [Key(3)] public List<IdVal> Levels { get; set; }

        public RecipeStatus()
        {
            Levels = new List<IdVal>();
        }

        protected IdVal GetById(int id, int startval)
        {
            if (Levels == null)
            {
                Levels = new List<IdVal>();
            }

            IdVal idval = Levels.FirstOrDefault(x => x.Id == id);
            if (idval == null)
            {
                idval = new IdVal() { Id = id };
                Levels.Add(idval);
            }
            if (idval.Val < startval)
            {
                idval.Val = startval;
            }

            return idval;
        }

        protected IdVal GetObject() { return GetById(LevelId, CraftingConstants.StartSkillLevel); }
        public int Get() { return (int)GetObject().Val; }
        public void SetLevel(int level) { GetObject().Val = level; }
        public void AddLevel(int level) { GetObject().Val += level; }

        protected IdVal GetMaxLevelObject() { return GetById(MaxLevelId, CraftingConstants.StartMaxSkillLevel); }
        public int GetMaxLevel() { return (int)GetMaxLevelObject().Val; }
        public void SetMaxLevel(int level) { GetMaxLevelObject().Val = level; }
        public void AddMaxLevel(int level) { GetMaxLevelObject().Val += level; }
    }
    [MessagePackObject]
    public class RecipeData : OwnerIdObjectList<RecipeStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public void AddRecipeStatus(long recipeTypeId)
        {
            RecipeStatus status = new RecipeStatus()
            {
                IdKey = recipeTypeId,
                Id = HashUtils.NewGuid(),
                OwnerId = Id,
            };
        }
    }
    [MessagePackObject]
    public class RecipeDataDto : OwnerDtoList<RecipeData, RecipeStatus>
    {
        [Key(0)] public override List<RecipeStatus> Children { get; set; }
        [Key(1)] public override RecipeData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }

    public class RecipeDataLoader : OwnerIdDataLoader<RecipeData, RecipeStatus> { }

    public class RecipeDataMapper : OwnerDataMapper<RecipeData, RecipeStatus, RecipeDataDto> { }
}


