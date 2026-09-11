using Hazel;
using System;
using UnityEngine;

namespace AmongUsRevamped;

internal class EACR
{
    public static bool PlayerControlReceiveRpc(PlayerControl pc, byte callId, MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost || pc == null || reader == null) return false;

        try
        {
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;

            switch (rpc)
            {
                case RpcCalls.StartMeeting:
                {
                    MeetingCheat(pc);
                    return true;
                }
            }

            switch (callId)
            {
                case 101: // Aum Chat
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();

                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);

                        if (!flag)
                        {
                            RPCCheat(pc, "Among Us Menu Chat");
                            return true;
                        }
                    }
                    catch
                    {

                    }
                    break;
                case unchecked((byte)42069): // 85 AUM
                    try
                    {
                        var aumid = sr.ReadByte();

                        if (aumid == pc.PlayerId)
                        {
                            RPCCheat(pc, "Among Us Menu");
                            return true;
                        }
                    }
                    catch
                    {

                    }
                    break;
                case 119: // KN Chat
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();

                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);

                        if (!flag)
                        {
                            RPCCheat(pc, "KillNetwork Chat");
                            return true;
                        }
                    }
                    catch
                    {

                    }
                    break;
                case 250: // KN
                    if (sr.BytesRemaining == 0)
                    {
                        RPCCheat(pc, "KillNetwork");
                        return true;
                    }
                    break;
                case unchecked((byte)420): // 164 Sicko
                    try
                    {
                        RPCCheat(pc, "SickoMenu");
                        return true;
                    }
                    catch
                    {

                    }
                    break;
                case 202: // SMC
                    try
                    {
                        RPCCheat(pc, "SlopMenuCrew");
                        return true;
                    }
                    catch
                    {

                    }
                    break;
                case 201: // SMC Chat
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();

                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);

                        if (!flag)
                        {
                            RPCCheat(pc, "SlopMenuCrew Chat");
                            return true;
                        }
                    }
                    catch
                    {

                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.Exception(e, "EACR");
        }
        return false;
    }

    public static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, byte amount)
    {
        var Mapid = Utils.GetActiveMapId();

        if (!AmongUsClient.Instance.AmHost)
        {
            return false;
        }

        if (player == null)
        {
            Logger.Warn("PlayerControl is null", "EACR-RpcUpdateSystemCheck");
            return true;
        }

        if (systemType == SystemTypes.Sabotage)
        {
            if (!player.Data.Role.IsImpostor)
            {
                SabotageCheat(player);            
            }
        }
        else if (systemType == SystemTypes.LifeSupp)
        {
            if (Mapid != 0 && Mapid != 1 && Mapid != 3) SabotageCheat(player); 
            else if (amount != 64 && amount != 65) SabotageCheat(player); 
        }
        else if (systemType == SystemTypes.Comms)
        {
            if (amount == 0)
            {
                if (Mapid == 1 || Mapid == 5) SabotageCheat(player); 
            }
            else if (amount == 64 || amount == 65 || amount == 32 || amount == 33 || amount == 16 || amount == 17)
            {
                if (!(Mapid == 1 || Mapid == 5)) SabotageCheat(player); 
            }
            else SabotageCheat(player); 
        }
        else if (systemType == SystemTypes.Electrical)
        {
            if (Mapid == 5 || amount >= 5) SabotageCheat(player); 
        }
        else if (systemType == SystemTypes.Laboratory)
        {
            if (Mapid != 2) SabotageCheat(player); 
            else if (!(amount == 64 || amount == 65 || amount == 32 || amount == 33)) SabotageCheat(player); 
        }
        else if (systemType == SystemTypes.Reactor)
        {
            if (Mapid == 2 || Mapid == 4) SabotageCheat(player); 
            else if (!(amount == 64 || amount == 65 || amount == 32 || amount == 33)) SabotageCheat(player); 
        }
        else if (systemType == SystemTypes.HeliSabotage)
        {
            if (Mapid != 4) SabotageCheat(player); 
            else if (!(amount == 64 || amount == 65 || amount == 16 || amount == 17 || amount == 32 || amount == 33)) SabotageCheat(player); 
        }
        else if (systemType == SystemTypes.MushroomMixupSabotage)
        {
            SabotageCheat(player); 
        }

        if (Utils.IsMeeting && MeetingHud.Instance.state != MeetingHud.MeetingStates.Animating)
        {
            SabotageCheat(player);
            return true;
        }

    return false;

    }

    public static void RPCCheat(PlayerControl player, string input)
    {
        if (Options.DetectBadRPC.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} Sent invalid RPC: {input} (hacking)");
        }
    }

    public static void SabotageCheat(PlayerControl player)
    {
        if (Options.DetectBadSabotage.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} invalidly sabotaged (hacking)");
        }
    }

    public static void VentCheat(PlayerControl player, Vent __instance)
    {
        if (Options.DetectBadVent.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} invalidly vented (hacking)");
        }
    }

    public static void PlayAnimationCheat(PlayerControl player)
    {
        if (Options.DetectBadAnimation.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} sent an invalid animation (hacking)");
        }
    }

    public static void TaskCheat(PlayerControl player)
    {
        if (Options.DetectBadTask.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} invalidly completed a task (hacking)");
        }
    }

    public static void MurderCheat(PlayerControl player)
    {
        if (Options.DetectBadKill.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} invalidly murdered a player (hacking)");
        }
    }
    
    public static void MeetingCheat(PlayerControl player)
    {
        if (Options.DetectBadMeeting.GetBool())
        {
            HackingPunishment(player, $"{player.Data.PlayerName} invalidly called a meeting (hacking)");
        }
    }

    public static void HackingPunishment(PlayerControl player, string input)
    {
        if (Options.HackingPunishment.GetValue() == 0)
        {
            AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);            
        }

        if (Options.HackingPunishment.GetValue() == 1)
        {
            AmongUsClient.Instance.KickPlayer(player.Data.ClientId, false);
        }

        Logger.SendInGame($"{input}");
        Logger.Info($" {input}", "EACR");
    }
}