using AmongUs.GameOptions;
using UnityEngine;

namespace AmongUsRevamped;

// https://github.com/astra1dev/AUnlocker/blob/main/src/OptionsPatches.cs
[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
public static class NumberOption_Increase
{
    public static bool Prefix(NumberOption __instance)
    {
        float increment = Hotkeys.IncrementMultiplier * __instance.Increment;

        if (Utils.isHideNSeek || Utils.IsLobby && !Utils.isHideNSeek && __instance.Title != StringNames.GameNumImpostors && __instance.Title != StringNames.GamePlayerSpeed)
        {

            if (__instance.Value + increment > __instance.ValidRange.max)
            {
                return true;
            }
            else
            {
                __instance.Value += increment;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
public static class NumberOption_Decrease
{
    public static bool Prefix(NumberOption __instance)
    {
        float increment = Hotkeys.IncrementMultiplier * __instance.Increment;

        if (Utils.isHideNSeek || Utils.IsLobby && !Utils.isHideNSeek && __instance.Title != StringNames.GameNumImpostors && __instance.Title != StringNames.GamePlayerSpeed)
        {
            if (__instance.Value - increment < __instance.ValidRange.min)
            {
                return true;
            }
            else
            {
                __instance.Value -= increment;
                __instance.UpdateValue();
                __instance.OnValueChanged.Invoke(__instance);
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
public static class NumberOption_Initialize
{
    public static void Postfix(NumberOption __instance)
    {
        if (ModGameOptionsMenu.TabIndex >= 3) return;

        if (Utils.isHideNSeek || Utils.IsLobby && !Utils.isHideNSeek && __instance.Title != StringNames.GameNumImpostors && __instance.Title != StringNames.GamePlayerSpeed)
        {
            __instance.ValidRange = new FloatRange(0f, 900f);
        }

        switch (__instance.Title)
        {
            case StringNames.GameVotingTime:
            case StringNames.GameDiscussTime:
            __instance.Increment = 5f;
            break;
            case StringNames.GameKillCooldown:
            __instance.Increment = 0.5f;
            break;
            case StringNames.GamePlayerSpeed:
            case StringNames.GameCrewLight:
            case StringNames.GameImpostorLight:
            __instance.Increment = 0.05f;
            break;
            
            default:
            __instance.Increment = 1f;
            break;
        }
    }
}