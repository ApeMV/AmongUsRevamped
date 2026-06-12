using AmongUs.GameOptions;
using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        Logger.Info(" -------- GAME STARTED --------", "StartGame");
        Logger.Info($" Gamemode: {Options.Gamemode.GetString()}", "StartGame");
        Logger.Info($" Players: {PlayerControl.AllPlayerControls.Count}", "StartGame");
        Logger.Info($" Map: {(MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId}", "StartGame");

        NormalGameEndChecker.imps.Clear();
        NormalGameEndChecker.LastWinReason = "";

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
class PlayerControlSetRolePatch
{
    public static bool FirstAssign;
    private static readonly HashSet<byte> ProcessedPlayers = new();
    public static HashSet<byte> Seekers = new();
    public static HashSet<PlayerControl> Jesters = new();
    private static readonly System.Random rand = new System.Random();

    public static bool Prefix(PlayerControl __instance, ref RoleTypes roleType, ref bool canOverrideRole)
    {
        if (!FirstAssign || !AmongUsClient.Instance.AmHost) return true;

        if (!ProcessedPlayers.Add(__instance.PlayerId)) return true;

        canOverrideRole = false;

        if (Main.GM.Value && __instance == PlayerControl.LocalPlayer)
        {
            roleType = RoleTypes.Crewmate;
            OnGameStartPatch.ScheduleExile = true;
        }

        if (Utils.isHideNSeek && Seekers.Count() == 0)
        {
            int seekersCount = Options.NumSeekers.GetInt();

            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (!Main.GM.Value || p != PlayerControl.LocalPlayer) candidates.Add(p);
            }

            seekersCount = Math.Min(seekersCount, candidates.Count);

            for (int j = candidates.Count - 1; j > 0; j--)
            {
                int k = rand.Next(j + 1);
                (candidates[j], candidates[k]) = (candidates[k], candidates[j]);
            }

            for (int j = 0; j < seekersCount; j++)
            {
                Seekers.Add(candidates[j].PlayerId);
            }
        }

        if (Utils.isHideNSeek)
        {
            if (Seekers.Contains(__instance.PlayerId)) roleType = RoleTypes.Impostor;
            else roleType = RoleTypes.Engineer;
        }

        if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek)
        {
            if (Options.EngineerMode.GetBool()) roleType = RoleTypes.Engineer;
            else roleType = RoleTypes.Crewmate;
        }

        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek)
        {
            if (roleType == RoleTypes.Impostor || roleType == RoleTypes.Phantom || roleType == RoleTypes.Viper) roleType = RoleTypes.Shapeshifter;
        }

        if (ProcessedPlayers.Count >= PlayerControl.AllPlayerControls.Count)
        {
            Seekers.Clear();
            ProcessedPlayers.Clear();
            FirstAssign = false;

            Logger.Info("PCSRP successful", "RoleManaging");
        }
        
        if (roleType == RoleTypes.Crewmate && Options.CrewmateAbility.GetValue() == 2 ||
            roleType == RoleTypes.Scientist && Options.ScientistAbility.GetValue() == 2 ||
            roleType == RoleTypes.Engineer && Options.EngineerAbility.GetValue() == 2 ||
            roleType == RoleTypes.Noisemaker && Options.NoisemakerAbility.GetValue() == 2 ||
            roleType == RoleTypes.Tracker && Options.TrackerAbility.GetValue() == 2 ||
            roleType == RoleTypes.Detective && Options.DetectiveAbility.GetValue() == 2)
        {
            Jesters.Add(__instance);
        }
    
        return true;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.OnGameStart))]
internal static class OnGameStartPatch
{
    public static bool ScheduleExile;
    public static void Postfix()
    {
        if (ScheduleExile)
        {
            PlayerControl.LocalPlayer.Exiled();
            ScheduleExile = false;
        }
    }
}