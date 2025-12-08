global using HarmonyLib;
global using System.Collections.Generic;
global using System.Linq;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace HNSRevamped;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static BepInEx.Logging.ManualLogSource Logger;
    public static BasePlugin Instance;

    public override void Load()
    {
        var handler = HNSRevamped.Logger.Handler("GitVersion");        
        Logger = BepInEx.Logging.Logger.CreateLogSource("HNSRevamped");
        HNSRevamped.Logger.Enable();
        Instance = this;

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
}
