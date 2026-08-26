using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;

namespace AmongUsRevamped;

public static class OptionManager
{
    private static float OldKillCooldown;

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
    }

    public static void RestoreOptions()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (Main.NormalOptions.KillCooldown != OldKillCooldown)
        {
            Main.NormalOptions.KillCooldown = OldKillCooldown;
            Logger.Info("Force overrided Kill Cooldown back to original", "RestoreOptions");
        }
        
        SyncGameOptions();
    }
}