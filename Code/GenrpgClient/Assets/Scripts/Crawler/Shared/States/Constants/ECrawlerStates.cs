namespace OxDb.SharedGame.Crawler.States.Constants
{
    public enum ECrawlerStates
    {
        None,
        DoNotChangeState,
        PopState,
        Lore,

        GuildMain,
        AddMember,
        RemoveMember,
        ChooseRace,
        RollStats,
        ChooseClass,
        ChoosePortrait,
        ChooseName,
        DeleteMember,
        DeleteConfirm,
        DeleteYes,
        DeleteNo,
        UpgradeParty,
        PartyOrder,

        Options,
        NewGame,
        SaveGame,
        QuitGame,
        Help,

        ExploreWorld,
        Camping,
        EnterMap,
        Error,
        GiveLoot,
        Riddle,
        ReturnToSafety,
        GainStats,
        TeleportConfirmation,
        LevelMap,


        SearchJunkPile,


        NpcMain,
        QuestDetail,
        QuestLog,

        WorldCast,
        SpecialSpellCast,

        SetWorldPortal,
        ReturnWorldPortal,
        TownPortal,
        TeleportPosition,
        JumpLength,
        PassWall,

        SelectAlly,
        SelectAllyTarget,
        SelectEnemyGroup,
        SelectItem,
        SelectSpell,
        OnSelectSpell,

        SelectUsableItem,
        OnSelectUseItem,

        Vendor,

        TavernMain,

        TrainingMain,
        TrainingLevelSelect,
        TrainingLevelMember,
        TrainingClassSelect,
        TrainingClassMember,
        TrainingUpgradeSelect,
        TrainingUpgradeMember,

        EnterHouse,

        Temple,

        ManaRegen,

        StartCombat,
        CombatFightRun,
        CombatPlayer,
        CombatConfirm,
        ProcessCombatRound,
        CombatDeath,
    }
}


