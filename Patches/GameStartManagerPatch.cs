using System;

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
}