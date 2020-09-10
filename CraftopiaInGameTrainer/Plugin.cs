using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using KeyboardShortcut = BepInEx.Configuration.KeyboardShortcut;
using HarmonyLib;

namespace CraftopiaInGameTrainer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        // exp needed to level up: 10*1.2^(level-1);

        #region const value
        //public const string GUID = "52B2D26E-7749-824D-31DD-310AA7D700BA";
        public const string GUID = "cn.zhuangcloud.craftopia.igt";
        public const string NAME = "Craftopia InGame Trainer";
        public const string VERSION = "1.0";
        private const string GAME_PROCESS = "Craftopia.exe";
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

        private ConfigEntry<KeyboardShortcut> AddMoney;
        private ConfigEntry<KeyboardShortcut> AddExp;
        private ConfigEntry<KeyboardShortcut> AddSkillPoint;

        private ConfigEntry<long> MoneyStep;
        private ConfigEntry<long> ExpStep;
        private ConfigEntry<int> SkillPointStep;
        private ConfigEntry<int> SkillPointStepByLevelUp;
        #endregion

        private void InitConfig()
        {
            GodMod = Config.Bind("Functions", "GodMod", false, new ConfigDescription("Switch of god mod.", null, new ConfigurationManagerAttributes { Order = 888, DispName = "无限血" }));
            InfMana = Config.Bind("Functions", "InfMana", false, new ConfigDescription("Switch of infinite mana.", null, new ConfigurationManagerAttributes { Order = 777, DispName = "无限蓝" }));
            InfStamina = Config.Bind("Functions", "InfStamina", false, new ConfigDescription("Switch of infinite stamina.", null, new ConfigurationManagerAttributes { Order = 666, DispName = "无限耐力" }));
            InfSatiety = Config.Bind("Functions", "InfSatiety", false, new ConfigDescription("Switch of infinite satiety.", null, new ConfigurationManagerAttributes { Order = 555, DispName = "永不饥饿" }));

            MaxLevelSetTo100 = Config.Bind("Functions", "MaxLevelSetTo100", false, new ConfigDescription("Set max level to 100. Need reload after change this value.", null, new ConfigurationManagerAttributes { Order = 444, DispName = "设置等级上限为100级" }));
            MaxLevelSetTo100.SettingChanged += (object sender, EventArgs e) => Traverse.Create(typeof(Oc.OcDefine)).Field("MAX_LEVEL").SetValue(MaxLevelSetTo100.Value ? 100 : 50);
            Traverse.Create(typeof(Oc.OcDefine)).Field("MAX_LEVEL").SetValue(MaxLevelSetTo100.Value ? 100 : 50);
            RepairAlwaysSuccess = Config.Bind("Functions", "RepairAlwaysSuccess", false, new ConfigDescription("Repair always success.", null, new ConfigurationManagerAttributes { Order = 333, DispName = "修理不会失败" }));
            Traverse.Create(typeof(Oc.OcDefine)).Field("SUCCESS_RATE_FOR_REPAIR_ITEM").SetValue(RepairAlwaysSuccess.Value ? 1f : 0.9f);
            RepairIsFree = Config.Bind("Functions", "RepairIsFree", false, new ConfigDescription("Free to repair.", null, new ConfigurationManagerAttributes { Order = 222, DispName = "免费修理" }));
            Traverse.Create(typeof(Oc.OcDefine)).Field("RAITO_OF_PRICE_TO_REPAIR").SetValue(RepairIsFree.Value ? 0f : 0.1f);
            RepairIsFast = Config.Bind("Functions", "RepairIsFast", false, new ConfigDescription("Fast repair.", null, new ConfigurationManagerAttributes { Order = 111, DispName = "快速修理" }));
            Traverse.Create(typeof(Oc.OcDefine)).Field("REPAIR_DURATION_SEC").SetValue(RepairIsFast.Value ? 0.1f : 3f);

            ToggleGodMod = Config.Bind("Hotkeys", "ToggleGodMod", new KeyboardShortcut(KeyCode.F1), new ConfigDescription("Toggle god mod.", null, new ConfigurationManagerAttributes { Order = 777, DispName = "无限血的快捷键" }));
            ToggleInfMana = Config.Bind("Hotkeys", "ToggleInfMana", new KeyboardShortcut(KeyCode.F2), new ConfigDescription("Toggle infinite mana.", null, new ConfigurationManagerAttributes { Order = 666, DispName = "无限蓝的快捷键" }));
            ToggleInfStamina = Config.Bind("Hotkeys", "ToggleInfStamina", new KeyboardShortcut(KeyCode.F3), new ConfigDescription("Toggle infinite stamina.", null, new ConfigurationManagerAttributes { Order = 555, DispName = "无限耐力的快捷键" }));
            ToggleInfSatiety = Config.Bind("Hotkeys", "ToggleInfSatiety", new KeyboardShortcut(KeyCode.F4), new ConfigDescription("Toggle infinite satiety.", null, new ConfigurationManagerAttributes { Order = 444, DispName = "永不饥饿的快捷键" }));

            AddMoney = Config.Bind("Hotkeys", "AddMoney", new KeyboardShortcut(KeyCode.F5), new ConfigDescription("Add money.", null, new ConfigurationManagerAttributes { Order =333, DispName = "加钱快捷键" }));
            AddExp = Config.Bind("Hotkeys", "AddExp", new KeyboardShortcut(KeyCode.F6), new ConfigDescription("Add exp.", null, new ConfigurationManagerAttributes { Order = 222, DispName = "加经验快捷键" }));
            AddSkillPoint = Config.Bind("Hotkeys", "AddSkillPoint", new KeyboardShortcut(KeyCode.F7), new ConfigDescription("Add skill point.", null, new ConfigurationManagerAttributes { Order = 111, DispName = "加技能点快捷键" }));

            MoneyStep = Config.Bind("Misc", "MoneyStep", (long)10000, new ConfigDescription("How many money will be added.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 444, DispName = "按一下加多少钱" }));
            ExpStep = Config.Bind("Misc", "ExpStep", (long)10000, new ConfigDescription("How many exp will be added.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 333, DispName = "按一下加多少经验" }));
            SkillPointStep = Config.Bind("Misc", "SkillPointStep", 1, new ConfigDescription("How many skill points will be added.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order =222, DispName = "按一下加多少技能点" }));
            SkillPointStepByLevelUp = Config.Bind("Misc", "SkillPointStepByLevelUp", 1, new ConfigDescription("How many skill points will get when level up. Need reload after change this value.", new AcceptableValueRange<int>(1, 10), new ConfigurationManagerAttributes { Order = 111, DispName = "每升一级获得多少技能点" }));
            Traverse.Create(typeof(Oc.OcDefine)).Field("INCREASE_SKILLPOINT_BY_LEVEL_UP").SetValue(SkillPointStepByLevelUp.Value);
        }

        void Awake()
        {
            Logger.LogInfo(NAME + " Version " + VERSION);
            Logger.LogInfo(NAME + " Starting...");
            InitConfig();
            Logger.LogInfo(NAME + " Standby");
        }

        void Update()
        {
            try
            {
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
                        Oc.OcPlMaster.Inst.Health.AddMoney(MoneyStep.Value);
                    if (AddExp.Value.IsDown())
                        Oc.OcPlMaster.Inst.PlLevelCtrl.AddExp(ExpStep.Value);
                    if (AddSkillPoint.Value.IsDown())
                        Oc.OcPlMaster.Inst.SkillCtrl.AddSkillPoint(SkillPointStep.Value);
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
            catch(Exception ex)
            {
                Logger.LogError(ex);
            }
        }

    }
}