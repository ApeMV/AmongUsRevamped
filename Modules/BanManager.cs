using InnerNet;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;

// https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/main/Modules/BanManager.cs
namespace AmongUsRevamped;

public static class BanManager
{
    public static readonly string DataPath =
#if ANDROID
        Application.persistentDataPath;
#else
        ".";
#endif

    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", "");
    private static readonly string DenyNameListPath = $"{DataPath}/AUR-DATA/DenyNameList.txt";
    private static string BanListPath = $"{DataPath}/AUR-DATA/BanList.txt";
    private static string ModeratorListPath = $"{DataPath}/AUR-DATA/ModeratorList.txt";
    public static List<string> TempBanWhiteList = [];
    public static void Init()
    {
        try
        {
            if (!Directory.Exists($"{DataPath}/AUR-DATA")) Directory.CreateDirectory($"{DataPath}/AUR-DATA");

            if (!File.Exists(DenyNameListPath))
            {
                Logger.Warn("Creating a new DenyNameList.txt file", "BanManager");
                File.Create(DenyNameListPath).Close();
            }
            if (!File.Exists(BanListPath))
            {
                Logger.Warn("Creating a new BanList.txt file", "BanManager");
                File.Create(BanListPath).Close();
            }
            if (!File.Exists(ModeratorListPath))
            {
                Logger.Warn("Creating a new ModeratorList.txt file", "BanManager");
                File.Create(ModeratorListPath).Close();
            }

        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "BanManager");
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return "";
        string puid = player.ProductUserId;
        return GetHashedPuid(puid);
    }
    public static string GetHashedPuid(string puid)
    {
        using SHA256 sha256 = SHA256.Create();

        // get sha-256 hash
        byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
        string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();

        // pick front 5 and last 4
        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }

    public static void AddBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || player == null) return;
        if (!CheckBanList(player?.FriendCode, player?.GetHashedPuid()) && !TempBanWhiteList.Contains(player?.GetHashedPuid()))
        {
            if (player?.GetHashedPuid() != "" && player?.GetHashedPuid() != null && player?.GetHashedPuid() != "e3b0cb855")
            {
                var additionalInfo = "";
                File.AppendAllText(BanListPath, $"{player?.FriendCode},{player?.GetHashedPuid()},{player.PlayerName.RemoveHtmlTags()}{additionalInfo}\n");
                Logger.SendInGame($"Added {player?.PlayerName.RemoveHtmlTags()}/{player?.FriendCode}/{player?.GetHashedPuid()} to the BanList");
            }
            else Logger.Info($"Failed to add player {player?.PlayerName.RemoveHtmlTags()}/{player?.FriendCode}/{player?.GetHashedPuid()} to the BanList", "AddBanPlayer");
        }
    }

    public static void CheckBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        string friendcode = player?.FriendCode;

        // Check file BanList.txt
        if (Options.ApplyBanList.GetBool() && CheckBanList(friendcode, player?.GetHashedPuid()))
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            Logger.Info($"{player.PlayerName} was in the BanList and has been banned", "BanListBan");
            return;
        }
        if (TempBanWhiteList.Contains(player?.GetHashedPuid()))
        {
            AmongUsClient.Instance.KickPlayer(player.Id, true);
            //This should not happen
            Logger.Info($"{player.PlayerName} was in the Temporary BanList", "TempBan");
            return;
        }
    }
    public static bool CheckBanList(string code, string hashedpuid = "")
    {
        bool OnlyCheckPuid = false;
        if (code == "" && hashedpuid != "") OnlyCheckPuid = true;
        else if (code == "") return false;

        string noDiscrim = "";
        if (code.Contains('#'))
        {
            int index = code.IndexOf('#');
            noDiscrim = code[..index];
        }

        try
        {
            if (!Directory.Exists($"{DataPath}/AUR-DATA")) Directory.CreateDirectory($"{DataPath}/AUR-DATA");
            if (!File.Exists(BanListPath)) File.Create(BanListPath).Close();

            using StreamReader sr = new(BanListPath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "") continue;
                if (!OnlyCheckPuid)
                {
                    if (line.Contains(code)) return true;
                    if (!string.IsNullOrEmpty(noDiscrim) && !line.Contains('#') && line.Contains(noDiscrim)) return true;
                }
                if (line.Contains(hashedpuid)) return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "CheckBanList");
        }
        return false;
    }
    public static bool IsPlayerInDenyName(ClientData client, string name)
    {
        if (name == "" || !AmongUsClient.Instance.AmHost || !Options.ApplyDenyNameList.GetBool()) return false;

        var deniedNames = File.ReadAllLines(DenyNameListPath);

        if (deniedNames.Where(code => !string.IsNullOrWhiteSpace(code)).Any(code => name.Contains(code, StringComparison.OrdinalIgnoreCase)))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);    
            Logger.Info($" {name} was kicked because their name was in DenyNameList.txt", "Kick");      
            Logger.SendInGame($" {name} was kicked because their name was in DenyNameList.txt");    
            return true;
        }
        else return false;
    }

}
[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Select))]
class BanMenuSelectPatch
{
    public static void Postfix(BanMenu __instance, int clientId)
    {
        ClientData recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
        if (recentClient == null) return;

        if (!BanManager.CheckBanList(recentClient?.FriendCode, recentClient?.GetHashedPuid()))
            __instance.BanButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
    }
}
