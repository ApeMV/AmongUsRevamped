using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost || __instance == null) return true;

        if (Options.DisableAnnoyingMeetingCalls.GetBool() && !Utils.CanCallMeetings && target == null)
        {
            Logger.Info($" {__instance.Data.PlayerName} is calling a meeting too fast, attempt blocked", "ReportDeadBodyPatch");
            return false;
        }

        // target == null means meeting
        if (target == null) return true;

        if (Options.DisableReport.GetBool() || Options.Gamemode.GetValue() == 2 || Options.Gamemode.GetValue() == 3)
        {
            Logger.Info($" Stopped {__instance.Data.PlayerName} reporting the body of {target.PlayerName}", "ReportDeadBodyPatch");
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class MurderPlayerPatch
{
    public static readonly Dictionary<byte, int> misfireCount = new();

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags, ref bool __state)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        //2 = Shift and Seek
        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek)
        {
            if (!resultFlags.HasFlag(MurderResultFlags.Succeeded))
            return;

            byte playerId = __instance.Data.PlayerId;

            if (target.Data.PlayerId == __instance.shapeshiftTargetPlayerId)
            {
                Logger.Info($" {__instance.Data.PlayerName} correctly killed {target.Data.PlayerName} ", "SNSKillManager");
            }
            else
            {
                if (!misfireCount.ContainsKey(playerId))
                misfireCount[playerId] = 0;

                misfireCount[playerId]++;

                if (misfireCount[__instance.Data.PlayerId] < Options.MisfiresToSuicide.GetFloat())
                {
                    __instance.RpcSetRole(RoleTypes.Crewmate);
                    __instance.isNew = true;
                    Logger.Info($" {__instance.Data.PlayerName} killed {target.Data.PlayerName} incorrectly and can't kill for {Options.CantKillTime.GetInt()}s", "SNSKillManager");
                    Logger.SendInGame($" {__instance.Data.PlayerName} killed {target.Data.PlayerName} incorrectly and can't kill for {Options.CantKillTime.GetInt()}s");

                    new LateTask(() =>
                    {
                        __instance.isNew = false;
                        if (!__instance.Data.IsDead) {__instance.RpcSetRole(RoleTypes.Shapeshifter, false);}
                    }, Options.CantKillTime.GetInt(), "SNSResetRole");
                }

                if (misfireCount[__instance.Data.PlayerId] >= Options.MisfiresToSuicide.GetFloat())
                {
                    __instance.RpcSetRole(RoleTypes.ImpostorGhost);
                    Logger.Info($" {__instance.Data.PlayerName} misfired {misfireCount[playerId]} times and suicided", "SNSKillManager");
                    Logger.SendInGame($" {__instance.Data.PlayerName} misfired {misfireCount[playerId]} times and suicided");
                }

            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
internal static class CheckShapeshiftPatch
{

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        // Canceling a Shapeshift freezes the player until they successfully Shapeshift again. Unavoidable game logic.
        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek && __instance.isNew)
        {
            Logger.Info($" {__instance.Data.PlayerName} shapeshifted during misfire cooldown, making the game temporarily freeze them.", "SNSShapeshiftManager");
            Logger.SendInGame($" {__instance.Data.PlayerName} shapeshifted during misfire cooldown, making the game temporarily freeze them.");
            return false;
        }
        else return true;
    }
}
