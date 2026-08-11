using AmongUs.GameOptions;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost || pc == null || __instance == null || pc.Data == null) return;

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

        if ((Options.Gamemode.GetValue() == 1 && !Options.SNSImpostorsCanVent.GetBool()) || (Options.Gamemode.GetValue() == 3 && !Options.PNSImpostorsCanVent.GetBool()))
        {
            if (pc.Data.RoleType == RoleTypes.Impostor || pc.Data.RoleType == RoleTypes.Shapeshifter || pc.Data.RoleType == RoleTypes.Phantom || pc.Data.RoleType == RoleTypes.Viper)
            {
                pc.MyPhysics.RpcBootFromVent(__instance.Id);
                Logger.SendInGame($"{pc.Data.PlayerName} tried to vent as Impostor, but got blocked");
                Logger.Info($" {pc.Data.PlayerName} tried to vent as Impostor, but got blocked", "VentManager");
            }
        }
    }
}