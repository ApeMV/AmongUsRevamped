using System.Threading.Tasks;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/OptionHolder.cs
namespace AmongUsRevamped
{
    [HarmonyPatch]
    public static class Options
    {
        static Task taskOptionsLoad;

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
        public static void OptionsLoadStart()
        {
            taskOptionsLoad = Task.Run(Load);
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
        public static void WaitOptionsLoad()
        {
            taskOptionsLoad.Wait();
        }

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
        public static OptionItem TabGroupGameplayGeneral;
        public static OptionItem DisableSabotage;
        public static OptionItem DisableCloseDoor;

        public static OptionItem DisableReport;

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

        public static OptionItem TabGroupSpeedrun;
        public static OptionItem SpeedrunSettingsOverride;
        public static OptionItem GameAutoEndsAfter;

        // Roles
        public static OptionItem TabGroupCrewmate;

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

            Gamemode = StringOptionItem.Create(1, "Gamemode", gameModes, 0, TabGroup.SystemSettings, false)
                .SetColor(Color.green)
                .SetHeader(true);

            TabGroupMain = TextOptionItem.Create(60000, "Moderation", TabGroup.SystemSettings)
                .SetColor(Color.blue);

            KickLowLevelPlayer = IntegerOptionItem.Create(60050, "Kick Players Under Level", new(0, 100, 1), 0, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Level);
            TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, "Ban Instead Of Kick", false, TabGroup.SystemSettings, false)
                .SetParent(KickLowLevelPlayer);

            KickInvalidFriendCodes = BooleanOptionItem.Create(60080, "Kick Invalid FriendCodes", true, TabGroup.SystemSettings, false);
            TempBanInvalidFriendCodes = BooleanOptionItem.Create(60081, "Ban Instead Of Kick", false, TabGroup.SystemSettings, false)
                .SetParent(KickInvalidFriendCodes);

            ApplyBanList = BooleanOptionItem.Create(60110, "Apply BanList", true, TabGroup.SystemSettings, true);
            ApplyDenyNameList = BooleanOptionItem.Create(60120, "Apply DenyName List", true, TabGroup.SystemSettings, false);
            ApplyModeratorList = BooleanOptionItem.Create(60121, "Apply Moderator List", true, TabGroup.SystemSettings, false);
            ModeratorCanUseCommand = BooleanOptionItem.Create(60122, "Moderators can use commands", true, TabGroup.SystemSettings, false);

            TabGroupAutomation = TextOptionItem.Create(60149, "Automation", TabGroup.SystemSettings)
                .SetColor(Color.yellow);

            AutoSendGameInfo = BooleanOptionItem.Create(60150, "Automatically send winner info", true, TabGroup.SystemSettings, false);
            AutoRejoinLobby = BooleanOptionItem.Create(60210, "Auto Rejoin Lobby", false, TabGroup.SystemSettings, false);
            AutoStartTimer = IntegerOptionItem.Create(64420, "Countdown For Auto Start", new(1, 600, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            WaitAutoStart = IntegerOptionItem.Create(64421, "Auto Start After", new(10, 600, 10), 300, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            PlayerAutoStart = IntegerOptionItem.Create(64422, "Players Needed To Auto Start", new(1, 15, 1), 1, TabGroup.SystemSettings, false);

            TabGroupMisc = TextOptionItem.Create(60379, "Miscelleneous", TabGroup.SystemSettings)
                .SetColor(Color.green);

            StartCountdown = IntegerOptionItem.Create(60380, "Default Starting Countdown", new(1, 600, 1), 5, TabGroup.SystemSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            ColorCommandLevel = StringOptionItem.Create(60381, "Who Can use Color Commands", colorLevels, 0, TabGroup.SystemSettings, false);
            AllowFortegreen = BooleanOptionItem.Create(60382, "Allow Fortegreen Color", false, TabGroup.SystemSettings, false);
            NoGameEnd = BooleanOptionItem.Create(60383, "No Game End", false, TabGroup.SystemSettings, false)
                .SetColor(Color.red);




            // Custom role settings
            TabGroupCrewmate = TextOptionItem.Create(100000, "Crewmate Roles", TabGroup.CustomRoleSettings)
                .SetColor(CL.Hex("#8cffff"));

            TabGroupNeutral = TextOptionItem.Create(101000, "Neutral Roles", TabGroup.CustomRoleSettings)
                .SetColor(CL.Hex("#FFFF99"));
            JesterPerc = IntegerOptionItem.Create(101001, "PLACEHOLDER", new(0, 100, 5), 0, TabGroup.CustomRoleSettings, false)
                .SetValueFormat(OptionFormat.Percent)
                .SetColor(CL.Hex("#ec62a5"));

            TabGroupImpostor = TextOptionItem.Create(102000, "Impostor Roles", TabGroup.CustomRoleSettings)
                .SetColor(CL.Hex("#ff1919"));



            // Gamemode Settings
            TabGroupHNS = TextOptionItem.Create(70000, "Hide and Seek", TabGroup.GamemodeSettings)
                .SetColor(Color.green);
            NumSeekers = IntegerOptionItem.Create(70001, "# Seekers", new(0, 15, 1), 1, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Level);


            TabGroup0Kcd = TextOptionItem.Create(70025, "0 Kill Cooldown", TabGroup.GamemodeSettings)
                .SetColor(Color.red);
            NoKcdSettingsOverride = BooleanOptionItem.Create(70026, "Auto Update Settings", true, TabGroup.GamemodeSettings, false);

            TabGroupSNS = TextOptionItem.Create(70050, "Shift and Seek", TabGroup.GamemodeSettings)
                .SetColor(Color.yellow);
            SNSSettingsOverride = BooleanOptionItem.Create(70051, "Auto Update Settings", true, TabGroup.GamemodeSettings, false);
            CantKillTime = IntegerOptionItem.Create(70053, "After Misfiring, Can't kill for", new(0, 60, 5), 20, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);
            MisfiresToSuicide = IntegerOptionItem.Create(70052, "Suicide After Amount Of Misfires", new(1, 10, 1), 2, TabGroup.GamemodeSettings, false);
            CrewAutoWinsGameAfter = IntegerOptionItem.Create(70054, "Crewmates Automatically Win After", new(0, 600, 10), 300, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);

            TabGroupSpeedrun = TextOptionItem.Create(70075, "Speedrun", TabGroup.GamemodeSettings)
                .SetColor(Color.blue);
            SpeedrunSettingsOverride = BooleanOptionItem.Create(70076, "Auto Update Settings", true, TabGroup.GamemodeSettings, false);
            GameAutoEndsAfter = IntegerOptionItem.Create(70077, "Game automatically ends after", new(0, 600, 10), 300, TabGroup.GamemodeSettings, false)
                .SetValueFormat(OptionFormat.Seconds);


            // Gameplay Settings
            TabGroupGameplayGeneral = TextOptionItem.Create(60564, "General", TabGroup.ModSettings)
                .SetColor(Color.blue);
            DisableSabotage = BooleanOptionItem.Create(60565, "Disable Critical Sabotages", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableCloseDoor = BooleanOptionItem.Create(60566, "Disable Door Sabotages", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(255, 153, 153, byte.MaxValue));
            DisableReport = BooleanOptionItem.Create(60520, "Disable Body Reports", false, TabGroup.ModSettings, false)
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



            DisableFungleTasks = BooleanOptionItem.Create(23300, "Disable Fungle Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableBuildSandcastle = BooleanOptionItem.Create(23301, "Disable Build Sandcastle", false, TabGroup.ModSettings, false)
               .SetParent(DisableFungleTasks);
          DisableCatchFish = BooleanOptionItem.Create(23302, "Disable Catch Fish", false, TabGroup.ModSettings, false)
               .SetParent(DisableFungleTasks);
           DisableCollectSamples = BooleanOptionItem.Create(23303, "Disable Collect Samples", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableCollectShells = BooleanOptionItem.Create(23304, "Disable Collect Shells", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableCollectVegetables = BooleanOptionItem.Create(23305, "Disable Collect Vegetables", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableCrankGenerator = BooleanOptionItem.Create(23306, "Disable Crank Generator", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableExtractFuel = BooleanOptionItem.Create(23307, "Disable Extract Fuel", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableFixAntenna = BooleanOptionItem.Create(23308, "Disable Fix Antenna", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableHelpCritter = BooleanOptionItem.Create(23309, "Disable Help Critter", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableHoistSupplies = BooleanOptionItem.Create(23310, "Disable Hoist Supplies", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableLiftWeights = BooleanOptionItem.Create(23311, "Disable Lift Weights", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableMineOres = BooleanOptionItem.Create(23312, "Disable Mine Ores", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisablePlayVideoGame = BooleanOptionItem.Create(23313, "Disable Play Video Game", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisablePolishGem = BooleanOptionItem.Create(23314, "Disable Polish Gem", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableReplaceParts = BooleanOptionItem.Create(23315, "Disable Replace Parts", false, TabGroup.ModSettings, false)
               .SetParent(DisableFungleTasks);
            DisableRoastMarshmallow = BooleanOptionItem.Create(23316, "Disable Roast Marshmallow", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableThrowFisbee = BooleanOptionItem.Create(23317, "Disable Throw Fisbee", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);
            DisableFindSignal = BooleanOptionItem.Create(23318, "Disable Find Signal", false, TabGroup.ModSettings, false)
                .SetParent(DisableFungleTasks);



            DisableMiscCommonTasks = BooleanOptionItem.Create(23400, "Disable Global Common Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableEnterIdCode = BooleanOptionItem.Create(23401, "Disable Enter Id Code", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscCommonTasks);
            DisableFixWiring = BooleanOptionItem.Create(23402, "Disable Fix Wiring", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscCommonTasks);
            DisableSwipeCard = BooleanOptionItem.Create(23403, "Disable Swipe Card", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscCommonTasks);



            DisableMiscShortTasks = BooleanOptionItem.Create(23500, "Disable Global Short Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableAssembleArtifact = BooleanOptionItem.Create(23501, "Disable Assemble Artifact", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableChartCourse = BooleanOptionItem.Create(23502, "Disable Chart Course", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableCleanO2Filter = BooleanOptionItem.Create(23503, "Disable Clean O2 Filter", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableCleanVent = BooleanOptionItem.Create(23504, "Disable Clean Vent", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableMonitorMushroom = BooleanOptionItem.Create(23506, "Disable Monitor Mushroom", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableMonitorTree = BooleanOptionItem.Create(23507, "Disable Monitor Tree", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisablePrimeShields = BooleanOptionItem.Create(23508, "Disable Prime Shields", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableRecordTemperature = BooleanOptionItem.Create(23509, "Disable Record Temperature", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableStabilizeSteering = BooleanOptionItem.Create(23510, "Disable Stabilize Steering", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);
            DisableStoreArtifacts = BooleanOptionItem.Create(23511, "Disable Store Artifacts", false, TabGroup.ModSettings, false)
               .SetParent(DisableMiscShortTasks);
            DisableUnlockManifolds = BooleanOptionItem.Create(23512, "Disable Unlock Manifolds", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscShortTasks);


            DisableMiscLongTasks = BooleanOptionItem.Create(23600, "Disable Global Long Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableAlignEngineOutput = BooleanOptionItem.Create(23601, "Disable Align Engine Output", false, TabGroup.ModSettings, false)
               .SetParent(DisableMiscLongTasks);
            DisableClearAsteroids = BooleanOptionItem.Create(23602, "Disable Clear Asteroids", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableEmptyChute = BooleanOptionItem.Create(23603, "Disable Empty Chute", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableInspectSample = BooleanOptionItem.Create(23604, "Disable Inspect Sample", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableReplaceWaterJug = BooleanOptionItem.Create(23605, "Disable Replace Water Jug", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableStartReactor = BooleanOptionItem.Create(23606, "Disable Start Reactor", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);
            DisableWaterPlants = BooleanOptionItem.Create(23607, "Disable Water Plants", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscLongTasks);



            DisableMiscMixedTasks = BooleanOptionItem.Create(23700, "Disable Global Mixed Tasks", false, TabGroup.ModSettings, false)
                .SetColor(new Color32(173, 216, 230, byte.MaxValue))
                .SetParent(OverrideTaskSettings);

            DisableCalibrateDistributor = BooleanOptionItem.Create(23701, "Disable Calibrate Distributor", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableDivertPower = BooleanOptionItem.Create(23702, "Disable Divert Power", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableEmptyGarbage = BooleanOptionItem.Create(23703, "Disable Empty Garbage", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableFuelEngines = BooleanOptionItem.Create(23704, "Disable Fuel Engines", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableSubmitScan = BooleanOptionItem.Create(23705, "Disable Submit Scan", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);
            DisableUploadData = BooleanOptionItem.Create(23706, "Disable Upload Data", false, TabGroup.ModSettings, false)
                .SetParent(DisableMiscMixedTasks);

            IsLoaded = true;
        }
    }
}