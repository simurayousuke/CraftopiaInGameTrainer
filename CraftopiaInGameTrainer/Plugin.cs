using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using KeyboardShortcut = BepInEx.Configuration.KeyboardShortcut;
using HarmonyLib;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CraftopiaInGameTrainer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]
    //[BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        // exp needed to level up: 10*1.2^(level-1);
        // todo : 鼠标穿透

        #region const value
        //public const string GUID = "52B2D26E-7749-824D-31DD-310AA7D700BA";
        public const string GUID = "cn.zhuangcloud.craftopia.igt";
        public const string NAME = "Craftopia InGame Trainer";
        public const string VERSION = "1.3";
        private const string GAME_PROCESS = "Craftopia.exe";
        private const string ANNOUNCEMENT_URL = "http://assets.zhuangcloud.cn/Craftopia/igt/announcement.txt";
        private const string VERSION_URL = "http://assets.zhuangcloud.cn/Craftopia/igt/version.txt";
        private const string LATEST_URL = "http://assets.zhuangcloud.cn/Craftopia/igt/CraftopiaInGameTrainer.dll";
        private const string RELEASE_URL = "https://github.com/simurayousuke/CraftopiaInGameTrainer/releases";
        private const string GITHUB_URL = "https://github.com/simurayousuke/CraftopiaInGameTrainer";
        #endregion

        #region config
        private ConfigEntry<bool> GodMod;
        private ConfigEntry<bool> InfMana;
        private ConfigEntry<bool> InfStamina;
        private ConfigEntry<bool> InfSatiety;
        private ConfigEntry<bool> MaxLevelSetTo100;
        private ConfigEntry<bool> RepairAlwaysSuccess;
        private ConfigEntry<bool> RepairIsFree;
        private ConfigEntry<bool> RepairIsFast;


        private ConfigEntry<KeyboardShortcut> ToggleGodMod;
        private ConfigEntry<KeyboardShortcut> ToggleInfMana;
        private ConfigEntry<KeyboardShortcut> ToggleInfStamina;
        private ConfigEntry<KeyboardShortcut> ToggleInfSatiety;
        private ConfigEntry<KeyboardShortcut> ToggleWindow;

        private ConfigEntry<KeyboardShortcut> AddMoney;
        private ConfigEntry<KeyboardShortcut> AddExp;
        private ConfigEntry<KeyboardShortcut> AddSkillPoint;

        private ConfigEntry<long> MoneyStep;
        private ConfigEntry<long> ExpStep;
        private ConfigEntry<int> SkillPointStep;
        private ConfigEntry<int> SkillPointStepByLevelUp;
        #endregion

        #region gui
        private bool WINDOW_SHOW = false;
        private bool WINDOW_ANNOUNCE = false;
        private bool WINDOW_NEW_VERSION = false;
        private static readonly float WINDOW_WIDTH = Mathf.Min(Screen.width, 650);
        private static readonly float WINDOW_HEIGHT = Screen.height < 560 ? Screen.height : Screen.height - 100;
        private static readonly float WINDOW_VERSION_WIDTH = Mathf.Min(Screen.width, 300);
        private static readonly float WINDOW__VERSION_HEIGHT = Mathf.Min(Screen.height, 150);
        private const float GROUP_MARGIN_WIDTH = 10f;
        private const float GROUP_MARGIN_HEIGHT = 10f;
        private Rect windowIgtRect = new Rect((Screen.width - WINDOW_WIDTH) / 2f, (Screen.height - WINDOW_HEIGHT) / 2f, WINDOW_WIDTH, WINDOW_HEIGHT);
        private Rect windowAnnounceRect = new Rect((Screen.width - WINDOW_WIDTH) / 2f, (Screen.height - WINDOW_HEIGHT) / 2f, WINDOW_WIDTH, WINDOW_HEIGHT);
        private Rect windowVersionRect = new Rect((Screen.width - WINDOW_VERSION_WIDTH) / 2f, (Screen.height - WINDOW__VERSION_HEIGHT) / 2f, WINDOW_VERSION_WIDTH, WINDOW__VERSION_HEIGHT);
        private Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        private Vector2 scrollViewIgtVector = Vector2.zero;
        private string ANNOUNCEMENT = "Unable to get announcement, check your internet connection.";
        private const string WINDOW_TITLE = NAME + " v" + VERSION;
        private Texture2D WindowBackground;
        private CursorLockMode _CURSOR_LOCK_MODE;
        private bool _CURSOR_VISIBLE;
        private GUIStyle BACKGROUND_STYLE;
        private static readonly IEnumerable<KeyCode> _keysToCheck = BepInEx.Configuration.KeyboardShortcut.AllKeyCodes.Except(new[] { KeyCode.Mouse0, KeyCode.None }).ToArray();
        private string SETTING_KS_ID = "";
        private string latestVersion = VERSION;
        private bool UPDATING = false;
        private bool UPDATE_FAIL = false;
        private bool IS_NEW = false;
        #endregion

        private void InitConfig()
        {
            GodMod = Config.Bind("Functions", "GodMod", false, new ConfigDescription("Switch of god mod."));
            InfMana = Config.Bind("Functions", "InfMana", false, new ConfigDescription("Switch of infinite mana."));
            InfStamina = Config.Bind("Functions", "InfStamina", false, new ConfigDescription("Switch of infinite stamina."));
            InfSatiety = Config.Bind("Functions", "InfSatiety", false, new ConfigDescription("Switch of infinite satiety."));

            MaxLevelSetTo100 = Config.Bind("Functions", "MaxLevelSetTo100", false, new ConfigDescription("Set max level to 100. Need reload after change this value."));
            Traverse.Create(typeof(Oc.OcDefine)).Field("MAX_LEVEL").SetValue(MaxLevelSetTo100.Value ? 100 : 50);
            RepairAlwaysSuccess = Config.Bind("Functions", "RepairAlwaysSuccess", false, new ConfigDescription("Repair always success."));
            Traverse.Create(typeof(Oc.OcDefine)).Field("SUCCESS_RATE_FOR_REPAIR_ITEM").SetValue(RepairAlwaysSuccess.Value ? 1f : 0.9f);
            RepairIsFree = Config.Bind("Functions", "RepairIsFree", false, new ConfigDescription("Free to repair."));
            Traverse.Create(typeof(Oc.OcDefine)).Field("RAITO_OF_PRICE_TO_REPAIR").SetValue(RepairIsFree.Value ? 0f : 0.1f);
            RepairIsFast = Config.Bind("Functions", "RepairIsFast", false, new ConfigDescription("Fast repair."));
            Traverse.Create(typeof(Oc.OcDefine)).Field("REPAIR_DURATION_SEC").SetValue(RepairIsFast.Value ? 0.1f : 3f);

            ToggleWindow = Config.Bind("Hotkeys", "ToggleWindow", new KeyboardShortcut(KeyCode.Home), new ConfigDescription("Toggle trainer window show/hide."));

            ToggleGodMod = Config.Bind("Hotkeys", "ToggleGodMod", new KeyboardShortcut(KeyCode.F1), new ConfigDescription("Toggle god mod."));
            ToggleInfMana = Config.Bind("Hotkeys", "ToggleInfMana", new KeyboardShortcut(KeyCode.F2), new ConfigDescription("Toggle infinite mana."));
            ToggleInfStamina = Config.Bind("Hotkeys", "ToggleInfStamina", new KeyboardShortcut(KeyCode.F3), new ConfigDescription("Toggle infinite stamina."));
            ToggleInfSatiety = Config.Bind("Hotkeys", "ToggleInfSatiety", new KeyboardShortcut(KeyCode.F4), new ConfigDescription("Toggle infinite satiety."));

            AddMoney = Config.Bind("Hotkeys", "AddMoney", new KeyboardShortcut(KeyCode.F5), new ConfigDescription("Add money."));
            AddExp = Config.Bind("Hotkeys", "AddExp", new KeyboardShortcut(KeyCode.F6), new ConfigDescription("Add exp."));
            AddSkillPoint = Config.Bind("Hotkeys", "AddSkillPoint", new KeyboardShortcut(KeyCode.F7), new ConfigDescription("Add skill point."));

            MoneyStep = Config.Bind("Misc", "MoneyStep", (long)10000, new ConfigDescription("How many money will be added."));
            ExpStep = Config.Bind("Misc", "ExpStep", (long)10000, new ConfigDescription("How many exp will be added."));
            SkillPointStep = Config.Bind("Misc", "SkillPointStep", 1, new ConfigDescription("How many skill points will be added."));
            SkillPointStepByLevelUp = Config.Bind("Misc", "SkillPointStepByLevelUp", 1, new ConfigDescription("How many skill points will get when level up. Need reload after change this value."));
            Traverse.Create(typeof(Oc.OcDefine)).Field("INCREASE_SKILLPOINT_BY_LEVEL_UP").SetValue(SkillPointStepByLevelUp.Value);
        }

        string GetWebString(string url)
        {
            try
            {
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(url));
                HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
                Stream respStream = httpResp.GetResponseStream();
                StreamReader respStreamReader = new StreamReader(respStream, Encoding.UTF8);
                char[] cbuffer = new char[256];
                int byteRead = respStreamReader.Read(cbuffer, 0, 256);
                string strBuff = "";
                while (byteRead != 0)
                {
                    strBuff += new string(cbuffer, 0, byteRead);
                    byteRead = respStreamReader.Read(cbuffer, 0, 256);
                }
                respStream.Close();
                return strBuff;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message.ToString());
                return "";
            }

        }

        bool HttpDownloadFile(string url, string path)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                Stream stream = new FileStream(path, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                stream.Close();
                responseStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Fail to download file.\n" + ex.Message.ToString());
                return false;
            }
        }

        bool ExecBat(string fileDir, string fileName, string cmd)
        {
            try
            {
                string filePath = fileDir + "\\" + fileName;
                if (File.Exists(filePath))
                    File.Delete(filePath);

                FileStream fs1 = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(cmd);
                sw.Close();
                fs1.Close();

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.WorkingDirectory = fileDir;
                proc.StartInfo.FileName = fileName;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Fail to exec bat.\n" + ex.Message.ToString());
                return false;
            }
        }

        bool UpdateVersion()
        {
            try
            {
                UPDATING = true;
                string currentDir = System.Environment.CurrentDirectory;
                string pluginDir = currentDir + "\\BepInEx\\plugins\\CraftopiaInGameTrainer";
                string newFile = pluginDir + "\\CraftopiaInGameTrainer.new";
                if (!Directory.Exists(pluginDir))
                    Directory.CreateDirectory(pluginDir);
                if (!HttpDownloadFile(LATEST_URL, newFile))
                    return false;
                string cmd = "@echo off\n"
                    + "taskkill /f /im Craftopia.exe\n"
                    + "del /q CraftopiaInGameTrainer.dll\n"
                    + "move CraftopiaInGameTrainer.new CraftopiaInGameTrainer.dll\n"
                    + "start steam://rungameid/1307550\n"
                    + "del %0";
                if (!ExecBat(pluginDir, "update.bat", cmd))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Fail to update.\n" + ex.Message.ToString());
                return false;
            }
        }

        bool CheckAnnounce()
        {
            string announce = GetWebString(ANNOUNCEMENT_URL);
            if (String.IsNullOrEmpty(announce))
                return false;
            ANNOUNCEMENT = announce;
            return true;
        }

        double CheckVersion()
        {
            string version = GetWebString(VERSION_URL);
            if (String.IsNullOrEmpty(version))
                return 0;
            latestVersion = version;
            return Convert.ToDouble(version);
        }

        void Awake()
        {
            Logger.LogInfo(NAME + " Version " + VERSION);
            Logger.LogInfo(NAME + " Starting...");
            try
            {
                InitConfig();
                WindowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                WindowBackground.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 1));
                WindowBackground.Apply();
                BACKGROUND_STYLE = new GUIStyle { normal = new GUIStyleState { background = WindowBackground } };
                if (CheckAnnounce())
                    WINDOW_ANNOUNCE = true;
                else
                    Logger.LogWarning("Fail to check announcement.");
                if (CheckVersion() > Convert.ToDouble(VERSION))
                    WINDOW_NEW_VERSION = true;
                else
                    Logger.LogWarning("Fail to check version.");
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Init Error.\n" + ex.Message.ToString());
            }
            finally
            {
                Logger.LogInfo(NAME + " Standby");
            }
        }

        void DoAddMoney()
        {
            Oc.OcPlMaster.Inst.Health.AddMoney(MoneyStep.Value);
        }

        void DoAddExp()
        {
            Oc.OcPlMaster.Inst.PlLevelCtrl.AddExp(ExpStep.Value);
        }

        void DoAddSkillPoint()
        {
            Oc.OcPlMaster.Inst.SkillCtrl.AddSkillPoint(SkillPointStep.Value);
        }

        void Update()
        {
            try
            {
                if (ToggleWindow.Value.IsDown())
                    ToggleWindowDisplay(!WINDOW_SHOW);

                Traverse.Create(typeof(Oc.OcDefine)).Field("MAX_LEVEL").SetValue(MaxLevelSetTo100.Value ? 100 : 50);
                Traverse.Create(typeof(Oc.OcDefine)).Field("SUCCESS_RATE_FOR_REPAIR_ITEM").SetValue(RepairAlwaysSuccess.Value ? 1f : 0.9f);
                Traverse.Create(typeof(Oc.OcDefine)).Field("RAITO_OF_PRICE_TO_REPAIR").SetValue(RepairIsFree.Value ? 0f : 0.1f);
                Traverse.Create(typeof(Oc.OcDefine)).Field("REPAIR_DURATION_SEC").SetValue(RepairIsFast.Value ? 0.1f : 3f);
                Traverse.Create(typeof(Oc.OcDefine)).Field("INCREASE_SKILLPOINT_BY_LEVEL_UP").SetValue(SkillPointStepByLevelUp.Value);

                if (ToggleGodMod.Value.IsDown())
                    GodMod.Value = !GodMod.Value;
                if (ToggleInfMana.Value.IsDown())
                    InfMana.Value = !InfMana.Value;
                if (ToggleInfStamina.Value.IsDown())
                    InfStamina.Value = !InfStamina.Value;
                if (ToggleInfSatiety.Value.IsDown())
                    InfSatiety.Value = !InfSatiety.Value;
                if (Oc.OcPlMaster.Inst != null)
                {
                    if (AddMoney.Value.IsDown())
                        DoAddMoney();
                    if (AddExp.Value.IsDown())
                        DoAddExp();
                    if (AddSkillPoint.Value.IsDown())
                        DoAddSkillPoint();
                    if (GodMod.Value)
                        Oc.OcPlMaster.Inst.Health.setForceHP(Oc.OcPlMaster.Inst.Health.MaxHP);
                    if (InfMana.Value)
                        Oc.OcPlMaster.Inst.Health.setForceMP(Oc.OcPlMaster.Inst.Health.MaxMP);
                    if (InfStamina.Value)
                        Oc.OcPlMaster.Inst.Health.SetMaxStamina(Oc.OcPlMaster.Inst.Health.MaxST);
                    if (InfSatiety.Value)
                        Oc.OcPlMaster.Inst.Health.setForceSP(Oc.OcPlMaster.Inst.Health.MaxSP);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message.ToString());
            }
        }

        void ToggleWindowDisplay(bool show)
        {
            if (!WINDOW_SHOW)
            {
                _CURSOR_LOCK_MODE = Cursor.lockState;
                _CURSOR_VISIBLE = Cursor.visible;
            }
            else
            {
                Cursor.lockState = _CURSOR_LOCK_MODE;
                Cursor.visible = _CURSOR_VISIBLE;
            }
            WINDOW_SHOW = show;

        }

        void OnGUI()
        {
            try
            {
                if (WINDOW_SHOW || WINDOW_ANNOUNCE || WINDOW_NEW_VERSION)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                if (WINDOW_ANNOUNCE)
                    windowAnnounceRect = GUI.Window(0, windowAnnounceRect, WindowAnnounce, WINDOW_TITLE);
                else if (WINDOW_NEW_VERSION)
                    windowVersionRect = GUI.Window(0, windowVersionRect, WindowVersion, WINDOW_TITLE);
                else if (WINDOW_SHOW)
                {
                    if (GUI.Button(screenRect, string.Empty, GUI.skin.box) &&
                       !windowIgtRect.Contains(Input.mousePosition))
                        ToggleWindowDisplay(false);
                    windowIgtRect = GUI.Window(0, windowIgtRect, WindowIGT, WINDOW_TITLE);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("GUI Error:\n" + ex.Message.ToString());
            }
        }

        void DrawCenteredLabel(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawBoolField(string dispName, ConfigEntry<bool> setting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(dispName);
            GUILayout.FlexibleSpace();
            bool boolVal = setting.Value;
            var result = GUILayout.Toggle(boolVal, boolVal ? "Enabled" : "Disabled", GUILayout.ExpandWidth(true), GUILayout.ExpandWidth(true));
            if (result != boolVal)
                setting.Value = result;
            GUILayout.EndHorizontal();
        }

        void DrawKeyboardShortcut(string dispName, ConfigEntry<KeyboardShortcut> setting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(dispName);
            GUILayout.FlexibleSpace();

            if (String.Equals(SETTING_KS_ID, dispName))
            {
                GUILayout.Label("Press keys you want to use", GUILayout.ExpandWidth(true));
                foreach (var key in _keysToCheck)
                    if (Input.GetKeyUp(key))
                    {
                        setting.Value = (new KeyboardShortcut(key, _keysToCheck.Where(Input.GetKey).ToArray()));
                        SETTING_KS_ID = "";
                        break;
                    }
                if (GUILayout.Button("Cancle", GUILayout.ExpandWidth(false)))
                    SETTING_KS_ID = "";
            }
            else
            {
                string key = setting.Value.ToString();
                //if (String.Equals(key, "Not set"))
                //    key = "未设置";
                if (GUILayout.Button(key, GUILayout.ExpandWidth(true)))
                    SETTING_KS_ID = dispName;

                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    setting.Value = KeyboardShortcut.Empty;
                    SETTING_KS_ID = "";
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawInt(string dispName, ConfigEntry<int> setting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(dispName);
            //GUILayout.FlexibleSpace();

            string value = GUILayout.TextField(setting.Value.ToString(), GUILayout.ExpandWidth(true));
            try
            {
                setting.Value = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Inputing none numbers.\n" + ex.Message.ToString());
            }
            GUILayout.EndHorizontal();
        }

        void DrawLong(string dispName, ConfigEntry<long> setting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(dispName);
            //GUILayout.FlexibleSpace();

            string value = GUILayout.TextField(setting.Value.ToString(), GUILayout.ExpandWidth(true));
            try
            {
                setting.Value = Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Inputing none numbers.\n" + ex.Message.ToString());
            }
            GUILayout.EndHorizontal();
        }

        void WindowIGT(int windowID)
        {
            float groupWidth = WINDOW_WIDTH - GROUP_MARGIN_WIDTH * 2f;
            float groupHeight = WINDOW_HEIGHT - GROUP_MARGIN_HEIGHT * 2f - 15f;
            Rect groupFunctions = new Rect(GROUP_MARGIN_WIDTH, GROUP_MARGIN_HEIGHT + 15f, groupWidth, groupHeight);
            //GUI.Box(groupFunctions, "功能");

            //GUILayout.BeginArea(groupFunctions);
            scrollViewIgtVector = GUILayout.BeginScrollView(scrollViewIgtVector, false, true);
            GUILayout.BeginVertical();
            DrawCenteredLabel("By default, press Home to hide/show this menu.");
            DrawCenteredLabel(String.Empty);

            DrawCenteredLabel("Functions/功能");
            DrawBoolField("Inf HP/无限血", GodMod);
            DrawBoolField("Inf MP/无限蓝", InfMana);
            DrawBoolField("Inf Stamina/无限耐力（绿条）", InfStamina);
            DrawBoolField("Inf Satiety/永不饥饿", InfSatiety);
            DrawBoolField("Set player max level to 100/玩家等级上限提升为100级", MaxLevelSetTo100);
            DrawBoolField("Repair never fail/修理不会失败", RepairAlwaysSuccess);
            DrawBoolField("Free repair/免费修理", RepairIsFree);
            DrawBoolField("Fast repair/快速修理", RepairIsFast);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add money/增加金钱"))
                DoAddMoney();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add exp/增加经验"))
                DoAddExp();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add skill point/增加技能点"))
                DoAddSkillPoint();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            DrawCenteredLabel(String.Empty);
            DrawCenteredLabel("Hotkeys/快捷键");
            DrawKeyboardShortcut("Show/Hide Menu(显示/隐藏菜单)", ToggleWindow);
            DrawKeyboardShortcut("Inf HP/无限血", ToggleGodMod);
            DrawKeyboardShortcut("Inf MP/无限蓝", ToggleInfMana);
            DrawKeyboardShortcut("Inf Stamina/无限耐力（绿条）", ToggleInfStamina);
            DrawKeyboardShortcut("Inf Satiety/永不饥饿", ToggleInfSatiety);
            DrawKeyboardShortcut("Add Money/增加金钱", AddMoney);
            DrawKeyboardShortcut("Add Exp/增加经验", AddExp);
            DrawKeyboardShortcut("Add Skill Point/增加技能点", AddSkillPoint);

            DrawCenteredLabel(String.Empty);
            DrawCenteredLabel("Misc/杂项");
            DrawInt("Skill points got per level up/每升一级增加多少技能点", SkillPointStepByLevelUp);
            DrawLong("Money added per hotkey/按一下增加多少钱", MoneyStep);
            DrawLong("Exp added per hotkey/按一下增加多少经验", ExpStep);
            DrawInt("Skill point added per hotkey/按一下增加多少技能点", SkillPointStep);

            DrawCenteredLabel(String.Empty);
            DrawCenteredLabel("Other/其他");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Github/打开主页"))
                System.Diagnostics.Process.Start(GITHUB_URL);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("View Announcement/查看公告"))
                if (CheckAnnounce())
                    WINDOW_ANNOUNCE = true;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(IS_NEW ? "Latest/已是最新" : "Check Update/检查更新"))
                if (CheckVersion() > Convert.ToDouble(VERSION))
                    WINDOW_NEW_VERSION = true;
                else
                    IS_NEW = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            //GUILayout.EndArea();
        }

        void WindowAnnounce(int windowID)
        {
            float groupWidth = WINDOW_WIDTH - GROUP_MARGIN_WIDTH * 2f;
            float groupHeight = WINDOW_HEIGHT - GROUP_MARGIN_HEIGHT * 2f - 15f;
            Rect groupAnnounce = new Rect(GROUP_MARGIN_WIDTH, GROUP_MARGIN_HEIGHT + 15f, groupWidth, groupHeight);
            GUI.Box(groupAnnounce, "Announcement/公告");
            GUI.BeginGroup(groupAnnounce);
            GUI.Label(groupAnnounce, ANNOUNCEMENT);
            if (GUI.Button(new Rect((groupWidth - 80f) / 2f, groupHeight - 40f, 80f, 30f), "Close/关闭"))
            {
                WINDOW_ANNOUNCE = false;
                ToggleWindowDisplay(true);
            }
            GUI.EndGroup();
        }

        void WindowVersion(int windowID)
        {
            if (!UPDATING)
            {
                string s = "Latest version(最新版本):" + latestVersion + "\n";
                s += "Current version(当前版本):" + VERSION + "\n\n";
                s += "是否前往更新？";
                s += "Do you want to update now?";

                GUILayout.BeginVertical();
                GUILayout.Label(s);
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Detail/详情"))
                    System.Diagnostics.Process.Start(RELEASE_URL);
                if (GUILayout.Button("Update/更新"))
                    if (!UpdateVersion())
                        UPDATE_FAIL = true;
                if (GUILayout.Button("Not now/下次再说"))
                {
                    WINDOW_NEW_VERSION = false;
                    ToggleWindowDisplay(true);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            else if (!UPDATE_FAIL)
                GUILayout.Label("Updating... Please wait. Game will restart after update.\n更新中请稍后。更新结束后会自动重启游戏。");
            else
                GUILayout.Label("Fail to update. Try to update manually.\nOr restart game and click 'Not now' to skip.\n更新失败，请尝试手动更新。\n或重启游戏后点击[下次再说]以忽略本次更新。");
        }
    }
}