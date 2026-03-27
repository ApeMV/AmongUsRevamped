namespace AmongUsRevamped;

class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class ExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            AfterExile(__instance.initData.networkedPlayer);
        }

        public static void AfterExile(NetworkedPlayerInfo ejectedPlayer)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            if (ejectedPlayer == null) return;

            PlayerControl pc = null;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId == ejectedPlayer.PlayerId)
                {
                    pc = p;
                }

                if (ejectedPlayer == PlayerControl.LocalPlayer.Data)
                {
                    if (p.Data.Role.IsImpostor)
                    {
                        p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=red><size=90%>({MurderPlayerPatch.killCount[p.Data.PlayerId]}†)";
                    }
                    else
                    {
                        if (!PlayerControlCompleteTaskPatch.playerTasksCompleted.ContainsKey(p))
                        {
                            PlayerControlCompleteTaskPatch.playerTasksCompleted[p] = 0;                
                        }
                        p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=green><size=90%>({PlayerControlCompleteTaskPatch.playerTasksCompleted[p]}/{PlayerControlCompleteTaskPatch.tasksPerPlayer[p]})";
                    }
                }
            }

            Logger.Info($" {ejectedPlayer.PlayerName} was ejected", "ExileController");

            if (CustomRoleManagement.PlayerRoles.TryGetValue(ejectedPlayer.PlayerId, out var role) && role == "Jester")
            {
                Utils.CustomWinnerEndGame(pc, 1);
                NormalGameEndChecker.CheckWinnerText("Jester");
                return;
            }

            if (CustomRoleManagement.HandlingRoleMessages)
            {
                Logger.Info(" SRM2 is called before SRM1 finished. This should not happen.", "ExileController");
            }
            else
            {
                CustomRoleManagement.SendRoleMessages(new Dictionary<string, string>
                {
                    { "Jester", Translator.Get("jesterPriv")},
                    { "Mayor", Translator.Get("mayorPriv", Options.MayorExtraVoteCount.GetInt())},
                });
            }
        }
    }

#if ANDROID
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirshipExileControllerPatchAndroid
    {
        public static void Postfix(AirshipExileController __instance)
        {
            ExileControllerPatch.AfterExile(__instance.initData.networkedPlayer);
        }
    }
#else
    [HarmonyPatch(typeof(AirshipExileController._WrapUpAndSpawn_d__11), nameof(AirshipExileController._WrapUpAndSpawn_d__11.MoveNext))]
    class AirshipExileControllerPatchPC
    {
        public static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance, ref bool __result)
        {
            var instance = __instance.__4__this;
            ExileControllerPatch.AfterExile(instance.initData.networkedPlayer);
        }
    }
#endif
}