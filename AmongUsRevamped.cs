global using HarmonyLib;
global using System.Collections.Generic;
global using System.Linq;

using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using System;
using UnityEngine;

namespace AmongUsRevamped;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static BepInEx.Logging.ManualLogSource Logger;
    public static BasePlugin Instance;

    public static ConfigEntry<bool> GM { get; private set; }
    public static ConfigEntry<bool> UnlockFps { get; private set; }
    public static ConfigEntry<bool> ShowFps { get; private set; }
    public static ConfigEntry<bool> AutoStart { get; private set; }
    public static ConfigEntry<bool> DarkTheme { get; private set; }
    public static ConfigEntry<bool> LobbyMusic { get; private set; }

    public static NormalGameOptionsV10 NormalOptions => GameOptionsManager.Instance != null ? GameOptionsManager.Instance.currentNormalGameOptions : null;
    public static bool HasArgumentException;
    public static string CredentialsText;
    public const string ModVersion = "v1.2.1";

    public static float GameTimer;

    public static PlayerControl[] AllPlayerControls
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            var i = 0;

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.PlayerId >= 254) continue;

                result[i++] = pc;
            }

            if (i == 0) return [];

            Array.Resize(ref result, i);
            return result;
        }
    }

    public override void Load()
    {
        var handler = AmongUsRevamped.Logger.Handler("GitVersion");        
        Logger = BepInEx.Logging.Logger.CreateLogSource("AmongUsRevamped");
        AmongUsRevamped.Logger.Enable();
        Instance = this;

        AutoStart = Config.Bind("Client Options", "Auto Start", false);
        GM = Config.Bind("Client Options", "Game Master", false);
        UnlockFps = Config.Bind("Client Options", "Unlock FPS", false);
        ShowFps = Config.Bind("Client Options", "Show FPS", false);
        AutoStart = Config.Bind("Client Options", "Auto Start", false);
        DarkTheme = Config.Bind("Client Options", "Dark Theme", false);
        LobbyMusic = Config.Bind("Client Options", "Lobby Music", false);

        BanManager.Init();
        
        Harmony.PatchAll();
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    class ModManagerLateUpdatePatch
    {
        public static void Prefix(ModManager __instance)
        {
            __instance.ShowModStamp();
            LateTask.Update(Time.deltaTime);
        }
    }

    public enum ColorToString
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Pink = 3,
        Orange = 4,
        Yellow = 5,
        Black = 6,
        White = 7,
        Purple = 8,
        Brown = 9,
        Cyan = 10,
        Lime = 11,
        Maroon = 12,
        Rose = 13,
        Banana = 14,
        Gray = 15,
        Tan = 16
    }

    // Innersloth now censors messages with more than 5 numbers. This is the best bypass I can give
    public enum BasicNumberToLetter
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10
    }
}