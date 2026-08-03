using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
internal static class CoShowIntroPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Logger.Info(" Intro initiated", "CoShowIntro");

        if (!AmongUsClient.Instance.AmHost) return;
        
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            p.cosmetics.nameText.text = p.Data.PlayerName;

            MurderPlayerPatch.killCount[p.PlayerId] = 0;
            MurderPlayerPatch.misfireCount[p.PlayerId] = 0;
            PlayerControlCompleteTaskPatch.playerTasksCompleted[p.PlayerId] = 0;
            PlayerControlCompleteTaskPatch.tasksPerPlayer[p.PlayerId] = 0;

            Logger.Info($" {p.Data.PlayerName} -> {p.Data.RoleType}", "RoleInfo");
        }

        if (Options.DisableAnnoyingMeetingCalls.GetBool() && !Utils.isHideNSeek)
        {
            Utils.CanCallMeetings = false;
            _ = new LateTask(() =>
            {       
                Utils.CanCallMeetings = true;
            }, 33f, "MeetingEnabled");     
        }

        if (Options.Gamemode.GetValue() == 1 && Options.SNSChatInGameExtend.GetBool())
        {
            if (Options.MisfiresToSuicide.GetInt() == 1 || Options.CantKillTime.GetInt() == 0)
            {
                Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", $"Crewmates win by doing tasks or surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone\n{Options.MisfiresToSuicide.GetInt()} wrong kill(s) = suicide", true);                
            }
            else
            {
                        
                Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", SendChatPatch.ConvertNum($"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Options.MisfiresToSuicide.GetInt()} wrong kills = suicide"), true);
            }            
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