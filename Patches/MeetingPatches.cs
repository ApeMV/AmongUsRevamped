using UnityEngine;

namespace AmongUsRevamped;

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
            PlayerControl pc = GameData.Instance.GetPlayerById(voterId)?.Object;
            if (pc == null) continue;
            if (votedFor == byte.MaxValue) continue;
            if (!votes.ContainsKey(votedFor)) votes[votedFor] = 0;

            visualVotes.Add(new MeetingHud.VoterState { VoterId = voterId, VotedForId = votedFor });

            if (!PlayerControlCompleteTaskPatch.playerTasksCompleted.ContainsKey(pc)) PlayerControlCompleteTaskPatch.playerTasksCompleted[pc] = 0;
            if (!MurderPlayerPatch.killCount.ContainsKey(voterId)) MurderPlayerPatch.killCount[voterId] = 0;

            int extraVotes = 0;
            int extraVotesCrewmate = Options.ExtraVotesCrewmate.GetInt();
            int extraVotesImpostor = Options.ExtraVotesImpostor.GetInt();
            int extraVotesPerTask = (int)(Options.ExtraVotesPerTask.GetFloat()*PlayerControlCompleteTaskPatch.playerTasksCompleted[pc]+0.1f);
            int extraVotesPerKill = (int)(Options.ExtraVotesPerKill.GetFloat()*MurderPlayerPatch.killCount[voterId]+0.1f);

            if (!votes.TryGetValue(votedFor, out int currentVotes)) currentVotes = 0;

            if (AbilityManagement.IsMayor(pc)) extraVotes += extraVotesCrewmate;               
            if (AbilityManagement.IsTyrant(pc)) extraVotes += extraVotesImpostor;               
            if (AbilityManagement.IsWorkhorse(pc)) extraVotes += extraVotesPerTask;               
            if (AbilityManagement.IsStealer(pc)) extraVotes += extraVotesPerKill;               

            for (int i = 0; i < extraVotes; i++)
            {
                visualVotes.Add(new MeetingHud.VoterState { VoterId = voterId, VotedForId = votedFor });
                votes[votedFor]++;
            }
        }

        var max = votes.MaxPair(out var tie);
        var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(p => !tie && p.PlayerId == max.Key);
        var statesArray = visualVotes.ToArray();

        __instance.RpcVotingComplete(statesArray, exiled, tie);

        return false;
    }
}

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

            if (player == null || !PlayerControl.LocalPlayer.Data.IsDead || Main.DisableInfoWhenDead.Value) continue;

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
            return $"<color=green>({tasksCompleted}/{totalTasks})</color> - {Utils.GetRoleText(player)}";
        }
        else
        {
            return $"<color=red>({MurderPlayerPatch.killCount[player.Data.PlayerId]}†)</color> - {Utils.GetRoleText(player)}";
        }
    }
}