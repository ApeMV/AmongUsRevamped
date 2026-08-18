using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        Logger.Info(" -------- GAME STARTED --------", "StartGame");
        Logger.Info($" Gamemode: {Options.Gamemode.GetString()}", "StartGame");
        Logger.Info($" Players: {PlayerControl.AllPlayerControls.Count}", "StartGame");
        Logger.Info($" Map: {(MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId}", "StartGame");

        if (Main.GM.Value)
        {
            Logger.Info($" Game Master Enabled", "StartGame");      
        }

        NormalGameEndChecker.imps.Clear();
        NormalGameEndChecker.LastWinReason = "";

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
class PlayerControlSetRolePatch
{
    public static bool FirstAssign;
    private static readonly HashSet<byte> ProcessedPlayers = new();
    public static HashSet<byte> Seekers = new();
    public static HashSet<byte> manualSeekers = new();
    private static readonly System.Random rand = new System.Random();

    public static bool Prefix(PlayerControl __instance, ref RoleTypes roleType, ref bool canOverrideRole)
    {
        if (!FirstAssign || !AmongUsClient.Instance.AmHost) return true;

        if (!ProcessedPlayers.Add(__instance.PlayerId)) return true;

        // canOverrideRole = false;

        if (Utils.isHideNSeek && Seekers.Count() == 0 && Options.NumSeekers.GetInt() > 0)
        {
            int seekersCount = Options.NumSeekers.GetInt();

            if (manualSeekers.Count > seekersCount) seekersCount = manualSeekers.Count;

            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (!Main.GM.Value || p != PlayerControl.LocalPlayer) candidates.Add(p);
            }

            if (manualSeekers.Count == 0)
            {
                seekersCount = Math.Min(seekersCount, candidates.Count);
            }
            else
            {
                seekersCount = Math.Min(seekersCount, candidates.Count + manualSeekers.Count);                
            }

            foreach (byte id in manualSeekers)
            {
                if (Seekers.Count >= seekersCount) break;
                Seekers.Add(id);
            }

            candidates.RemoveAll(p => manualSeekers.Contains(p.PlayerId));

            for (int j = candidates.Count - 1; j > 0; j--)
            {
                int k = rand.Next(j + 1);
                (candidates[j], candidates[k]) = (candidates[k], candidates[j]);
            }

            foreach (var p in candidates)
            {
                if (Seekers.Count >= seekersCount) break;
                
                Seekers.Add(p.PlayerId);
            }
        }

        if (Utils.isHideNSeek && Options.NumSeekers.GetInt() > 0)
        {
            if (Seekers.Contains(__instance.PlayerId)) roleType = RoleTypes.Impostor;
            else roleType = RoleTypes.Engineer;
        }

        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek)
        {
            if (Options.EngineerMode.GetBool()) roleType = RoleTypes.Engineer;
            else roleType = RoleTypes.Crewmate;
        }

        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek)
        {
            if (roleType == RoleTypes.Impostor || roleType == RoleTypes.Phantom || roleType == RoleTypes.Viper) roleType = RoleTypes.Shapeshifter;
        }

        if (ProcessedPlayers.Count >= PlayerControl.AllPlayerControls.Count)
        {
            Seekers.Clear();
            ProcessedPlayers.Clear();

            Logger.Info(" PCSRP successful", "RoleManaging");
        }
    
        return true;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.OnGameStart))]
internal static class OnGameStartPatch
{
    public static bool PastStartScreen;
    public static void Postfix()
    {
        if (AmongUsClient.Instance.AmHost && Options.Gamemode.GetValue() == 1 && Options.SNSChatInGame.GetBool() && !Utils.isHideNSeek)
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            if (MeetingHud.Instance != null) MeetingHud.Instance.RpcClose();
        }

        if (AmongUsClient.Instance.AmHost && Options.Gamemode.GetValue() == 3 && Options.PNSChatInGame.GetBool() && !Utils.isHideNSeek)
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            if (MeetingHud.Instance != null) MeetingHud.Instance.RpcClose();
        }

        if (AmongUsClient.Instance.AmHost && Main.GM.Value)
        {
            Logger.Info($" Game Master Successful", "StartGame");
            PlayerControl.LocalPlayer.Exiled();
            PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.CrewmateGhost);
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            Utils.StoredRoleText[p.PlayerId] = Utils.GetRoleText(p);
        }

        PastStartScreen = true;
    }

    // All Patches below are for the speeded up Chat In Game.
    // We force the complete intro to skip using 2 patches.
    // Some PlayerData related things return null when you do this, so we have to set those all as well.
    [HarmonyPatch(typeof(ShhhBehaviour), nameof(ShhhBehaviour.PlayAnimation))]
    public static class ShhhBehaviourPlayAnimationPatch
	{
		static bool Prefix()
		{
            if (!AmongUsClient.Instance.AmHost) return true;

            if ((Options.SNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 1) || (Options.PNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 3))
            {
			    HudManager.Instance.shhhEmblem.gameObject.SetActive(false);
			    return false;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class IntroCutsceneCoBeginPatch
    {
        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if ((Options.SNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 1) || (Options.PNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 3))
            {
                __result = SkipIntro(__instance, __result).WrapToIl2Cpp();
            }
        }

        private static System.Collections.IEnumerator SkipIntro(IntroCutscene i, Il2CppSystem.Collections.IEnumerator k)
        {
            int steps = 0;

            while (k.MoveNext() && steps++ < 1024) {}

            if (steps >= 1024)
            {
                if (i != null && i.gameObject != null) i.gameObject.SetActive(false);
            }
            yield break;
        }
    }

    private static NamePlateViewData meetingNameplate;
    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.GetNameplate))]
    public static class CosmeticsCacheGetNameplatePatch
    {
        public static bool Prefix(ref NamePlateViewData __result)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if ((Options.SNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 1) || (Options.PNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 3))
            {
                if (MeetingHud.Instance == null) return true;

                if (meetingNameplate == null)
                {
                    meetingNameplate = ScriptableObject.CreateInstance<NamePlateViewData>();
                    meetingNameplate.Image = null;
                }

                __result = meetingNameplate;
                return false;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.Destroy))]
    public static class CosmeticsCacheDestroyPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;

            if (meetingNameplate != null) Object.Destroy(meetingNameplate);
            meetingNameplate = null;
        }
    }

    [HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
    public static class ShapeshifterPanelSetPlayerPatch
    {
        public static bool Prefix(ShapeshifterPanel __instance, int index, NetworkedPlayerInfo playerInfo, Il2CppSystem.Action onShift)
        {
            if (__instance == null || playerInfo == null || !AmongUsClient.Instance.AmHost) return true;

           if ((Options.SNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 1) || (Options.PNSChatInGameFast.GetBool() && Options.Gamemode.GetValue() == 3))
           {
                __instance.shapeshift = onShift;
                __instance.PlayerIcon.SetFlipX(false);
                __instance.PlayerIcon.ToggleName(false);

                SpriteRenderer[] componentsInChildren = __instance.GetComponentsInChildren<SpriteRenderer>();
                foreach (var spriteRenderer in componentsInChildren)
                {
                    spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
                }

                __instance.PlayerIcon.SetMaskLayer(index + 2);
                __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);

                __instance.NameText.text = playerInfo.PlayerName;
                __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);

                return false;
            }
            else return true;
        }
    }
}