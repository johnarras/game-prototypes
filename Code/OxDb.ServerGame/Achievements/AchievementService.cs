using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Achievements.Constants;
using OxDb.SharedGame.Achievements.Messages;
using OxDb.SharedGame.Achievements.PlayerData;
using OxDb.SharedGame.Achievements.Settings;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;

namespace OxDb.ServerGame.Achievements
{
    public class AchievementService : IAchievementService
    {
        private IGameData _gameData = null;
        private IFullRepositoryService _repoService = null;

        public void UpdateAchievement(MapObject mapObject, long achievementTypeId, long quantity)
        {
            if (!(mapObject is Character ch))
            {
                return;
            }

            AchievementData adata = mapObject.Get<AchievementData>();

            long currQuantity = adata.Data[achievementTypeId];


            AchievementType type = _gameData.Get<AchievementSettings>(ch).Get(achievementTypeId);

            if (type?.Category == AchievementCategories.Max)
            {
                if (quantity > currQuantity)
                {
                    adata.Data[achievementTypeId] = quantity;
                    _repoService.QueueSave(adata);
                    ch.AddMessage(new OnUpdateAchievement() { AchievementTypeId = achievementTypeId, Quantity = quantity });
                    // Send to clients
                }
            }
            else
            {
                _repoService.QueueSave(adata);
                ch.AddMessage(new OnUpdateAchievement() { AchievementTypeId = achievementTypeId, Quantity = quantity });
                // Send to client
            }
        }
    }
}


