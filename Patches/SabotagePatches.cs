using Hazel;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class SabotageSystemTypeRepairDamagePatch
{
    private static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        byte amount;
        {
            var newReader = MessageReader.Get(msgReader);
            amount = newReader.ReadByte();
            newReader.Recycle();
        }
        var Sabo = (SystemTypes)amount;

        Logger.Info($" {player.Data.PlayerName} is trying to sabotage: {Sabo}", "SabotageCheck");
        if (Options.DisableSabotage.GetBool() || Options.Gamemode.GetValue() == 2)
        {
            Logger.Info($" Sabotage {Sabo} by: {player.Data.PlayerName} was blocked", "SabotageCheck");
            return false;
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

        if (Options.DisableCloseDoor.GetBool() || Options.Gamemode.GetValue() == 2)
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

        var amount = MessageReader.Get(reader).ReadByte();
        if (EACR.RpcUpdateSystemCheck(player, systemType, amount))
        {
            Logger.Info("EACR patched Sabotage RPC", "MessageReaderUpdateSystemPatch");
            return false;
        }
        else return true;
    }
}