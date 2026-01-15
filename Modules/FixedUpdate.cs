using Hazel;
using System;
using System.Runtime.CompilerServices;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
public static class FixedUpdate
{
    public static void Postfix()
    {
        GameObject n = GameObject.Find("NewRequestInactive");
        if (n != null)
        {
            n.SetActive(false);
        }

        GameObject nr = GameObject.Find("NewRequest");
        if (nr != null)
        {
            nr.SetActive(false);
        }

        if (!AmongUsClient.Instance.AmHost) return;

        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            // 2 = Shift and Seek
            if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek && Options.CrewAutoWinsGameAfter.GetInt() != 0 && !Options.NoGameEnd.GetBool())
            {
                Main.GameTimer += Time.fixedDeltaTime;
                        
                if (Main.GameTimer > Options.CrewAutoWinsGameAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    MessageWriter writer = AmongUsClient.Instance.StartEndGame();
                    writer.Write((byte)GameOverReason.ImpostorDisconnect);
                    AmongUsClient.Instance.FinishEndGame(writer);
                    Logger.Info($" Crewmates won because the game took longer than {Options.CrewAutoWinsGameAfter.GetInt()}s", "SNSManager");
                    NormalGameEndChecker.LastWinReason = $"★ Crewmates win! (Timer)\n\nImpostors:\n" + string.Join("\n", NormalGameEndChecker.imps.Select(p => p.Data.PlayerName));
                }
            }
            // 3 = Speedrun
            if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek && Options.GameAutoEndsAfter.GetInt() != 0 && !Options.NoGameEnd.GetBool())
            {
                Main.GameTimer += Time.fixedDeltaTime;
                        
                if (Main.GameTimer > Options.GameAutoEndsAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    MessageWriter writer = AmongUsClient.Instance.StartEndGame();
                    writer.Write((byte)GameOverReason.CrewmatesByVote);
                    AmongUsClient.Instance.FinishEndGame(writer);
                    Logger.Info($" No one won because the game took longer than {Options.GameAutoEndsAfter.GetInt()}s", "SpeedrunManager");
                    NormalGameEndChecker.LastWinReason = $"★ No one wins! (Timer)";
                }
            }
        }
    }
}