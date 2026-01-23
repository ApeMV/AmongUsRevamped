using System;
using TMPro;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static bool CustomTimerApplied;
    public static bool Autostarting;

    public static void Prefix(GameStartManager __instance)
    {
        if (__instance == null || MeetingHud.Instance != null) return;
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

        if (__instance.StartButton == null || __instance.GameStartText == null) return;

        __instance.MinPlayers = 1;

        if (Main.AutoStart.Value && OnGameJoinedPatch.AutoStartCheck && GameStartManager.InstanceExists && __instance.startState != GameStartManager.StartingStates.Countdown && GameData.Instance?.PlayerCount >= Options.PlayerAutoStart.GetInt())
        {
            __instance.startState = GameStartManager.StartingStates.Countdown;
            __instance.countDownTimer = Options.AutoStartTimer.GetFloat();
            __instance.StartButton.gameObject.SetActive(false);
            Autostarting = true;
        }

        if (__instance.startState == GameStartManager.StartingStates.Countdown && !CustomTimerApplied && !Autostarting)
        {
            __instance.countDownTimer = Options.StartCountdown.GetInt();
            CustomTimerApplied = true;
        }

        if (__instance.startState != GameStartManager.StartingStates.Countdown)
        {
            Autostarting = false;
            CustomTimerApplied = false;
        }
    }

    public static void Postfix(GameStartManager __instance)
    {
        string warningMessage = "";

        if (!AmongUsClient.Instance.AmHost || __instance.GameStartText == null) return;

        if (GameStartManagerStartPatch.warningText != null)
        {
            if (warningMessage == "")
            {
                GameStartManagerStartPatch.warningText.gameObject.SetActive(false);
            }
            else
            {
                GameStartManagerStartPatch.warningText.text = warningMessage;
                GameStartManagerStartPatch.warningText.gameObject.SetActive(true);
            }
        }

        if (__instance.GameStartText != null)
        {
            __instance.GameStartText.transform.localPosition = new Vector3(__instance.GameStartText.transform.localPosition.x, 2f, __instance.GameStartText.transform.localPosition.z);
        }

        if (GameStartManagerStartPatch.cancelButton != null)
        {
            GameStartManagerStartPatch.cancelButton.gameObject.SetActive(__instance.startState == GameStartManager.StartingStates.Countdown);
        }
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public class GameStartManagerStartPatch
{
    public static TextMeshPro warningText;
    public static PassiveButton cancelButton;

    public static void Postfix(GameStartManager __instance)
    {
        if (__instance == null || !Utils.IsLobby) return;

        warningText = UnityEngine.Object.Instantiate(__instance.GameStartText, __instance.transform.parent);
        warningText.name = "WarningText";
        warningText.transform.localPosition = new Vector3(0f, __instance.transform.localPosition.y + 3f, -1f);
        warningText.gameObject.SetActive(false);

        cancelButton = UnityEngine.Object.Instantiate(__instance.StartButton, __instance.transform);
        cancelButton.name = "CancelButton";

        var cancelLabel = cancelButton.buttonText;
        cancelLabel.DestroyTranslator();
        cancelLabel.text = "Cancel";

        cancelButton.activeTextColor = cancelButton.inactiveTextColor = Color.white;
        cancelButton.gameObject.SetActive(false);

        cancelButton.OnClick = new();
        cancelButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            __instance.ResetStartState();
        }));
    }
}
    