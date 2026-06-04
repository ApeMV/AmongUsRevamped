using InnerNet;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
internal static class CommandHelper
{
    private static TextMeshPro HelperText;

    public class Cmd
    {
        public string[] Name;
        public string Prompt;
        public string Description;
        public bool HasVariable;
    }

    static readonly List<Cmd> Commands = new()
    {
        new Cmd { Name = new[] { "/help" }, Prompt = "/help", Description = Translator.Get("helpDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/sns" }, Prompt = "/sns", Description = Translator.Get("snsDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/shiftandseek" }, Prompt = "/shiftandseek", Description = Translator.Get("snsDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/0kc" }, Prompt = "/0kc", Description = Translator.Get("0kcDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/0killcooldown" }, Prompt = "/0killcooldown", Description = Translator.Get("0kcDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/sr" }, Prompt = "/sr", Description = Translator.Get("SrDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/speedrun" }, Prompt = "/speedrun", Description = Translator.Get("SrDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/eg" }, Prompt = "/eg", Description = Translator.Get("egDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/endgame" }, Prompt = "/endgame", Description = Translator.Get("egDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/em" }, Prompt = "/em", Description = Translator.Get("emDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/endmeeting" }, Prompt = "/endmeeting", Description = Translator.Get("emDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/aur" }, Prompt = "/aur", Description = Translator.Get("aurDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/socials" }, Prompt = "/socials", Description = Translator.Get("aurDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/s" }, Prompt = "/s", Description = Translator.Get("sDesc"), HasVariable = false },
        new Cmd { Name = new[] { "/start" }, Prompt = "/start", Description = Translator.Get("sDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/r" }, Prompt = "/r {role}", Description = Translator.Get("rDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/roles" }, Prompt = "/roles", Description = Translator.Get("rolesDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/kick" }, Prompt = "/kick {name}", Description = Translator.Get("kickDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/ckick" }, Prompt = "/ckick {color}", Description = Translator.Get("ckickDesc"), HasVariable = true },

        new Cmd { Name = new[] { "/ban" }, Prompt = "/ban {name}", Description = Translator.Get("banDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/cban" }, Prompt = "/cban {color}", Description = Translator.Get("cbanDesc"), HasVariable = true },

        new Cmd { Name = new[] { "/dump" }, Prompt = "/dump", Description = Translator.Get("dumpDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/col" }, Prompt = "/col {color}", Description = Translator.Get("colDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/color" }, Prompt = "/color {color}", Description = Translator.Get("colDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/colour" }, Prompt = "/colour {color}", Description = Translator.Get("colDesc"), HasVariable = true },

        new Cmd { Name = new[] { "/reload" }, Prompt = "/reload", Description = Translator.Get("reloadDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/vip" }, Prompt = "/vip {name}", Description = Translator.Get("vipDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/moderator" }, Prompt = "/moderator {name}", Description = Translator.Get("moderatorDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/admin" }, Prompt = "/admin {name}", Description = Translator.Get("adminDesc"), HasVariable = true },

        new Cmd { Name = new[] { "/removevip" }, Prompt = "/removevip {name}", Description = Translator.Get("cremoveVipDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/removemoderator" }, Prompt = "/removemoderator {name}", Description = Translator.Get("removeModeratorDesc"), HasVariable = true },
        new Cmd { Name = new[] { "/removeadmin" }, Prompt = "/removeadmin {name}", Description = Translator.Get("removeAdminDesc"), HasVariable = true },

        new Cmd { Name = new[] { "/lastgame" }, Prompt = "/lastgame", Description = Translator.Get("lastgameDesc"), HasVariable = false },

        new Cmd { Name = new[] { "/t" }, Prompt = "/t {template}", Description = Translator.Get("templateDesc"), HasVariable = true }
    };

    [HarmonyPostfix]
    public static void Postfix(TextBoxTMP __instance)
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Chat == null || HudManager.Instance.Chat.freeChatField == null || __instance != HudManager.Instance.Chat.freeChatField.textArea) return;

        if (Main.DisableCommandHelper.Value || !AmongUsClient.Instance.AmHost)
        {
            HelperText.enabled = false;
            return;
        }

        if (!HelperText)
        {
            HelperText = Object.Instantiate(__instance.outputText, __instance.outputText.transform.parent);
            HelperText.name = "HelperText";
            HelperText.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            HelperText.transform.localPosition = __instance.outputText.transform.localPosition;
            HelperText.enabled = false;
        }

        string t = __instance.text;

        if (string.IsNullOrEmpty(t) || !t.StartsWith("/"))
        {
            HelperText.enabled = false;
            return;
        }

        string textCmd = t.Split(' ')[0];

        Cmd u = null;
        Cmd v = null;

        foreach (var cmd in Commands)
        {
            foreach (var a in cmd.Name)
            {
                string aliasCmd = a;

                if (aliasCmd.Equals(textCmd))
                {
                    v = cmd;
                    u = cmd;
                    break;
                }

                if (aliasCmd.StartsWith(textCmd))
                {
                    u ??= cmd;
                }
            }

            if (v != null) break;
        }

        if (v != null && !v.HasVariable && t.Length > textCmd.Length)
        {
            HelperText.enabled = false;
            return;
        }

        __instance.outputText.enabled = true;

        if (v != null && textCmd.Equals(v.Name[0]))
        {
            HelperText.text = $"\n\n<size=80%><color=#FF0000>{v.Description}</color></size>";
        }
        else if (u != null)
        {
            HelperText.text = u.Prompt;
        }
        else
        {
            HelperText.enabled = false;
            return;
        }
        HelperText.enabled = true;
    }
}