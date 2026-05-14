namespace AmongUsRevamped;

//Old Patch for Mayor

/*
[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (CustomRoleManagement.PlayerRoles.TryGetValue(pc.PlayerId, out var role) && role == "Mayor")
        {
            pc.CmdReportDeadBody(null);
            Logger.Info($" The Mayor, {pc.Data.PlayerName}, called a meeting by venting", "EnterVent");
        }
    }
}
*/