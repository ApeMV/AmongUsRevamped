using System;
using System.Text;
using TMPro;
using UnityEngine;

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
            AppendSeparator();
            Sb.Append($"Server: {Utils.GetRegionName()}");

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
}

// https://github.com/3X3CODE/MainMenuEnhanced/blob/main/MainMenuEnhanced/VisualPatch.cs
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{
    public static void Postfix(MainMenuManager __instance)
    {
        if (__instance == null) return;

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
}