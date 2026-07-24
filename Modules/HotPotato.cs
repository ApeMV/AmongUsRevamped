using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AmongUsRevamped;

public static class HotPotato
{
    public const int GamemodeId = 4;
    private static byte PotatoColor => (byte)Options.PotatoHolderColor.GetValue();
    private static byte SafeColor => (byte)Options.SafePlayerColor.GetValue();
    private const int DesiredTicks = 10;
    private static int timerTicks = DesiredTicks;
    public static byte HolderId = byte.MaxValue;
    public static bool RoundActive;

    // Set while we hand the host their countdown tasks so RpcSetTasksPatch leaves them alone
    public static bool AssigningTimerTasks;

    private static float tickTimer;
    private static int ticksDone;
    private static float passLock;
    private static float announceLock;

    public static bool IsActive => Options.Gamemode.GetValue() == GamemodeId && !Utils.isHideNSeek;

    public static PlayerControl Holder
    {
        get
        {
            if (HolderId == byte.MaxValue) return null;

            foreach (PlayerControl pc in Utils.AllAlivePlayerControls)
            {
                if (pc.PlayerId == HolderId) return pc;
            }

            return null;
        }
    }

    public static void Reset()
    {
        HolderId = byte.MaxValue;
        RoundActive = false;
        AssigningTimerTasks = false;
        tickTimer = 0f;
        ticksDone = 0;
        passLock = 0f;
        announceLock = 0f;
    }

    // Overlong chat messages get the sender banned, so everything we send is clamped first
    private const int MaxChatLength = 100;

    private static string Clamp(string message)
    {
        return message.Length <= MaxChatLength ? message : message[..MaxChatLength];
    }

    // Messages with more than five digits get censored by the server; ConvertNum swaps them
    // for circled digits when that threshold is crossed
    private static void Announce(string message, float cooldown = 1.5f)
    {
        if (announceLock > 0f) return;

        announceLock = cooldown;
        PlayerControl.LocalPlayer.RpcSendChat(Clamp(SendChatPatch.ConvertNum(message)));
    }

    public static void Begin()
    {
        if (!AmongUsClient.Instance.AmHost || !IsActive) return;

        try
        {
            Reset();

            PlayerControl[] alive = Utils.AllAlivePlayerControls;
            Logger.Info($" starting with {alive.Length} alive, ship={(ShipStatus.Instance != null)}", "HotPotato");

            if (alive.Length < 1) return;

            PlayerControl first = alive[Random.Range(0, alive.Length)];

            RoundActive = true;

            Scatter(first);
            ClearOtherTaskLists();
            GiveTo(first, announce: false, newPotato: true);

            Announce($"Hot Potato! {first.Data.PlayerName} has the potato.\nTouch someone to pass it.", 3f);
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString(), "HotPotato.Begin");
        }
    }

    // Scatters everyone except the holder, who is left standing alone at the spawn room with
    // the potato. Vents near spawn are skipped so nobody lands back on top of them.
    private static void Scatter(PlayerControl holder)
    {
        if (ShipStatus.Instance == null) return;

        var vents = ShipStatus.Instance.AllVents;
        if (vents == null || vents.Length == 0) return;

        // Measured from the map's own spawn point rather than a hardcoded room, so this still
        // does the right thing on maps that do not start players in Cafeteria
        Vector2 spawn = ShipStatus.Instance.InitialSpawnCenter;
        float clearRadius = Options.PotatoSpawnClearRadius.GetFloat();

        List<Vent> usable = [];

        foreach (Vent vent in vents)
        {
            if (Vector2.Distance(spawn, vent.transform.position) < clearRadius) continue;

            usable.Add(vent);
        }

        // A tight map, or too generous a radius, can rule out everything; better to scatter
        // into the spawn room than not to scatter at all
        if (usable.Count == 0)
        {
            foreach (Vent vent in vents) usable.Add(vent);
        }

        foreach (PlayerControl pc in Utils.AllAlivePlayerControls)
        {
            if (pc.PlayerId == holder.PlayerId || pc.MyPhysics == null) continue;

            pc.MyPhysics.RpcBootFromVent(usable[Random.Range(0, usable.Count)].Id);
        }
    }

    private static void ClearOtherTaskLists()
    {
        if (ShipStatus.Instance == null) return;

        AssigningTimerTasks = true;

        try
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.Data == null || pc == PlayerControl.LocalPlayer) continue;

                pc.Data.RpcSetTasks(new Il2CppStructArray<byte>(0));
            }
        }
        finally
        {
            AssigningTimerTasks = false;
        }
    }

    private static void GiveTo(PlayerControl holder, bool announce = true, bool newPotato = false)
    {
        if (holder == null || holder.Data == null) return;

        HolderId = holder.PlayerId;
        passLock = Options.PotatoPassCooldown.GetFloat();

        BatchedMessage batch = new();

        foreach (PlayerControl pc in Utils.AllAlivePlayerControls)
        {
            batch.QueueSetColor(pc, pc.PlayerId == HolderId ? PotatoColor : SafeColor);
        }

        batch.FinishBatch();

        if (newPotato) ResetTimerBar();

        if (announce) Announce($"{holder.Data.PlayerName} has the potato!");
    }

    // Hands the host a fresh task list, which is what empties the shared bar
    private static void ResetTimerBar()
    {
        tickTimer = 0f;
        ticksDone = 0;

        if (ShipStatus.Instance == null) return;

        AssigningTimerTasks = true;

        try
        {
            PlayerControl.LocalPlayer.Data.RpcSetTasks(BuildTimerTasks());
        }
        finally
        {
            AssigningTimerTasks = false;
        }

    }

    // Built through the game's own task picker rather than by guessing ids. It skips task types
    // it has already used, so a map with few short tasks yields a coarser countdown.
    private static Il2CppStructArray<byte> BuildTimerTasks()
    {
        Il2CppSystem.Collections.Generic.List<byte> ids = new();
        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> usedTypes = new();
        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> pool = new();

        foreach (NormalPlayerTask task in ShipStatus.Instance.ShortTasks) pool.Add(task);

        int start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, DesiredTicks, ids, usedTypes, pool);

        timerTicks = Math.Max(1, ids.Count);

        Il2CppStructArray<byte> result = new(ids.Count);
        for (int i = 0; i < ids.Count; i++) result[i] = ids[i];

        return result;
    }

    public static void Update(float deltaTime)
    {
        if (!AmongUsClient.Instance.AmHost || !IsActive || !RoundActive) return;
        if (!Utils.InGame || Utils.IsMeeting || ExileController.Instance) return;

        if (announceLock > 0f) announceLock -= deltaTime;
        if (passLock > 0f) passLock -= deltaTime;

        PlayerControl holder = Holder;

        // Holder left or died some other way; hand the potato to whoever is left
        if (holder == null)
        {
            PlayerControl[] alive = Utils.AllAlivePlayerControls;
            if (alive.Length > 0) GiveTo(alive[Random.Range(0, alive.Length)], newPotato: true);
            return;
        }

        if (CheckWin()) return;

        TickTimer(holder, deltaTime);
        CheckProximity(holder);
    }

    private static void TickTimer(PlayerControl holder, float deltaTime)
    {
        float interval = Options.PotatoTimer.GetFloat() / timerTicks;

        tickTimer += deltaTime;
        if (tickTimer < interval) return;

        tickTimer = 0f;

        if (ticksDone < timerTicks)
        {
            PlayerControl.LocalPlayer.RpcCompleteTask((uint)ticksDone);
            ticksDone++;

            // Read one tick after the reset rather than straight after it, so the game has had
            // a chance to recompute the shared counters instead of reporting stale ones
            if (ticksDone == 1 && GameData.Instance != null)
            {
                Logger.Info($" bar at first tick: {GameData.Instance.CompletedTasks}/{GameData.Instance.TotalTasks}, ticks={timerTicks}", "HotPotato");
            }

            return;
        }

        Burn(holder);
    }

    private static void Burn(PlayerControl holder)
    {
        string name = holder.Data.PlayerName;

        holder.RpcSetRole(RoleTypes.CrewmateGhost, false);
        Logger.Info($" {name} was burned by the potato", "HotPotato");

        PlayerControl next = Options.PotatoBurnPassTarget.GetValue() == 1
            ? RandomAlive(holder)
            : NearestTo(holder, ignore: holder);

        if (next == null)
        {
            CheckWin();
            return;
        }

        Announce($"{name} got burned!\n{next.Data.PlayerName} has the potato.", 3f);
        GiveTo(next, announce: false, newPotato: true);
    }

    private static void CheckProximity(PlayerControl holder)
    {
        if (passLock > 0f) return;

        float radius = Options.PotatoPassRadius.GetFloat();

        PlayerControl target = NearestTo(holder, ignore: holder, maxDistance: radius);

        if (target == null) return;

        GiveTo(target);
    }

    private static PlayerControl NearestTo(PlayerControl origin, PlayerControl ignore, float maxDistance = float.MaxValue)
    {
        Vector2 from = origin.GetTruePosition();
        PlayerControl best = null;
        float bestDistance = maxDistance;

        foreach (PlayerControl pc in Utils.AllAlivePlayerControls)
        {
            if (pc == ignore || pc.PlayerId == origin.PlayerId) continue;

            float distance = Vector2.Distance(from, pc.GetTruePosition());
            if (distance > bestDistance) continue;

            bestDistance = distance;
            best = pc;
        }

        return best;
    }

    private static PlayerControl RandomAlive(PlayerControl ignore)
    {
        List<PlayerControl> pool = [];

        foreach (PlayerControl pc in Utils.AllAlivePlayerControls)
        {
            if (pc == ignore || pc.PlayerId == ignore.PlayerId) continue;

            pool.Add(pc);
        }

        return pool.Count == 0 ? null : pool[Random.Range(0, pool.Count)];
    }

    private static bool CheckWin()
    {
        if (Options.NoGameEnd.GetBool()) return false;

        PlayerControl[] alive = Utils.AllAlivePlayerControls;
        if (alive.Length > 1) return false;

        RoundActive = false;

        if (alive.Length == 1)
        {
            Utils.CustomWinnerEndGame(alive[0], 1);
            NormalGameEndChecker.LastWinReason = $"{alive[0].Data.PlayerName} wins! (Survived the potato)";
        }
        else
        {
            Utils.CustomWinnerEndGame(PlayerControl.LocalPlayer, 0);
            NormalGameEndChecker.LastWinReason = "Nobody survived the potato";
        }

        NormalGameEndChecker.canUpdateWinnerText = false;
        return true;
    }


    private static string[] Rules()
    {
        string color = Options.playerColors[Options.PotatoHolderColor.GetValue()];

        return
        [
            $"Hot Potato: \n{color} holds the potato, avoid {color}.\n{color} can pass the potato by touching someone.",
            $"Potato burns the holder after {Options.PotatoTimer.GetFloat():0.#}s. Last one standing wins.",
            "The task bar is the timer - watch it fill.",
        ];
    }

    // Roughly matches the pacing Utils.ChatCommand uses between its two messages
    private const float RuleSpacing = 2.2f;

    public static void SendRules()
    {
        if (OnGameJoinedPatch.WaitingForChat) return;

        OnGameJoinedPatch.WaitingForChat = true;

        string[] rules = Rules();

        for (int i = 0; i < rules.Length; i++)
        {
            string line = Clamp(SendChatPatch.ConvertNum(rules[i]));

            new LateTask(() => PlayerControl.LocalPlayer.RpcSendChat(line), RuleSpacing * i, $"HotPotatoRules{i}", false);
        }

        new LateTask(() => OnGameJoinedPatch.WaitingForChat = false, RuleSpacing * rules.Length, "HotPotatoRulesEnd", false);
    }
}
