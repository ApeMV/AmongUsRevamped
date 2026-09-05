using Hazel;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class SabotageSystemTypeRepairDamagePatch
{
    private static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
    {
        byte amount;
        {
            var newReader = MessageReader.Get(msgReader);
            amount = newReader.ReadByte();
            newReader.Recycle();
        }
        var Sabo = (SystemTypes)amount;
        Logger.Info($" {player.Data.PlayerName} is trying to sabotage: {Sabo}", "SabotageCheck");

        if (Options.Gamemode.GetValue() == 0)
        {
            if (Sabo == SystemTypes.LifeSupp && Options.DisableOxygen.GetBool() ||
            Sabo == SystemTypes.Reactor && Options.DisableReactor.GetBool() ||
            Sabo == SystemTypes.Electrical && Options.DisableLights.GetBool() ||
            Sabo == SystemTypes.Comms && Options.DisableComms.GetBool() ||
            Sabo == SystemTypes.HeliSabotage && Options.DisableHeli.GetBool() ||
            Sabo == SystemTypes.MushroomMixupSabotage && Options.DisableMushroomMixup.GetBool() ||
            Sabo == SystemTypes.Laboratory && Options.DisableReactor.GetBool() && msgReader != null ||
            player.Data.IsDead && !Options.DeadImpostorsCanSabotage.GetBool())
            {
                Logger.Info($" Sabotage {Sabo} by: {player.Data.PlayerName} was blocked", "SabotageCheck");
                return false;
            }
            return true;
        }

        if (Options.Gamemode.GetValue() == 1)
        {
            if (Sabo == SystemTypes.LifeSupp && Options.SNSDisableOxygen.GetBool() ||
            Sabo == SystemTypes.Reactor && Options.SNSDisableReactor.GetBool() ||
            Sabo == SystemTypes.Electrical && Options.SNSDisableLights.GetBool() ||
            Sabo == SystemTypes.Comms && Options.SNSDisableComms.GetBool() ||
            Sabo == SystemTypes.HeliSabotage && Options.SNSDisableHeli.GetBool() ||
            Sabo == SystemTypes.MushroomMixupSabotage && Options.SNSDisableMushroomMixup.GetBool() ||
            Sabo == SystemTypes.Laboratory && Options.SNSDisableReactor.GetBool() && msgReader != null ||
            player.Data.IsDead && !Options.DeadImpostorsCanSabotage.GetBool())
            {
                Logger.Info($" Sabotage {Sabo} by: {player.Data.PlayerName} was blocked", "SnSSabotageCheck");
                return false;
            }
            return true;
        }

        if (Options.Gamemode.GetValue() == 3)
        {
            if (Sabo == SystemTypes.LifeSupp && Options.PNSDisableOxygen.GetBool() ||
            Sabo == SystemTypes.Reactor && Options.PNSDisableReactor.GetBool() ||
            Sabo == SystemTypes.Electrical && Options.PNSDisableLights.GetBool() ||
            Sabo == SystemTypes.Comms && Options.PNSDisableComms.GetBool() ||
            Sabo == SystemTypes.HeliSabotage && Options.PNSDisableHeli.GetBool() ||
            Sabo == SystemTypes.MushroomMixupSabotage && Options.PNSDisableMushroomMixup.GetBool() ||
            Sabo == SystemTypes.Laboratory && Options.PNSDisableReactor.GetBool() && msgReader != null ||
            player.Data.IsDead && !Options.DeadImpostorsCanSabotage.GetBool())
            {
                Logger.Info($" Sabotage {Sabo} by: {player.Data.PlayerName} was blocked", "SnSSabotageCheck");
                return false;
            }
            return true;
        }
        else return true;
    }

    public static void Postfix(SabotageSystemType __instance)
    {
        if (!Options.CustomizeSabotages.GetBool() || !AmongUsClient.Instance.AmHost) return;

        __instance.Timer = Options.SabotageCooldown.GetInt();
        __instance.IsDirty = true;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
class ShipStatusCloseDoorsPatch
{
    public static bool Prefix(SystemTypes room)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        
        Logger.Info($" Trying to close the door in: {room}", "DoorCheck");

        if ((Options.DisableCloseDoor.GetBool() && Options.Gamemode.GetValue() == 0) || (Options.Gamemode.GetValue() == 1 && Options.SNSDisableCloseDoor.GetBool()) || (Options.Gamemode.GetValue() == 3 && Options.PNSDisableCloseDoor.GetBool()))
        {
            Logger.Info($" Door sabotage in: {room} was blocked", "DoorCheck");
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
public static class MessageReaderUpdateSystemPatch
{
    public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
    {
        if (systemType is
            SystemTypes.Ventilation
            or SystemTypes.Security
            or SystemTypes.Decontamination
            or SystemTypes.Decontamination2
            or SystemTypes.Decontamination3
            or SystemTypes.MedBay) return true;

        if (player.Data.ClientId == AmongUsClient.Instance.HostId) return true;

        var amount = MessageReader.Get(reader).ReadByte();
        if (EACR.RpcUpdateSystemCheck(player, systemType, amount))
        {
            Logger.Info("EACR patched Sabotage RPC", "MessageReaderUpdateSystemPatch");
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(ReactorSystemType))]
public static class ReactorSystemTypePatch
{
    private static bool SetDuration = true;

    [HarmonyPatch(nameof(ReactorSystemType.Deteriorate))]
    [HarmonyPrefix]
    public static void Deteriorate_Prefix(ReactorSystemType __instance)
    {
        if (!AmongUsClient.Instance.AmHost || Main.NormalOptions.MapId == 4 || !Options.CustomizeSabotages.GetBool()) return;

        if (!__instance.IsActive || !ShipStatus.Instance || !SetDuration)
        {
            if (!SetDuration && !__instance.IsActive) SetDuration = true;
            return;
        }

        Logger.Info($" {ShipStatus.Instance.Type} - {SetDuration}", "ReactorSystemTypePatch");
        SetDuration = false;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship:
                __instance.Countdown = Options.SkeldReactorDuration.GetFloat();
                return;
            case ShipStatus.MapType.Hq:
                __instance.Countdown = Options.MiraReactorDuration.GetFloat();
                return;
            case ShipStatus.MapType.Pb:
                __instance.Countdown = Options.PolusReactorDuration.GetFloat();
                return;
            case ShipStatus.MapType.Fungle:
                __instance.Countdown = Options.FungleReactorDuration.GetFloat();
                return;
            default:
                return;
        }
    }
    [HarmonyPatch(nameof(ReactorSystemType.Deteriorate))]
    [HarmonyPostfix]
    public static void Deteriorate_Postfix(ReactorSystemType __instance)
    {
        if (__instance.IsActive && __instance.Countdown <= 0 && !Utils.HandlingGameEnd)
        {
            Utils.ContinueEndGame((byte)GameOverReason.ImpostorsBySabotage);
            NormalGameEndChecker.CheckWinnerText("ImpostorSabotage");
        }
    }
}

[HarmonyPatch(typeof(HeliSabotageSystem))]
public static class HeliSabotageSystemPatch
{
    private static bool SetDuration = true;

    [HarmonyPatch(nameof(HeliSabotageSystem.Deteriorate))]
    [HarmonyPrefix]
    public static void Deteriorate_Prefix(HeliSabotageSystem __instance)
    {
        if (!AmongUsClient.Instance.AmHost || Main.NormalOptions.MapId != 4 || !Options.CustomizeSabotages.GetBool()) return;

        if (!__instance.IsActive || !ShipStatus.Instance || !SetDuration)
        {
            if (!SetDuration && !__instance.IsActive) SetDuration = true;
            return;
        }

        Logger.Info($" {ShipStatus.Instance.Type} - {SetDuration}", "HeliSabotageSystemPatch");
        SetDuration = false;

        __instance.Countdown = Options.AirshipReactorDuration.GetFloat();
    }
    [HarmonyPatch(nameof(HeliSabotageSystem.Deteriorate))]
    [HarmonyPostfix]
    public static void Deteriorate_Postfix(HeliSabotageSystem __instance)
    {
        if (__instance.IsActive && __instance.Countdown <= 0 && !Utils.HandlingGameEnd)
        {
            Utils.ContinueEndGame((byte)GameOverReason.ImpostorsBySabotage);
            NormalGameEndChecker.CheckWinnerText("ImpostorSabotage");
        }
    }
}

[HarmonyPatch(typeof(LifeSuppSystemType))]
public static class LifeSuppSystemTypePatch
{
    private static bool SetDuration = true;

    [HarmonyPatch(nameof(LifeSuppSystemType.Deteriorate))]
    [HarmonyPrefix]
    public static void Deteriorate_Prefix(LifeSuppSystemType __instance)
    {
        if (!AmongUsClient.Instance.AmHost || Main.NormalOptions.MapId == 2 || Main.NormalOptions.MapId == 4 || Main.NormalOptions.MapId == 5 || !Options.CustomizeSabotages.GetBool()) return;

        if (!__instance.IsActive || !ShipStatus.Instance || !SetDuration)
        {
            if (!SetDuration && !__instance.IsActive) SetDuration = true;
            return;
        }

        Logger.Info($" {ShipStatus.Instance.Type} - {SetDuration}", "LifeSuppSystemType");
        SetDuration = false;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship:
                __instance.Countdown = Options.SkeldO2Duration.GetFloat();
                return;
            case ShipStatus.MapType.Hq:
                __instance.Countdown = Options.MiraO2Duration.GetFloat();
                return;
            default:
                return;
        }
    }
    [HarmonyPatch(nameof(LifeSuppSystemType.Deteriorate))]
    [HarmonyPostfix]
    public static void Deteriorate_Postfix(LifeSuppSystemType __instance)
    {
        if (__instance.IsActive && __instance.Countdown <= 0 && !Utils.HandlingGameEnd)
        {
            Utils.ContinueEndGame((byte)GameOverReason.ImpostorsBySabotage);
            NormalGameEndChecker.CheckWinnerText("ImpostorSabotage");
        }
    }
}

[HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.Deteriorate))]
public static class MushroomMixupSabotageSystemPatch
{
    private static bool SetDuration = true;
    public static void Prefix(MushroomMixupSabotageSystem __instance, ref bool __state)
    {
        __state = __instance.IsActive;

        if (!AmongUsClient.Instance.AmHost || Main.NormalOptions.MapId != 5 || !Options.CustomizeSabotages.GetBool()) return;

        if (!__instance.IsActive || !ShipStatus.Instance || !SetDuration)
        {
            if (!SetDuration && !__instance.IsActive) SetDuration = true;
            return;
        }

        Logger.Info($" {ShipStatus.Instance.Type} - {SetDuration}", "MushroomMixupSabotageSystem");
        SetDuration = false;

        __instance.currentSecondsUntilHeal = Options.FungleMushroomMixupDuration.GetFloat();
    }
}