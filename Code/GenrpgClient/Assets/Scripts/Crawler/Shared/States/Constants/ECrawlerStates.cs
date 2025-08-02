using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.States.Constants
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
        SaveGame,
        QuitGame,
        Help,

        TavernMain,
        ExploreWorld,
        EnterMap,
        Error,
        GiveLoot,
        Riddle,
        ReturnToSafety,
        GainStats,
        TeleportConfirmation,
        
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

        UseItemExplore,
        UseItemCombat,

        SelectAlly,
        SelectAllyTarget,
        SelectEnemyGroup,
        SelectItem,
        SelectSpell,
        OnSelectSpell,

        Vendor,

        TrainingMain,
        TrainingLevelSelect,
        TrainingLevelMember,
        TrainingClassSelect,
        TrainingClassMember,
        TrainingUpgradeSelect,
        TrainingUpgradeMember,

        EnterHouse,

        Temple,

        StartCombat,
        CombatFightRun,
        CombatPlayer,
        CombatConfirm,
        ProcessCombatRound,
        CombatDeath,
    }
}
