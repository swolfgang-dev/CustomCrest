using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMProOld;
using UnityEngine;
using WheresWolfgang.SaveScopedConfig;

namespace CustomCrest
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(SaveScopedConfigPlugin.PluginGuid)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "whereswolfgang.customcrest";
        public const string PluginName = "Custom Crest";
        public const string PluginVersion = "0.1.0";

        private static readonly Harmony Harmony = new Harmony(PluginGuid);
        private static readonly FieldInfo CrestEquipMsgField =
            AccessTools.Field(typeof(InventoryItemToolManager), "crestEquipMsg");
        private const string CrestBenchMessage = "Change Crests while resting at a bench.";
        private const string AttackBenchMessage = "Change attacks while resting at a bench.";
        private static bool showAttackBenchMessageOnce;

        private static readonly AttackSource[] SelectableAttackSources =
        {
            AttackSource.Hunter,
            AttackSource.Reaper,
            AttackSource.Wanderer,
            AttackSource.Beast,
            AttackSource.Witch,
            AttackSource.Architect,
            AttackSource.Shaman,
        };

        private static readonly string[] KnownCrestIds =
        {
            "Default",
            "Reaper",
            "Wanderer",
            "Beast",
            "Witch",
            "Architect",
            "Shaman",
        };

        private ConfigEntry<bool> allowLockedAttackSources;
        private ConfigEntry<bool> requireBenchToChangeAttack;
        private readonly Dictionary<string, CrestAttackSourceConfig> saveAttackSourcesByCrest = new Dictionary<string, CrestAttackSourceConfig>();
        private SaveScopedConfigFile saveConfig;

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            saveConfig = SaveScopedConfig.For(this);

            BindConfig();
            Harmony.PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo(PluginName + " loaded.");
        }

        private void BindConfig()
        {
            allowLockedAttackSources = Config.Bind(
                "Global: Attack Sources",
                "Allow Locked Crest Attacks",
                false,
                "If true, attack source selectors can choose crests that have not been unlocked on this save.");
            requireBenchToChangeAttack = Config.Bind(
                "Global: Attack Sources",
                "Require Bench To Change Attack",
                true,
                "Require resting at a bench to change attack source selections.");
            BindKnownCrestConfigs();
        }

        internal bool CanChangeAttackSources()
        {
            return requireBenchToChangeAttack == null ||
                !requireBenchToChangeAttack.Value ||
                (GameManager.instance != null &&
                    GameManager.instance.playerData != null &&
                    GameManager.instance.playerData.atBench) ||
                CheatManager.CanChangeEquipsAnywhere;
        }

        internal static void ShowAttackBenchMessage(InventoryItemToolManager manager)
        {
            if (manager == null)
            {
                return;
            }

            showAttackBenchMessageOnce = true;
            manager.ShowCrestEquipMsg();
        }

        private static void SetCrestBenchMessageText(InventoryItemToolManager manager, string message)
        {
            object crestEquipMsg = CrestEquipMsgField == null || manager == null
                ? null
                : CrestEquipMsgField.GetValue(manager);
            Component component = crestEquipMsg as Component;
            if (component == null)
            {
                return;
            }

            TextMeshPro[] texts = component.GetComponentsInChildren<TextMeshPro>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshPro text = texts[i];
                if (text != null && !string.IsNullOrEmpty(text.text))
                {
                    text.text = message;
                }
            }
        }

        internal AttackSource NeutralAttackSource
        {
            get { return GetEffectiveAttackSource(GetEquippedCrestId(), AttackSlot.Neutral); }
        }

        internal AttackSource UpAttackSource
        {
            get { return GetEffectiveAttackSource(GetEquippedCrestId(), AttackSlot.Up); }
        }

        internal AttackSource DownAttackSource
        {
            get { return GetEffectiveAttackSource(GetEquippedCrestId(), AttackSlot.Down); }
        }

        internal AttackSource DashAttackSource
        {
            get { return GetEffectiveAttackSource(GetEquippedCrestId(), AttackSlot.Dash); }
        }

        internal AttackSource NeedleStrikeSource
        {
            get { return GetEffectiveAttackSource(GetEquippedCrestId(), AttackSlot.NeedleStrike); }
        }

        internal AttackSource GetSaveAttackSource(string crestId, AttackSlot attackSlot)
        {
            SaveScopedConfigEntry<AttackSource> entry = GetSaveAttackSourceEntry(crestId, attackSlot);
            return entry == null ? GetDefaultAttackSourceForCrest(crestId) : entry.Value;
        }

        internal AttackSource CycleSaveAttackSource(string crestId, AttackSlot attackSlot)
        {
            return CycleSaveAttackSource(crestId, attackSlot, 1);
        }

        internal AttackSource CycleSaveAttackSource(string crestId, AttackSlot attackSlot, int direction)
        {
            crestId = NormalizeCrestId(crestId);
            SaveScopedConfigEntry<AttackSource> entry = GetSaveAttackSourceEntry(crestId, attackSlot);
            if (entry == null)
            {
                return GetDefaultAttackSourceForCrest(crestId);
            }

            List<AttackSource> sources = GetAvailableAttackSources(crestId);
            AttackSource current = entry.Value;
            int index = sources.IndexOf(current);
            int step = direction < 0 ? -1 : 1;
            entry.Value = sources[(index + step + sources.Count) % sources.Count];
            if (crestId == NormalizeCrestId(GetEquippedCrestId()))
            {
                AttackOverrideController.Apply(HeroController.UnsafeInstance);
            }

            return entry.Value;
        }

        internal bool ResetSaveAttackSources(string crestId)
        {
            crestId = NormalizeCrestId(crestId);
            CrestAttackSourceConfig crestConfig = GetCrestAttackSourceConfig(crestId);
            if (crestConfig == null)
            {
                return false;
            }

            AttackSource defaultSource = GetDefaultAttackSourceForCrest(crestId);
            bool changed = false;
            changed |= SetAttackSourceIfDifferent(crestConfig.Neutral, defaultSource);
            changed |= SetAttackSourceIfDifferent(crestConfig.Up, defaultSource);
            changed |= SetAttackSourceIfDifferent(crestConfig.Down, defaultSource);
            changed |= SetAttackSourceIfDifferent(crestConfig.Dash, defaultSource);
            changed |= SetAttackSourceIfDifferent(crestConfig.NeedleStrike, defaultSource);
            if (crestId == NormalizeCrestId(GetEquippedCrestId()))
            {
                AttackOverrideController.Apply(HeroController.UnsafeInstance);
            }

            return changed;
        }

        internal static string GetAttackSourceDisplayName(AttackSource attackSource)
        {
            switch (attackSource)
            {
                case AttackSource.Hunter:
                    return "Hunter";
                case AttackSource.Reaper:
                    return "Reaper";
                case AttackSource.Wanderer:
                    return "Wanderer";
                case AttackSource.Beast:
                    return "Beast";
                case AttackSource.Witch:
                    return "Witch";
                case AttackSource.Architect:
                    return "Architect";
                case AttackSource.Shaman:
                    return "Shaman";
                default:
                    return attackSource.ToString();
            }
        }

        private SaveScopedConfigEntry<AttackSource> GetSaveAttackSourceEntry(string crestId, AttackSlot attackSlot)
        {
            CrestAttackSourceConfig crestConfig = GetCrestAttackSourceConfig(crestId);
            if (crestConfig == null)
            {
                return null;
            }

            switch (attackSlot)
            {
                case AttackSlot.Neutral:
                    return crestConfig.Neutral;
                case AttackSlot.Up:
                    return crestConfig.Up;
                case AttackSlot.Down:
                    return crestConfig.Down;
                case AttackSlot.Dash:
                    return crestConfig.Dash;
                case AttackSlot.NeedleStrike:
                    return crestConfig.NeedleStrike;
                default:
                    return null;
            }
        }

        private CrestAttackSourceConfig GetCrestAttackSourceConfig(string crestId)
        {
            crestId = NormalizeCrestId(crestId);
            if (string.IsNullOrEmpty(crestId) || saveConfig == null || !IsSupportedCrestId(crestId))
            {
                return null;
            }

            CrestAttackSourceConfig crestConfig;
            if (saveAttackSourcesByCrest.TryGetValue(crestId, out crestConfig))
            {
                return crestConfig;
            }

            int crestOrder = GetCrestOrder(crestId);
            string section = FormatCrestSection(crestId, crestOrder);
            AttackSource defaultSource = GetDefaultAttackSourceForCrest(crestId);
            crestConfig = new CrestAttackSourceConfig
            {
                Neutral = saveConfig.Bind(
                    section,
                    "Neutral Attack",
                    defaultSource,
                    CreateAttackSourceDescription("Crest to copy neutral attack behavior from for " + GetCrestDisplayName(crestId) + " on this save.", crestOrder, 10)),
                Up = saveConfig.Bind(
                    section,
                    "Up Attack",
                    defaultSource,
                    CreateAttackSourceDescription("Crest to copy up attack behavior from for " + GetCrestDisplayName(crestId) + " on this save.", crestOrder, 30)),
                Down = saveConfig.Bind(
                    section,
                    "Down Attack",
                    defaultSource,
                    CreateAttackSourceDescription("Crest to copy down attack behavior from for " + GetCrestDisplayName(crestId) + " on this save.", crestOrder, 20)),
                Dash = saveConfig.Bind(
                    section,
                    "Dash Attack",
                    defaultSource,
                    CreateAttackSourceDescription("Crest to copy dash attack behavior from for " + GetCrestDisplayName(crestId) + " on this save.", crestOrder, 40)),
                NeedleStrike = saveConfig.Bind(
                    section,
                    "Needle Strike",
                    defaultSource,
                    CreateAttackSourceDescription("Crest to copy Needle Strike behavior from for " + GetCrestDisplayName(crestId) + " on this save.", crestOrder, 50)),
            };
            saveAttackSourcesByCrest[crestId] = crestConfig;
            return crestConfig;
        }

        private void BindKnownCrestConfigs()
        {
            for (int i = 0; i < KnownCrestIds.Length; i++)
            {
                GetCrestAttackSourceConfig(KnownCrestIds[i]);
            }
        }

        private static SaveScopedConfigDescription CreateAttackSourceDescription(string description, int crestOrder, int entryOrder)
        {
            return new SaveScopedConfigDescription(
                description,
                new AcceptableEnumList<AttackSource>(SelectableAttackSources))
            {
                Order = () => GetSaveSlotOrderBase() + (SelectableAttackSources.Length - crestOrder) * 100 + entryOrder,
            };
        }

        private static bool SetAttackSourceIfDifferent(SaveScopedConfigEntry<AttackSource> entry, AttackSource source)
        {
            if (entry == null || entry.Value == source)
            {
                return false;
            }

            entry.Value = source;
            return true;
        }

        internal static string NormalizeCrestId(string crestId)
        {
            crestId = string.IsNullOrEmpty(crestId) ? GetEquippedCrestId() : crestId;
            AttackSource source;
            return TryGetAttackSourceForCrestName(crestId, out source)
                ? GetCanonicalCrestId(source)
                : crestId;
        }

        internal static string GetEquippedCrestId()
        {
            PlayerData playerData = GameManager.instance == null ? null : GameManager.instance.playerData;
            return playerData == null ? null : playerData.CurrentCrestID;
        }

        private static string GetCrestDisplayName(string crestId)
        {
            if (string.IsNullOrEmpty(crestId))
            {
                return "Selected Crest";
            }

            AttackSource source;
            return TryGetAttackSourceForCrestName(crestId, out source)
                ? GetAttackSourceDisplayName(source)
                : crestId;
        }

        private static string FormatCrestSection(string crestId, int crestOrder)
        {
            return "Attack Sources: " + GetCrestDisplayName(crestId);
        }

        private static int GetCrestOrder(string crestId)
        {
            AttackSource source = GetDefaultAttackSourceForCrest(crestId);
            for (int i = 0; i < SelectableAttackSources.Length; i++)
            {
                if (SelectableAttackSources[i] == source)
                {
                    return i;
                }
            }

            return SelectableAttackSources.Length;
        }

        private static int GetSaveSlotOrderBase()
        {
            int saveSlot = SaveScopedConfig.ActiveSaveSlot;
            if (saveSlot < 1 || saveSlot > 4)
            {
                saveSlot = 4;
            }

            return saveSlot * 10000;
        }

        private static AttackSource GetDefaultAttackSourceForCrest(string crestId)
        {
            AttackSource source;
            return TryGetAttackSourceForCrestName(crestId, out source)
                ? source
                : AttackSource.Hunter;
        }

        internal static bool IsSupportedCrestId(string crestId)
        {
            AttackSource source;
            return TryGetAttackSourceForCrestName(crestId, out source);
        }

        private static string GetCanonicalCrestId(AttackSource source)
        {
            switch (source)
            {
                case AttackSource.Hunter:
                    return "Default";
                case AttackSource.Reaper:
                    return "Reaper";
                case AttackSource.Wanderer:
                    return "Wanderer";
                case AttackSource.Beast:
                    return "Beast";
                case AttackSource.Witch:
                    return "Witch";
                case AttackSource.Architect:
                    return "Architect";
                case AttackSource.Shaman:
                    return "Shaman";
                default:
                    return source.ToString();
            }
        }

        private static List<AttackSource> GetAvailableAttackSources(string selectedCrestId)
        {
            var sources = new List<AttackSource>();

            List<ToolCrest> crests = ToolItemManager.GetAllCrests();
            if (crests == null)
            {
                AddAllAttackSources(sources);
                return sources;
            }

            bool allowLocked = Plugin.Instance != null &&
                Plugin.Instance.allowLockedAttackSources != null &&
                Plugin.Instance.allowLockedAttackSources.Value;
            for (int i = 0; i < SelectableAttackSources.Length; i++)
            {
                AttackSource source = SelectableAttackSources[i];
                if ((allowLocked || IsAttackSourceVisible(crests, source)) && !sources.Contains(source))
                {
                    sources.Add(source);
                }
            }

            if (sources.Count == 0)
            {
                AddAllAttackSources(sources);
            }

            return sources;
        }

        private static void AddAllAttackSources(List<AttackSource> sources)
        {
            for (int i = 0; i < SelectableAttackSources.Length; i++)
            {
                AttackSource source = SelectableAttackSources[i];
                if (!sources.Contains(source))
                {
                    sources.Add(source);
                }
            }
        }

        private static bool IsAttackSourceVisible(List<ToolCrest> crests, AttackSource source)
        {
            for (int i = 0; i < crests.Count; i++)
            {
                ToolCrest crest = crests[i];
                AttackSource crestSource;
                if (crest != null &&
                    crest.IsVisible &&
                    TryGetAttackSourceForCrestName(crest.name, out crestSource) &&
                    crestSource == source)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetAttackSourceForCrestName(string crestName, out AttackSource source)
        {
            source = AttackSource.Hunter;
            if (string.IsNullOrEmpty(crestName))
            {
                return false;
            }

            if (crestName.IndexOf("Hunter", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                crestName.StartsWith("Default", System.StringComparison.OrdinalIgnoreCase))
            {
                source = AttackSource.Hunter;
                return true;
            }

            switch (crestName)
            {
                case "Default":
                case "Hunter":
                case "Hunter_V1":
                case "Hunter_V2":
                case "Hunter_V3":
                case "HunterUpg":
                case "HunterUpgraded":
                    source = AttackSource.Hunter;
                    return true;
                case "Reaper":
                case "Scythe":
                    source = AttackSource.Reaper;
                    return true;
                case "Wanderer":
                case "Dagger":
                    source = AttackSource.Wanderer;
                    return true;
                case "Warrior":
                case "Beast":
                    source = AttackSource.Beast;
                    return true;
                case "Witch":
                case "Whip":
                    source = AttackSource.Witch;
                    return true;
                case "Toolmaster":
                case "Architect":
                    source = AttackSource.Architect;
                    return true;
                case "Spell":
                case "Shaman":
                    source = AttackSource.Shaman;
                    return true;
                default:
                    return false;
            }
        }

        private AttackSource GetEffectiveAttackSource(string crestId, AttackSlot attackSlot)
        {
            return GetSaveAttackSource(crestId, attackSlot);
        }

        private void OnDestroy()
        {
            AttackOverrideController.Clear();
            if (saveConfig != null)
            {
                saveConfig.Dispose();
                saveConfig = null;
            }

            Harmony.UnpatchSelf();
            Instance = null;
            Log = null;
        }

        private sealed class CrestAttackSourceConfig
        {
            internal SaveScopedConfigEntry<AttackSource> Neutral;
            internal SaveScopedConfigEntry<AttackSource> Up;
            internal SaveScopedConfigEntry<AttackSource> Down;
            internal SaveScopedConfigEntry<AttackSource> Dash;
            internal SaveScopedConfigEntry<AttackSource> NeedleStrike;
        }

        [HarmonyPatch(typeof(HeroController), "SetConfigGroup")]
        private static class HeroControllerSetConfigGroupPatch
        {
            private static void Postfix(HeroController __instance)
            {
                AttackOverrideController.Apply(__instance);
            }
        }

        [HarmonyPatch(typeof(HeroController), "Config", MethodType.Getter)]
        private static class HeroControllerConfigPatch
        {
            private static void Postfix(HeroController __instance, ref HeroControllerConfig __result)
            {
                HeroControllerConfig config;
                if (AttackOverrideController.TryGetActiveConfig(__instance, out config))
                {
                    __result = config;
                }
            }
        }

        [HarmonyPatch(typeof(HeroController), "Attack")]
        private static class HeroControllerAttackPatch
        {
            private static void Prefix(HeroController __instance, AttackDirection attackDir)
            {
                AttackOverrideController.BeginAttackOverride(__instance, attackDir);
            }

            private static void Postfix()
            {
                AttackOverrideController.EndAttackOverride();
            }
        }

        [HarmonyPatch(typeof(HeroController), "Update")]
        private static class HeroControllerUpdatePatch
        {
            private static void Postfix(HeroController __instance)
            {
                AttackOverrideController.ClearAnimationConfigIfReady(__instance);
                AttackOverrideController.RestoreAttackSpoofIfReady(__instance);
            }
        }

        [HarmonyPatch(typeof(HeroAnimationController), "GetClip")]
        private static class HeroAnimationControllerGetClipPatch
        {
            private static void Postfix(HeroAnimationController __instance, string clipName, ref tk2dSpriteAnimationClip __result)
            {
                tk2dSpriteAnimationClip clip;
                if (AttackOverrideController.TryGetActiveAnimationClip(__instance, clipName, out clip))
                {
                    __result = clip;
                }
            }
        }

        [HarmonyPatch(typeof(InventoryToolCrestList), "Update")]
        private static class InventoryToolCrestListUpdatePatch
        {
            private static bool Prefix()
            {
                if (InventoryAttackSourcePanel.IsAnyAttackSwitcherOpen)
                {
                    Log?.LogInfo("Custom Crest blocking InventoryToolCrestList.Update because attack switcher is open.");
                    return false;
                }

                bool consumeCancel = InventoryAttackSourcePanel.ConsumePendingCrestSwitcherCancel();
                if (consumeCancel)
                {
                    Log?.LogInfo("Custom Crest consumed pending crest switcher cancel in InventoryToolCrestList.Update.");
                }

                return !consumeCancel;
            }

            private static void Postfix(InventoryToolCrestList __instance)
            {
                InventoryAttackSourcePanel.AttachOrRefresh(__instance);
            }
        }

        [HarmonyPatch(typeof(InventoryToolCrestList), "StopSwitchingCrests")]
        private static class InventoryToolCrestListStopSwitchingCrestsPatch
        {
            private static bool Prefix(bool keepNewSelection)
            {
                Log?.LogInfo(
                    "Custom Crest StopSwitchingCrests prefix: keepNewSelection=" + keepNewSelection +
                    ", attackSwitcherOpen=" + InventoryAttackSourcePanel.IsAnyAttackSwitcherOpen);
                if (InventoryAttackSourcePanel.ConsumePendingCrestSwitcherCancel())
                {
                    Log?.LogInfo("Custom Crest StopSwitchingCrests blocked by pending cancel consumption.");
                    return false;
                }

                if (!InventoryAttackSourcePanel.IsAnyAttackSwitcherOpen)
                {
                    Log?.LogInfo("Custom Crest StopSwitchingCrests allowing vanilla because attack switcher is not open.");
                    return true;
                }

                if (keepNewSelection)
                {
                    Log?.LogInfo("Custom Crest StopSwitchingCrests allowing vanilla apply while attack switcher is open.");
                    return true;
                }

                if (!keepNewSelection)
                {
                    InventoryAttackSourcePanel.CloseOpenAttackSwitcher();
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryToolCrestList), "ApplyCurrentCrest")]
        private static class InventoryToolCrestListApplyCurrentCrestPatch
        {
            private static bool Prefix(InventoryToolCrestList __instance)
            {
                string currentCrest = __instance.CurrentCrest == null ? "<null>" : __instance.CurrentCrest.gameObject.name;
                Log?.LogInfo(
                    "Custom Crest ApplyCurrentCrest prefix: currentCrest=" + currentCrest +
                    ", equippedBefore=" + GetEquippedCrestId() +
                    ", attackSwitcherOpen=" + InventoryAttackSourcePanel.IsAnyAttackSwitcherOpen +
                    ", isSwitching=" + __instance.IsSwitchingCrests);
                return true;
            }

            private static void Postfix(InventoryToolCrestList __instance)
            {
                string currentCrest = __instance.CurrentCrest == null ? "<null>" : __instance.CurrentCrest.gameObject.name;
                Log?.LogInfo(
                    "Custom Crest ApplyCurrentCrest postfix: currentCrest=" + currentCrest +
                    ", equippedAfter=" + GetEquippedCrestId() +
                    ", attackSwitcherOpen=" + InventoryAttackSourcePanel.IsAnyAttackSwitcherOpen +
                    ", isSwitching=" + __instance.IsSwitchingCrests);
            }
        }

        [HarmonyPatch(typeof(InventoryPaneInput), "PressCancel")]
        private static class InventoryPaneInputPressCancelPatch
        {
            private static bool Prefix()
            {
                bool handled = InventoryAttackSourcePanel.CloseOpenAttackSwitcher();
                if (handled)
                {
                    Log?.LogInfo("Custom Crest handled InventoryPaneInput.PressCancel by closing attack switcher.");
                }

                return !handled;
            }
        }

        [HarmonyPatch(typeof(InventoryPaneInput), "PressDirection")]
        private static class InventoryPaneInputPressDirectionPatch
        {
            private static bool Prefix(InventoryPaneBase.InputEventType direction)
            {
                return !InventoryAttackSourcePanel.MoveOpenAttackSwitcher(direction);
            }
        }

        [HarmonyPatch(typeof(InventoryPaneInput), "PressSubmit")]
        private static class InventoryPaneInputPressSubmitPatch
        {
            private static bool Prefix()
            {
                bool prepared = InventoryAttackSourcePanel.PrepareVanillaCrestSubmitFromOpenAttackSwitcher();
                if (prepared)
                {
                    Log?.LogInfo("Custom Crest scheduled vanilla crest apply from attack switcher.");
                }

                return !prepared;
            }
        }

        [HarmonyPatch(typeof(InventoryItemToolManager), "ShowCrestEquipMsg")]
        private static class InventoryItemToolManagerShowCrestEquipMsgPatch
        {
            private static void Prefix(InventoryItemToolManager __instance)
            {
                SetCrestBenchMessageText(
                    __instance,
                    showAttackBenchMessageOnce ? AttackBenchMessage : CrestBenchMessage);
                showAttackBenchMessageOnce = false;
            }
        }
    }
}
