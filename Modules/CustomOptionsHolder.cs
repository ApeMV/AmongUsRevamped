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

        //[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        //public static void WaitOptionsLoad()
        //{
            //taskOptionsLoad.Wait();
        //}

        public const int PresetId = 0;

        private static readonly string[] presets =
        {
            Main.Preset1.Value, Main.Preset2.Value, Main.Preset3.Value,
            Main.Preset4.Value, Main.Preset5.Value
        };

        public static readonly string[] gameModes =
        {
            "None", "0 Kill Cooldown", "Shift And Seek", "Speedrun"
        };

        public static readonly string[] colorLevels =
        {
            "Moderators", "Everyone", "Nobody"
        };

        //System
        public static OptionItem Gamemode;

        public static OptionItem TabGroupMain;

        public static OptionItem KickLowLevelPlayer;
        public static OptionItem TempBanLowLevelPlayer;

        public static OptionItem KickInvalidFriendCodes;
        public static OptionItem TempBanInvalidFriendCodes;

        public static OptionItem ApplyBanList;
        public static OptionItem ApplyDenyNameList;
        public static OptionItem ApplyModeratorList;
        public static OptionItem ModeratorCanUseCommand;

        public static OptionItem AutoKickStart;
        public static OptionItem AutoKickStartAsBan;
        public static OptionItem AutoKickStartTimes;
        public static OptionItem AutoKickStartStrength;

        public static OptionItem TabGroupAutomation;

        public static OptionItem AutoSendGameInfo;
        public static OptionItem AutoRejoinLobby;
        public static OptionItem AutoStartTimer;
        public static OptionItem WaitAutoStart;
        public static OptionItem PlayerAutoStart;

        public static OptionItem TabGroupMisc;

        public static OptionItem StartCountdown;
        public static OptionItem ColorCommandLevel;
        public static OptionItem AllowFortegreen;
        public static OptionItem NoGameEnd;
        
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
        public static OptionItem DisableReport;

        public static OptionItem ChangeDecontaminationTime;
        public static OptionItem DecontaminationTimeOnMiraHQ;
        public static OptionItem DecontaminationDoorOpenTimeOnMiraHQ;
        public static OptionItem DecontaminationTimeOnPolus;
        public static OptionItem DecontaminationDoorOpenTimeOnPolus;

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

        // Gamemode
        public static OptionItem TabGroupHNS;
        public static OptionItem NumSeekers;

        public static OptionItem TabGroup0Kcd;
        public static OptionItem NoKcdSettingsOverride;

        public static OptionItem TabGroupSNS;
        public static OptionItem SNSSettingsOverride;
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

        // Roles
        public static OptionItem TabGroupCrewmate;
        public static OptionItem MayorPerc;

        public static OptionItem TabGroupNeutral;
        public static OptionItem JesterPerc;        

        public static OptionItem TabGroupImpostor;

        public static bool IsLoaded = false;

        public static void Load()
        {
            if (IsLoaded) return;

            _ = PresetOptionItem.Create(0, TabGroup.SystemSettings)
                .SetColor(new Color32(204, 204, 0, 255))
                .SetHeader(true);

            Gamemode = StringOptionItem.Create(1, Translator.Get("gamemode"), gameModes, 0, TabGroup.SystemSettings, false)
                .SetColor(Color.green)
                .SetHeader(true);

            TabGroupMain = TextOptionItem.Create(60000, Translator.Get("tabGroupMain"), TabGroup.SystemSettings)
                .SetColor(Color.blue);

            KickLowLevelPlayer = IntegerOptionItem.Create(60050, Translator.Get("kickLowLevelPlayer"), new(0, 100, 1), 0, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Level);
            TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, Translator.Get("tempBanLowLevelPlayer"), false, TabGroup.SystemSettings, false)
                .SetParent(KickLowLevelPlayer);

            KickInvalidFriendCodes = BooleanOptionItem.Create(60080, Translator.Get("kickInvalidFriendCodes"), true, TabGroup.SystemSettings, false);
            TempBanInvalidFriendCodes = BooleanOptionItem.Create(60081, Translator.Get("tempBanInvalidFriendCodes"), false, TabGroup.SystemSettings, false)
                .SetParent(KickInvalidFriendCodes);

            ApplyBanList = BooleanOptionItem.Create(60110, Translator.Get("applyBanList"), true, TabGroup.SystemSettings, true);
            ApplyDenyNameList = BooleanOptionItem.Create(60120, Translator.Get("applyDenyNameList"), true, TabGroup.SystemSettings, false);
            ApplyModeratorList = BooleanOptionItem.Create(60121, Translator.Get("applyModeratorList"), true, TabGroup.SystemSettings, false);
            ModeratorCanUseCommand = BooleanOptionItem.Create(60122, Translator.Get("moderatorCanUseCommand"), true, TabGroup.SystemSettings, false);

            AutoKickStart = BooleanOptionItem.Create(60123, Translator.Get("autoKickStart"), false, TabGroup.SystemSettings, false);
            AutoKickStartAsBan = BooleanOptionItem.Create(60124, Translator.Get("autoKickStartAsBan"), false, TabGroup.SystemSettings, false)
                .SetParent(AutoKickStart);
            AutoKickStartTimes = IntegerOptionItem.Create(60125, Translator.Get("autoKickStartTimes"), new(1, 10, 1), 1, TabGroup.SystemSettings, false)
                .SetParent(AutoKickStart)
                .SetValueFormat(OptionFormat.Times);
            AutoKickStartStrength = BooleanOptionItem.Create(60126, Translator.Get("autoKickStartStrength"), false, TabGroup.SystemSettings, false)
                .SetParent(AutoKickStart);

            TabGroupAutomation = TextOptionItem.Create(60149, Translator.Get("tabGroupAutomation"), TabGroup.SystemSettings)
                .SetColor(Color.yellow);

            AutoSendGameInfo = BooleanOptionItem.Create(60150, Translator.Get("autoSendGameInfo"), true, TabGroup.SystemSettings, false);
            AutoRejoinLobby = BooleanOptionItem.Create(60210, Translator.Get("autoRejoinLobby"), false, TabGroup.SystemSettings, false);
            AutoStartTimer = IntegerOptionItem.Create(64420, Translator.Get("autoStartTimer"), new(1, 600, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            WaitAutoStart = IntegerOptionItem.Create(64421, Translator.Get("waitAutoStart"), new(10, 600, 10), 300, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            PlayerAutoStart = IntegerOptionItem.Create(64422, Translator.Get("playerAutoStart"), new(1, 15, 1), 1, TabGroup.SystemSettings, false);

            TabGroupMisc = TextOptionItem.Create(60379, Translator.Get("tabGroupMisc"), TabGroup.SystemSettings)
                .SetColor(Color.green);

            StartCountdown = IntegerOptionItem.Create(60380, Translator.Get("startCountdown"), new(1, 600, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            ColorCommandLevel = StringOptionItem.Create(60381, Translator.Get("colorCommandLevel"), colorLevels, 0, TabGroup.SystemSettings, false);
            AllowFortegreen = BooleanOptionItem.Create(60382, Translator.Get("allowFortegreen"), false, TabGroup.SystemSettings, false);
            NoGameEnd = BooleanOptionItem.Create(60383, Translator.Get("noGameEnd"), false, TabGroup.SystemSettings, false)
                .SetColor(Color.red);




            // Custom role settings
            TabGroupCrewmate = TextOptionItem.Create(100000, Translator.Get("tabGroupCrewmate"), TabGroup.CustomRoleSettings)
                .SetColor(CL.Hex("#8cffff"));
            MayorPerc = IntegerOptionItem.Create(100001, Translator.Get("mayorPerc"), new(0, 100, 5), 0, TabGroup.CustomRoleSettings, false)
                .SetValueFormat(OptionFormat.Percent)
                .SetColor(CL.Hex("#204d42"));

            TabGroupNeutral = TextOptionItem.Create(101000, Translator.Get("tabGroupNeutral"), TabGroup.CustomRoleSettings)
                .SetColor(CL.Hex("#FFFF99"));
            JesterPerc = IntegerOptionItem.Create(101001, Translator.Get("jesterPerc"), new(0, 100, 5), 0, TabGroup.CustomRoleSettings, false)
                .SetValueFormat(OptionFormat.Percent)
                .SetColor(CL.Hex("#ec62a5"));

            TabGroupImpostor = TextOptionItem.Create(102000, Translator.Get("tabGroupImpostor"), TabGroup.CustomRoleSettings)
                .SetColor(CL.Hex("#ff1919"));

            // Gamemode Settings
            TabGroupHNS = TextOptionItem.Create(70000, Translator.Get("tabGroupHNS"), TabGroup.GamemodeSettings)
                .SetColor(Color.green);
            NumSeekers = IntegerOptionItem.Create(70001, Translator.Get("numSeekers"), new(1, 15, 1), 1, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Level);

            TabGroup0Kcd = TextOptionItem.Create(70025, Translator.Get("tabGroup0Kcd"), TabGroup.GamemodeSettings)
                .SetColor(Color.red);
            NoKcdSettingsOverride = BooleanOptionItem.Create(70026, Translator.Get("noKcdSettingsOverride"), true, TabGroup.GamemodeSettings, false);

            TabGroupSNS = TextOptionItem.Create(70050, Translator.Get("tabGroupSNS"), TabGroup.GamemodeSettings)
                .SetColor(Color.yellow);
            SNSSettingsOverride = BooleanOptionItem.Create(70051, Translator.Get("snsSettingsOverride"), true, TabGroup.GamemodeSettings, false);
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
                .SetColor(Color.blue);
            SpeedrunSettingsOverride = BooleanOptionItem.Create(70076, Translator.Get("speedrunSettingsOverride"), true, TabGroup.GamemodeSettings, false);
            GameAutoEndsAfter = IntegerOptionItem.Create(70077, Translator.Get("gameAutoEndsAfter"), new(0, 600, 10), 300, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);

            // Gameplay Settings
            TabGroupSabotages = TextOptionItem.Create(60450, Translator.Get("tabGroupSabotages"), TabGroup.ModSettings)
                .SetColor(Color.red);
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
            DisableAnnoyingMeetingCalls = BooleanOptionItem.Create(60565, Translator.Get("disableAnnoyingMeetingCalls"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));
            DisableReport = BooleanOptionItem.Create(60520, Translator.Get("disableReport"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));

            ChangeDecontaminationTime = BooleanOptionItem.Create(60550, Translator.Get("changeDecontaminationTime"), false, TabGroup.ModSettings, false)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));
            DecontaminationTimeOnMiraHQ = FloatOptionItem.Create(60551, Translator.Get("decontaminationTimeOnMiraHQ"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));
            DecontaminationDoorOpenTimeOnMiraHQ = FloatOptionItem.Create(60552, Translator.Get("decontaminationDoorOpenTimeOnMiraHQ"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));
            DecontaminationTimeOnPolus = FloatOptionItem.Create(60553, Translator.Get("decontaminationTimeOnPolus"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));
            DecontaminationDoorOpenTimeOnPolus = FloatOptionItem.Create(60554, Translator.Get("decontaminationDoorOpenTimeOnPolus"), new(0.5f, 10f, 0.25f), 3f, TabGroup.ModSettings, false)
                .SetParent(ChangeDecontaminationTime)
                .SetValueFormat(OptionFormat.Seconds)
                .SetColor(new Color32(19, 188, 233, byte.MaxValue));

            DisableDevices = BooleanOptionItem.Create(22900, Translator.Get("disableDevices"), false, TabGroup.ModSettings, false)
                .SetColor(Color.red);

            DisableSkeldDevices = BooleanOptionItem.Create(22905, Translator.Get("disableSkeldDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableSkeldAdmin = BooleanOptionItem.Create(22906, Translator.Get("disableSkeldAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSkeldDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableSkeldCamera = BooleanOptionItem.Create(22907, Translator.Get("disableSkeldCamera"), false, TabGroup.ModSettings, false)
                .SetParent(DisableSkeldDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisableMiraHQDevices = BooleanOptionItem.Create(22908, Translator.Get("disableMiraHQDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableMiraHQAdmin = BooleanOptionItem.Create(22909, Translator.Get("disableMiraHQAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraHQDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableMiraHQDoorLog = BooleanOptionItem.Create(22910, Translator.Get("disableMiraHQDoorLog"), false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraHQDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisablePolusDevices = BooleanOptionItem.Create(22911, Translator.Get("disablePolusDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisablePolusAdmin = BooleanOptionItem.Create(22912, Translator.Get("disablePolusAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisablePolusCamera = BooleanOptionItem.Create(22913, Translator.Get("disablePolusCamera"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisablePolusVital = BooleanOptionItem.Create(22914, Translator.Get("disablePolusVital"), false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisableAirshipDevices = BooleanOptionItem.Create(22915, Translator.Get("disableAirshipDevices"), false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableAirshipCockpitAdmin = BooleanOptionItem.Create(22916, Translator.Get("disableAirshipCockpitAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableAirshipRecordsAdmin = BooleanOptionItem.Create(22917, Translator.Get("disableAirshipRecordsAdmin"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableAirshipCamera = BooleanOptionItem.Create(22918, Translator.Get("disableAirshipCamera"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableAirshipVital = BooleanOptionItem.Create(22919, Translator.Get("disableAirshipVital"), false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisableFungleDevices = BooleanOptionItem.Create(22925, "Disable Fungle Devices", false, TabGroup.ModSettings, false)
                .SetParent(DisableDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisableFungleCamera = BooleanOptionItem.Create(22926, "Disable Fungle Camera", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));

            DisableFungleVital = BooleanOptionItem.Create(22927, "Disable Fungle Vitals", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleDevices)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));


            TabGroupTasks = TextOptionItem.Create(22997, "Tasks", TabGroup.ModSettings)
                .SetColor(Color.yellow);
            AllPlayersSameTasks = BooleanOptionItem.Create(22998, "Everyone has same tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(255, 255, 153, byte.MaxValue));
            OverrideTaskSettings = BooleanOptionItem.Create(22999, "<#ffd>Di<#ffb>sa<#ff9>bl<#ffb>e T<#ffd>as<#ffb>ks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(239, 89, 175, byte.MaxValue));

            DisableMiraTasks = BooleanOptionItem.Create(23000, "Disable Mira HQ Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableBuyBeverage = BooleanOptionItem.Create(23001, "Disable Buy Beverage", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableMeasureWeather = BooleanOptionItem.Create(23002, "Disable Measure Weather", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableProcessData = BooleanOptionItem.Create(23003, "Disable Process Data", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableRunDiagnostics = BooleanOptionItem.Create(23004, "Disable Run Diagnostics", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);
            DisableSortSamples = BooleanOptionItem.Create(23005, "Disable Sort Samples", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiraTasks);



            DisablePolusTasks = BooleanOptionItem.Create(23100, "Disable Polus Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableActivateWeatherNodes = BooleanOptionItem.Create(23101, "Disable Activate Weather Nodes", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableAlignTelescope = BooleanOptionItem.Create(23102, "Disable Align Telescope", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableFillCanisters = BooleanOptionItem.Create(23103, "Disable Fill Canisters", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableInsertKeys = BooleanOptionItem.Create(23104, "Disable Insert Keys", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableOpenWaterways = BooleanOptionItem.Create(23105, "Disable Open Waterways", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableRebootWifi = BooleanOptionItem.Create(23106, "Disable Reboot Wifi", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableRepairDrill = BooleanOptionItem.Create(23107, "Disable Repair Drill", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);
            DisableScanBoardingPass = BooleanOptionItem.Create(23108, "Disable Scan Boarding Pass", false, TabGroup.ModSettings, false)
                .SetParent(DisablePolusTasks);



            DisableAirshipTasks = BooleanOptionItem.Create(23200, "Disable Airship Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableCleanToilet = BooleanOptionItem.Create(23201, "Disable Clean Toilet", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableDecontaminate = BooleanOptionItem.Create(23202, "Disable Decontaminate", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableDevelopPhotos = BooleanOptionItem.Create(23203, "Disable Develop Photos", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableDressMannequin = BooleanOptionItem.Create(23204, "Disable Dress Mannequin", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableFixShower = BooleanOptionItem.Create(23205, "Disable Fix Shower", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableMakeBurger = BooleanOptionItem.Create(23206, "Disable Make Burger", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePickUpTowels = BooleanOptionItem.Create(23207, "Disable Pick Up Towels", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePolishRuby = BooleanOptionItem.Create(23208, "Disable Polish Ruby", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePutAwayPistols = BooleanOptionItem.Create(23209, "Disable Put Away Pistols", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisablePutAwayRifles = BooleanOptionItem.Create(23210, "Disable Put Away Rifles", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableRewindTapes = BooleanOptionItem.Create(23211, "Disable Rewind Tapes", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableResetBreaker = BooleanOptionItem.Create(23212, "Disable Reset Breaker Task", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableSortRecords = BooleanOptionItem.Create(23213, "Disable Sort Records", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableStartFans = BooleanOptionItem.Create(23214, "Disable Start Fans", false, TabGroup.ModSettings, false)
                .SetParent(DisableAirshipTasks);
            DisableUnlockSafe = BooleanOptionItem.Create(23215, "Disable Unlock Safe Task", false, TabGroup.ModSettings, false)
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