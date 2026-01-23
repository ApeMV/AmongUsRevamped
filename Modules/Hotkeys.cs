using AmongUs.Data;
using Hazel;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class Hotkeys
{
    public static void Postfix()
    {
        // I don't know which psychopath would use right keys, but I know someday, someone will complain
        bool Shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool Ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool Enter = Input.GetKeyDown(KeyCode.Return);

        if (Input.GetKey(KeyCode.L) && Shift && Enter)
        {
            MessageWriter writer = AmongUsClient.Instance.StartEndGame();
            writer.Write((byte)GameOverReason.ImpostorDisconnect);
            AmongUsClient.Instance.FinishEndGame(writer);
        }
        
        if (Input.GetKey(KeyCode.M) && Shift && Enter && Utils.InGame)
        {
            if (Utils.IsMeeting)
            {
                MeetingHud.Instance.RpcClose();
            }
            else
            {
                PlayerControl.LocalPlayer.ReportDeadBody(PlayerControl.LocalPlayer.Data);
            }
        }

        if (Shift && GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown && !HudManager.Instance.Chat.IsOpenOrOpening)
        {
            GameStartManager.Instance.countDownTimer = 0;
        }

        if ((!AmongUsClient.Instance.IsGameStarted || !Utils.IsOnlineGame) && Utils.CanMove && PlayerControl.LocalPlayer.Collider != null)
        {
            PlayerControl.LocalPlayer.Collider.enabled = !Ctrl;
        }

        if (Input.GetKeyDown(KeyCode.C) && GameStartManager.InstanceExists && GameStartManager.Instance != null && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown && !HudManager.Instance.Chat.IsOpenOrOpening && Utils.IsLobby)
        {
            Logger.Info("Resetted start countdown", "KeyCommand");
            GameStartManager.Instance.ResetStartState();
            Logger.SendInGame("Starting countdown canceled");
        }
    }
}