using System;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace AmongUsRevamped;

// https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Mode/SuperHostRoles/BlockTool.cs
internal static class DisableDevice
{
    public static readonly List<byte> DesyncComms = [];
    private static int frame;

    public static readonly Dictionary<string, Vector2> DevicePos = new()
    {
        ["SkeldAdmin"] = new(3.48f, -8.62f),
        ["SkeldCamera"] = new(-13.06f, -2.45f),
        ["MiraHQAdmin"] = new(21.02f, 19.09f),
        ["MiraHQDoorLog"] = new(16.22f, 5.82f),
        ["PolusLeftAdmin"] = new(22.80f, -21.52f),
        ["PolusRightAdmin"] = new(24.66f, -21.52f),
        ["PolusCamera"] = new(2.96f, -12.74f),
        ["PolusVital"] = new(26.70f, -15.94f),
        ["DleksAdmin"] = new(-3.48f, -8.62f),
        ["DleksCamera"] = new(13.06f, -2.45f),
        ["AirshipCockpitAdmin"] = new(-22.32f, 0.91f),
        ["AirshipRecordsAdmin"] = new(19.89f, 12.60f),
        ["AirshipCamera"] = new(8.10f, -9.63f),
        ["AirshipVital"] = new(25.24f, -7.94f),
        ["FungleCamera"] = new(6.20f, 0.10f),
        ["FungleVital"] = new(-2.50f, -9.80f),
        ["SubmergedVital"] = new(5f, 32.54f),
        ["SubmergedLeftAdmin"] = new(-9.45f, 10.16f),
        ["SubmergedRightAdmin"] = new(-7.07f, 10.16f),
        ["SubmergedCamera"] = new(-3.41f, -34.56f)
    };

    public static bool DoDisable => Options.DisableDevices.GetBool();

    public static float UsableDistance => Main.CurrentMap switch
    {
        MapNames.Skeld => 1.5f,
        MapNames.MiraHQ => 2.2f,
        MapNames.Polus => 1.4f,
        MapNames.Dleks => 1.5f,
        MapNames.Airship => 1.6f,
        MapNames.Fungle => 1.7f,
        _ => 2f
    };

    public static void FixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost || ShipStatus.Instance == null || !Utils.GamePastRoleSelection) return;
        frame = frame == 5 ? 0 : ++frame;
        if (frame != 0) return;

        if (!DoDisable) return;

        foreach (PlayerControl pc in Main.AllPlayerControls)
        {
            if (pc.Data.IsDead || Main.GM.Value && pc == PlayerControl.LocalPlayer) return;
            
            try
            {
                var doComms = false;
                Vector2 PlayerPos = pc.GetTruePosition();

                if (!Utils.IsActive(SystemTypes.Comms))
                {
                    switch (Main.NormalOptions.MapId)
                    {
                        case 0:
                            if (Options.DisableSkeldAdmin.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["SkeldAdmin"]) <= UsableDistance;
                            if (Options.DisableSkeldCamera.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["SkeldCamera"]) <= UsableDistance;
                            break;
                        case 1:
                            if (Options.DisableMiraHQAdmin.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["MiraHQAdmin"]) <= UsableDistance;
                            if (Options.DisableMiraHQDoorLog.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["MiraHQDoorLog"]) <= UsableDistance;
                            break;
                        case 2:
                            if (Options.DisablePolusAdmin.GetBool())
                            {
                                doComms |= Vector2.Distance(PlayerPos, DevicePos["PolusLeftAdmin"]) <= UsableDistance;
                                doComms |= Vector2.Distance(PlayerPos, DevicePos["PolusRightAdmin"]) <= UsableDistance;
                            }

                            if (Options.DisablePolusCamera.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["PolusCamera"]) <= UsableDistance;
                            if (Options.DisablePolusVital.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["PolusVital"]) <= UsableDistance;
                            break;
                        case 3:
                            if (Options.DisableSkeldAdmin.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["DleksAdmin"]) <= UsableDistance;
                            if (Options.DisableSkeldCamera.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["DleksCamera"]) <= UsableDistance;
                            break;
                        case 4:
                            if (Options.DisableAirshipCockpitAdmin.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["AirshipCockpitAdmin"]) <= UsableDistance;
                            if (Options.DisableAirshipRecordsAdmin.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["AirshipRecordsAdmin"]) <= UsableDistance;
                            if (Options.DisableAirshipCamera.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["AirshipCamera"]) <= UsableDistance;
                            if (Options.DisableAirshipVital.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["AirshipVital"]) <= UsableDistance;
                            break;
                        case 5:
                            if (Options.DisableFungleCamera.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["FungleCamera"]) <= UsableDistance;
                            if (Options.DisableFungleVital.GetBool()) doComms |= Vector2.Distance(PlayerPos, DevicePos["FungleVital"]) <= UsableDistance;
                            break;
                    }
                }

                if (doComms && !pc.inVent)
                {
                    if (!DesyncComms.Contains(pc.PlayerId))
                    {
                        DesyncComms.Add(pc.PlayerId);
                        SendDesyncRepair(pc, SystemTypes.Comms, 128);
                    }
                }
                else if (!doComms && DesyncComms.Contains(pc.PlayerId))
                {
                    DesyncComms.Remove(pc.PlayerId);
                    SendDesyncRepair(pc, SystemTypes.Comms, 16);

                    if (Main.NormalOptions.MapId is 1 or 5) SendDesyncRepair(pc, SystemTypes.Comms, 17);
                }
            }
            catch (Exception ex) { Logger.Exception(ex, "DisableDevice"); }
        }
    }
    public static void SendDesyncRepair(PlayerControl target, SystemTypes systemType, byte amount)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (target == null || target.Data == null || target.Data.ClientId == AmongUsClient.Instance.HostId) return;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
            ShipStatus.Instance.NetId,
            (byte)RpcCalls.UpdateSystem,
            SendOption.Reliable,
            target.Data.ClientId
        );

        writer.Write((byte)systemType);
        writer.WritePacked(target.NetId);
        writer.Write(amount);

        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public class RemoveDisableDevicesPatch
{
    public static void Postfix()
    {
        if (!Options.DisableDevices.GetBool()) return;

        UpdateDisableDevices();
    }

    public static void UpdateDisableDevices()
    {
        PlayerControl player = PlayerControl.LocalPlayer;

        Il2CppArrayBase<MapConsole> admins = Object.FindObjectsOfType<MapConsole>(true);
        Il2CppArrayBase<SystemConsole> consoles = Object.FindObjectsOfType<SystemConsole>(true);
        if (admins == null || consoles == null) return;

        switch (Main.NormalOptions.MapId)
        {
            case 3:
            case 0:
                if (Options.DisableSkeldAdmin.GetBool()) admins[0].gameObject.GetComponent<CircleCollider2D>().enabled = false;
                if (Options.DisableSkeldCamera.GetBool()) consoles.DoIf(x => x.name == "SurvConsole", x => x.gameObject.GetComponent<PolygonCollider2D>().enabled = false);
                break;
            case 1:
                if (Options.DisableMiraHQAdmin.GetBool()) admins[0].gameObject.GetComponent<CircleCollider2D>().enabled = false;
                if (Options.DisableMiraHQDoorLog.GetBool()) consoles.DoIf(x => x.name == "SurvLogConsole", x => x.gameObject.GetComponent<BoxCollider2D>().enabled = false);

                break;
            case 2:
                if (Options.DisablePolusAdmin.GetBool()) admins.Do(x => x.gameObject.GetComponent<BoxCollider2D>().enabled = false);
                if (Options.DisablePolusCamera.GetBool()) consoles.DoIf(x => x.name == "Surv_Panel", x => x.gameObject.GetComponent<BoxCollider2D>().enabled = false);
                if (Options.DisablePolusVital.GetBool()) consoles.DoIf(x => x.name == "panel_vitals", x => x.gameObject.GetComponent<BoxCollider2D>().enabled = false);
                break;
            case 4:
                admins.Do(x =>
                {
                    if ((Options.DisableAirshipCockpitAdmin.GetBool() && x.name == "panel_cockpit_map") ||
                        (Options.DisableAirshipRecordsAdmin.GetBool() && x.name == "records_admin_map"))
                        x.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                });

                if (Options.DisableAirshipCamera.GetBool()) consoles.DoIf(x => x.name == "task_cams", x => x.gameObject.GetComponent<BoxCollider2D>().enabled = false);
                if (Options.DisableAirshipVital.GetBool()) consoles.DoIf(x => x.name == "panel_vitals", x => x.gameObject.GetComponent<CircleCollider2D>().enabled = false);
                break;
            case 5:
                if (Options.DisableFungleCamera.GetBool()) consoles.DoIf(x => x.name == "BinocularsSecurityConsole", x => x.gameObject.GetComponent<PolygonCollider2D>().enabled = false);
                if (Options.DisableFungleVital.GetBool()) consoles.DoIf(x => x.name == "VitalsConsole", x => x.gameObject.GetComponent<BoxCollider2D>().enabled = false);
                break;
        }
    }
}