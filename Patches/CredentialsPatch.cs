using System;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//https://github.com/Gurge44/EndlessHostRoles/blob/main/Patches/CredentialsPatch.cs
namespace AmongUsRevamped
{
    public enum ErrorCode
    {
        Main_DictionaryError = 10003
    }

    public class ErrorText
    {
        public static ErrorText Instance;
        public TextMeshPro Text;

        private readonly List<ErrorCode> _errors = new();

        public static void Create(TextMeshPro baseText)
        {
            if (Instance != null) return;

            var text = UnityEngine.Object.Instantiate(baseText);
            text.name = "ErrorText";

            text.enabled = false;
            text.text = "-";
            text.color = Color.red;
            text.alignment = TextAlignmentOptions.Top;

            Instance = new ErrorText
           {
                Text = text
            };
        }

        public void AddError(ErrorCode code)
        {
            if (!_errors.Contains(code))
                _errors.Add(code);

            Text.enabled = true;
            Text.text = $"Error: {code}";
        }
    }

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    internal static class VersionShowerStartPatch
    {
        private static void Postfix(VersionShower __instance)
        {
            Utils.ClearLeftoverData();
            NormalGameEndChecker.LastWinReason = "";

            Main.CredentialsText = $"<color=#FFD700>Among Us Revamped</color><color=#ffffff> {Main.ModVersion}</color>";

            var credentials = UnityEngine.Object.Instantiate(__instance.text);
            credentials.text = Main.CredentialsText;
            credentials.alignment = TextAlignmentOptions.Right;
            credentials.transform.position = new Vector3(1f, 2.67f, -2f);
            credentials.fontSize = credentials.fontSizeMax = credentials.fontSizeMin = 2f;

            ErrorText.Create(__instance.text);
            if (Main.HasArgumentException && ErrorText.Instance != null)
            {
                ErrorText.Instance.AddError(ErrorCode.Main_DictionaryError);
            }
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    internal static class PingTrackerUpdatePatch
    {
        public static PingTracker Instance;
        private static readonly StringBuilder Sb = new();
        private static long LastUpdate;
        private static readonly List<float> LastFPS = new();

        public static bool Prefix(PingTracker __instance)
        {
            FpsSampler.TickFrame();

            if (!Instance) Instance = __instance;
            var instance = Instance;

            if (AmongUsClient.Instance == null) return false;

            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                instance.gameObject.SetActive(false);
                return false;
            }

            if (instance.name != "HNSR_SettingsText")
            {
                Vector3 pos = !AmongUsClient.Instance.IsGameStarted ? instance.lobbyPos : instance.gamePos;
                pos.y += 0.1f;
                instance.aspectPosition.DistanceFromEdge = pos;
                instance.text.alignment = TextAlignmentOptions.Center;
                instance.text.text = Sb.ToString();
            }

            long now = Utils.TimeStamp;
            if (now == LastUpdate) return false;
            LastUpdate = now;

            Sb.Clear();

            Sb.Append(Utils.IsLobby ? "\r\n<size=2.5>" : "<size=2.5>");
            Sb.Append(Main.CredentialsText);

            int ping = AmongUsClient.Instance.Ping;
            string color = ping switch
            {
                < 30 => "#44dfcc",
                < 100 => "#7bc690",
                < 200 => "#f3920e",
                < 400 => "#ff146e",
                _ => "#ff4500"
            };

            Sb.Append(Utils.InGame ? "  -  " : "\r\n");
            Sb.Append($"<color={color}>Ping: {ping}</color>");

            if (Utils.GetRegionName() != "")
            {
                AppendSeparator();
                Sb.Append(Utils.GetRegionName());
            }

            if (Main.ShowFps.Value && LastFPS.Count > 0)
            {
                float fps = LastFPS.Average();
                Color fpscolor = fps switch
                {
                    < 10f => Color.red,
                    < 25f => Color.yellow,
                    < 50f => Color.green,
                    _ => new Color32(0, 165, 255, 255)
                };

                AppendSeparator();
                Sb.Append(Utils.ColorString(fpscolor, Utils.ColorString(Color.cyan, "FPS: ") + (int)fps));
            }

            if (Utils.InGame) Sb.Append("\r\n.");

            return false;

            void AppendSeparator() => Sb.Append(Utils.InGame ? "  -  " : " - ");
        }

        private static class FpsSampler
        {
            private static int Frames;
            private static float Elapsed;
            private const float SampleInterval = 0.5f;

            public static void TickFrame()
            {
                Frames++;
                Elapsed += Time.unscaledDeltaTime;
                if (Elapsed < SampleInterval) return;
                LastFPS.Add(Frames / Elapsed);
                if (LastFPS.Count > 10) LastFPS.RemoveAt(0);
                Frames = 0;
                Elapsed = 0f;
            }
        }
    }

    // https://github.com/3X3CODE/MainMenuEnhanced/blob/main/MainMenuEnhanced/VisualPatch.cs
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        private static PassiveButton template;
        private static PassiveButton discordButton;
        private static PassiveButton gitHubButton;
        private static Transform buttonParent;
        public static void Postfix(MainMenuManager __instance)
        {
            if (__instance == null) return;
            if (template == null) template = __instance.quitButton;
            if (template == null) return;

            if (buttonParent == null) buttonParent = template.transform.parent;
            if (discordButton == null)
            {
                discordButton = CreateButton(
                    __instance,
                    "DiscordButton",
                    new(2.1f, 4.05f, 1f),
                    new(88, 101, 242, byte.MaxValue),
                    new(148, 161, byte.MaxValue, byte.MaxValue),
                    () => Application.OpenURL("https://discord.gg/83Zhzhyhya"),
                    "Discord");
            }

            if (gitHubButton == null)
            {
                gitHubButton = CreateButton(
                    __instance,
                    "GitHubButton",
                    new(3.8f, 4.05f, 1f),
                    new(153, 153, 153, byte.MaxValue),
                    new(209, 209, 209, byte.MaxValue),
                    () => Application.OpenURL("https://github.com/ApeMV/AmongUsRevamped"),
                    "GitHub");
            }

            var bg = GameObject.Find("BackgroundTexture");
            if (bg != null)
            {
                bg.SetActive(false);
            }

            var leftPanel = GameObject.Find("LeftPanel");
            if (leftPanel != null)
            {
                leftPanel.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }

            var div = GameObject.Find("MainUI/AspectScaler/LeftPanel/Main Buttons/Divider");
            if (div != null)
            {
                div.SetActive(false);
            }


            Transform tintTrans = __instance.transform.Find("MainUI/Tint");
            var tint = tintTrans.gameObject;
            if (tint != null)
            {
                tint.SetActive(false);
            }

            DisableObject("WindowShine");
            DisableComponent("RightPanel");
            DisableComponent("MaskedBlackScreen");

            Transform playTransform = __instance.transform.Find("MainUI/AspectScaler/LeftPanel/Main Buttons/PlayButton/FontPlacer/Text_TMP");
            if (playTransform != null) 
            {
                var playbutton = playTransform.gameObject;
                if (playbutton != null)
                {
                    if (playbutton.TryGetComponent<TextTranslatorTMP>(out var tmp))
                    {
                        tmp.enabled = false;
                    }
                    if (playbutton.TryGetComponent<TextMeshPro>(out var text))
                    {
                        text.text = "Start";
                    }
                }
            }
            
            static void DisableObject(string name)
            {
                var obj = GameObject.Find(name);
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            static void DisableComponent(string name)
            {
                var obj = GameObject.Find(name);
                if (obj != null)
                {
                    if (obj.TryGetComponent<SpriteRenderer>(out var renderer))
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        private static PassiveButton CreateButton(MainMenuManager menu, string name, Vector3 localPosition, Color32 normalColor, Color32 hoverColor, Action action, string label)
        {
            var parent = menu.transform.Find("MainUI/AspectScaler/LeftPanel/Main Buttons");
            if (parent == null) return null;

            var button = Object.Instantiate(menu.quitButton, parent);
            button.name = name;

            button.transform.localPosition = localPosition;
            button.transform.localScale = new Vector3(0.8f, 1f, 1f);

            var aspect = button.GetComponent<AspectPosition>();
            if (aspect != null)
            {
                aspect.enabled = false;
            }

            button.OnClick = new();
            button.OnClick.AddListener(action);

            var buttonText = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TMP_Text>();
            Utils.DestroyTranslator(buttonText);
            buttonText.text = label;
            buttonText.fontSize = buttonText.fontSizeMax = buttonText.fontSizeMin = 3.5f;
            buttonText.enableWordWrapping = false;
            buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;

            var normalSprite = button.inactiveSprites.GetComponent<SpriteRenderer>();
            var hoverSprite = button.activeSprites.GetComponent<SpriteRenderer>();
            normalSprite.color = normalColor;
            hoverSprite.color = hoverColor;

            button.gameObject.SetActive(true);
            return button;
        }
    }
}

#if !ANDROID
[HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
public static class ServerDropdownPatch
{
    public static bool Prefix(ServerDropdown __instance)
    {
        if (SceneManager.GetActiveScene().name == "FindAGame") return true;
        SpriteRenderer bg = __instance.background;
        bg.size = new Vector2(4, 1);
        ServerManager sm = ServerManager.Instance;
        TranslationController tc = TranslationController.Instance;
        int totalCols = Mathf.Max(1, Mathf.CeilToInt(sm.AvailableRegions.Length / (float)5));
        int rowLimit = Mathf.Min(sm.AvailableRegions.Length, 5);

        for (var index = 0; index < sm.AvailableRegions.Length; index++)
        {
            IRegionInfo ri = sm.AvailableRegions[index];
            var b = __instance.ButtonPool.Get<ServerListButton>();
            b.transform.localPosition = new Vector3(((index / 5) - ((totalCols - 1) / 2f)) * 3.15f, __instance.y_posButton - (0.5f * (index % 5)), -1f);
            b.Text.text = tc.GetStringWithDefault(ri.TranslateName, ri.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            b.Text.ForceMeshUpdate();
            b.Button.OnClick.RemoveAllListeners();
            b.Button.OnClick.AddListener((Action)(() => __instance.ChooseOption(ri)));
            __instance.controllerSelectable.Add(b.Button);
        }

        float h = 1.2f + (0.5f * (rowLimit - 1));
        float w = totalCols > 1 ? (3.15f * (totalCols - 1)) + bg.size.x : bg.size.x;
        bg.transform.localPosition = new Vector3(0f, __instance.initialYPos - ((h - 1.2f) / 2f), 0f);
        bg.size = new Vector2(w, h);
        return false;
    }
}
#endif