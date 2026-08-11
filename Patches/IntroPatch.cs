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

        //OptionManager.CacheOptions();

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

        if (Options.Gamemode.GetValue() == 1 && Options.SNSChatInGameExtend.GetBool() && !Utils.isHideNSeek)
        {
            Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nMeetings & Reports = Off", $"Crewmates win by doing tasks or surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone\n{Options.MisfiresToSuicide.GetInt()} wrong kill(s) = suicide", true);         
        }

        if ((GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) == 0.01f))
        {
            Main.NormalOptions.KillCooldown = 8f;
            OptionManager.SyncGameOptions();

            _ = new LateTask(() =>
            {       
                Main.NormalOptions.KillCooldown = 0.01f;
                OptionManager.SyncGameOptions();
            }, 13f, "NoKcdEnabled");     
        }

        if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek)
        {
            if (Options.PNSChatInGame.GetBool())
            {
                Main.NormalOptions.KillCooldown = 13f;
                OptionManager.SyncGameOptions();

                _ = new LateTask(() =>
                {       
                    Main.NormalOptions.KillCooldown = 0.01f;
                    OptionManager.SyncGameOptions();
                }, 18f, "NoKcdEnabled");
            }
            else
            {
                Main.NormalOptions.KillCooldown = 8f;
                OptionManager.SyncGameOptions();

                _ = new LateTask(() =>
                {       
                    Main.NormalOptions.KillCooldown = 0.01f;
                    OptionManager.SyncGameOptions();
                }, 13f, "NoKcdEnabled");
            }          
        }

        if (Options.Gamemode.GetValue() == 3 && Options.PNSChatInGameExtend.GetBool() && !Utils.isHideNSeek)
        {
            Utils.ModeratorChatCommand($"Poof and Seek:\n\nImpostors can only move while vanished\n Meetings & Reports = Off\nVisibly moving {Options.BadMoveTimeToSuicide.GetInt()}s as Phantom = Suicide", $"Crewmates win by doing tasks or surviving {Options.PNSCrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone", true);
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