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

        //Mod
        public static OptionItem DisableTasks;


        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;

            //Main settings
            KickLowLevelPlayer = IntegerOptionItem.Create(60050, "KickLowLevelPlayer", new(0, 100, 1), 0, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Level)
                .SetHeader(true);
            TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, "TempBanLowLevelPlayer", false, TabGroup.SystemSettings, false)
                .SetParent(KickLowLevelPlayer)
                .SetValueFormat(OptionFormat.Times);

            KickInvalidFriendCodes = BooleanOptionItem.Create(60080, "KickInvalidFriendCodes", true, TabGroup.SystemSettings, false);
            TempBanInvalidFriendCodes = BooleanOptionItem.Create(60081, "TempBanInvalidFriendCodes", false, TabGroup.SystemSettings, false)
                .SetParent(KickInvalidFriendCodes);

            ApplyBanList = BooleanOptionItem.Create(60110, "ApplyBanList", true, TabGroup.SystemSettings, true);
            ApplyModeratorList = BooleanOptionItem.Create(60120, "ApplyModeratorList", false, TabGroup.SystemSettings, false);

            IsLoaded = true;
        }
    }
}