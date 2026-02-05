using System;
using AmongUs.Data;
using Discord;
using HarmonyLib;

namespace AmongUsRevamped;

// Originally from "Town of Us Rewritten", by Det
[HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
public class DiscordRPC
{
    private static string Lobbycode = "";
    private static string Region = "";

    public static void Prefix([HarmonyArgument(0)] Activity activity)
    {
        if (activity == null) return;

        var details = $"Revamped {Main.ModVersion}";
        activity.Details = details;

        activity.Assets = new ActivityAssets
        {
            LargeImage = "https://i.imgur.com/ZnC1toT.png"
        };

        try
        {
            if (activity.State != "In Menus")
            {
                if (!DataManager.Settings.Gameplay.StreamerMode)
                {
                    if (Utils.IsLobby)
                    {
                        Lobbycode = GameStartManager.Instance.GameRoomNameCode.text;
                        Region = Utils.GetRegionName();
                    }

                    if (Lobbycode != "" && Region != "") details = $"Revamped - {Lobbycode} ({Region})";
                }
                else
                    details = $"Revamped {Main.ModVersion}";

                activity.Details = details;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error in updating discord rpc", "DiscordPatch");
            Logger.Exception(ex, "DiscordPatch");
            details = $"Revamped v{Main.ModVersion}";
            activity.Details = details;
        }
    }
}