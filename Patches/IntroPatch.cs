using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
internal static class CoShowIntroPatch
{
    public static bool IntroInitiated;
    public static void Postfix(IntroCutscene __instance)
    {
        Logger.Info(" Intro initiated", "CoShowIntro");

        if (!AmongUsClient.Instance.AmHost) return;

        IntroInitiated = true;

        AbilityManagement.SendRoleMessages();
        
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            p.cosmetics.nameText.text = p.Data.PlayerName;

            MurderPlayerPatch.killCount[p.PlayerId] = 0;
            MurderPlayerPatch.misfireCount[p.PlayerId] = 0;
            PlayerControlCompleteTaskPatch.playerTasksCompleted[p.PlayerId] = 0;
            PlayerControlCompleteTaskPatch.tasksPerPlayer[p.PlayerId] = 0;

            Logger.Info($" {p.Data.PlayerName} -> {p.Data.RoleType}", "RoleInfo");
        }

        if (!Utils.isHideNSeek && Options.Gamemode.GetValue() < 2) Logger.Info($" {AbilityManagement.RoleList()}", "AbilityInfo");

        // Run while the intro cutscene is still covering the screen, so players are already
        // scattered and recoloured by the time they can see the map. Waiting for OnGameStart
        // meant doing all of it in the open, several seconds after the round had begun.
        if (HotPotato.IsActive) new LateTask(HotPotato.Begin, 1f, "HotPotatoBegin");

        if (Options.DisableAnnoyingMeetingCalls.GetBool() && !Utils.isHideNSeek)
        {
            Utils.CanCallMeetings = false;
            _ = new LateTask(() =>
            {       
                Utils.CanCallMeetings = true;
            }, 33f, "MeetingEnabled");     
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
class BeginCrewmatePatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (Main.GM.Value)
        {
            __instance.TeamTitle.text = "Game Master";
        }
    }
}