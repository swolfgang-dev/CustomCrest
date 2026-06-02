using GlobalEnums;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CrestLoadouts
{
    internal static class AttackOverrideController
    {
        private static readonly FieldInfo ConfigsField = AccessTools.Field(typeof(HeroController), "configs");
        private static readonly FieldInfo CrestConfigField = AccessTools.Field(typeof(HeroController), "crestConfig");
        private static readonly FieldInfo CurrentConfigGroupField = AccessTools.Field(typeof(HeroController), "<CurrentConfigGroup>k__BackingField");
        private static readonly FieldInfo AnimationHeroControllerField = AccessTools.Field(typeof(HeroAnimationController), "heroCtrl");

        private static readonly FieldInfo NormalSlashField = AccessTools.Field(typeof(HeroController), "normalSlash");
        private static readonly FieldInfo NormalSlashDamagerField = AccessTools.Field(typeof(HeroController), "normalSlashDamager");
        private static readonly FieldInfo AlternateSlashField = AccessTools.Field(typeof(HeroController), "alternateSlash");
        private static readonly FieldInfo AlternateSlashDamagerField = AccessTools.Field(typeof(HeroController), "alternateSlashDamager");
        private static readonly FieldInfo UpSlashField = AccessTools.Field(typeof(HeroController), "upSlash");
        private static readonly FieldInfo UpSlashDamagerField = AccessTools.Field(typeof(HeroController), "upSlashDamager");
        private static readonly FieldInfo AltUpSlashField = AccessTools.Field(typeof(HeroController), "altUpSlash");
        private static readonly FieldInfo AltUpSlashDamagerField = AccessTools.Field(typeof(HeroController), "altUpSlashDamager");
        private static readonly FieldInfo DownSpikeField = AccessTools.Field(typeof(HeroController), "downSpike");
        private static readonly FieldInfo DownSlashField = AccessTools.Field(typeof(HeroController), "downSlash");
        private static readonly FieldInfo DownSlashDamagerField = AccessTools.Field(typeof(HeroController), "downSlashDamager");
        private static readonly FieldInfo AltDownSpikeField = AccessTools.Field(typeof(HeroController), "altDownSpike");
        private static readonly FieldInfo AltDownSlashField = AccessTools.Field(typeof(HeroController), "altDownSlash");
        private static readonly FieldInfo AltDownSlashDamagerField = AccessTools.Field(typeof(HeroController), "altDownSlashDamager");
        private static readonly FieldInfo DashStabField = AccessTools.Field(typeof(HeroController.ConfigGroup), "DashStab");
        private static readonly FieldInfo DashStabAltField = AccessTools.Field(typeof(HeroController.ConfigGroup), "DashStabAlt");
        private static readonly FieldInfo ChargeSlashField = AccessTools.Field(typeof(HeroController.ConfigGroup), "ChargeSlash");

        private static readonly HashSet<GameObject> ActivatedSourceRoots = new HashSet<GameObject>();
        private static readonly Stack<AttackContext> AttackContexts = new Stack<AttackContext>();
        private static AttackContext activeSpoofContext;

        private static HeroController downConfigHero;
        private static HeroControllerConfig downConfig;
        private static int downConfigDepth;
        private static HeroController animationConfigHero;
        private static HeroControllerConfig animationConfig;

        internal static void Apply(HeroController heroController)
        {
            if (Plugin.Instance == null || heroController == null)
            {
                return;
            }

            if (!Plugin.IsSupportedCrestId(Plugin.GetEquippedCrestId()))
            {
                Clear();
                return;
            }

            var usedRoots = new HashSet<GameObject>();
            ApplyNeutral(heroController, ResolveSourceGroup(heroController, Plugin.Instance.NeutralAttackSource), usedRoots);
            ApplyUp(heroController, ResolveSourceGroup(heroController, Plugin.Instance.UpAttackSource), usedRoots);
            ApplyDash(heroController, ResolveSourceGroup(heroController, Plugin.Instance.DashAttackSource), usedRoots);
            ApplyNeedleStrike(heroController, ResolveSourceGroup(heroController, Plugin.Instance.NeedleStrikeSource), usedRoots);

            HeroController.ConfigGroup downSourceGroup = ResolveSourceGroup(heroController, Plugin.Instance.DownAttackSource);
            if (downSourceGroup != null && downSourceGroup.Config != null && downSourceGroup.Config.DownSlashType != HeroControllerConfig.DownSlashTypes.Custom)
            {
                ApplyDown(heroController, downSourceGroup, usedRoots);
            }

            SyncSourceRoots(heroController.CurrentConfigGroup, usedRoots);
        }

        internal static void BeginAttackOverride(HeroController heroController, AttackDirection attackDirection)
        {
            ClearAnimationConfigIfReady(heroController);
            HeroController.ConfigGroup sourceGroup = ResolveSourceGroup(heroController, GetAttackSource(attackDirection));
            if (heroController == null || sourceGroup == null || sourceGroup.Config == null)
            {
                AttackContexts.Push(null);
                return;
            }

            animationConfigHero = heroController;
            animationConfig = sourceGroup.Config;
            ActivateSourceRoot(sourceGroup, heroController.CurrentConfigGroup);

            if (attackDirection != AttackDirection.downward)
            {
                AttackContexts.Push(null);
                return;
            }

            if (sourceGroup.Config.DownSlashType == HeroControllerConfig.DownSlashTypes.Custom)
            {
                BeginCustomDownSpoof(heroController, sourceGroup);
                return;
            }

            downConfigHero = heroController;
            downConfig = sourceGroup.Config;
            downConfigDepth++;
            ApplyDown(heroController, sourceGroup, null);
            AttackContexts.Push(new AttackContext
            {
                HeroController = heroController,
                SourceGroup = sourceGroup,
                Mode = AttackOverrideMode.SimpleDownConfig,
            });
        }

        internal static void EndAttackOverride()
        {
            if (AttackContexts.Count == 0)
            {
                return;
            }

            AttackContext context = AttackContexts.Pop();
            if (context == null)
            {
                return;
            }

            if (context.Mode == AttackOverrideMode.SimpleDownConfig)
            {
                if (downConfigDepth > 0)
                {
                    downConfigDepth--;
                }
                return;
            }

            if (context.Mode == AttackOverrideMode.CustomDownSpoof && context.HeroController != null)
            {
                activeSpoofContext = context;
                SoftRestoreAttackSpoof(context);
                ActivateSourceRoot(context.SourceGroup, context.HeroController.CurrentConfigGroup);
                RestoreAttackSpoofIfReady(context.HeroController);
            }
        }

        internal static void RestoreAttackSpoofIfReady(HeroController heroController)
        {
            if (activeSpoofContext == null ||
                heroController == null ||
                heroController != activeSpoofContext.HeroController ||
                Time.time < activeSpoofContext.RestoreTime ||
                IsAnyAttackState(heroController))
            {
                return;
            }

            SoftRestoreAttackSpoof(activeSpoofContext);
            activeSpoofContext = null;
            downConfigHero = null;
            downConfig = null;
            downConfigDepth = 0;
            ClearAnimationConfigIfReady(heroController);
            Apply(heroController);
        }

        internal static bool TryGetActiveConfig(HeroController heroController, out HeroControllerConfig config)
        {
            config = null;
            if (heroController == null || heroController != downConfigHero || downConfig == null)
            {
                return false;
            }

            if (downConfigDepth > 0 || IsDownAttackState(heroController))
            {
                config = downConfig;
                return true;
            }

            return false;
        }

        internal static bool TryGetActiveAnimationClip(HeroAnimationController animationController, string clipName, out tk2dSpriteAnimationClip clip)
        {
            clip = null;
            if (animationController == null ||
                animationConfigHero == null ||
                animationConfig == null ||
                !IsAttackAnimationClip(clipName))
            {
                return false;
            }

            HeroController heroController = AnimationHeroControllerField == null
                ? HeroController.UnsafeInstance
                : AnimationHeroControllerField.GetValue(animationController) as HeroController;
            if (heroController != animationConfigHero || !IsAnyAttackState(heroController))
            {
                return false;
            }

            clip = animationConfig.GetAnimationClip(clipName);
            return clip != null;
        }

        internal static void ClearAnimationConfigIfReady(HeroController heroController)
        {
            if (heroController != null &&
                heroController == animationConfigHero &&
                !IsAnyAttackState(heroController))
            {
                animationConfigHero = null;
                animationConfig = null;
            }
        }

        private static void BeginCustomDownSpoof(HeroController heroController, HeroController.ConfigGroup sourceGroup)
        {
            var context = new AttackContext
            {
                HeroController = heroController,
                OriginalConfig = activeSpoofContext == null
                    ? CrestConfigField == null ? null : CrestConfigField.GetValue(heroController) as HeroControllerConfig
                    : activeSpoofContext.OriginalConfig,
                OriginalGroup = activeSpoofContext == null ? heroController.CurrentConfigGroup : activeSpoofContext.OriginalGroup,
                SourceGroup = sourceGroup,
                RestoreTime = Time.time + GetSpoofHoldTime(sourceGroup.Config),
                Mode = AttackOverrideMode.CustomDownSpoof,
            };

            downConfigHero = heroController;
            downConfig = sourceGroup.Config;
            downConfigDepth++;
            AttackContexts.Push(context);
            ActivateSourceRoot(sourceGroup, heroController.CurrentConfigGroup);
            CrestConfigField?.SetValue(heroController, sourceGroup.Config);
        }

        private static void SoftRestoreAttackSpoof(AttackContext context)
        {
            if (context == null || context.HeroController == null)
            {
                return;
            }

            CrestConfigField?.SetValue(context.HeroController, context.OriginalConfig);
            CurrentConfigGroupField?.SetValue(context.HeroController, context.OriginalGroup);
            if (context.OriginalGroup != null && context.OriginalGroup.ActiveRoot != null)
            {
                context.OriginalGroup.ActiveRoot.SetActive(true);
            }
        }

        private static void ApplyNeutral(HeroController heroController, HeroController.ConfigGroup sourceGroup, HashSet<GameObject> usedRoots)
        {
            if (sourceGroup == null)
            {
                return;
            }

            AddSourceRoot(sourceGroup, usedRoots);
            NormalSlashField?.SetValue(heroController, sourceGroup.NormalSlash);
            NormalSlashDamagerField?.SetValue(heroController, sourceGroup.NormalSlashDamager);
            AlternateSlashField?.SetValue(heroController, sourceGroup.AlternateSlash);
            AlternateSlashDamagerField?.SetValue(heroController, sourceGroup.AlternateSlashDamager);
        }

        private static void ApplyUp(HeroController heroController, HeroController.ConfigGroup sourceGroup, HashSet<GameObject> usedRoots)
        {
            if (sourceGroup == null)
            {
                return;
            }

            AddSourceRoot(sourceGroup, usedRoots);
            UpSlashField?.SetValue(heroController, sourceGroup.UpSlash);
            UpSlashDamagerField?.SetValue(heroController, sourceGroup.UpSlashDamager);
            AltUpSlashField?.SetValue(heroController, sourceGroup.AltUpSlash);
            AltUpSlashDamagerField?.SetValue(heroController, sourceGroup.AltUpSlashDamager);
        }

        private static void ApplyDown(HeroController heroController, HeroController.ConfigGroup sourceGroup, HashSet<GameObject> usedRoots)
        {
            if (sourceGroup == null)
            {
                return;
            }

            AddSourceRoot(sourceGroup, usedRoots);
            DownSpikeField?.SetValue(heroController, sourceGroup.Downspike);
            DownSlashField?.SetValue(heroController, sourceGroup.DownSlash);
            DownSlashDamagerField?.SetValue(heroController, sourceGroup.DownSlashDamager);
            AltDownSpikeField?.SetValue(heroController, sourceGroup.AltDownspike);
            AltDownSlashField?.SetValue(heroController, sourceGroup.AltDownSlash);
            AltDownSlashDamagerField?.SetValue(heroController, sourceGroup.AltDownSlashDamager);
        }

        private static void ApplyDash(HeroController heroController, HeroController.ConfigGroup sourceGroup, HashSet<GameObject> usedRoots)
        {
            if (sourceGroup == null || heroController == null || heroController.CurrentConfigGroup == null)
            {
                return;
            }

            AddSourceRoot(sourceGroup, usedRoots);
            DashStabField?.SetValue(heroController.CurrentConfigGroup, DashStabField.GetValue(sourceGroup));
            DashStabAltField?.SetValue(heroController.CurrentConfigGroup, DashStabAltField.GetValue(sourceGroup));
            CopyConfigFields(sourceGroup.Config, heroController.Config, "dashStabSpeed", "dashStabTime", "forceShortDashStabBounce", "dashStabBounceJumpSpeed", "dashStabSteps", "canHarpoonDash");
        }

        private static void ApplyNeedleStrike(HeroController heroController, HeroController.ConfigGroup sourceGroup, HashSet<GameObject> usedRoots)
        {
            if (sourceGroup == null || heroController == null || heroController.CurrentConfigGroup == null)
            {
                return;
            }

            AddSourceRoot(sourceGroup, usedRoots);
            ChargeSlashField?.SetValue(heroController.CurrentConfigGroup, ChargeSlashField.GetValue(sourceGroup));
            CopyConfigFields(sourceGroup.Config, heroController.Config, "chargeSlashRecoils", "chargeSlashLungeSpeed", "chargeSlashLungeDeceleration", "chargeSlashChain");
        }

        private static void CopyConfigFields(HeroControllerConfig source, HeroControllerConfig target, params string[] fieldNames)
        {
            if (source == null || target == null || fieldNames == null)
            {
                return;
            }

            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo field = AccessTools.Field(typeof(HeroControllerConfig), fieldNames[i]);
                if (field != null)
                {
                    field.SetValue(target, field.GetValue(source));
                }
            }
        }

        private static AttackSource GetAttackSource(AttackDirection attackDirection)
        {
            if (Plugin.Instance == null)
            {
                return AttackSource.Hunter;
            }

            switch (attackDirection)
            {
                case AttackDirection.normal:
                    return Plugin.Instance.NeutralAttackSource;
                case AttackDirection.upward:
                    return Plugin.Instance.UpAttackSource;
                case AttackDirection.downward:
                    return Plugin.Instance.DownAttackSource;
                default:
                    return AttackSource.Hunter;
            }
        }

        private static HeroController.ConfigGroup ResolveSourceGroup(HeroController heroController, AttackSource attackSource)
        {
            if (heroController == null)
            {
                return null;
            }

            HeroController.ConfigGroup[] configs = ConfigsField == null
                ? null
                : ConfigsField.GetValue(heroController) as HeroController.ConfigGroup[];
            if (configs == null)
            {
                return null;
            }

            string[] crestNames = GetInternalCrestNames(attackSource);
            for (int i = 0; i < configs.Length; i++)
            {
                HeroController.ConfigGroup config = configs[i];
                if (config == null || config.Config == null)
                {
                    continue;
                }

                for (int j = 0; j < crestNames.Length; j++)
                {
                    if (config.Config.name == crestNames[j] ||
                        (config.ActiveRoot != null && config.ActiveRoot.name == crestNames[j]))
                    {
                        return config;
                    }
                }
            }

            return null;
        }

        private static bool IsDownAttackState(HeroController heroController)
        {
            return heroController != null &&
                (heroController.cState.downSpikeAntic ||
                heroController.cState.downSpiking ||
                heroController.cState.downSpikeRecovery ||
                heroController.cState.downSpikeBouncing ||
                heroController.cState.downAttacking);
        }

        private static bool IsAnyAttackState(HeroController heroController)
        {
            return heroController != null &&
                (heroController.cState.attacking ||
                heroController.cState.upAttacking ||
                IsDownAttackState(heroController));
        }

        private static bool IsAttackAnimationClip(string clipName)
        {
            switch (clipName)
            {
                case "Slash":
                case "SlashAlt":
                case "Slash Land":
                case "Slash Land Run":
                case "Slash Land Run Alt":
                case "Slash To Run":
                case "UpSlash":
                case "UpSlashAlt":
                case "DownSlash":
                case "DownSlashAlt":
                case "DownSpike":
                case "DownSpike Antic":
                case "DownSpikeBounce 1":
                case "Downspike Recovery":
                case "Downspike Recovery Land":
                case "Charge":
                case "Charge Loop":
                case "Charge End":
                case "Charge Attack":
                case "ChargeSlash":
                case "ChargeSlashAlt":
                case "Needle Strike":
                    return true;
                default:
                    return clipName != null &&
                        (clipName.IndexOf("Charge", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        clipName.IndexOf("Needle", System.StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private static float GetSpoofHoldTime(HeroControllerConfig config)
        {
            if (config == null)
            {
                return 0.75f;
            }

            return Mathf.Max(0.75f, Mathf.Max(config.AttackDuration, config.AttackRecoveryTime) + 0.15f);
        }

        private static string[] GetInternalCrestNames(AttackSource attackSource)
        {
            switch (attackSource)
            {
                case AttackSource.Hunter:
                    return new[] { "Default", "Hunter", "Hunter_V1", "Hunter_V2", "Hunter_V3" };
                case AttackSource.Reaper:
                    return new[] { "Scythe", "Reaper" };
                case AttackSource.Wanderer:
                    return new[] { "Wanderer", "Dagger" };
                case AttackSource.Beast:
                    return new[] { "Warrior", "Beast" };
                case AttackSource.Witch:
                    return new[] { "Witch", "Whip" };
                case AttackSource.Architect:
                    return new[] { "Toolmaster", "Architect" };
                case AttackSource.Shaman:
                    return new[] { "Shaman", "Spell" };
                default:
                    return new[] { attackSource.ToString() };
            }
        }

        private static void AddSourceRoot(HeroController.ConfigGroup sourceGroup, HashSet<GameObject> usedRoots)
        {
            if (sourceGroup != null && sourceGroup.ActiveRoot != null && usedRoots != null)
            {
                usedRoots.Add(sourceGroup.ActiveRoot);
            }
        }

        private static void ActivateSourceRoot(HeroController.ConfigGroup sourceGroup, HeroController.ConfigGroup currentGroup)
        {
            if (sourceGroup == null || sourceGroup.ActiveRoot == null)
            {
                return;
            }

            GameObject currentRoot = currentGroup == null ? null : currentGroup.ActiveRoot;
            if (sourceGroup.ActiveRoot != currentRoot)
            {
                sourceGroup.ActiveRoot.SetActive(true);
                ActivatedSourceRoots.Add(sourceGroup.ActiveRoot);
            }
        }

        private static void SyncSourceRoots(HeroController.ConfigGroup currentGroup, HashSet<GameObject> usedRoots)
        {
            GameObject currentRoot = currentGroup == null ? null : currentGroup.ActiveRoot;
            foreach (GameObject root in ActivatedSourceRoots)
            {
                if (root != null && root != currentRoot && !usedRoots.Contains(root))
                {
                    root.SetActive(false);
                }
            }

            ActivatedSourceRoots.Clear();
            foreach (GameObject root in usedRoots)
            {
                if (root != null && root != currentRoot)
                {
                    root.SetActive(true);
                    ActivatedSourceRoots.Add(root);
                }
            }
        }

        internal static void Clear()
        {
            foreach (GameObject root in ActivatedSourceRoots)
            {
                if (root != null)
                {
                    root.SetActive(false);
                }
            }

            ActivatedSourceRoots.Clear();
            AttackContexts.Clear();
            activeSpoofContext = null;
            downConfigHero = null;
            downConfig = null;
            downConfigDepth = 0;
            animationConfigHero = null;
            animationConfig = null;
        }

        private enum AttackOverrideMode
        {
            SimpleDownConfig,
            CustomDownSpoof,
        }

        private sealed class AttackContext
        {
            internal HeroController HeroController;
            internal HeroControllerConfig OriginalConfig;
            internal HeroController.ConfigGroup OriginalGroup;
            internal HeroController.ConfigGroup SourceGroup;
            internal float RestoreTime;
            internal AttackOverrideMode Mode;
        }
    }
}
