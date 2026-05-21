using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;

namespace OxDb.ServerGame.Achievements
{
    public interface IAchievementService : IInjectable
    {
        void UpdateAchievement(MapObject mapObject, long achievementTypeId, long quantity);
    }
}


