using Hazel;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))] // SetInitialSabotageCooldown - set sabotage cooldown in start game
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