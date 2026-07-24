using System;
using Assets.InnerNet;
using Il2CppInterop.Runtime;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

public static class InviteAllFriends
{
    // Invites go out one at a time and the next one is only scheduled once the server has answered
    // the previous, so the pace follows whatever the endpoint actually tolerates.
    private const float MinInterval = 0.4f;
    private const float MaxInterval = 5f;
    private const float ResponseTimeout = 8f;
    private const int MaxRetries = 2;
    private const float Cooldown = 20f;

    // The invite endpoint answers 429 when we are going too fast
    private const int RateLimitedCode = 429;

    private static float lastUsed = -Cooldown;
    private static bool sending;

    private static readonly List<Target> Queue = [];
    private static int queueIndex;
    private static int retries;
    private static float interval = MinInterval;
    private static int waitToken;
    private static bool awaitingResponse;

    private static int succeeded;
    private static int failed;
    private static int rateLimitHits;

    private readonly struct Target(string puid, string friendCode)
    {
        public readonly string Puid = puid;
        public readonly string FriendCode = friendCode;
    }

    public static bool Available =>
        AmongUsClient.Instance != null &&
        FriendsListManager.InstanceExists &&
        Utils.IsLobby && Utils.IsOnlineGame &&
        !IsGuestAccount();

    public static bool Ready => Available && !sending && Time.realtimeSinceStartup - lastUsed >= Cooldown;

    public static bool DebugIsLobby => AmongUsClient.Instance != null && Utils.IsLobby;
    public static bool DebugIsOnline => AmongUsClient.Instance != null && Utils.IsOnlineGame;
    public static bool DebugIsGuest => IsGuestAccount();

    public static float RemainingCooldown => Mathf.Max(0f, Cooldown - (Time.realtimeSinceStartup - lastUsed));

    private static bool IsGuestAccount()
    {
        // Guest accounts have no friends list, EOSManager reports them as not logged in
        return EOSManager.InstanceExists && !EOSManager.Instance.HasFinishedLoginFlow();
    }

    public static void SendAll()
    {
        if (!Ready) return;

        string roomCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        if (string.IsNullOrEmpty(roomCode))
        {
            Logger.SendInGame(Translator.Get("inviteAllNoLobby"));
            return;
        }

        FriendsListManager manager = FriendsListManager.Instance;

        sending = true;
        lastUsed = Time.realtimeSinceStartup;

        // The list is only filled once the friends panel has been opened, so refresh it first
        manager.StartCoroutine(manager.RefreshFriendsList((Il2CppSystem.Action)(() => BuildQueue(manager, roomCode)), false));
    }

    private static void BuildQueue(FriendsListManager manager, string roomCode)
    {
        try
        {
            var friends = manager.Friends;

            if (friends == null || friends.Count == 0)
            {
                Abort();
                lastUsed = -Cooldown;
                Logger.SendInGame(Translator.Get("inviteAllNoFriends"));
                return;
            }

            Queue.Clear();
            queueIndex = 0;
            retries = 0;
            interval = MinInterval;
            succeeded = failed = rateLimitHits = 0;
            awaitingResponse = false;

            foreach (ResponseFriends friend in friends)
            {
                if (friend == null || string.IsNullOrEmpty(friend.FriendPuid)) continue;
                if (manager.IsPlayerBlocked(friend.FriendPuid) || manager.HasPlayerBlockedMe(friend.FriendPuid)) continue;

                Queue.Add(new Target(friend.FriendPuid, friend.FriendCode));
            }

            if (Queue.Count == 0)
            {
                Abort();
                lastUsed = -Cooldown;
                Logger.SendInGame(Translator.Get("inviteAllNoFriends"));
                return;
            }

            Logger.Info($"Sending {Queue.Count} lobby invites for {roomCode}", "InviteAllFriends");
            Logger.SendInGame(Translator.Get("inviteAllSending", Queue.Count));

            SendNext(manager, roomCode);
        }
        catch (Exception ex)
        {
            Abort();
            Logger.Exception(ex, "InviteAllFriends");
        }
    }

    private static void SendNext(FriendsListManager manager, string roomCode)
    {
        if (queueIndex >= Queue.Count)
        {
            Finish();
            return;
        }

        // A lobby switch or disconnect mid-batch makes the remaining invites point at a dead room
        if (!Available || GameCode.IntToGameName(AmongUsClient.Instance.GameId) != roomCode)
        {
            Logger.Warn($"Stopped after {succeeded + failed}/{Queue.Count} invites, no longer in lobby {roomCode}", "InviteAllFriends");
            Finish();
            return;
        }

        Target target = Queue[queueIndex];
        int token = ++waitToken;
        awaitingResponse = true;

        manager.SendGameInvite(target.Puid, roomCode,
            DelegateSupport.ConvertDelegate<Il2CppSystem.Action<ResponseState, Response<ResponseFriendsListRequest>>>(
                new Action<ResponseState, Response<ResponseFriendsListRequest>>((state, response) =>
                    OnResponse(manager, roomCode, target, token, state, response))));

        // The callback is not guaranteed to fire, so never let the batch stall on it
        new LateTask(() =>
        {
            if (token != waitToken || !awaitingResponse) return;

            failed++;
            Logger.Warn($"Invite to {target.FriendCode} timed out after {ResponseTimeout}s", "InviteAllFriends");
            Advance(manager, roomCode, token, false);
        }, ResponseTimeout, "", false);
    }

    private static void OnResponse(FriendsListManager manager, string roomCode, Target target, int token,
        ResponseState state, Response<ResponseFriendsListRequest> response)
    {
        if (token != waitToken || !awaitingResponse) return;

        if (state == ResponseState.Success)
        {
            succeeded++;
            // Back off gradually once the server is keeping up again
            interval = Mathf.Max(MinInterval, interval * 0.8f);
            Advance(manager, roomCode, token, false);
            return;
        }

        bool rateLimited = IsRateLimited(response, out int code, out string title);

        if (rateLimited)
        {
            rateLimitHits++;
            interval = Mathf.Min(MaxInterval, Mathf.Max(interval * 2f, MinInterval * 2f));
            Logger.Warn($"Rate limited on {target.FriendCode} (code {code}), interval now {interval:0.00}s", "InviteAllFriends");

            if (retries < MaxRetries)
            {
                retries++;
                Advance(manager, roomCode, token, true);
                return;
            }
        }
        else
        {
            Logger.Warn($"Invite to {target.FriendCode} failed ({state}, code {code}{(string.IsNullOrEmpty(title) ? "" : $", {title}")})", "InviteAllFriends");
        }

        failed++;
        Advance(manager, roomCode, token, false);
    }

    private static bool IsRateLimited(Response<ResponseFriendsListRequest> response, out int code, out string title)
    {
        code = 0;
        title = null;

        var errors = response?.Errors;
        if (errors == null) return false;

        for (int i = 0; i < errors.Length; i++)
        {
            ResponseError error = errors[i];
            if (error == null) continue;

            code = error.Code;
            title = error.Title;

            if (error.Code == RateLimitedCode || error.Detail == RateLimitedCode) return true;
        }

        return false;
    }

    private static void Advance(FriendsListManager manager, string roomCode, int token, bool retrySameTarget)
    {
        if (token != waitToken) return;

        awaitingResponse = false;
        if (!retrySameTarget)
        {
            queueIndex++;
            retries = 0;
        }

        new LateTask(() => SendNext(manager, roomCode), interval, "", false);
    }

    private static void Finish()
    {
        sending = false;
        awaitingResponse = false;
        waitToken++;

        Logger.Info($"Invite results: {succeeded} ok, {failed} failed, {rateLimitHits} rate limited, final interval {interval:0.00}s", "InviteAllFriends");
        Logger.SendInGame(Translator.Get("inviteAllResult", succeeded, Queue.Count));
    }

    private static void Abort()
    {
        sending = false;
        awaitingResponse = false;
        waitToken++;
        Queue.Clear();
    }
}
