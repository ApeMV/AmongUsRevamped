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
        if (!AmongUsClient.Instance.AmHost || AmongUsClient.Instance == null) return;

        __instance.MinPlayers = 1;

        if (Main.AutoStart.Value && OnGameJoinedPatch.AutoStartCheck && GameStartManager.InstanceExists && GameStartManager.Instance.startState != GameStartManager.StartingStates.Countdown && GameData.Instance?.PlayerCount >= Options.PlayerAutoStart.GetInt())
        {
            GameStartManager.Instance.startState = GameStartManager.StartingStates.Countdown;
            GameStartManager.Instance.countDownTimer = Options.AutoStartTimer.GetFloat();
            GameStartManager.Instance?.StartButton.gameObject.SetActive(false);
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

        if (!AmongUsClient.Instance.AmHost) return;

        if (warningMessage == "")
        {
            GameStartManagerStartPatch.warningText.gameObject.SetActive(false);
        }
        else
        {
            GameStartManagerStartPatch.warningText.text = warningMessage;
            GameStartManagerStartPatch.warningText.gameObject.SetActive(true);
        }

        __instance.GameStartText.transform.localPosition = new Vector3(__instance.GameStartText.transform.localPosition.x, 2f, __instance.GameStartText.transform.localPosition.z);
        GameStartManagerStartPatch.cancelButton.gameObject.SetActive(__instance.startState == GameStartManager.StartingStates.Countdown);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public class GameStartManagerStartPatch
{
    public static TextMeshPro warningText;
    private static Vector3 GameStartTextlocalPosition;
    public static PassiveButton cancelButton;

    public static void Postfix(GameStartManager __instance)
    {
        warningText = UnityEngine.Object.Instantiate(__instance.GameStartText, __instance.transform.parent);
        warningText.name = "WarningText";
        warningText.transform.localPosition = new(0f, __instance.transform.localPosition.y + 3f, -1f);
        warningText.gameObject.SetActive(false);

        cancelButton = UnityEngine.Object.Instantiate(__instance.StartButton, __instance.transform);
        cancelButton.name = "CancelButton";
        var cancelLabel = cancelButton.buttonText;
        cancelLabel.DestroyTranslator();
        cancelLabel.text = "Cancel";
        //cancelButton.transform.localScale = new(0.5f, 0.5f, 1f);
        var cancelButtonInactiveRenderer = cancelButton.inactiveSprites.GetComponent<SpriteRenderer>();
        cancelButtonInactiveRenderer.color = new(0.8f, 0f, 0f, 1f);
        var cancelButtonActiveRenderer = cancelButton.activeSprites.GetComponent<SpriteRenderer>();
        cancelButtonActiveRenderer.color = Color.red;
        var cancelButtonInactiveShine = cancelButton.inactiveSprites.transform.Find("Shine");
        if (cancelButtonInactiveShine)
        {
            cancelButtonInactiveShine.gameObject.SetActive(false);
        }
        cancelButton.activeTextColor = cancelButton.inactiveTextColor = Color.white;
        //cancelButton.transform.localPosition = new(2f, 0.13f, 0f);
        GameStartTextlocalPosition = __instance.GameStartText.transform.localPosition;
        cancelButton.OnClick = new();
        cancelButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            __instance.ResetStartState();
        }));
        cancelButton.gameObject.SetActive(false);
    }
}
    