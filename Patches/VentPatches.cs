using AmongUs.GameOptions;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (pc.Data.RoleType != RoleTypes.Impostor && 
            pc.Data.RoleType != RoleTypes.Shapeshifter &&
            pc.Data.RoleType != RoleTypes.Phantom &&
            pc.Data.RoleType != RoleTypes.Viper &&
            pc.Data.RoleType != RoleTypes.Engineer &&
            !pc.Data.IsDead)
        {
            EACR.VentCheat(pc, __instance);
        }

        if (__instance.Id > 15)
        {
            EACR.VentCheat(pc, __instance);
        }
    }
}