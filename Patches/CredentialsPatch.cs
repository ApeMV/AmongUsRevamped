using BepInEx.Unity.IL2CPP;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            CleanupOldUpdates();

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

        private static void CleanupOldUpdates()
        {
            try
            {
                string pluginsPath = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins");
                if (!Directory.Exists(pluginsPath)) return;

                foreach (string file in Directory.GetFiles(pluginsPath, "*.dll.old"))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.IndexOf("AUR", StringComparison.OrdinalIgnoreCase) >= 0
                        || fileName.IndexOf("AmongUsRevamped", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try
                        {
                            File.Delete(file);
                            Logger.Info($"Cleaned up old update: {fileName}", "Updater");
                        }
                        catch (Exception ex)
                        {
                            Logger.Info($"Could not delete {fileName}: {ex.Message}", "Updater");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Exception(e, "CleanupOldUpdates");
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
        private static PassiveButton updatesButton;
        private static PassiveButton animationSceneButton;
        private static Transform buttonParent;
        private static bool updateDownloaded = false;

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
                    new(1.5f, 4.05f, 1f),
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
                    new(2.7f, 4.05f, 1f),
                    new(153, 153, 153, byte.MaxValue),
                    new(209, 209, 209, byte.MaxValue),
                    () => Application.OpenURL("https://github.com/ApeMV/AmongUsRevamped"),
                    "GitHub");
            }

            if (updatesButton == null)
            {
                updatesButton = CreateButton(
                    __instance,
                    "UpdatesButton",
                    new(3.9f, 4.05f, 1f),
                    new(0, 165, 0, byte.MaxValue),
                    new(100, 220, 100, byte.MaxValue),
                    () => OnUpdatesButtonClick(),
                    "Update");
            }

            if (animationSceneButton == null)
            {
                animationSceneButton = CreateButton(
                    __instance,
                    "AnimationTestSceneButton",
                    new(0f, 2.65f, 1f),
                    new(139, 0, 0, byte.MaxValue),
                    new(255, 127, 127, byte.MaxValue),
                    () => SceneChanger.ChangeScene("AnimationTestScene"),
                    "Animation Tester");
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

            var logo = GameObject.Find("MainUI/AspectScaler/LeftPanel/Sizer/LOGO-AU");
            if (logo != null)
            {
                logo.SetActive(false);
            }

            var h1 = GameObject.Find("AccountManager/AccountTab/GameHeader/BarSprite/FriendsButton/Highlight/NewRequestActive/Text_TMP");
            if (h1 != null)
            {
                Object.Destroy(h1);
            }

            var h2 = GameObject.Find("AccountManager/AccountTab/GameHeader/BarSprite/FriendsButton/Highlight/NewRequestActive/Background");
            if (h2 != null)
            {
                Object.Destroy(h2);
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

        private static void OnUpdatesButtonClick()
        {
            if (updateDownloaded)
            {
                DisconnectPopup.Instance.gameObject.SetActive(true);
                DisconnectPopup.Instance._textArea.enableWordWrapping = true;
                DisconnectPopup.Instance._textArea.text =
                    "Latest update already installed!\n\nPlease restart Among Us to apply the update.";
                return;
            }

            _ = CheckForUpdatesAsync();
        }

        private static void SetUpdatesButtonDisabled()
        {
            if (updatesButton == null) return;

            updateDownloaded = true;

            var normalSprite = updatesButton.inactiveSprites.GetComponent<SpriteRenderer>();
            var hoverSprite = updatesButton.activeSprites.GetComponent<SpriteRenderer>();
            if (normalSprite != null) normalSprite.color = new Color32(100, 100, 100, byte.MaxValue);
            if (hoverSprite != null) hoverSprite.color = new Color32(130, 130, 130, byte.MaxValue);

            var buttonText = updatesButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TMP_Text>();
            if (buttonText != null) buttonText.color = new Color32(160, 160, 160, byte.MaxValue);
        }

        private static PassiveButton CreateButton(MainMenuManager menu, string name, Vector3 localPosition, Color32 normalColor, Color32 hoverColor, Action action, string label)
        {
            var parent = menu.transform.Find("MainUI/AspectScaler/LeftPanel/Main Buttons");
            if (parent == null) return null;

            var button = Object.Instantiate(menu.quitButton, parent);
            button.name = name;

            button.transform.localPosition = localPosition;

            if (name == "GitHubButton" || name == "DiscordButton" || name == "UpdatesButton")
            {
                button.transform.localScale = new Vector3(0.6f, 0.8f, 1f);
            }
            else
            {
                button.transform.localScale = new Vector3(1.8f, 1.6f, 1f);
            }

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

        private static async Task CheckForUpdatesAsync()
        {
#if ANDROID
            return;
#else
            DisconnectPopup.Instance.gameObject.SetActive(true);
            DisconnectPopup.Instance._textArea.enableWordWrapping = true;
            DisconnectPopup.Instance._textArea.text = "Searching for updates...";

            try
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.ParseAdd("AmongUsRevamped-Updater");

                var json = await http.GetStringAsync(
                    "https://api.github.com/repos/ApeMV/AmongUsRevamped/releases/latest");

                string tag = ExtractJsonString(json, "tag_name")?.TrimStart('v') ?? "";
                string assetUrl = "";

                int assetsIndex = json.IndexOf("\"assets\"", StringComparison.Ordinal);
                if (assetsIndex >= 0)
                {
                    string assetsSection = json.Substring(assetsIndex);
                    int searchFrom = 0;

                    while (true)
                    {
                        string name = ExtractJsonString(assetsSection, "name", searchFrom);
                        if (name == null) break;

                        if (name.StartsWith("AUR.v", StringComparison.OrdinalIgnoreCase)
                            && name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            int namePos = assetsSection.IndexOf($"\"{name}\"", searchFrom, StringComparison.Ordinal);
                            if (namePos < 0) break;

                            string afterName = assetsSection.Substring(namePos, Math.Min(1500, assetsSection.Length - namePos));
                            assetUrl = ExtractJsonString(afterName, "browser_download_url") ?? "";

                            Logger.Info($"UpdateCheck: found DLL '{name}', url='{assetUrl}'", "Updater");
                            break;
                        }

                        int nextPos = assetsSection.IndexOf($"\"{name}\"", searchFrom, StringComparison.Ordinal);
                        searchFrom = nextPos >= 0 ? nextPos + name.Length + 2 : assetsSection.Length;
                    }
                }

                Logger.Info($"UpdateCheck: tag='{tag}', assetUrl='{assetUrl}'", "Updater");

                if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(assetUrl))
                {
                    DisconnectPopup.Instance._textArea.text = "No updates found!";
                    return;
                }

                var currentVer = Version.Parse(Main.ModVersion.TrimStart('v'));
                var remoteVer = Version.Parse(tag);

                Logger.Info($"UpdateCheck: current={currentVer}, remote={remoteVer}", "Updater");

                if (remoteVer > currentVer)
                {
                    DisconnectPopup.Instance._textArea.text =
                        $"Update available: v{remoteVer}\nSee the changelog on GitHub\n\nDownloading... 0%";

                    string pluginsPath = Path.Combine(
                        Environment.CurrentDirectory, "BepInEx", "plugins");
                    string tempFile = Path.Combine(pluginsPath, $"AUR.v{remoteVer}.dll.tmp");
                    string newDll = Path.Combine(pluginsPath, $"AUR.v{remoteVer}.dll");
                    string newDllName = $"AUR.v{remoteVer}.dll";

                    using var response = await http.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    int bytesRead;
                    int lastPercent = -1;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;

                        if (totalBytes.HasValue && totalBytes.Value > 0)
                        {
                            int percent = (int)(totalRead * 100 / totalBytes.Value);
                            if (percent != lastPercent)
                            {
                                lastPercent = percent;
                                DisconnectPopup.Instance._textArea.text =
                                    $"Update available: v{remoteVer}\nSee the changelog on GitHub\n\nDownloading... {percent}%";
                            }
                        }
                    }

                    fileStream.Close();

                    if (File.Exists(newDll)) File.Delete(newDll);
                    File.Move(tempFile, newDll);

                    foreach (string file in Directory.GetFiles(pluginsPath, "*.dll"))
                    {
                        string fileName = Path.GetFileName(file);

                        if (fileName.Equals(newDllName, StringComparison.OrdinalIgnoreCase))
                            continue;

                        bool isAurDll = fileName.IndexOf("AUR", StringComparison.OrdinalIgnoreCase) >= 0
                                     || fileName.IndexOf("AmongUsRevamped", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (isAurDll)
                        {
                            try
                            {
                                string backupPath = file + ".old";
                                if (File.Exists(backupPath)) File.Delete(backupPath);
                                File.Move(file, backupPath);
                                Logger.Info($"Renamed old DLL: {fileName} -> {fileName}.old", "Updater");
                            }
                            catch (Exception ex)
                            {
                                Logger.Info($"Could not rename {fileName}: {ex.Message}", "Updater");
                            }
                        }
                    }

                    DisconnectPopup.Instance._textArea.text =
                        $"Update v{remoteVer} downloaded!\n\nPlease restart Among Us to apply the update.";

                    Logger.Info($"Update v{remoteVer} downloaded successfully ({totalRead} bytes)", "Updater");

                    SetUpdatesButtonDisabled();
                }
                else
                {
                    DisconnectPopup.Instance._textArea.text = "No updates found!";
                }
            }
            catch (Exception e)
            {
                Logger.Exception(e, "UpdateCheck");
                DisconnectPopup.Instance._textArea.text =
                    $"Update check failed:\n{e.Message}";
            }
#endif
        }

        private static string ExtractJsonString(string json, string key, int startIndex = 0)
        {
            string keyPattern = $"\"{key}\"";
            int keyPos = json.IndexOf(keyPattern, startIndex, StringComparison.Ordinal);
            if (keyPos < 0) return null;

            int colonPos = json.IndexOf(':', keyPos + keyPattern.Length);
            if (colonPos < 0) return null;

            int quoteStart = json.IndexOf('"', colonPos + 1);
            if (quoteStart < 0) return null;

            int valueStart = quoteStart + 1;
            int pos = valueStart;
            while (pos < json.Length)
            {
                if (json[pos] == '\\' && pos + 1 < json.Length)
                {
                    pos += 2;
                    continue;
                }
                if (json[pos] == '"')
                {
                    return json.Substring(valueStart, pos - valueStart);
                }
                pos++;
            }

            return null;
        }
    }

    [HarmonyPatch(typeof(SignInStatusComponent), nameof(SignInStatusComponent.SetOnline))]
    public static class SignInStatusComponentSetOnlinePatch
    {
        private static void Postfix()
        {
            int pluginCount = IL2CPPChainloader.Instance.Plugins.Count;
            Logger.Info($" {pluginCount} Plugins detected", "PluginCheck");

            if (pluginCount > 1 && !Main.DisableCompatibilityWarning.Value)
            {
                DisconnectPopup.Instance.gameObject.SetActive(true);
                DisconnectPopup.Instance._textArea.enableWordWrapping = false;
                DisconnectPopup.Instance._textArea.text = Translator.Get("pluginWarning");
            }
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