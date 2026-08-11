using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;

namespace AmongUsRevamped;

public static class OptionManager
{
    private static OptionBackupData CachedOptions;
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

        CachedOptions = new OptionBackupData(GameOptionsManager.Instance.CurrentGameOptions);
    }

    public static void RestoreOptions()
    {
        if (!AmongUsClient.Instance.AmHost || CachedOptions == null) return;

        var restored = CachedOptions.Restore(GameOptionsManager.Instance.CurrentGameOptions);

        GameOptionsManager.Instance.CurrentGameOptions = restored;

        SyncGameOptions();
    }
}