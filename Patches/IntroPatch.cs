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

        Utils.ModeratorChatCommand(Translator.Get("abilitiesGenericOne", Options.CrewmateAbility.GetString(), Options.ScientistAbility.GetString(), Options.EngineerAbility.GetString(), Options.NoisemakerAbility.GetString(), Options.TrackerAbility.GetString()), Translator.Get("abilitiesGenericTwo", Options.DetectiveAbility.GetString(), Options.ImpostorAbility.GetString(), Options.ShapeshifterAbility.GetString(), Options.PhantomAbility.GetString(), Options.ViperAbility.GetString()), true);
        
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            p.cosmetics.nameText.text = p.Data.PlayerName;

            MurderPlayerPatch.killCount[p.Data.PlayerId] = 0;
            MurderPlayerPatch.misfireCount[p.Data.PlayerId] = 0;
            PlayerControlCompleteTaskPatch.playerTasksCompleted[p] = 0;
            PlayerControlCompleteTaskPatch.tasksPerPlayer[p] = 0;
        }

        if (Options.DisableAnnoyingMeetingCalls.GetBool() && !Utils.isHideNSeek)
        {
            Utils.CanCallMeetings = false;
            _ = new LateTask(() =>
            {       
                Utils.CanCallMeetings = true;
            }, 33f, "MeetingEnabled");     
        }

        if (Options.Gamemode.GetValue() == 2 && Options.SNSChatInGame.GetBool() /*|| Options.Gamemode.GetValue() == 0 && Options.ChatBeforeFirstMeeting.GetBool()*/)
        {
            _ = new LateTask(() =>
            {  
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                if (MeetingHud.Instance != null) MeetingHud.Instance.RpcClose(); 

            }, 9f, "SetChatVisible");  
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