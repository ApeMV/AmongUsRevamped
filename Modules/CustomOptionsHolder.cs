using System.Threading.Tasks;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionHolder.cs
namespace AmongUsRevamped
{
    public static class CL
    {
        public static Color32 Hex(string hex)
        {
            if (hex.StartsWith("#"))
            hex = hex.Substring(1);

            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length != 6)
                throw new System.ArgumentException("Hex color must be 6 or 3 characters long.");

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, 255);
        }
    }

    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            taskOptionsLoad = Task.Run(Load);
        }

        public const int PresetId = 0;

        private static readonly string[] presets =
        {
            Main.Preset1.Value, Main.Preset2.Value, Main.Preset3.Value,
            Main.Preset4.Value, Main.Preset5.Value
        };

        public static readonly string[] gameModes =
        {
            "Standard", "Shift And Seek", "Speedrun"
        };

        public static readonly string[] accessLevels =
        {
            Translator.Get("everyone"), Translator.Get("vipAndAbove"), Translator.Get("moderatorAndAbove"), Translator.Get("admin"), Translator.Get("onlyYou")
        };

        public static readonly string[] afkPenalties =
        {
            Translator.Get("kick"), Translator.Get("ban"), Translator.Get("notifyMe")
        };



        //System
        public static OptionItem Language;

        public static OptionItem Gamemode;

        public static OptionItem TabGroupMain;

        public static OptionItem TabGroupLevelKick;
        public static OptionItem EnableKickLowLevelPlayer;
        public static OptionItem KickLowLevelPlayer;
        public static OptionItem DontKickLevelOnes;
        public static OptionItem TempBanLowLevelPlayer;
        public static OptionItem EnableKickHighLevelPlayer;
        public static OptionItem KickHighLevelPlayer;
        public static OptionItem TempBanHighLevelPlayer;

        public static OptionItem TabGroupKickInvalidFriendCodes;
        public static OptionItem KickInvalidFriendCodes;
        public static OptionItem TempBanInvalidFriendCodes;

        public static OptionItem TabGroupDeniedWords;
        public static OptionItem EnableDeniedWords;
        public static OptionItem DeniedWordsToKick;
        public static OptionItem DeniedWordsAsBan;
        public static OptionItem DeniedWordsStrength;

        public static OptionItem TabGroupAutoKickStart;
        public static OptionItem AutoKickStart;
        public static OptionItem AutoKickStartAsBan;
        public static OptionItem AutoKickStartTimes;
        public static OptionItem AutoKickStartStrength;

        public static OptionItem TabGroupAutomation;
        public static OptionItem AutoSendGameInfo;
        public static OptionItem AutoRejoinLobby;
        public static OptionItem EnableJoinMessages;
        public static OptionItem MessagePerPlayerNum;
        public static OptionItem AutomaticMapPoll;
        public static OptionItem TimeBeforeMapPoll;
        public static OptionItem AutomaticAllMapPoll;
        public static OptionItem TimeBeforeAllMapPoll;

        public static OptionItem TabGroupAutostart;
        public static OptionItem AutoStartTimer;
        public static OptionItem WaitAutoStart;
        public static OptionItem PlayerAutoStart;
        public static OptionItem Rehost;
        public static OptionItem Public;

        public static OptionItem TabGroupAfk;
        public static OptionItem EnableAfkDetection;
        public static OptionItem AfkTimer;
        public static OptionItem AfkPenalty;
        public static OptionItem OnlyDetectSpawn;


        public static OptionItem TabGroupMisc;
        public static OptionItem MapVoteDuration;
        public static OptionItem StartCountdown;
        public static OptionItem ColorCommandLevel;
        public static OptionItem AllowFortegreen;
        public static OptionItem NoGameEnd;

        public static OptionItem TabGroupAccess;
        public static OptionItem SlashColorCmd;
        public static OptionItem SlashRolesAndGamemodeCmd;
        public static OptionItem SlashLastGameCmd;
        public static OptionItem SlashMapAndSeekerCmd;
        public static OptionItem SlashKickCmd;
        public static OptionItem SlashBanCmd;
        public static OptionItem SlashEndMeetingCmd;
        public static OptionItem SlashStartAndEndGameCmd;
        
        //Gameplay
        public static OptionItem TabGroupSabotages;
        public static OptionItem DeadImpostorsCanSabotage;
        public static OptionItem DisableSabotage;
        public static OptionItem DisableReactor;
        public static OptionItem DisableOxygen;
        public static OptionItem DisableLights;
        public static OptionItem DisableComms;
        public static OptionItem DisableHeli;
        public static OptionItem DisableMushroomMixup;
        public static OptionItem DisableCloseDoor;

        public static OptionItem TabGroupGameplayGeneral;
        public static OptionItem DisableAnnoyingMeetingCalls;

        public static OptionItem ChangeDecontaminationTime;
        public static OptionItem DecontaminationTimeOnMiraHQ;
        public static OptionItem DecontaminationDoorOpenTimeOnMiraHQ;
        public static OptionItem DecontaminationTimeOnPolus;
        public static OptionItem DecontaminationDoorOpenTimeOnPolus;
        public static OptionItem DisableSporeTrigger;

        public static OptionItem DisableDevices;
        private static OptionItem DisableSkeldDevices;
        public static OptionItem DisableSkeldAdmin;
        public static OptionItem DisableSkeldCamera;
        private static OptionItem DisableMiraHQDevices;
        public static OptionItem DisableMiraHQAdmin;
        public static OptionItem DisableMiraHQDoorLog;
        private static OptionItem DisablePolusDevices;
        public static OptionItem DisablePolusAdmin;
        public static OptionItem DisablePolusCamera;
        public static OptionItem DisablePolusVital;
        private static OptionItem DisableAirshipDevices;
        public static OptionItem DisableAirshipCockpitAdmin;
        public static OptionItem DisableAirshipRecordsAdmin;
        public static OptionItem DisableAirshipCamera;
        public static OptionItem DisableAirshipVital;
        private static OptionItem DisableFungleDevices;
        public static OptionItem DisableFungleCamera;
        public static OptionItem DisableFungleVital;


        public static OptionItem TabGroupTasks;
        public static OptionItem OverrideTaskSettings;
        public static OptionItem AllPlayersSameTasks;
        public static OptionItem TaskPercentNeededToWin;

        public static OptionItem DisableMiraTasks;

        public static OptionItem DisableMeasureWeather;
        public static OptionItem DisableBuyBeverage;
        public static OptionItem DisableSortSamples;
        public static OptionItem DisableProcessData;
        public static OptionItem DisableRunDiagnostics;


        public static OptionItem DisablePolusTasks;

        public static OptionItem DisableActivateWeatherNodes;
        public static OptionItem DisableAlignTelescope;
        public static OptionItem DisableFillCanisters;
        public static OptionItem DisableInsertKeys;
        public static OptionItem DisableOpenWaterways;
        public static OptionItem DisableRebootWifi;
        public static OptionItem DisableRepairDrill;
        public static OptionItem DisableStoreArtifacts;


        public static OptionItem DisableAirshipTasks;

        public static OptionItem DisablePutAwayPistols;
        public static OptionItem DisablePutAwayRifles;
        public static OptionItem DisableMakeBurger;
        public static OptionItem DisableCleanToilet;
        public static OptionItem DisableDecontaminate;
        public static OptionItem DisableSortRecords;
        public static OptionItem DisableFixShower;
        public static OptionItem DisablePickUpTowels;
        public static OptionItem DisablePolishRuby;
        public static OptionItem DisableDressMannequin;
        public static OptionItem DisableUnlockSafe;
        public static OptionItem DisableResetBreaker;
        public static OptionItem DisableDevelopPhotos;
        public static OptionItem DisableRewindTapes;
        public static OptionItem DisableStartFans;


        public static OptionItem DisableFungleTasks;

        public static OptionItem DisableRoastMarshmallow;
        public static OptionItem DisableCollectSamples;
        public static OptionItem DisableReplaceParts;
        public static OptionItem DisableCollectVegetables;
        public static OptionItem DisableMineOres;
        public static OptionItem DisableExtractFuel;
        public static OptionItem DisableCatchFish;
        public static OptionItem DisablePolishGem;
        public static OptionItem DisableHelpCritter;
        public static OptionItem DisableHoistSupplies;
        public static OptionItem DisableFixAntenna;
        public static OptionItem DisableBuildSandcastle;
        public static OptionItem DisableCrankGenerator;
        public static OptionItem DisableMonitorMushroom;
        public static OptionItem DisablePlayVideoGame;
        public static OptionItem DisableFindSignal;
        public static OptionItem DisableThrowFisbee;
        public static OptionItem DisableLiftWeights;
        public static OptionItem DisableCollectShells;


        public static OptionItem DisableMiscCommonTasks;

        public static OptionItem DisableEnterIdCode;
        public static OptionItem DisableFixWiring;
        public static OptionItem DisableScanBoardingPass;
        public static OptionItem DisableSwipeCard;


        public static OptionItem DisableMiscShortTasks;

        public static OptionItem DisableAssembleArtifact;
        public static OptionItem DisableChartCourse;
        public static OptionItem DisableCleanO2Filter;
        public static OptionItem DisableCleanVent;
        public static OptionItem DisableMonitorTree;
        public static OptionItem DisablePrimeShields;
        public static OptionItem DisableRecordTemperature;
        public static OptionItem DisableStabilizeSteering;
        public static OptionItem DisableUnlockManifolds;


        public static OptionItem DisableMiscLongTasks;

        public static OptionItem DisableAlignEngineOutput;
        public static OptionItem DisableEmptyChute;
        public static OptionItem DisableInspectSample;
        public static OptionItem DisableReplaceWaterJug;
        public static OptionItem DisableStartReactor;
        public static OptionItem DisableWaterPlants;


        public static OptionItem DisableMiscMixedTasks;

        public static OptionItem DisableCalibrateDistributor;
        public static OptionItem DisableClearAsteroids;
        public static OptionItem DisableDivertPower;
        public static OptionItem DisableEmptyGarbage;
        public static OptionItem DisableFuelEngines;
        public static OptionItem DisableSubmitScan;
        public static OptionItem DisableUploadData;

        public static OptionItem TabGroupHNS;
        public static OptionItem NumSeekers;

        public static OptionItem TabGroupSNS;
        public static OptionItem SNSSettingsOverride;
        public static OptionItem SNSChatInGame;
        public static OptionItem SNSChatInGameExtend;
        public static OptionItem CrewAutoWinsGameAfter;
        public static OptionItem CantKillTime;
        public static OptionItem MisfiresToSuicide;
        public static OptionItem SNSDisableSabotage;
        public static OptionItem SNSDisableReactor;
        public static OptionItem SNSDisableOxygen;
        public static OptionItem SNSDisableLights;
        public static OptionItem SNSDisableComms;
        public static OptionItem SNSDisableHeli;
        public static OptionItem SNSDisableMushroomMixup;
        public static OptionItem SNSDisableCloseDoor;

        public static OptionItem TabGroupSpeedrun;
        public static OptionItem SpeedrunSettingsOverride;
        public static OptionItem GameAutoEndsAfter;
        public static OptionItem EngineerMode;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;

            _ = PresetOptionItem.Create(0, TabGroup.SystemSettings)
                .SetColor(Color.yellow)
                .SetHeader(true);

            Gamemode = StringOptionItem.Create(1, Translator.Get("gamemode"), gameModes, 0, TabGroup.SystemSettings, false)
                .SetColor(Color.yellow)
                .SetHeader(true);

            TabGroupLevelKick = TextOptionItem.Create(60048, Translator.Get("tabGroupLevelKick"), TabGroup.ModerationSettings)
                .SetColor(Color.red);
            EnableKickLowLevelPlayer = BooleanOptionItem.Create(60049, Translator.Get("enableKickLowLevelPlayer"), false, TabGroup.ModerationSettings, false);
            KickLowLevelPlayer = IntegerOptionItem.Create(60050, Translator.Get("kickLowLevelPlayer"), new(0, 100, 1), 0, TabGroup.ModerationSettings, false)
                .SetParent(EnableKickLowLevelPlayer)
                .SetValueFormat(OptionFormat.Level);
            TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, Translator.Get("tempBanLowLevelPlayer"), false, TabGroup.ModerationSettings, false)
                .SetParent(EnableKickLowLevelPlayer);
            DontKickLevelOnes = BooleanOptionItem.Create(60052, Translator.Get("dontKickLevelOnes"), false, TabGroup.ModerationSettings, false)
                .SetParent(EnableKickLowLevelPlayer);
            EnableKickHighLevelPlayer = BooleanOptionItem.Create(60053, Translator.Get("enableKickHighLevelPlayer"), false, TabGroup.ModerationSettings, false);
            KickHighLevelPlayer = IntegerOptionItem.Create(60054, Translator.Get("kickHighLevelPlayer"), new(100, 5000, 50), 1000, TabGroup.ModerationSettings, false)
                .SetParent(EnableKickHighLevelPlayer)
                .SetValueFormat(OptionFormat.Level);
            TempBanHighLevelPlayer = BooleanOptionItem.Create(60055, Translator.Get("tempBanLowLevelPlayer"), false, TabGroup.ModerationSettings, false)
                .SetParent(EnableKickHighLevelPlayer);

            TabGroupKickInvalidFriendCodes = TextOptionItem.Create(60079, Translator.Get("tabGroupKickInvalidFriendCodes"), TabGroup.ModerationSettings)
                .SetColor(Color.red);
            KickInvalidFriendCodes = BooleanOptionItem.Create(60080, Translator.Get("kickInvalidFriendCodes"), true, TabGroup.ModerationSettings, false);
            TempBanInvalidFriendCodes = BooleanOptionItem.Create(60081, Translator.Get("tempBanInvalidFriendCodes"), false, TabGroup.ModerationSettings, false)
                .SetParent(KickInvalidFriendCodes);

            TabGroupDeniedWords = TextOptionItem.Create(60082, Translator.Get("tabGroupDeniedWords"), TabGroup.ModerationSettings)
                .SetColor(Color.red);
            EnableDeniedWords = BooleanOptionItem.Create(60083, Translator.Get("enableDeniedWords"), false, TabGroup.ModerationSettings, false);
            DeniedWordsAsBan = BooleanOptionItem.Create(60084, Translator.Get("autoKickStartAsBan"), false, TabGroup.ModerationSettings, false)
                .SetParent(EnableDeniedWords);
            DeniedWordsToKick = IntegerOptionItem.Create(60085, Translator.Get("deniedWordsToKick"), new(1, 5, 1), 1, TabGroup.ModerationSettings, false)
                .SetParent(EnableDeniedWords)
                .SetValueFormat(OptionFormat.Level);
            DeniedWordsStrength = BooleanOptionItem.Create(60086, Translator.Get("autoKickStartStrength"), false, TabGroup.ModerationSettings, false)
                .SetParent(EnableDeniedWords);

            TabGroupAutoKickStart = TextOptionItem.Create(600122, Translator.Get("tabGroupAutoKickStart"), TabGroup.ModerationSettings)
                .SetColor(Color.red);
            AutoKickStart = BooleanOptionItem.Create(60123, Translator.Get("autoKickStart"), false, TabGroup.ModerationSettings, false);
            AutoKickStartAsBan = BooleanOptionItem.Create(60124, Translator.Get("autoKickStartAsBan"), false, TabGroup.ModerationSettings, false)
                .SetParent(AutoKickStart);
            AutoKickStartTimes = IntegerOptionItem.Create(60125, Translator.Get("autoKickStartTimes"), new(1, 10, 1), 1, TabGroup.ModerationSettings, false)
                .SetParent(AutoKickStart)
                .SetValueFormat(OptionFormat.Times);
            AutoKickStartStrength = BooleanOptionItem.Create(60126, Translator.Get("autoKickStartStrength"), false, TabGroup.ModerationSettings, false)
                .SetParent(AutoKickStart);

            TabGroupAccess = TextOptionItem.Create(60127, Translator.Get("tabGroupAccess"), TabGroup.ModerationSettings)
                .SetColor(Color.red);
            SlashColorCmd = StringOptionItem.Create(60128, Translator.Get("slashColorCmd"), accessLevels, 1, TabGroup.ModerationSettings, false);
            SlashRolesAndGamemodeCmd = StringOptionItem.Create(60129, Translator.Get("slashRolesAndGamemodeCmd"), accessLevels, 1, TabGroup.ModerationSettings, false);
            SlashLastGameCmd = StringOptionItem.Create(60130, Translator.Get("slashLastGameCmd"), accessLevels, 1, TabGroup.ModerationSettings, false);
            SlashMapAndSeekerCmd = StringOptionItem.Create(60131, Translator.Get("slashMapAndSeekerCmd"), accessLevels, 1, TabGroup.ModerationSettings, false);
            SlashKickCmd = StringOptionItem.Create(60132, Translator.Get("slashKickCmd"), accessLevels, 2, TabGroup.ModerationSettings, false);
            SlashBanCmd = StringOptionItem.Create(60133, Translator.Get("slashBanCmd"), accessLevels, 2, TabGroup.ModerationSettings, false);
            SlashEndMeetingCmd = StringOptionItem.Create(60134, Translator.Get("slashEndMeetingCmd"), accessLevels, 3, TabGroup.ModerationSettings, false);
            SlashStartAndEndGameCmd = StringOptionItem.Create(60135, Translator.Get("slashStartAndEndGameCmd"), accessLevels, 3, TabGroup.ModerationSettings, false);

            TabGroupAutomation = TextOptionItem.Create(60149, Translator.Get("tabGroupAutomation"), TabGroup.SystemSettings)
                .SetColor(Color.yellow);
            AutoSendGameInfo = BooleanOptionItem.Create(60150, Translator.Get("autoSendGameInfo"), true, TabGroup.SystemSettings, false);
            AutoRejoinLobby = BooleanOptionItem.Create(60151, Translator.Get("autoRejoinLobby"), false, TabGroup.SystemSettings, false);
            EnableJoinMessages = BooleanOptionItem.Create(60152, Translator.Get("enableJoinMessages"), true, TabGroup.SystemSettings, false);
            MessagePerPlayerNum = IntegerOptionItem.Create(60153, Translator.Get("messagePerPlayerNum"), new(1, 15, 1), 5, TabGroup.SystemSettings, false)
                .SetParent(EnableJoinMessages);
            AutomaticMapPoll = BooleanOptionItem.Create(60154, Translator.Get("automaticMapPoll"), false, TabGroup.SystemSettings, false);
            TimeBeforeMapPoll = IntegerOptionItem.Create(60155, Translator.Get("timeBeforeMapPoll"), new(10, 300, 5), 10, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(AutomaticMapPoll);
            AutomaticAllMapPoll = BooleanOptionItem.Create(60156, Translator.Get("automaticAllMapPoll"), false, TabGroup.SystemSettings, false);
            TimeBeforeAllMapPoll = IntegerOptionItem.Create(60157, Translator.Get("timeBeforeMapPoll"), new(10, 300, 5), 10, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(AutomaticAllMapPoll);

            TabGroupAutostart = TextOptionItem.Create(64425, Translator.Get("tabGroupAutostart"), TabGroup.SystemSettings)
                .SetColor(Color.yellow);
            AutoStartTimer = IntegerOptionItem.Create(64426, Translator.Get("autoStartTimer"), new(1, 5, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            WaitAutoStart = IntegerOptionItem.Create(64427, Translator.Get("waitAutoStart"), new(10, 600, 10), 300, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            PlayerAutoStart = IntegerOptionItem.Create(64428, Translator.Get("playerAutoStart"), new(1, 15, 1), 1, TabGroup.SystemSettings, false);
            Rehost = BooleanOptionItem.Create(64429, Translator.Get("rehost"), false, TabGroup.SystemSettings, false);
            Public = BooleanOptionItem.Create(64430, Translator.Get("public"), false, TabGroup.SystemSettings, false)
                .SetParent(Rehost);

            TabGroupAfk = TextOptionItem.Create(60370, Translator.Get("tabGroupAfk"), TabGroup.SystemSettings)
                .SetColor(Color.yellow);
            EnableAfkDetection = BooleanOptionItem.Create(60371, Translator.Get("enableAfkDetection"), false, TabGroup.SystemSettings, false);
            AfkTimer = IntegerOptionItem.Create(60372, Translator.Get("afkTimer"), new(15, 120, 5), 30, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetParent(EnableAfkDetection);
            AfkPenalty = StringOptionItem.Create(60373, Translator.Get("afkPenalty"), afkPenalties, 0, TabGroup.SystemSettings, false)
                .SetParent(EnableAfkDetection);
            OnlyDetectSpawn = BooleanOptionItem.Create(60374, Translator.Get("onlyDetectSpawn"), true, TabGroup.SystemSettings, false)
                .SetParent(EnableAfkDetection);

            TabGroupMisc = TextOptionItem.Create(60378, Translator.Get("tabGroupMisc"), TabGroup.SystemSettings)
                .SetColor(Color.yellow);
            MapVoteDuration = IntegerOptionItem.Create(60379, Translator.Get("mapVoteDuration"), new(5, 60, 5), 20, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetColor(Color.red);
            StartCountdown = IntegerOptionItem.Create(60380, Translator.Get("startCountdown"), new(1, 5, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds)
                .SetColor(Color.red);
            AllowFortegreen = BooleanOptionItem.Create(60382, Translator.Get("allowFortegreen"), false, TabGroup.SystemSettings, false)
                .SetColor(Color.red);
            NoGameEnd = BooleanOptionItem.Create(60383, Translator.Get("noGameEnd"), false, TabGroup.SystemSettings, false)
                .SetColor(Color.red);

            // Gamemode Settings

            TabGroupHNS = TextOptionItem.Create(70000, Translator.Get("tabGroupHNS"), TabGroup.GamemodeSettings)
                .SetColor(Color.green);
            NumSeekers = IntegerOptionItem.Create(70001, Translator.Get("numSeekers"), new(1, 15, 1), 1, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Level);

            TabGroupSNS = TextOptionItem.Create(70050, Translator.Get("tabGroupSNS"), TabGroup.GamemodeSettings)
                .SetColor(Color.green);
            SNSSettingsOverride = BooleanOptionItem.Create(70051, Translator.Get("snsSettingsOverride"), true, TabGroup.GamemodeSettings, false);
            SNSChatInGame = BooleanOptionItem.Create(70063, Translator.Get("snsChatInGame"), false, TabGroup.GamemodeSettings, false);
            SNSChatInGameExtend = BooleanOptionItem.Create(70064, Translator.Get("snsChatInGameExtend"), false, TabGroup.GamemodeSettings, false)
                .SetParent(SNSChatInGame);
            CantKillTime = IntegerOptionItem.Create(70053, Translator.Get("cantKillTime"), new(0, 60, 5), 20, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            MisfiresToSuicide = IntegerOptionItem.Create(70052, Translator.Get("misfiresToSuicide"), new(1, 10, 1), 2, TabGroup.GamemodeSettings, false);
            CrewAutoWinsGameAfter = IntegerOptionItem.Create(70054, Translator.Get("crewAutoWinsGameAfter"), new(0, 600, 10), 300, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            SNSDisableSabotage = BooleanOptionItem.Create(70055, Translator.Get("snsDisableSabotage"), true, TabGroup.GamemodeSettings, false)
                .SetColor(Color.red);
            SNSDisableReactor = BooleanOptionItem.Create(70056, Translator.Get("snsDisableReactor"), true, TabGroup.GamemodeSettings, false)
                .SetParent(SNSDisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            SNSDisableOxygen = BooleanOptionItem.Create(70057, Translator.Get("snsDisableOxygen"), true, TabGroup.GamemodeSettings, false)
                .SetParent(SNSDisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            SNSDisableLights = BooleanOptionItem.Create(70058, Translator.Get("snsDisableLights"), true, TabGroup.GamemodeSettings, false)
                .SetParent(SNSDisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            SNSDisableComms = BooleanOptionItem.Create(70059, Translator.Get("snsDisableComms"), true, TabGroup.GamemodeSettings, false)
                .SetParent(SNSDisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            SNSDisableHeli = BooleanOptionItem.Create(70060, Translator.Get("snsDisableHeli"), true, TabGroup.GamemodeSettings, false)
                .SetParent(SNSDisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            SNSDisableMushroomMixup = BooleanOptionItem.Create(70061, Translator.Get("snsDisableMushroomMixup"), true, TabGroup.GamemodeSettings, false)
                .SetParent(SNSDisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            SNSDisableCloseDoor = BooleanOptionItem.Create(70062, Translator.Get("snsDisableCloseDoor"), true, TabGroup.GamemodeSettings, false)
                .SetColor(Color.red);

            TabGroupSpeedrun = TextOptionItem.Create(70075, Translator.Get("tabGroupSpeedrun"), TabGroup.GamemodeSettings)
                .SetColor(Color.green);
            SpeedrunSettingsOverride = BooleanOptionItem.Create(70076, Translator.Get("speedrunSettingsOverride"), true, TabGroup.GamemodeSettings, false);
            GameAutoEndsAfter = IntegerOptionItem.Create(70077, Translator.Get("gameAutoEndsAfter"), new(0, 600, 10), 300, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            EngineerMode = BooleanOptionItem.Create(70078, Translator.Get("engineerMode"), false, TabGroup.GamemodeSettings, false);

            // Gameplay Settings
            TabGroupSabotages = TextOptionItem.Create(60450, Translator.Get("tabGroupSabotages"), TabGroup.ModSettings)
                .SetColor(Color.blue);
            DeadImpostorsCanSabotage = BooleanOptionItem.Create(60455, Translator.Get("deadImpostorsCanSabotage"), true, TabGroup.ModSettings, false)
                .SetColor(Color.red);
            DisableSabotage = BooleanOptionItem.Create(60456, Translator.Get("disableSabotage"), false, TabGroup.ModSettings, false)
                .SetColor(Color.red);
            DisableReactor = BooleanOptionItem.Create(60457, Translator.Get("disableReactor"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableOxygen = BooleanOptionItem.Create(60458, Translator.Get("disableOxygen"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableLights = BooleanOptionItem.Create(60459, Translator.Get("disableLights"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableComms = BooleanOptionItem.Create(60460, Translator.Get("disableComms"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableHeli = BooleanOptionItem.Create(60461, Translator.Get("disableHeli"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableMushroomMixup = BooleanOptionItem.Create(60462, Translator.Get("disableMushroomMixup"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSabotage)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableCloseDoor = BooleanOptionItem.Create(60566, Translator.Get("disableCloseDoor"), false, TabGroup.ModSettings, false)
                .SetColor(Color.red);

            TabGroupGameplayGeneral = TextOptionItem.Create(60564, Translator.Get("tabGroupGameplayGeneral"), TabGroup.ModSettings)
                .SetColor(Color.blue);
            DisableAnnoyingMeetingCalls = BooleanOptionItem.Create(60565, Translator.Get("disableAnnoyingMeetingCalls"), false, TabGroup.ModSettings, false);

            ChangeDecontaminationTime = BooleanOptionItem.Create(60550, Translator.Get("changeDecontaminationTime"), false, TabGroup.ModSettings, false);
            DecontaminationTimeOnMiraHQ = FloatOptionItem.Create(60551, Translator.Get("decontaminationTimeOnMiraHQ"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds);
            DecontaminationDoorOpenTimeOnMiraHQ = FloatOptionItem.Create(60552, Translator.Get("decontaminationDoorOpenTimeOnMiraHQ"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds);
            DecontaminationTimeOnPolus = FloatOptionItem.Create(60553, Translator.Get("decontaminationTimeOnPolus"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds);
            DecontaminationDoorOpenTimeOnPolus = FloatOptionItem.Create(60554, Translator.Get("decontaminationDoorOpenTimeOnPolus"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds);

            DisableSporeTrigger = BooleanOptionItem.Create(60558, Translator.Get("disableSporeTrigger"), false, TabGroup.ModSettings, false);

            DisableDevices = BooleanOptionItem.Create(22900, Translator.Get("disableDevices"), false, TabGroup.ModSettings, false);

            DisableSkeldDevices = BooleanOptionItem.Create(22905, Translator.Get("disableSkeldDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableSkeldAdmin = BooleanOptionItem.Create(22906, Translator.Get("disableSkeldAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSkeldDevices);
            DisableSkeldCamera = BooleanOptionItem.Create(22907, Translator.Get("disableSkeldCamera"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSkeldDevices);

            DisableMiraHQDevices = BooleanOptionItem.Create(22908, Translator.Get("disableMiraHQDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableMiraHQAdmin = BooleanOptionItem.Create(22909, Translator.Get("disableMiraHQAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraHQDevices);
            DisableMiraHQDoorLog = BooleanOptionItem.Create(22910, Translator.Get("disableMiraHQDoorLog"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraHQDevices);

            DisablePolusDevices = BooleanOptionItem.Create(22911, Translator.Get("disablePolusDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisablePolusAdmin = BooleanOptionItem.Create(22912, Translator.Get("disablePolusAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusDevices);
            DisablePolusCamera = BooleanOptionItem.Create(22913, Translator.Get("disablePolusCamera"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusDevices);
            DisablePolusVital = BooleanOptionItem.Create(22914, Translator.Get("disablePolusVital"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusDevices);

            DisableAirshipDevices = BooleanOptionItem.Create(22915, Translator.Get("disableAirshipDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableAirshipCockpitAdmin = BooleanOptionItem.Create(22916, Translator.Get("disableAirshipCockpitAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices);
            DisableAirshipRecordsAdmin = BooleanOptionItem.Create(22917, Translator.Get("disableAirshipRecordsAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices);
            DisableAirshipCamera = BooleanOptionItem.Create(22918, Translator.Get("disableAirshipCamera"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices);
            DisableAirshipVital = BooleanOptionItem.Create(22919, Translator.Get("disableAirshipVital"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices);

            DisableFungleDevices = BooleanOptionItem.Create(22925, Translator.Get("disableFungleDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisableFungleVital = BooleanOptionItem.Create(22927, Translator.Get("disableFungleVital"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleDevices);

            TabGroupTasks = TextOptionItem.Create(22995, Translator.Get("tabGroupTasks"), TabGroup.ModSettings)
                .SetColor(Color.blue);
            TaskPercentNeededToWin = IntegerOptionItem.Create(22996, Translator.Get("taskPercentNeededToWin"), new(30, 100, 5), 100, TabGroup.ModSettings, false)
                .SetValueFormat(OptionFormat.Percent);
            AllPlayersSameTasks = BooleanOptionItem.Create(22998, Translator.Get("allPlayersSameTasks"), false, TabGroup.ModSettings, false);
            OverrideTaskSettings = BooleanOptionItem.Create(22999, Translator.Get("overrideTaskSettings"), false, TabGroup.ModSettings, false);

            DisableMiraTasks = BooleanOptionItem.Create(23000, Translator.Get("disableMiraTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableBuyBeverage = BooleanOptionItem.Create(23001, Translator.Get("disableBuyBeverage"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableMeasureWeather = BooleanOptionItem.Create(23002, Translator.Get("disableMeasureWeather"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableProcessData = BooleanOptionItem.Create(23003, Translator.Get("disableProcessData"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableRunDiagnostics = BooleanOptionItem.Create(23004, Translator.Get("disableRunDiagnostics"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableSortSamples = BooleanOptionItem.Create(23005, Translator.Get("disableSortSamples"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);

            DisablePolusTasks = BooleanOptionItem.Create(23100, Translator.Get("disablePolusTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableActivateWeatherNodes = BooleanOptionItem.Create(23101, Translator.Get("disableActivateWeatherNodes"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableAlignTelescope = BooleanOptionItem.Create(23102, Translator.Get("disableAlignTelescope"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableFillCanisters = BooleanOptionItem.Create(23103, Translator.Get("disableFillCanisters"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableInsertKeys = BooleanOptionItem.Create(23104, Translator.Get("disableInsertKeys"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableOpenWaterways = BooleanOptionItem.Create(23105, Translator.Get("disableOpenWaterways"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableRebootWifi = BooleanOptionItem.Create(23106, Translator.Get("disableRebootWifi"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableRepairDrill = BooleanOptionItem.Create(23107, Translator.Get("disableRepairDrill"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableScanBoardingPass = BooleanOptionItem.Create(23108, Translator.Get("disableScanBoardingPass"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);

            DisableAirshipTasks = BooleanOptionItem.Create(23200, Translator.Get("disableAirshipTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableCleanToilet = BooleanOptionItem.Create(23201, Translator.Get("disableCleanToilet"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableDecontaminate = BooleanOptionItem.Create(23202, Translator.Get("disableDecontaminate"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableDevelopPhotos = BooleanOptionItem.Create(23203, Translator.Get("disableDevelopPhotos"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableDressMannequin = BooleanOptionItem.Create(23204, Translator.Get("disableDressMannequin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableFixShower = BooleanOptionItem.Create(23205, Translator.Get("disableFixShower"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableMakeBurger = BooleanOptionItem.Create(23206, Translator.Get("disableMakeBurger"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePickUpTowels = BooleanOptionItem.Create(23207, Translator.Get("disablePickUpTowels"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePolishRuby = BooleanOptionItem.Create(23208, Translator.Get("disablePolishRuby"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePutAwayPistols = BooleanOptionItem.Create(23209, Translator.Get("disablePutAwayPistols"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePutAwayRifles = BooleanOptionItem.Create(23210, Translator.Get("disablePutAwayRifles"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableRewindTapes = BooleanOptionItem.Create(23211, Translator.Get("disableRewindTapes"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableResetBreaker = BooleanOptionItem.Create(23212, Translator.Get("disableResetBreaker"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableSortRecords = BooleanOptionItem.Create(23213, Translator.Get("disableSortRecords"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableStartFans = BooleanOptionItem.Create(23214, Translator.Get("disableStartFans"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableUnlockSafe = BooleanOptionItem.Create(23215, Translator.Get("disableUnlockSafe"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);



            DisableFungleTasks = BooleanOptionItem.Create(23300, Translator.Get("disableFungleTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableBuildSandcastle = BooleanOptionItem.Create(23301, Translator.Get("disableBuildSandcastle"), false, TabGroup.ModSettings, false)
               .SetParent(DisableFungleTasks);
            DisableCatchFish = BooleanOptionItem.Create(23302, Translator.Get("disableCatchFish"), false, TabGroup.ModSettings, false)
               .SetParent(DisableFungleTasks);
            DisableCollectSamples = BooleanOptionItem.Create(23303, Translator.Get("disableCollectSamples"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableCollectShells = BooleanOptionItem.Create(23304, Translator.Get("disableCollectShells"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableCollectVegetables = BooleanOptionItem.Create(23305, Translator.Get("disableCollectVegetables"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableCrankGenerator = BooleanOptionItem.Create(23306, Translator.Get("disableCrankGenerator"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableExtractFuel = BooleanOptionItem.Create(23307, Translator.Get("disableExtractFuel"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableFixAntenna = BooleanOptionItem.Create(23308, Translator.Get("disableFixAntenna"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableHelpCritter = BooleanOptionItem.Create(23309, Translator.Get("disableHelpCritter"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableHoistSupplies = BooleanOptionItem.Create(23310, Translator.Get("disableHoistSupplies"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableLiftWeights = BooleanOptionItem.Create(23311, Translator.Get("disableLiftWeights"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableMineOres = BooleanOptionItem.Create(23312, Translator.Get("disableMineOres"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisablePlayVideoGame = BooleanOptionItem.Create(23313, Translator.Get("disablePlayVideoGame"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisablePolishGem = BooleanOptionItem.Create(23314, Translator.Get("disablePolishGem"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableReplaceParts = BooleanOptionItem.Create(23315, Translator.Get("disableReplaceParts"), false, TabGroup.ModSettings, false)
               .SetParent(DisableFungleTasks);
            DisableRoastMarshmallow = BooleanOptionItem.Create(23316, Translator.Get("disableRoastMarshmallow"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableThrowFisbee = BooleanOptionItem.Create(23317, Translator.Get("disableThrowFisbee"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableFindSignal = BooleanOptionItem.Create(23318, Translator.Get("disableFindSignal"), false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);

            DisableMiscCommonTasks = BooleanOptionItem.Create(23400, Translator.Get("disableMiscCommonTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableEnterIdCode = BooleanOptionItem.Create(23401, Translator.Get("disableEnterIdCode"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscCommonTasks);
            DisableFixWiring = BooleanOptionItem.Create(23402, Translator.Get("disableFixWiring"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscCommonTasks);
            DisableSwipeCard = BooleanOptionItem.Create(23403, Translator.Get("disableSwipeCard"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscCommonTasks);

            DisableMiscShortTasks = BooleanOptionItem.Create(23500, Translator.Get("disableMiscShortTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableAssembleArtifact = BooleanOptionItem.Create(23501, Translator.Get("disableAssembleArtifact"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableChartCourse = BooleanOptionItem.Create(23502, Translator.Get("disableChartCourse"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableCleanO2Filter = BooleanOptionItem.Create(23503, Translator.Get("disableCleanO2Filter"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableCleanVent = BooleanOptionItem.Create(23504, Translator.Get("disableCleanVent"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableMonitorMushroom = BooleanOptionItem.Create(23506, Translator.Get("disableMonitorMushroom"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableMonitorTree = BooleanOptionItem.Create(23507, Translator.Get("disableMonitorTree"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisablePrimeShields = BooleanOptionItem.Create(23508, Translator.Get("disablePrimeShields"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableRecordTemperature = BooleanOptionItem.Create(23509, Translator.Get("disableRecordTemperature"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableStabilizeSteering = BooleanOptionItem.Create(23510, Translator.Get("disableStabilizeSteering"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableStoreArtifacts = BooleanOptionItem.Create(23511, Translator.Get("disableStoreArtifacts"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableUnlockManifolds = BooleanOptionItem.Create(23512, Translator.Get("disableUnlockManifolds"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);

            DisableMiscLongTasks = BooleanOptionItem.Create(23600, Translator.Get("disableMiscLongTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableAlignEngineOutput = BooleanOptionItem.Create(23601, Translator.Get("disableAlignEngineOutput"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableClearAsteroids = BooleanOptionItem.Create(23602, Translator.Get("disableClearAsteroids"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableEmptyChute = BooleanOptionItem.Create(23603, Translator.Get("disableEmptyChute"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableInspectSample = BooleanOptionItem.Create(23604, Translator.Get("disableInspectSample"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableReplaceWaterJug = BooleanOptionItem.Create(23605, Translator.Get("disableReplaceWaterJug"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableStartReactor = BooleanOptionItem.Create(23606, Translator.Get("disableStartReactor"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableWaterPlants = BooleanOptionItem.Create(23607, Translator.Get("disableWaterPlants"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);



            DisableMiscMixedTasks = BooleanOptionItem.Create(23700, Translator.Get("disableMiscMixedTasks"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableCalibrateDistributor = BooleanOptionItem.Create(23701, Translator.Get("disableCalibrateDistributor"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableDivertPower = BooleanOptionItem.Create(23702, Translator.Get("disableDivertPower"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableEmptyGarbage = BooleanOptionItem.Create(23703, Translator.Get("disableEmptyGarbage"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableFuelEngines = BooleanOptionItem.Create(23704, Translator.Get("disableFuelEngines"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableSubmitScan = BooleanOptionItem.Create(23705, Translator.Get("disableSubmitScan"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableUploadData = BooleanOptionItem.Create(23706, Translator.Get("disableUploadData"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);

            IsLoaded = true;
        }
    }
}