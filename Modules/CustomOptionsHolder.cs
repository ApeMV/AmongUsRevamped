using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionHolder.cs
namespace HNSRevamped
{
    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            taskOptionsLoad = Task.Run(Load);
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        public static void WaitOptionsLoad()
        {
            taskOptionsLoad.Wait();
        }

        //System
        public static OptionItem KickLowLevelPlayer;
        public static OptionItem TempBanLowLevelPlayer;

        public static OptionItem KickInvalidFriendCodes;
        public static OptionItem TempBanInvalidFriendCodes;

        public static OptionItem ApplyBanList;
        public static OptionItem ApplyModeratorList;

        public static OptionItem AutoRejoinLobby;

        public static OptionItem NoGameEnd;
        
        //Mod
        public static OptionItem DisableTasks;

        // HNS
        public static OptionItem NumSeekers;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;

            //Main settings
            KickLowLevelPlayer = IntegerOptionItem.Create(60050, "Kick Players Under Level", new(0, 100, 1), 0, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Level)
                .SetHeader(true);
            TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, "Ban Instead Of Kick", false, TabGroup.SystemSettings, false)
                .SetParent(KickLowLevelPlayer)
                .SetValueFormat(OptionFormat.Times);

            KickInvalidFriendCodes = BooleanOptionItem.Create(60080, "Kick Invalid FriendCodes", true, TabGroup.SystemSettings, false);
            TempBanInvalidFriendCodes = BooleanOptionItem.Create(60081, "Ban Instead Of Kick", false, TabGroup.SystemSettings, false)
                .SetParent(KickInvalidFriendCodes);

            ApplyBanList = BooleanOptionItem.Create(60110, "Apply BanList", true, TabGroup.SystemSettings, true);
            ApplyModeratorList = BooleanOptionItem.Create(60120, "Apply ModeratorList", false, TabGroup.SystemSettings, false);

            AutoRejoinLobby = BooleanOptionItem.Create(60210, "Auto Rejoin Lobby", false, TabGroup.SystemSettings, false);

            NoGameEnd = BooleanOptionItem.Create(60380, "No Game End", false, TabGroup.SystemSettings, false)
                .SetColor(Color.red)
                .SetHeader(true);



            NumSeekers = IntegerOptionItem.Create(70000, "# Seekers", new(0, 15, 1), 1, TabGroup.HNSSettings, false)
                .SetValueFormat(OptionFormat.Level)
                .SetHeader(true);
            IsLoaded = true;
        }
    }
}