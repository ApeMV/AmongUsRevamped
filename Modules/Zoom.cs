using System;
using UnityEngine;

namespace AmongUsRevamped;

// https://github.com/Yumenopai/TownOfHost_Y
#if !ANDROID
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Zoom
{
    private const float DefaultZoom = 3.0f;
    private const float MaxZoom = 18.0f;
    private const float ZoomFactor = 1.2f;
    private const float Epsilon = 0.01f;

    private static float LastZoom = DefaultZoom;

    public static void Postfix()
    {
        bool canZoom = (Utils.IsShip && !Utils.IsMeeting && Utils.CanMove && PlayerControl.LocalPlayer.Data.IsDead) || (Utils.IsLobby && Utils.CanMove);

        if (!canZoom)
        {
            ResetZoom();
            return;
        }

        if (Input.mouseScrollDelta.y > 0)
            ChangeZoom(1f / ZoomFactor);

        if (Input.mouseScrollDelta.y < 0 && (Utils.IsDead || Utils.IsFreePlay || Utils.IsLobby))
            ChangeZoom(ZoomFactor);
    }

    private static void ChangeZoom(float multiplier)
    {
        float target = Camera.main.orthographicSize * multiplier;
        target = Mathf.Clamp(target, DefaultZoom, MaxZoom);

        if (Mathf.Abs(target - DefaultZoom) < Epsilon)
            target = DefaultZoom;

        ApplyZoom(target);
    }

    private static void ResetZoom()
    {
        ApplyZoom(DefaultZoom);
    }

    private static void ApplyZoom(float size)
    {
        if (Mathf.Abs(size - LastZoom) < Epsilon)
            return;

        LastZoom = size;

        Camera.main.orthographicSize = size;
        HudManager.Instance.UICamera.orthographicSize = size;

        bool isDefault = Mathf.Abs(size - DefaultZoom) < Epsilon;

        DestroyableSingleton<HudManager>.Instance?.ShadowQuad?.gameObject
            ?.SetActive(isDefault && !Utils.IsDead);

        ResolutionManager.ResolutionChanged.Invoke(
            (float)Screen.width / Screen.height,
            Screen.width,
            Screen.height,
            Screen.fullScreen
        );
    }
}

public static class Flag
{
    private static readonly List<string> OneTimeList = new();
    private static readonly List<string> FirstRunList = new();

    public static void Run(Action action, string type, bool firstRun = false)
    {
        if (OneTimeList.Contains(type) || (firstRun && !FirstRunList.Contains(type)))
        {
            if (!FirstRunList.Contains(type))
                FirstRunList.Add(type);

            OneTimeList.Remove(type);
            action();
        }
    }

    public static void NewFlag(string type)
    {
        if (!OneTimeList.Contains(type))
            OneTimeList.Add(type);
    }

    public static void DeleteFlag(string type)
    {
        OneTimeList.Remove(type);
    }
}
#endif