using System.Collections.Generic;
using System.Data;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using System.IO;
using System.Reflection;
using System;
using System.Security.Cryptography;
using System.Text;
using AmongUs.InnerNet.GameDataMessages;

using Object = UnityEngine.Object;

namespace HNSRevamped;

    public static class Utils
    {
        public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;

        public static string ColorString(Color32 color, string str) => $"<#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
        public static string ColorToHex(Color32 color) => $"#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}";

    public static bool IsPlayerModerator(string friendCode)
    {
        if (friendCode == "") return false;

        var friendCodesFilePath = @"./HNSR-DATA/Moderators.txt";
        var friendCodes = File.ReadAllLines(friendCodesFilePath);
        return friendCodes.Any(code => code.Contains(friendCode));
    }

        public static string GetTabName(TabGroup tab)
        {
            switch (tab)
            {
                case TabGroup.SystemSettings:
                    return "System Settings";
                case TabGroup.ModSettings:
                    return "Mod Settings";

                default:
                    return "";
            }
        }

        public static void DestroyTranslator(this GameObject obj)
        {
            var translator = obj.GetComponent<TextTranslatorTMP>();
            if (translator != null)
            {
                Object.Destroy(translator);
            }
        }

        public static void DestroyTranslator(this MonoBehaviour obj) => obj.gameObject.DestroyTranslator();

        public static void CustomSettingsChangeMessageLogic(this NotificationPopper notificationPopper, OptionItem optionItem, string text, bool playSound)
        {
            if (notificationPopper.lastMessageKey == 10000 + optionItem.Id && notificationPopper.activeMessages.Count > 0)
            {
                notificationPopper.activeMessages[notificationPopper.activeMessages.Count - 1].UpdateMessage(text);
            }
            else
            {
                notificationPopper.lastMessageKey = 10000 + optionItem.Id;
                LobbyNotificationMessage settingmessage = Object.Instantiate(notificationPopper.notificationMessageOrigin, Vector3.zero, Quaternion.identity, notificationPopper.transform);
                settingmessage.transform.localPosition = new Vector3(0f, 0f, -2f);
                settingmessage.SetUp(text, notificationPopper.settingsChangeSprite, notificationPopper.settingsChangeColor, new Action(() =>
                {
                    notificationPopper.OnMessageDestroy(settingmessage);
                }));
                notificationPopper.ShiftMessages();
                notificationPopper.AddMessageToQueue(settingmessage);
            }
            if (playSound)
            {
                SoundManager.Instance.PlaySoundImmediate(notificationPopper.settingsChangeSound, false, 1f, 1f, null);
            }
        }

        public static string GetOptionNameSCM(this OptionItem optionItem)
        {
            if (optionItem.Name == "Enable")
            {
                int id = optionItem.Id;
                while (id % 10 != 0)
                    --id;
                var optionItem2 = OptionItem.AllOptions.FirstOrDefault(opt => opt.Id == id);
                return optionItem2 != null ? optionItem2.GetName() : optionItem.GetName();
            }
            else
                return optionItem.GetName();
        }
    }