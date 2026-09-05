using Hazel;
using System;
using UnityEngine;

namespace AmongUsRevamped;

internal class EACR
{
    public static bool PlayerControlReceiveRpc(PlayerControl pc, byte callId, MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (pc == null || reader == null) return false;

        try
        {
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;
            switch (rpc)
            {
                case RpcCalls.StartMeeting:
                {
                    AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                    Logger.SendInGame($"{pc.Data.PlayerName} was banned for being calling an invalid meeting (cheating)");
                    Logger.Info($" {pc.Data.PlayerName} was banned for being calling an invalid meeting (cheating)", "EACR");
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
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} was banned for AUM chat CallId (cheating)");
                            Logger.Info($" {pc.Data.PlayerName} was banned for AUM chat CallId (cheating)", "EACR");
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
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} was banned for AUM CallId (cheating)");
                            Logger.Info($" {pc.Data.PlayerName} was banned for AUM CallId (cheating)", "EACR");
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
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} was banned for KN chat CallId (cheating)");
                            Logger.Info($" {pc.Data.PlayerName} was banned for KN chat CallId (cheating)", "EACR");
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
                        AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                        Logger.SendInGame($"{pc.Data.PlayerName} was banned for KN CallId (cheating)");
                        Logger.Info($" {pc.Data.PlayerName} was banned for KN CallId (cheating)", "EACR");
                        return true;
                    }
                    break;
                case unchecked((byte)420): // 164 Sicko
                    if (sr.BytesRemaining == 0)
                    {
                        AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                        Logger.SendInGame($"{pc.Data.PlayerName} was banned for Sickomenu RPC (cheating)");
                        Logger.Info($" {pc.Data.PlayerName} was banned for Sickomenu RPC (cheating)", "EACR");
                        return true;
                    }
                    break;
                case 202: // SMC
                    try
                    {
                        AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                        Logger.SendInGame($"{pc.Data.PlayerName} was banned for MMC CallId (cheating)");
                        Logger.Info($" {pc.Data.PlayerName} was banned for MMC CallId (cheating)", "EACR");
                        return true;
                    }
                    catch
                    {

                    }
                    break;
                case 201:
                    try
                    {
                        var firstString = sr.ReadString();
                        var secondString = sr.ReadString();
                        sr.ReadInt32();

                        var flag = string.IsNullOrEmpty(firstString) && string.IsNullOrEmpty(secondString);

                        if (!flag)
                        {
                            AmongUsClient.Instance.KickPlayer(pc.Data.ClientId, true);
                            Logger.SendInGame($"{pc.Data.PlayerName} was banned for MMC chat CallId (cheating)");
                            Logger.Info($" {pc.Data.PlayerName} was banned for MMC chat CallId (cheating)", "EACR");
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
            if (!player.Data.Role.IsImpostor && !player.isNew)
            {
                AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
                Logger.SendInGame($"{player.Data.PlayerName} was banned for invalid sabotage (cheating)");
                Logger.Info($" {player.Data.PlayerName} was banned for invalid sabotage", "EACR");                
            }
        }
        else if (systemType == SystemTypes.LifeSupp)
        {
            if (Mapid != 0 && Mapid != 1 && Mapid != 3) goto YesCheat;
            else if (amount != 64 && amount != 65) goto YesCheat;
        }
        else if (systemType == SystemTypes.Comms)
        {
            if (amount == 0)
            {
                if (Mapid == 1 || Mapid == 5) goto YesCheat;
            }
            else if (amount == 64 || amount == 65 || amount == 32 || amount == 33 || amount == 16 || amount == 17)
            {
                if (!(Mapid == 1 || Mapid == 5)) goto YesCheat;
            }
            else goto YesCheat;
        }
        else if (systemType == SystemTypes.Electrical)
        {
            if (Mapid == 5) goto YesCheat;
            if (amount >= 5)
            {
                goto YesCheat;
            }
        }
        else if (systemType == SystemTypes.Laboratory)
        {
            if (Mapid != 2) goto YesCheat;
            else if (!(amount == 64 || amount == 65 || amount == 32 || amount == 33)) goto YesCheat;
        }
        else if (systemType == SystemTypes.Reactor)
        {
            if (Mapid == 2 || Mapid == 4) goto YesCheat;
            else if (!(amount == 64 || amount == 65 || amount == 32 || amount == 33)) goto YesCheat;
        }
        else if (systemType == SystemTypes.HeliSabotage)
        {
            if (Mapid != 4) goto YesCheat;
            else if (!(amount == 64 || amount == 65 || amount == 16 || amount == 17 || amount == 32 || amount == 33)) goto YesCheat;
        }
        else if (systemType == SystemTypes.MushroomMixupSabotage)
        {
            goto YesCheat;
        }

        if (Utils.IsMeeting && MeetingHud.Instance.state != MeetingHud.MeetingStates.Animating)
        {
            Logger.SendInGame($"{player.Data.PlayerName} might have called an invalid sabotage (cheating)");
            Logger.Info($" {player.Data.PlayerName} might have called an invalid sabotage (cheating)", "EACR");
            return true;
        }

    return false;

    YesCheat:
        {
            AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
            Logger.SendInGame($"{player.Data.PlayerName} was banned for an invalid sabotage (cheating)");
            Logger.Info($" {player.Data.PlayerName} was banned for an invalid sabotage", "EACR");     
            return true;
        }
    }

    public static void VentCheat(PlayerControl player, Vent __instance)
    {
        AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
        Logger.SendInGame($"{player.Data.PlayerName} was banned for invalid venting (cheating)");
        Logger.Info($" {player.Data.PlayerName} was banned for invalid venting", "EACR");
    }

    public static void PlayAnimationCheat(PlayerControl player)
    {
        AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
        Logger.SendInGame($"{player.Data.PlayerName} was banned for an invalid animation (cheating)");
        Logger.Info($" {player.Data.PlayerName} was banned for an invalid animation", "EACR");
    }

    public static void TaskCheat(PlayerControl player)
    {
        AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
        Logger.SendInGame($"{player.Data.PlayerName} was banned for invalidly completing a task (cheating)");
        Logger.Info($" {player.Data.PlayerName} was banned for invalidly completing a task", "EACR");
    }

    public static void MurderCheat(PlayerControl player)
    {
        AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
        Logger.SendInGame($"{player.Data.PlayerName} was banned for invalidly murdering a player (cheating)");
        Logger.Info($" {player.Data.PlayerName} was banned for invalidly murdering a player", "EACR");
    }
    
    public static void MeetingCheat(PlayerControl player)
    {
        AmongUsClient.Instance.KickPlayer(player.Data.ClientId, true);
        Logger.SendInGame($"{player.Data.PlayerName} was banned for invalidly calling a meeting (cheating)");
        Logger.Info($" {player.Data.PlayerName} was banned for invalidly calling a meeting", "EACR");
    }
}