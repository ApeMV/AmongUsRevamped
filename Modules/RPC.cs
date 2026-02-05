using Hazel;
using InnerNet;
using UnityEngine;
using System;
using AmongUs.GameOptions;

namespace AmongUsRevamped
{
    public enum CustomRPC
    {
        SyncCustomOptions,
        AddCustomSettingsChangeMessage,

    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.HandleRpc))]
    class GameManagerHandleRpc
    {
        public static void Postfix(GameManager __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId < 70) return;
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.SyncCustomOptions:
                    for (int i = reader.ReadInt32(); i < OptionItem.AllOptions.Count; ++i)
                    {
                        if (reader.BytesRemaining == 0) break;
                        var co = OptionItem.AllOptions[i];
                        if (co is TextOptionItem) continue;
                        if (co.Id == 0)
                        {
                            co.SetValue(9);
                            continue;
                        }
                        if (co.Id >= 1000 && co.IsHiddenOn()) continue;
                        co.SetValue(reader.ReadInt32());
                    }
                    var viewSettingsPane = Object.FindObjectOfType<LobbyViewSettingsPane>();
                    if (viewSettingsPane != null)
                    {
                        if (viewSettingsPane.currentTab != StringNames.OverviewCategory && viewSettingsPane.currentTab != StringNames.RolesCategory)
                            viewSettingsPane.ChangeTab(viewSettingsPane.currentTab);
                        LobbyViewPatch.ReCreateButtons(viewSettingsPane);
                    }
                    break;
                case CustomRPC.AddCustomSettingsChangeMessage:
                    int optionId = reader.ReadInt32();
                    var optionItem = OptionItem.AllOptions.FirstOrDefault(opt => opt.Id == optionId);
                    if (optionItem == null) break;
                    __instance.AddCustomSettingsChangeMessage(optionItem, reader.ReadString(), reader.ReadBoolean());
                    break;
            }
        }
    }

    static class RPC
    {

        public static void AddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            string optionName = "";
            if (optionItem.Parent?.Parent?.Parent != null)
                optionName += optionItem.Parent.Parent.Parent.GetOptionNameSCM() + " → ";
            if (optionItem.Parent?.Parent != null)
                optionName += optionItem.Parent.Parent.GetOptionNameSCM() + " → ";
            if (optionItem.Parent != null)
                optionName += optionItem.Parent.GetOptionNameSCM() + " → ";
            optionName += optionItem.GetOptionNameSCM();
            string text = $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{optionName}</font>: <font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{value}</font>";
            HudManager.Instance.Notifier.CustomSettingsChangeMessageLogic(optionItem, text, playSound);
        }


        public static void RpcSyncCustomOptions(this GameManager manager)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable, -1);
            writer.Write(0);
            for (int i = 0; i < OptionItem.AllOptions.Count; ++i)
            {
                var co = OptionItem.AllOptions[i];
                if (co.Id == 0 || co is TextOptionItem) continue;
                if (co.Id >= 1000 && co.IsHiddenOn()) continue;
                if (writer.Length > 500)
                {
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.SyncCustomOptions, SendOption.Reliable, -1);
                    writer.Write(i);
                }
                writer.Write(co.CurrentValue);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcAddCustomSettingsChangeMessage(this GameManager manager, OptionItem optionItem, string value, bool playSound)
        {
            manager.AddCustomSettingsChangeMessage(optionItem, value, playSound);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(manager.NetId, (byte)CustomRPC.AddCustomSettingsChangeMessage, SendOption.Reliable, -1);
            writer.Write(optionItem.Id);
            writer.Write(value);
            writer.Write(playSound);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}