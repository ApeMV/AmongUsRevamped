namespace AmongUsRevamped;

class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class ExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            var ejectedPlayer = __instance.initData?.networkedPlayer;
            if (ejectedPlayer == null) return;

            PlayerControl pc = null;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId == ejectedPlayer.PlayerId)
                {
                    pc = p;
                    break;
                }
            }

            Logger.Info($" {ejectedPlayer.PlayerName} was ejected", "ExileController");

            if (!CustomRoleManagement.PlayerRoles.TryGetValue(ejectedPlayer.PlayerId, out var role)) return;

            if (role == "Jester")
            {
                Utils.CustomWinnerEndGame(pc, 1);
            }
        }
    }
}