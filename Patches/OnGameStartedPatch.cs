using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class ApplyCustomImpostorCount
{
    public static void Postfix(AmongUsClient __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        // Normal game = Impostor to prevent blocking task win. HNS = Crewmate to prevent Seeker detection range bugging.
        if (Main.GM.Value)
        {
            if (!Utils.isHideNSeek)
            {
            PlayerControl.LocalPlayer.RpcSetRole(AmongUs.GameOptions.RoleTypes.ImpostorGhost, false);
            }
            if (Utils.isHideNSeek)
            {
            PlayerControl.LocalPlayer.RpcSetRole(AmongUs.GameOptions.RoleTypes.CrewmateGhost, false);
            }
        }


        if (Utils.isHideNSeek)
        {
            int seekersCount = Options.NumSeekers.GetInt();

            int toAdd = Math.Max(0, seekersCount - 1);

            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.Data.Role.TeamType != RoleTeamTypes.Impostor && !p.Data.IsDead)
                    candidates.Add(p);
            }

            toAdd = Math.Min(toAdd, candidates.Count);

            System.Random rand = new System.Random();
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            // 1st RpcSetRole forces the role on that user. 2nd is needed to allow them to kill.
            for (int i = 0; i < toAdd; i++)
            {
                var player = candidates[i];

                player.RpcSetRole(AmongUs.GameOptions.RoleTypes.Impostor, false);

                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.Role.TeamType == RoleTeamTypes.Impostor)
                    {
                        new LateTask(() =>
                        {
                            p.RpcSetRole(AmongUs.GameOptions.RoleTypes.Impostor, false);
                        }, 5f, "Secondary RPCSetRole");

                        new LateTask(() =>
                        {
                            p.RpcSetRole(AmongUs.GameOptions.RoleTypes.Impostor, false);
                        }, 9f, "Tertairy RPCSetRole");
                    }
                }
            }
        }
    }
}