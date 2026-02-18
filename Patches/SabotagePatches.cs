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

        if (Options.Gamemode.GetValue() == 0 || Options.Gamemode.GetValue() == 1)
        {
            if (Sabo == SystemTypes.LifeSupp && Options.DisableOxygen.GetBool() ||
            Sabo == SystemTypes.Reactor && Options.DisableReactor.GetBool() ||
            Sabo == SystemTypes.Electrical && Options.DisableLights.GetBool() ||
            Sabo == SystemTypes.Comms && Options.DisableComms.GetBool() ||
            Sabo == SystemTypes.HeliSabotage && Options.DisableHeli.GetBool() ||
            Sabo == SystemTypes.MushroomMixupSabotage && Options.DisableMushroomMixup.GetBool() ||
            player.Data.IsDead && !Options.DeadImpostorsCanSabotage.GetBool())
            {
                Logger.Info($" Sabotage {Sabo} by: {player.Data.PlayerName} was blocked", "SabotageCheck");
                return false;
            }
            return true;
        }

        if (Options.Gamemode.GetValue() == 2)
        {
            if (Sabo == SystemTypes.LifeSupp && Options.SNSDisableOxygen.GetBool() ||
            Sabo == SystemTypes.Reactor && Options.SNSDisableReactor.GetBool() ||
            Sabo == SystemTypes.Electrical && Options.SNSDisableLights.GetBool() ||
            Sabo == SystemTypes.Comms && Options.SNSDisableComms.GetBool() ||
            Sabo == SystemTypes.HeliSabotage && Options.SNSDisableHeli.GetBool() ||
            Sabo == SystemTypes.MushroomMixupSabotage && Options.SNSDisableMushroomMixup.GetBool() ||
            player.Data.IsDead && !Options.DeadImpostorsCanSabotage.GetBool())
            {
                Logger.Info($" Sabotage {Sabo} by: {player.Data.PlayerName} was blocked", "SnSSabotageCheck");
                return false;
            }
            return true;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
class ShipStatusCloseDoorsPatch
{
    public static bool Prefix(SystemTypes room)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        
        Logger.Info($" Trying to close the door in: {room}", "DoorCheck");

        if (Options.DisableCloseDoor.GetBool() && (Options.Gamemode.GetValue() == 0 || Options.Gamemode.GetValue() == 1) || Options.Gamemode.GetValue() == 2 && Options.SNSDisableCloseDoor.GetBool())
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