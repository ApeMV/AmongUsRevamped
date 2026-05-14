using UnityEngine;

namespace AmongUsRevamped;

//Old Patch for Mayor
/*
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class CheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (!__instance.playerStates.All(ps => ps.AmDead || ps.DidVote)) return true;

        var votes = __instance.CalculateVotes();
        var visualVotes = new List<MeetingHud.VoterState>();

        foreach (var ps in __instance.playerStates)
        {
            if (ps == null) continue;

            byte voterId = ps.TargetPlayerId;
            byte votedFor = ps.VotedFor;
            if (votedFor == byte.MaxValue) continue;

            visualVotes.Add(new MeetingHud.VoterState { VoterId = voterId, VotedForId = votedFor });

            if (CustomRoleManagement.PlayerRoles.TryGetValue(voterId, out string role) && role == "Mayor")
            {
                int extraVotes = Options.MayorExtraVoteCount.GetInt();
                if (!votes.TryGetValue(votedFor, out int currentVotes)) currentVotes = 0;
                votes[votedFor] = currentVotes + extraVotes;

                for (int i = 0; i < extraVotes; i++)
                    visualVotes.Add(new MeetingHud.VoterState { VoterId = voterId, VotedForId = votedFor });
            }
        }

        var max = votes.MaxPair(out var tie);
        var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(p => !tie && p.PlayerId == max.Key);
        var statesArray = visualVotes.ToArray();

        __instance.RpcVotingComplete(statesArray, exiled, tie);

        return false;
    }
}
*/

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
class MeetingHudStartPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        foreach (var playerState in __instance.playerStates)
        {
            PlayerControl player = null;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.PlayerId == playerState.TargetPlayerId)
                {
                    player = pc;
                    break;
                }
            }

            if (player == null || !PlayerControl.LocalPlayer.Data.IsDead) continue;

            var textTemplate = playerState.NameText;
            var taskText = GetProgressText(player);

            var taskTextMeeting = UnityEngine.Object.Instantiate(textTemplate);
            taskTextMeeting.transform.SetParent(textTemplate.transform);
            taskTextMeeting.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            taskTextMeeting.fontSize = 1.6f;
            taskTextMeeting.text = taskText;
            taskTextMeeting.gameObject.name = "TaskTextMeeting";
            taskTextMeeting.enableWordWrapping = false;
            taskTextMeeting.enabled = player.AmOwner || player.PlayerId == playerState.TargetPlayerId;
        }
    }

    public static string GetProgressText(PlayerControl player)
    {
        int totalTasks = 0;
        int tasksCompleted = 0;

        if (player.Data.Tasks != null && !player.Data.Role.IsImpostor)
        {
            foreach (var task in player.Data.Tasks)
            {
                totalTasks++;
                if (task.Complete) tasksCompleted++;
            }
            return $"<color=green>({tasksCompleted}/{totalTasks})</color>";
        }
        else
        {
            return $"<color=red>({MurderPlayerPatch.killCount[player.Data.PlayerId]}†)</color>";
        }
    }
}