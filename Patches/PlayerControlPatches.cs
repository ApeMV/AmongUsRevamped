using AmongUs.Data;
using HarmonyLib;
using AmongUs.GameOptions;
using InnerNet;

namespace HNSRevamped;
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255) return;

        if (AmongUsClient.Instance.AmHost)
        {
            if (__instance.Data.PlayerLevel != 0 && __instance.Data.PlayerLevel < Options.KickLowLevelPlayer.GetInt() && __instance.Data.ClientId != AmongUsClient.Instance.HostId)
            {
                if (!Options.TempBanLowLevelPlayer.GetBool()) 
                {
                    AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, false);
                    Logger.Info($" {__instance.Data.PlayerName} was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}", "KickLowLevelPlayer");
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, true);
                    Logger.Info($" {__instance.Data.PlayerName} was banned for being under level {Options.KickLowLevelPlayer.GetInt()} ", "BanLowLevelPlayer");
                }
            }
        }
    }
}