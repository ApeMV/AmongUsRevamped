using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace AmongUsRevamped
{
    // https://github.com/tukasa0001/TownOfHost
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenuBehaviourStartPatch
    {
        private static ClientOptionItem GM;
        private static ClientOptionItem UnlockFPS;
        private static ClientOptionItem ShowFPS;
        private static ClientOptionItem AutoStart;
        private static ClientOptionItem DarkTheme;
        private static ClientOptionItem LobbyMusic;

        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.DisableMouseMovement == null) return;

            if (GM == null || GM.ToggleButton == null)
            {
                GM = ClientOptionItem.Create(Translator.Get("gameMaster"), Main.GM, __instance, GMButtonToggle);

                static void GMButtonToggle()
                {
                    if (Main.GM.Value)
                        HudManager.Instance.ShowPopUp(Translator.Get("gameMasterWarning"));
                }
            }

            if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
            {
                UnlockFPS = ClientOptionItem.Create(Translator.Get("unlockFPS"), Main.UnlockFps, __instance, UnlockFPSButtonToggle);

                static void UnlockFPSButtonToggle()
                {
                    Application.targetFrameRate = Main.UnlockFps.Value ? 120 : 60;
                    Logger.SendInGame($"FPS Set To {Application.targetFrameRate}");
                }
            }

            if (ShowFPS == null || ShowFPS.ToggleButton == null)
                ShowFPS = ClientOptionItem.Create(Translator.Get("showFPS"), Main.ShowFps, __instance);

            if (AutoStart == null || AutoStart.ToggleButton == null)
            {
                AutoStart = ClientOptionItem.Create(Translator.Get("autoStart"), Main.AutoStart, __instance, AutoStartButtonToggle);

                static void AutoStartButtonToggle()
                {
                    if (!Main.AutoStart.Value && GameStartManager.InstanceExists &&
                        GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                    {
                        GameStartManager.Instance.ResetStartState();
                        Logger.SendInGame(Translator.Get("CancelStartCountDown"));
                    }
                }
            }

            if (DarkTheme == null || DarkTheme.ToggleButton == null)
                DarkTheme = ClientOptionItem.Create(Translator.Get("enableDarkTheme"), Main.DarkTheme, __instance);

            if (LobbyMusic == null || LobbyMusic.ToggleButton == null)
                LobbyMusic = ClientOptionItem.Create(Translator.Get("enableLobbyMusic"), Main.LobbyMusic, __instance);
        }
    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
    public static class OptionsMenuBehaviourClosePatch
    {
        public static void Postfix()
        {
            ClientOptionItem.CustomBackground?.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    internal class SplashLogoAnimatorPatch
    {
        public static void Prefix(SplashManager __instance)
        {
            __instance.sceneChanger.AllowFinishLoadingScene();
            __instance.startedSceneLoad = true;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManager_Update
    {
	    public static void Postfix(HudManager __instance)
        {
            if (Main.GM.Value && AmongUsClient.Instance.AmHost)
            {
			    __instance.Chat.gameObject.SetActive(true);
			    __instance.MapButton.gameObject.SetActive(true); 
            }
        }
    }
}