using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;

namespace AmongUsRevamped;

public static class OptionManager
{
    private static float OldKillCooldown;
    private static float OldGuardianCooldown;

    public static void SyncGameOptions()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var options = GameOptionsManager.Instance.CurrentGameOptions;

        foreach (var component in GameManager.Instance.LogicComponents)
        {
            var logicOptions = component.TryCast<LogicOptions>();

            if (logicOptions != null)
            {
                logicOptions.SetGameOptions(options);
            }
        }
    }

    public static void CacheOptions()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        OldKillCooldown = Main.NormalOptions.KillCooldown;
        if (Main.NormalOptions.roleOptions.TryGetRoleOptions<GuardianAngelRoleOptionsV12>(RoleTypes.GuardianAngel, out var GuardianAngelOptions))
        {
            OldGuardianCooldown = GuardianAngelOptions.ProtectionDurationSeconds;
        }
    }

    public static void RestoreOptions()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (Main.NormalOptions.KillCooldown != OldKillCooldown)
        {
            Main.NormalOptions.KillCooldown = OldKillCooldown;
            Logger.Info("Force overrided Kill Cooldown back to original", "RestoreOptions");
        }
        if (Main.NormalOptions.roleOptions.TryGetRoleOptions<GuardianAngelRoleOptionsV12>(RoleTypes.GuardianAngel, out var GuardianAngelOptions) && GuardianAngelOptions.ProtectionDurationSeconds != OldGuardianCooldown)
        {
            GuardianAngelOptions.ProtectionDurationSeconds = OldGuardianCooldown;
            Logger.Info("Force overrided Guardian Angel Cooldown back to original", "RestoreOptions");
        }
        
        SyncGameOptions();
    }
}