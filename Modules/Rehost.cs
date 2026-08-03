using InnerNet;
using UnityEngine;
using System;
using AmongUs.GameOptions;

namespace AmongUsRevamped
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
    public static class DisconnectManager
    {
        public static bool Rehosting;
        public static void Prefix(InnerNetClient __instance, DisconnectReasons reason, string stringReason)
        {
           Logger.Info($" Disconnected due to {reason}/{stringReason}, ping:{__instance.Ping})", "DisconnectManager");

            if (!Options.Rehost.GetBool() || reason == DisconnectReasons.NewConnection || reason == DisconnectReasons.ConnectionLimit || reason == DisconnectReasons.ExitGame) return;

            _ = new LateTask(() =>
            {
                Rehosting = true;
                PSManager.Instance.CreateGame(GameModes.Normal);
                Logger.Info(Translator.Get("rehostSuccess"), "DisconnectManager");

            }, 5f, "RehostManager");
        }
    }
}
