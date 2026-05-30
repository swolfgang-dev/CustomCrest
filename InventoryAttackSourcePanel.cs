using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMProOld;
using UnityEngine;

namespace CustomCrest
{
    internal sealed class InventoryAttackSourcePanel : MonoBehaviour
    {
        private const string PanelName = "CustomCrestAttackSourcePanel";
        private const string AttackPromptName = "CustomCrestAttackSourcePrompt";
        private const string AttackPromptText = "Change Attacks";
        private const string DefaultPromptText = "Set to Default";
        private static readonly Vector3 PanelLocalPosition = new Vector3(-0.2f, -4.72f, -0.25f);
        private static readonly Vector3 SelectorPanelLocalPosition = new Vector3(-0.55f, -4.35f, -0.25f);
        private static readonly Vector3 AttackPromptOffset = new Vector3(0f, 1.15f, 0f);
        private static readonly Vector3 AttackPromptIconNudge = new Vector3(-0.2f, -0.96f, 0f);
        private const float AttackPromptIconScale = 1.34f;
        private const float AttackPromptKeyboardBoundsWidthScale = 0.58f;
        private const float CrestDescriptionWidthScale = 0.82f;
        private static readonly FieldInfo ActionButtonIconLabelField =
            typeof(ActionButtonIcon).GetField("label", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconLiveUpdateField =
            typeof(ActionButtonIcon).GetField("liveUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconTextContainerField =
            typeof(ActionButtonIcon).GetField("textContainer", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconActionField =
            typeof(ActionButtonIcon).GetField("action", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseSquareWidthField =
            typeof(ActionButtonIconBase).GetField("sqrWidth", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseSquareHeightField =
            typeof(ActionButtonIconBase).GetField("sqrHeight", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseSquareFontMaxField =
            typeof(ActionButtonIconBase).GetField("sqrFontMax", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseSquareFontMinField =
            typeof(ActionButtonIconBase).GetField("sqrFontMin", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseWideWidthField =
            typeof(ActionButtonIconBase).GetField("wideWidth", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseWideHeightField =
            typeof(ActionButtonIconBase).GetField("wideHeight", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseWideFontMaxField =
            typeof(ActionButtonIconBase).GetField("wideFontMax", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionButtonIconBaseWideFontMinField =
            typeof(ActionButtonIconBase).GetField("wideFontMin", BindingFlags.Instance | BindingFlags.NonPublic);
        private const float PositionLerpSpeed = 6.5f;
        private static readonly Vector3 RowScale = Vector3.one;
        private static readonly Vector2 ColliderSize = new Vector2(2.75f, 1.32f);
        private static readonly Vector2 SelectionFramePadding = new Vector2(0.08f, 0.06f);
        private static readonly Vector2 SelectionFrameNudge = new Vector2(0.18f, -0.08f);
        private static readonly FieldInfo CrestListChangeCrestButtonField =
            typeof(InventoryToolCrestList).GetField("changeCrestButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ToolManagerChangeCrestButtonField =
            typeof(InventoryItemToolManager).GetField("changeCrestButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ToolManagerToolListField =
            typeof(InventoryItemToolManager).GetField("toolList", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ToolManagerComboButtonPromptDisplayField =
            typeof(InventoryItemToolManager).GetField("comboButtonPromptDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestDescriptionDisplayField =
            typeof(InventoryToolCrestList).GetField("crestDescriptionDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo CrestListApplyCurrentCrestMethod =
            typeof(InventoryToolCrestList).GetMethod("ApplyCurrentCrest", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListIsWaitingForApplyField =
            typeof(InventoryToolCrestList).GetField("isWaitingForApply", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListPreviousEquippedCrestField =
            typeof(InventoryToolCrestList).GetField("previousEquippedCrest", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListQueuedPaneEndedField =
            typeof(InventoryToolCrestList).GetField("queuedPaneEnded", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListWasChangeCrestButtonPressedField =
            typeof(InventoryToolCrestList).GetField("wasChangeCrestButtonPressed", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListChangeCrestExitAudioField =
            typeof(InventoryToolCrestList).GetField("changeCrestExitAudio", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListSwitchSequenceRoutineField =
            typeof(InventoryToolCrestList).GetField("crestSwitchSequenceRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CrestListSwitchMoveRoutineField =
            typeof(InventoryToolCrestList).GetField("crestSwitchMoveRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Vector2 LabelBoxSize = new Vector2(3.4f, 0.7f);
        private static readonly Vector2 ValueBoxSize = new Vector2(2.3f, 0.7f);
        private const float LabelX = -1.4f;
        private const float ValueX = 0.65f;
        private const float LeftArrowX = -0.77f;
        private const float RightArrowX = 2.26f;
        private const float ArrowHeight = 0.32f;
        private const float ColumnNudgeStep = 0.04f;
        private const float PostEquipAttackPanelHoldSeconds = 1f;
        private const float PostCloseDescriptionSuppressSeconds = 1f;
        private const int DebugSpriteGridColumns = 22;
        private const int DebugSpriteGridMaxItems = 260;
        private static readonly Vector2 DebugSpriteGridCellSize = new Vector2(0.24f, 0.24f);
        private static readonly Vector3 DebugSpriteGridLocalPosition = new Vector3(-3.2f, 2.15f, -1.2f);
        private const float ColumnSpacing = 3.0f;
        private const float LeftColumnX = -1.9f;
        private const float RightColumnX = 1.54f;
        private InventoryToolCrestList crestList;
        private InventoryAttackSourceButton upButton;
        private InventoryAttackSourceButton downButton;
        private InventoryAttackSourceButton neutralButton;
        private InventoryAttackSourceButton dashButton;
        private InventoryAttackSourceButton needleStrikeButton;
        private InventoryAttackSourceButton selectedButton;
        private InventoryAttackSourceButton lastSelectedButton;
        private GameObject attackPrompt;
        private TMP_Text attackPromptLabel;
        private ActionButtonIcon attackPromptIcon;
        private TMP_Text attackPromptIconLabel;
        private TextContainer attackPromptIconTextContainer;
        private Transform attackPromptIconTransform;
        private GameObject comboButtonPromptObject;
        private Vector3 attackPromptControllerIconScale = Vector3.one;
        private HeroActionButton attackPromptKeyboardAction = HeroActionButton.MENU_SUBMIT;
        private bool? attackPromptIconKeyboardState;
        private float crestDescriptionBaseWidth;
        private bool attackSwitcherOpen;
        private GameObject debugSpriteGrid;
        private int pendingVanillaCrestApplyFrame = -1;
        private float pendingVanillaCrestCommitTime = -1f;
        private bool followCrestDescriptionClose;
        private float suppressCrestDescriptionUntilTime = -1f;
        private float panelAlpha = 1f;
        private static InventoryAttackSourcePanel openPanel;
        private static bool consumeNextCrestSwitcherCancel;

        public static void AttachOrRefresh(InventoryToolCrestList crestList)
        {
            if (Plugin.Instance == null || crestList == null || crestList.CurrentCrest == null)
            {
                return;
            }

            InventoryAttackSourcePanel panel = crestList.GetComponentInChildren<InventoryAttackSourcePanel>(true);
            if (!crestList.IsSwitchingCrests)
            {
                if (panel != null)
                {
                    if (panel.HandleClosedCrestSwitcherState())
                    {
                        panel.gameObject.SetActive(true);
                        return;
                    }

                    panel.CloseAttackSwitcher(false, false);
                    panel.gameObject.SetActive(false);
                }

                return;
            }

            if (!Plugin.IsSupportedCrestId(crestList.CurrentCrest.gameObject.name))
            {
                if (panel != null)
                {
                    panel.CloseAttackSwitcher(false);
                    panel.gameObject.SetActive(false);
                }

                return;
            }

            if (panel == null)
            {
                panel = CreatePanel(crestList, FindTemplateText(crestList));
                Plugin.Log?.LogInfo("Created inventory attack source panel.");
            }

            panel.Refresh();
        }

        public void Refresh()
        {
            gameObject.SetActive(crestList != null && (crestList.IsSwitchingCrests || ShouldFollowCrestSwitcherClose || ShouldSuppressCrestDescriptionAfterClose));
            if (crestList == null || !crestList.IsSwitchingCrests)
            {
                if (HandleClosedCrestSwitcherState())
                {
                    return;
                }

                CloseAttackSwitcher(false, false);
                gameObject.SetActive(false);
                return;
            }

            UpdatePosition();
            EnsureAttackPrompt();
            UpdateAttackPromptVisibility();
            UpdateComboButtonPromptVisibility();
            SetRowsActive(IsAttackSwitcherVisible);
            UpdateDescriptionVisibility();
            upButton?.RefreshText();
            downButton?.RefreshText();
            neutralButton?.RefreshText();
            dashButton?.RefreshText();
            needleStrikeButton?.RefreshText();
            FitSelectionFrameToPanel();
            RefreshSelectionDisplay();
            if (IsAttackSwitcherVisible)
            {
                RefreshNavigation();
            }
        }

        public void SetSelectedButton(InventoryAttackSourceButton button)
        {
            selectedButton = button;
            if (button != null)
            {
                lastSelectedButton = button;
            }

            RefreshSelectionDisplay();
        }

        public void ClearSelectedButton(InventoryAttackSourceButton button)
        {
            if (selectedButton != button)
            {
                return;
            }

            if (IsAttackSwitcherVisible)
            {
                lastSelectedButton = button;
                RefreshSelectionDisplay();
                return;
            }

            selectedButton = null;
            RefreshSelectionDisplay();
        }

        public static bool CloseOpenAttackSwitcher()
        {
            if (openPanel != null && openPanel.IsAttackSwitcherVisible)
            {
                Plugin.Log?.LogInfo("Custom Crest closing open attack switcher from openPanel.");
                openPanel.CloseAttackSwitcher(true);
                return true;
            }

            InventoryAttackSourcePanel[] panels = Resources.FindObjectsOfTypeAll<InventoryAttackSourcePanel>();
            for (int i = 0; i < panels.Length; i++)
            {
                InventoryAttackSourcePanel panel = panels[i];
                if (panel == null || !panel.IsAttackSwitcherVisible)
                {
                    continue;
                }

                Plugin.Log?.LogInfo("Custom Crest closing open attack switcher from scan: " + GetTransformPath(panel.transform));
                panel.CloseAttackSwitcher(true);
                return true;
            }

            return false;
        }

        public static bool IsAnyAttackSwitcherOpen
        {
            get { return (openPanel != null && openPanel.IsAttackSwitcherVisible) || FindOpenAttackSwitcher() != null; }
        }

        public static bool ConsumePendingCrestSwitcherCancel()
        {
            if (!consumeNextCrestSwitcherCancel)
            {
                return false;
            }

            consumeNextCrestSwitcherCancel = false;
            return true;
        }

        public static bool SubmitOpenAttackSwitcher()
        {
            InventoryAttackSourcePanel panel = openPanel != null && openPanel.IsAttackSwitcherVisible
                ? openPanel
                : FindOpenAttackSwitcher();
            if (panel == null)
            {
                return false;
            }

            InventoryAttackSourceButton button = panel.selectedButton ?? panel.GetPreferredPanelEntry();
            if (button != null)
            {
                button.Submit();
            }

            return true;
        }

        public static bool ApplyCurrentCrestFromOpenAttackSwitcher()
        {
            InventoryAttackSourcePanel panel = openPanel != null && openPanel.IsAttackSwitcherVisible
                ? openPanel
                : FindOpenAttackSwitcher();
            if (panel == null || panel.crestList == null)
            {
                Plugin.Log?.LogInfo(
                    "Custom Crest apply from attack switcher skipped: panel=" + (panel == null ? "<null>" : GetTransformPath(panel.transform)) +
                    ", crestList=" + (panel == null || panel.crestList == null ? "<null>" : GetTransformPath(panel.crestList.transform)));
                return false;
            }

            string beforeEquipped = Plugin.GetEquippedCrestId();
            string currentCrest = panel.crestList.CurrentCrest == null ? "<null>" : panel.crestList.CurrentCrest.gameObject.name;
            Plugin.Log?.LogInfo(
                "Custom Crest apply from attack switcher: currentCrest=" + currentCrest +
                ", selectedCrestId=" + panel.SelectedCrestId +
                ", equippedBefore=" + beforeEquipped +
                ", isSwitching=" + panel.crestList.IsSwitchingCrests);
            panel.CloseAttackSwitcher(false);
            panel.crestList.StopSwitchingCrests(true);
            Plugin.Log?.LogInfo(
                "Custom Crest apply from attack switcher complete: equippedAfter=" + Plugin.GetEquippedCrestId() +
                ", isSwitching=" + panel.crestList.IsSwitchingCrests);
            return true;
        }

        public static bool PrepareVanillaCrestSubmitFromOpenAttackSwitcher()
        {
            InventoryAttackSourcePanel panel = openPanel != null && openPanel.IsAttackSwitcherVisible
                ? openPanel
                : FindOpenAttackSwitcher();
            if (panel == null || panel.crestList == null)
            {
                return false;
            }

            string beforeEquipped = Plugin.GetEquippedCrestId();
            string currentCrest = panel.crestList.CurrentCrest == null ? "<null>" : panel.crestList.CurrentCrest.gameObject.name;
            Plugin.Log?.LogInfo(
                "Custom Crest preparing vanilla submit from attack switcher: currentCrest=" + currentCrest +
                ", selectedCrestId=" + panel.SelectedCrestId +
                ", equippedBefore=" + beforeEquipped +
                ", isSwitching=" + panel.crestList.IsSwitchingCrests);
            panel.pendingVanillaCrestApplyFrame = Time.frameCount + 1;
            Plugin.Log?.LogInfo(
                "Custom Crest prepared vanilla submit from attack switcher: attackSwitcherOpen=" + IsAnyAttackSwitcherOpen +
                ", isSwitching=" + panel.crestList.IsSwitchingCrests +
                ", pendingApplyFrame=" + panel.pendingVanillaCrestApplyFrame);
            return true;
        }

        public static bool MoveOpenAttackSwitcher(InventoryPaneBase.InputEventType direction)
        {
            InventoryAttackSourcePanel panel = openPanel != null && openPanel.IsAttackSwitcherVisible
                ? openPanel
                : FindOpenAttackSwitcher();
            if (panel == null)
            {
                return false;
            }

            panel.MoveSelection(direction);
            return true;
        }

        private static InventoryAttackSourcePanel FindOpenAttackSwitcher()
        {
            InventoryAttackSourcePanel[] panels = Resources.FindObjectsOfTypeAll<InventoryAttackSourcePanel>();
            for (int i = 0; i < panels.Length; i++)
            {
                InventoryAttackSourcePanel panel = panels[i];
                if (panel != null && panel.IsAttackSwitcherVisible)
                {
                    return panel;
                }
            }

            return null;
        }

        public string SelectedCrestId
        {
            get
            {
                InventoryToolCrest currentCrest = crestList == null ? null : crestList.CurrentCrest;
                if (currentCrest != null && !string.IsNullOrEmpty(currentCrest.gameObject.name))
                {
                    return currentCrest.gameObject.name;
                }

                return Plugin.GetEquippedCrestId();
            }
        }

        private void Update()
        {
            if (crestList == null || !crestList.IsSwitchingCrests)
            {
                pendingVanillaCrestApplyFrame = -1;
                pendingVanillaCrestCommitTime = -1f;
                if (HandleClosedCrestSwitcherState())
                {
                    return;
                }

                followCrestDescriptionClose = false;
                CloseAttackSwitcher(false, false);
                return;
            }

            if (RunPendingVanillaCrestApply())
            {
                return;
            }

            if (RunPendingVanillaCrestCommit())
            {
                return;
            }

            if (pendingVanillaCrestCommitTime >= 0f)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                RebuildDebugSpriteGrid();
                DumpAttackArrowDebug();
            }

            if (!attackSwitcherOpen)
            {
                if (IsOpenAttackSwitcherInputPressed())
                {
                    OpenAttackSwitcher();
                }

                return;
            }

            if (HandleRightColumnNudgeInput())
            {
                return;
            }

            if (!ShouldHandleDefaultInput() || !IsOpenAttackSwitcherInputPressed())
            {
                return;
            }

            if (Plugin.Instance == null)
            {
                return;
            }

            InventoryItemToolManager manager = GetComponentInParent<InventoryItemToolManager>();
            if (!Plugin.Instance.CanChangeAttackSources())
            {
                Plugin.ShowAttackBenchMessage(manager);
                return;
            }

            bool changed = Plugin.Instance.ResetSaveAttackSources(SelectedCrestId);
            Refresh();
            if (changed && manager != null)
            {
                manager.PlayMoveSound();
            }
        }

        private bool RunPendingVanillaCrestApply()
        {
            if (pendingVanillaCrestApplyFrame < 0 || Time.frameCount < pendingVanillaCrestApplyFrame)
            {
                return false;
            }

            int applyFrame = pendingVanillaCrestApplyFrame;
            pendingVanillaCrestApplyFrame = -1;
            if (CrestListApplyCurrentCrestMethod == null || CrestListIsWaitingForApplyField == null)
            {
                Plugin.Log?.LogWarning("Custom Crest cannot run pending vanilla crest apply: crest apply reflection failed.");
                return false;
            }

            string beforeEquipped = Plugin.GetEquippedCrestId();
            InventoryToolCrest currentCrestObject = crestList.CurrentCrest;
            InventoryToolCrest previousEquippedCrest = CrestListPreviousEquippedCrestField == null
                ? null
                : CrestListPreviousEquippedCrestField.GetValue(crestList) as InventoryToolCrest;
            string currentCrest = currentCrestObject == null ? "<null>" : currentCrestObject.gameObject.name;
            string previousCrest = previousEquippedCrest == null ? "<null>" : previousEquippedCrest.gameObject.name;
            Plugin.Log?.LogInfo(
                "Custom Crest running pending vanilla crest equip: scheduledFrame=" + applyFrame +
                ", frame=" + Time.frameCount +
                ", currentCrest=" + currentCrest +
                ", previousEquippedCrest=" + previousCrest +
                ", equippedBefore=" + beforeEquipped +
                ", isSwitching=" + crestList.IsSwitchingCrests);

            CrestListIsWaitingForApplyField.SetValue(crestList, true);
            if (currentCrestObject != null && currentCrestObject != previousEquippedCrest)
            {
                currentCrestObject.DoEquip(ApplyCurrentCrestAfterEquipAnimation);
            }
            else
            {
                ApplyCurrentCrestAfterEquipAnimation();
            }

            PlayVanillaCrestExitAudio();
            CrestListQueuedPaneEndedField?.SetValue(crestList, false);
            CrestListWasChangeCrestButtonPressedField?.SetValue(crestList, false);
            Plugin.Log?.LogInfo(
                "Custom Crest pending vanilla crest equip started: equippedAfter=" + Plugin.GetEquippedCrestId() +
                ", isSwitching=" + crestList.IsSwitchingCrests);
            return true;
        }

        private void ApplyCurrentCrestAfterEquipAnimation()
        {
            if (crestList == null || CrestListApplyCurrentCrestMethod == null)
            {
                return;
            }

            pendingVanillaCrestCommitTime = Time.unscaledTime + PostEquipAttackPanelHoldSeconds;
            Plugin.Log?.LogInfo(
                "Custom Crest holding attack panel after equip animation: commitTime=" + pendingVanillaCrestCommitTime +
                ", currentTime=" + Time.unscaledTime +
                ", isSwitching=" + crestList.IsSwitchingCrests);
        }

        private bool RunPendingVanillaCrestCommit()
        {
            if (pendingVanillaCrestCommitTime < 0f || Time.unscaledTime < pendingVanillaCrestCommitTime)
            {
                return false;
            }

            pendingVanillaCrestCommitTime = -1f;
            if (crestList == null || CrestListApplyCurrentCrestMethod == null)
            {
                return true;
            }

            Plugin.Log?.LogInfo(
                "Custom Crest applying crest after post-animation hold: equippedBefore=" + Plugin.GetEquippedCrestId() +
                ", isSwitching=" + crestList.IsSwitchingCrests);
            followCrestDescriptionClose = true;
            CrestListApplyCurrentCrestMethod.Invoke(crestList, null);
            SetCrestDescriptionVisible(false);

            Plugin.Log?.LogInfo(
                "Custom Crest applied crest after post-animation hold: equippedAfter=" + Plugin.GetEquippedCrestId() +
                ", isSwitching=" + (crestList != null && crestList.IsSwitchingCrests) +
                ", followCrestDescriptionClose=" + followCrestDescriptionClose +
                ", crestSwitchSequenceRunning=" + IsCrestSwitchSequenceRunning() +
                ", crestSwitchMoveRunning=" + IsCrestSwitchMoveRunning());
            return true;
        }

        private bool ShouldFollowCrestSwitcherClose
        {
            get { return followCrestDescriptionClose && IsCrestSwitchSequenceRunning() && panelAlpha > 0.01f; }
        }

        private bool ShouldSuppressCrestDescriptionAfterClose
        {
            get { return suppressCrestDescriptionUntilTime >= 0f && Time.unscaledTime < suppressCrestDescriptionUntilTime; }
        }

        private bool HandleClosedCrestSwitcherState()
        {
            if (ShouldFollowCrestSwitcherClose)
            {
                UpdateClosingPanelFade();
                return true;
            }

            if (followCrestDescriptionClose)
            {
                followCrestDescriptionClose = false;
                suppressCrestDescriptionUntilTime = Time.unscaledTime + PostCloseDescriptionSuppressSeconds;
                Plugin.Log?.LogInfo(
                    "Custom Crest suppressing crest description after close until " + suppressCrestDescriptionUntilTime +
                    " currentTime=" + Time.unscaledTime);
            }

            if (!ShouldSuppressCrestDescriptionAfterClose)
            {
                suppressCrestDescriptionUntilTime = -1f;
                return false;
            }

            HideAttackPanelAndSuppressCrestDescription();
            return true;
        }

        private void PlayVanillaCrestExitAudio()
        {
            if (CrestListChangeCrestExitAudioField == null)
            {
                return;
            }

            try
            {
                AudioEvent audioEvent = (AudioEvent)CrestListChangeCrestExitAudioField.GetValue(crestList);
                audioEvent.SpawnAndPlayOneShot(GlobalSettings.Audio.DefaultUIAudioSourcePrefab, transform.position, null);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning("Custom Crest could not play vanilla crest exit audio: " + ex.Message);
            }
        }

        private void LateUpdate()
        {
            if (IsAttackSwitcherVisible || ShouldFollowCrestSwitcherClose)
            {
                UpdateClosingPanelFade();
            }
            else if (ShouldSuppressCrestDescriptionAfterClose)
            {
                HideAttackPanelAndSuppressCrestDescription();
            }

            if (attackPrompt == null || !attackPrompt.activeInHierarchy || !IsKeyboardInputActive())
            {
                return;
            }

            SetAttackPromptIconRenderersEnabled(true);
            SetAttackPromptIconTextContainerEnabled(true);
            SetAttackPromptIconKeyboardScale(true);
            ForceAttackPromptWidePresetToSquare();
            attackPromptIcon.SetAction(attackPromptKeyboardAction);
            SetAttackPromptMainIconLabel("C");
            SetAttackPromptKeyboardBoundsWidth();
            if (Input.GetKeyDown(KeyCode.F7))
            {
                DumpAttackPromptIconLabels();
            }
        }

        private bool ShouldHandleDefaultInput()
        {
            return gameObject.activeInHierarchy && IsAttackSwitcherVisible;
        }

        private static bool IsDefaultInputPressed()
        {
            return Input.GetKeyDown(KeyCode.R);
        }

        private static bool IsOpenAttackSwitcherInputPressed()
        {
            return Input.GetKeyDown(KeyCode.C) ||
                Input.GetKeyDown(KeyCode.JoystickButton2);
        }

        private bool IsAttackSwitcherVisible
        {
            get { return attackSwitcherOpen && crestList != null && crestList.IsSwitchingCrests; }
        }

        private void OpenAttackSwitcher()
        {
            followCrestDescriptionClose = false;
            suppressCrestDescriptionUntilTime = -1f;
            SetPanelAlpha(1f);
            attackSwitcherOpen = true;
            openPanel = this;
            SetRowsActive(true);
            UpdateAttackPromptVisibility();
            UpdateComboButtonPromptVisibility();
            UpdateDescriptionVisibility();
            RefreshNavigation();
            InventoryAttackSourceButton entry = GetPreferredPanelEntry();
            if (entry != null)
            {
                selectedButton = entry;
                lastSelectedButton = entry;
                RefreshSelectionDisplay();
            }
            else
            {
                RefreshSelectionDisplay();
            }
        }

        private void CloseAttackSwitcher(bool returnToCrest)
        {
            CloseAttackSwitcher(returnToCrest, true);
        }

        private void CloseAttackSwitcher(bool returnToCrest, bool restoreDescription)
        {
            Plugin.Log?.LogInfo(
                "Custom Crest CloseAttackSwitcher: returnToCrest=" + returnToCrest +
                ", restoreDescription=" + restoreDescription +
                ", attackSwitcherOpen=" + attackSwitcherOpen +
                ", anyRowActive=" + IsAnyRowActive());
            if (!attackSwitcherOpen && !IsAnyRowActive())
            {
                if (restoreDescription)
                {
                    UpdateDescriptionVisibility();
                }

                return;
            }

            if (returnToCrest)
            {
                consumeNextCrestSwitcherCancel = true;
            }

            attackSwitcherOpen = false;
            followCrestDescriptionClose = false;
            suppressCrestDescriptionUntilTime = -1f;
            SetPanelAlpha(1f);
            if (openPanel == this)
            {
                openPanel = null;
            }

            selectedButton = null;
            SetRowsActive(false);
            UpdateAttackPromptVisibility();
            UpdateComboButtonPromptVisibility();
            if (restoreDescription)
            {
                UpdateDescriptionVisibility();
            }
            RefreshSelectionDisplay();
        }

        private void MoveSelection(InventoryPaneBase.InputEventType direction)
        {
            InventoryAttackSourceButton current = selectedButton ?? GetPreferredPanelEntry();
            if (direction == InventoryPaneBase.InputEventType.Left ||
                direction == InventoryPaneBase.InputEventType.Right)
            {
                CycleSelectedAttackSource(current, direction == InventoryPaneBase.InputEventType.Right ? 1 : -1);
                return;
            }

            InventoryAttackSourceButton next = GetDirectionalButton(current, direction);
            if (next == null)
            {
                return;
            }

            selectedButton = next;
            lastSelectedButton = next;
            RefreshSelectionDisplay();
            InventoryItemToolManager manager = GetComponentInParent<InventoryItemToolManager>();
            if (manager != null)
            {
                manager.PlayMoveSound();
            }
        }

        private void CycleSelectedAttackSource(InventoryAttackSourceButton current, int direction)
        {
            InventoryAttackSourceButton button = current ?? GetPreferredPanelEntry();
            if (button == null)
            {
                return;
            }

            selectedButton = button;
            lastSelectedButton = button;
            button.Cycle(direction);
            RefreshSelectionDisplay();
        }

        private bool HandleRightColumnNudgeInput()
        {
            Vector2 delta = Vector2.zero;

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                delta.x -= ColumnNudgeStep;
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                delta.x += ColumnNudgeStep;
            }

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                delta.y += ColumnNudgeStep;
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                delta.y -= ColumnNudgeStep;
            }

            if (delta == Vector2.zero)
            {
                return false;
            }

            NudgeRightColumn(delta);
            LogRightColumnPositions();
            return true;
        }

        private void NudgeRightColumn(Vector2 delta)
        {
            NudgeButton(dashButton, delta);
            NudgeButton(needleStrikeButton, delta);
        }

        private static void NudgeButton(InventoryAttackSourceButton button, Vector2 delta)
        {
            if (button != null)
            {
                button.transform.localPosition += (Vector3)delta;
            }
        }

        private void LogRightColumnPositions()
        {
            Plugin.Log?.LogInfo(
                "Custom Crest right column positions: dash=" + GetButtonLocalPosition(dashButton) +
                ", strike=" + GetButtonLocalPosition(needleStrikeButton));
        }

        private static Vector3 GetButtonLocalPosition(InventoryAttackSourceButton button)
        {
            return button == null ? Vector3.zero : button.transform.localPosition;
        }

        private InventoryAttackSourceButton GetDirectionalButton(InventoryAttackSourceButton current, InventoryPaneBase.InputEventType direction)
        {
            if (current == null)
            {
                return GetPreferredPanelEntry();
            }

            switch (direction)
            {
                case InventoryPaneBase.InputEventType.Up:
                    if (current == downButton)
                    {
                        return upButton;
                    }

                    if (current == neutralButton)
                    {
                        return downButton;
                    }

                    if (current == dashButton)
                    {
                        return neutralButton;
                    }

                    if (current == needleStrikeButton)
                    {
                        return dashButton;
                    }

                    return current;
                case InventoryPaneBase.InputEventType.Down:
                    if (current == upButton)
                    {
                        return downButton;
                    }

                    if (current == downButton)
                    {
                        return neutralButton;
                    }

                    if (current == neutralButton)
                    {
                        return dashButton;
                    }

                    if (current == dashButton)
                    {
                        return needleStrikeButton;
                    }

                    return current;
                case InventoryPaneBase.InputEventType.Left:
                    return current;
                case InventoryPaneBase.InputEventType.Right:
                    return current;
                default:
                    return current;
            }
        }

        private bool IsAnyRowActive()
        {
            return IsRowActive(upButton) ||
                IsRowActive(downButton) ||
                IsRowActive(neutralButton) ||
                IsRowActive(dashButton) ||
                IsRowActive(needleStrikeButton);
        }

        private static bool IsRowActive(InventoryAttackSourceButton button)
        {
            return button != null && button.gameObject.activeSelf;
        }

        private void SetRowsActive(bool active)
        {
            SetRowActive(upButton, active);
            SetRowActive(downButton, active);
            SetRowActive(neutralButton, active);
            SetRowActive(dashButton, active);
            SetRowActive(needleStrikeButton, active);
        }

        private static void SetRowActive(InventoryAttackSourceButton button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        private void EnsureAttackPrompt()
        {
            if (attackPrompt != null || crestList == null)
            {
                return;
            }

            TMP_Text selectText = FindSelectCrestText();
            if (selectText == null)
            {
                return;
            }

            attackPrompt = new GameObject(AttackPromptName);
            Transform promptTransform = attackPrompt.transform;
            promptTransform.SetParent(selectText.transform.parent, false);
            attackPrompt.name = AttackPromptName;
            promptTransform.localPosition = Vector3.zero;
            promptTransform.localRotation = Quaternion.identity;
            promptTransform.localScale = Vector3.one;

            TMP_Text clonedText = Instantiate(selectText, promptTransform, false);
            clonedText.gameObject.name = "Text";
            clonedText.transform.localPosition = selectText.transform.localPosition + AttackPromptOffset;
            attackPromptLabel = clonedText;
            SetClonedPromptText();

            Transform iconTemplate = FindSelectCrestActionIcon(selectText.transform);
            if (iconTemplate != null)
            {
                Transform clonedIcon = Instantiate(iconTemplate, promptTransform, false);
                clonedIcon.gameObject.name = "Icon";
                attackPromptIconTransform = clonedIcon;
                Vector3 sourceIconLocalPosition = selectText.transform.parent.InverseTransformPoint(iconTemplate.position);
                clonedIcon.localPosition = sourceIconLocalPosition + AttackPromptOffset;
                clonedIcon.localRotation = iconTemplate.localRotation;
                clonedIcon.localScale = iconTemplate.localScale * AttackPromptIconScale;
                attackPromptControllerIconScale = clonedIcon.localScale;
                ActionButtonIcon icon = clonedIcon.GetComponent<ActionButtonIcon>();
                attackPromptKeyboardAction = GetActionButtonIconAction(icon);
                attackPromptIcon = icon;
                attackPromptIconLabel = ActionButtonIconLabelField == null || icon == null
                    ? null
                    : ActionButtonIconLabelField.GetValue(icon) as TMP_Text;
                attackPromptIconTextContainer = ActionButtonIconTextContainerField == null || icon == null
                    ? null
                    : ActionButtonIconTextContainerField.GetValue(icon) as TextContainer;

                attackPromptIconKeyboardState = null;
                ConfigureAttackPromptIcon();
                AlignAttackPromptIconToTemplate(iconTemplate, clonedIcon);
                clonedIcon.localPosition += AttackPromptIconNudge;
            }
        }

        private void ConfigureAttackPromptIcon()
        {
            if (attackPromptIcon == null)
            {
                return;
            }

            bool keyboardActive = IsKeyboardInputActive();
            if (attackPromptIconKeyboardState.HasValue && attackPromptIconKeyboardState.Value == keyboardActive)
            {
                return;
            }

            attackPromptIconKeyboardState = keyboardActive;
            if (keyboardActive)
            {
                if (ActionButtonIconLiveUpdateField != null)
                {
                    ActionButtonIconLiveUpdateField.SetValue(attackPromptIcon, true);
                }

                SetAttackPromptIconRenderersEnabled(true);
                SetAttackPromptIconTextContainerEnabled(true);
                SetAttackPromptIconKeyboardScale(true);
                ForceAttackPromptWidePresetToSquare();
                attackPromptIcon.SetAction(attackPromptKeyboardAction);
                SetAttackPromptMainIconLabel("C");
                SetAttackPromptKeyboardBoundsWidth();
            }
            else
            {
                if (ActionButtonIconLiveUpdateField != null)
                {
                    ActionButtonIconLiveUpdateField.SetValue(attackPromptIcon, true);
                }

                SetAttackPromptIconTextContainerEnabled(false);
                SetAttackPromptIconKeyboardScale(false);
                attackPromptIcon.SetAction(HeroActionButton.MENU_EXTRA);
            }
        }

        private void SetAttackPromptIconKeyboardScale(bool keyboard)
        {
            if (attackPromptIconTransform == null)
            {
                return;
            }

            attackPromptIconTransform.localScale = keyboard
                ? attackPromptControllerIconScale
                : attackPromptControllerIconScale;
        }

        private void ForceAttackPromptWidePresetToSquare()
        {
            if (attackPromptIcon == null)
            {
                return;
            }

            CopyActionButtonIconBaseField(
                ActionButtonIconBaseSquareWidthField,
                ActionButtonIconBaseWideWidthField);
            CopyActionButtonIconBaseField(
                ActionButtonIconBaseSquareHeightField,
                ActionButtonIconBaseWideHeightField);
            CopyActionButtonIconBaseField(
                ActionButtonIconBaseSquareFontMaxField,
                ActionButtonIconBaseWideFontMaxField);
            CopyActionButtonIconBaseField(
                ActionButtonIconBaseSquareFontMinField,
                ActionButtonIconBaseWideFontMinField);
        }

        private void CopyActionButtonIconBaseField(FieldInfo sourceField, FieldInfo targetField)
        {
            if (sourceField == null || targetField == null || attackPromptIcon == null)
            {
                return;
            }

            targetField.SetValue(attackPromptIcon, sourceField.GetValue(attackPromptIcon));
        }

        private static HeroActionButton GetActionButtonIconAction(ActionButtonIcon icon)
        {
            if (ActionButtonIconActionField == null || icon == null)
            {
                return HeroActionButton.MENU_SUBMIT;
            }

            object value = ActionButtonIconActionField.GetValue(icon);
            return value is HeroActionButton action ? action : HeroActionButton.MENU_SUBMIT;
        }

        private void SetAttackPromptIconTextContainerEnabled(bool enabled)
        {
            if (attackPromptIconTextContainer == null)
            {
                return;
            }

            attackPromptIconTextContainer.gameObject.SetActive(true);
            attackPromptIconTextContainer.enabled = enabled;
        }

        private void SetAttackPromptIconRenderersEnabled(bool enabled)
        {
            if (attackPromptIconTransform == null)
            {
                return;
            }

            Renderer[] renderers = attackPromptIconTransform.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
        }

        private void SetAttackPromptIconLabel(string text)
        {
            if (attackPromptIconLabel != null)
            {
                attackPromptIconLabel.text = text;
                attackPromptIconLabel.enabled = true;
                attackPromptIconLabel.ForceMeshUpdate();
            }

            if (attackPromptIconTransform == null)
            {
                return;
            }

            TMP_Text[] labels = attackPromptIconTransform.GetComponentsInChildren<TMP_Text>(true);
            SetAttackPromptIconLabels(labels, text);
            if (attackPromptIconTextContainer != null)
            {
                SetAttackPromptIconLabels(
                    attackPromptIconTextContainer.GetComponentsInChildren<TMP_Text>(true),
                    text);
            }
        }

        private void SetAttackPromptMainIconLabel(string text)
        {
            if (attackPromptIconTransform == null)
            {
                return;
            }

            TMP_Text[] labels = attackPromptIconTransform.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                TMP_Text label = labels[i];
                if (label == null)
                {
                    continue;
                }

                label.text = text;
                label.enabled = true;
                label.ForceMeshUpdate();
            }
        }

        private void DumpAttackPromptIconLabels()
        {
            if (attackPromptIconTransform == null)
            {
                Plugin.Log?.LogInfo("Attack prompt label dump: no icon transform.");
                return;
            }

            TMP_Text[] labels = attackPromptIconTransform.GetComponentsInChildren<TMP_Text>(true);
            Plugin.Log?.LogInfo("Attack prompt label dump: " + labels.Length + " TMP labels under icon.");
            for (int i = 0; i < labels.Length; i++)
            {
                TMP_Text label = labels[i];
                if (label == null)
                {
                    continue;
                }

                Plugin.Log?.LogInfo(
                    "Attack prompt label " + i +
                    ": path=" + GetTransformPath(label.transform) +
                    ", text=\"" + label.text + "\"" +
                    ", active=" + label.gameObject.activeInHierarchy +
                    ", enabled=" + label.enabled +
                    ", rendererEnabled=" + IsRendererEnabled(label) +
                    ", isActionLabel=" + (label == attackPromptIconLabel));
            }

            if (attackPromptIconTextContainer != null)
            {
                TMP_Text[] containerLabels = attackPromptIconTextContainer.GetComponentsInChildren<TMP_Text>(true);
                Plugin.Log?.LogInfo("Attack prompt label dump: " + containerLabels.Length + " TMP labels under textContainer.");
                for (int i = 0; i < containerLabels.Length; i++)
                {
                    TMP_Text label = containerLabels[i];
                    if (label == null)
                    {
                        continue;
                    }

                    Plugin.Log?.LogInfo(
                        "Attack prompt container label " + i +
                        ": path=" + GetTransformPath(label.transform) +
                        ", text=\"" + label.text + "\"" +
                        ", active=" + label.gameObject.activeInHierarchy +
                        ", enabled=" + label.enabled +
                        ", rendererEnabled=" + IsRendererEnabled(label) +
                        ", isActionLabel=" + (label == attackPromptIconLabel));
                }
            }
        }

        private static bool IsRendererEnabled(Component component)
        {
            Renderer renderer = component == null ? null : component.GetComponent<Renderer>();
            return renderer != null && renderer.enabled;
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            List<string> parts = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private void SetAttackPromptKeyboardBoundsWidth()
        {
            if (attackPromptIconTransform == null)
            {
                return;
            }

            TMP_Text[] labels = attackPromptIconTransform.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                TextMeshPro textMesh = labels[i] as TextMeshPro;
                if (textMesh != null)
                {
                    SetTextContainerKeyboardBounds(textMesh.textContainer);
                }
            }

            if (attackPromptIconTextContainer != null)
            {
                SetTextContainerKeyboardBounds(attackPromptIconTextContainer);
            }
        }

        private static void SetTextContainerKeyboardBounds(TextContainer container)
        {
            if (container == null)
            {
                return;
            }

            Rect rect = container.rect;
            float targetWidth = rect.height > 0f
                ? rect.height * 1.1f
                : rect.width * AttackPromptKeyboardBoundsWidthScale;
            if (targetWidth <= 0f)
            {
                return;
            }

            container.width = targetWidth;
            container.size = new Vector2(targetWidth, container.height);
            container.rect = new Rect(rect.x, rect.y, targetWidth, rect.height);

            RectTransform rectTransform = container.rectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(targetWidth, rectTransform.sizeDelta.y);
            }
        }

        private static void SetAttackPromptIconLabels(TMP_Text[] labels, string text)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                TMP_Text label = labels[i];
                if (label == null)
                {
                    continue;
                }

                label.text = text;
                label.enabled = true;
                label.ForceMeshUpdate();
            }
        }

        private static bool IsKeyboardInputActive()
        {
            Platform platform = Platform.Current;
            return platform != null && platform.WasLastInputKeyboard;
        }

        private static void AlignAttackPromptIconToTemplate(Transform iconTemplate, Transform clonedIcon)
        {
            if (iconTemplate == null || clonedIcon == null || clonedIcon.parent == null)
            {
                return;
            }

            Renderer templateRenderer = FindLargestRenderer(iconTemplate);
            Renderer clonedRenderer = FindLargestRenderer(clonedIcon);
            if (templateRenderer == null || clonedRenderer == null)
            {
                return;
            }

            Vector3 desiredWorldCenter = templateRenderer.bounds.center + iconTemplate.parent.TransformVector(AttackPromptOffset);
            Vector3 visibleCenterDelta = desiredWorldCenter - clonedRenderer.bounds.center;
            clonedIcon.localPosition += clonedIcon.parent.InverseTransformVector(visibleCenterDelta);
        }

        private static Renderer FindLargestRenderer(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Renderer largest = null;
            float largestArea = 0f;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                Vector3 size = renderer.bounds.size;
                float area = size.x * size.y;
                if (area > largestArea)
                {
                    largestArea = area;
                    largest = renderer;
                }
            }

            return largest;
        }

        private TMP_Text FindSelectCrestText()
        {
            Component promptRoot = crestList.GetComponentInParent<InventoryItemToolManager>() as Component ?? crestList;
            TMP_Text[] texts = promptRoot.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text != null && text.text != null && text.text.IndexOf("Select Crest", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return text;
                }
            }

            return null;
        }

        private static Transform FindSelectCrestActionIcon(Transform textTransform)
        {
            Transform parent = textTransform == null ? null : textTransform.parent;
            if (parent == null)
            {
                return null;
            }

            ActionButtonIcon[] nearbyIcons = parent.parent == null
                ? new ActionButtonIcon[0]
                : parent.parent.GetComponentsInChildren<ActionButtonIcon>(true);
            Renderer textRenderer = textTransform.GetComponent<Renderer>();
            Bounds textBounds = textRenderer == null
                ? new Bounds(textTransform.position, Vector3.zero)
                : textRenderer.bounds;
            ActionButtonIcon nearest = null;
            float bestHorizontalDistance = float.MaxValue;
            for (int i = 0; i < nearbyIcons.Length; i++)
            {
                ActionButtonIcon icon = nearbyIcons[i];
                if (icon == null)
                {
                    continue;
                }

                Vector3 iconPosition = icon.transform.position;
                float horizontalDistance = iconPosition.x - textBounds.max.x;
                if (horizontalDistance <= 0f)
                {
                    continue;
                }

                float verticalDistance = Mathf.Abs(iconPosition.y - textBounds.center.y);
                if (verticalDistance > 0.55f)
                {
                    continue;
                }

                if (horizontalDistance < bestHorizontalDistance)
                {
                    bestHorizontalDistance = horizontalDistance;
                    nearest = icon;
                }
            }

            return nearest == null ? null : nearest.transform;
        }

        private void UpdateAttackPromptVisibility()
        {
            if (attackPrompt != null)
            {
                SetClonedPromptText();
                ConfigureAttackPromptIcon();
                attackPrompt.SetActive(crestList != null && crestList.IsSwitchingCrests);
            }
        }

        private void UpdateComboButtonPromptVisibility()
        {
            GameObject promptObject = GetComboButtonPromptObject();
            if (promptObject != null)
            {
                promptObject.SetActive(!IsAttackSwitcherVisible);
            }
        }

        private GameObject GetComboButtonPromptObject()
        {
            if (comboButtonPromptObject != null)
            {
                return comboButtonPromptObject;
            }

            InventoryItemToolManager manager = crestList == null
                ? GetComponentInParent<InventoryItemToolManager>(true)
                : crestList.GetComponentInParent<InventoryItemToolManager>(true);
            if (manager == null || ToolManagerComboButtonPromptDisplayField == null)
            {
                return null;
            }

            Component promptDisplay = ToolManagerComboButtonPromptDisplayField.GetValue(manager) as Component;
            comboButtonPromptObject = promptDisplay == null ? null : promptDisplay.gameObject;
            return comboButtonPromptObject;
        }

        private void SetClonedPromptText()
        {
            if (attackPromptLabel != null)
            {
                attackPromptLabel.text = IsAttackSwitcherVisible ? DefaultPromptText : AttackPromptText;
            }
        }

        private void UpdateDescriptionVisibility()
        {
            SetCrestDescriptionVisible(!IsAttackSwitcherVisible);
        }

        private void SetCrestDescriptionVisible(bool visible)
        {
            TextMeshPro description = CrestDescriptionDisplayField == null || crestList == null
                ? null
                : CrestDescriptionDisplayField.GetValue(crestList) as TextMeshPro;
            if (description != null)
            {
                ShrinkDescriptionWidth(description);
                SetRenderersEnabled(description.transform, visible);
            }
        }

        private bool IsCrestSwitchSequenceRunning()
        {
            return CrestListSwitchSequenceRoutineField != null &&
                crestList != null &&
                CrestListSwitchSequenceRoutineField.GetValue(crestList) != null;
        }

        private bool IsCrestSwitchMoveRunning()
        {
            return CrestListSwitchMoveRoutineField != null &&
                crestList != null &&
                CrestListSwitchMoveRoutineField.GetValue(crestList) != null;
        }

        private void UpdateClosingPanelFade()
        {
            float alpha = 1f;
            if (followCrestDescriptionClose)
            {
                float descriptionAlpha = GetCrestDescriptionAlpha();
                alpha = IsCrestSwitchMoveRunning() ? 1f - descriptionAlpha : 1f;
            }

            SetPanelAlpha(alpha);
            SetCrestDescriptionVisible(false);
        }

        private void HideAttackPanelAndSuppressCrestDescription()
        {
            attackSwitcherOpen = false;
            if (openPanel == this)
            {
                openPanel = null;
            }

            selectedButton = null;
            SetPanelAlpha(0f);
            SetRowsActive(false);
            if (attackPrompt != null)
            {
                attackPrompt.SetActive(false);
            }

            UpdateComboButtonPromptVisibility();
            SetCrestDescriptionVisible(false);
            RefreshSelectionDisplay();
        }

        private float GetCrestDescriptionAlpha()
        {
            TextMeshPro description = CrestDescriptionDisplayField == null || crestList == null
                ? null
                : CrestDescriptionDisplayField.GetValue(crestList) as TextMeshPro;
            if (description == null)
            {
                return 0f;
            }

            float alpha = description.color.a;
            Renderer[] renderers = description.transform.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.sharedMaterial == null || !renderer.sharedMaterial.HasProperty("_Color"))
                {
                    continue;
                }

                alpha = Mathf.Max(alpha, renderer.sharedMaterial.color.a);
            }

            return Mathf.Clamp01(alpha);
        }

        private void SetPanelAlpha(float alpha)
        {
            panelAlpha = Mathf.Clamp01(alpha);
            upButton?.SetPanelAlpha(panelAlpha);
            downButton?.SetPanelAlpha(panelAlpha);
            neutralButton?.SetPanelAlpha(panelAlpha);
            dashButton?.SetPanelAlpha(panelAlpha);
            needleStrikeButton?.SetPanelAlpha(panelAlpha);
            if (attackPromptLabel != null)
            {
                Color color = attackPromptLabel.color;
                color.a = panelAlpha;
                attackPromptLabel.color = color;
            }

            if (attackPromptIconLabel != null)
            {
                Color color = attackPromptIconLabel.color;
                color.a = panelAlpha;
                attackPromptIconLabel.color = color;
            }
        }

        private void ShrinkDescriptionWidth(TextMeshPro description)
        {
            TextContainer container = description.textContainer;
            if (container == null)
            {
                return;
            }

            if (crestDescriptionBaseWidth <= 0f)
            {
                crestDescriptionBaseWidth = container.width;
            }

            if (crestDescriptionBaseWidth <= 0f)
            {
                return;
            }

            float targetWidth = crestDescriptionBaseWidth * CrestDescriptionWidthScale;
            container.width = targetWidth;
            container.size = new Vector2(targetWidth, container.height);
            Rect rect = container.rect;
            container.rect = new Rect(rect.x, rect.y, targetWidth, rect.height);

            RectTransform rectTransform = container.rectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(targetWidth, rectTransform.sizeDelta.y);
            }

            description.ForceMeshUpdate();
        }

        private static void SetRenderersEnabled(Transform transform, bool enabled)
        {
            if (transform == null)
            {
                return;
            }

            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
        }

        private void UpdatePosition()
        {
            Vector3 targetPosition = crestList != null && crestList.IsSwitchingCrests
                ? SelectorPanelLocalPosition
                : PanelLocalPosition;
            if ((transform.localPosition - targetPosition).sqrMagnitude < 0.0001f)
            {
                transform.localPosition = targetPosition;
                return;
            }

            float t = 1f - Mathf.Exp(-PositionLerpSpeed * Time.unscaledDeltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, t);
        }

        private static InventoryAttackSourcePanel CreatePanel(InventoryToolCrestList crestList, TextMeshPro template)
        {
            var panelObject = new GameObject(PanelName);
            panelObject.transform.SetParent(crestList.transform, false);
            panelObject.transform.localPosition = PanelLocalPosition;
            panelObject.transform.localRotation = Quaternion.identity;
            panelObject.transform.localScale = Vector3.one;

            InventoryAttackSourcePanel panel = panelObject.AddComponent<InventoryAttackSourcePanel>();
            panel.crestList = crestList;
            panel.upButton = CreateRow(panelObject.transform, template, AttackSlot.Up, "Up", LeftColumnX, 0.48f);
            panel.downButton = CreateRow(panelObject.transform, template, AttackSlot.Down, "Down", LeftColumnX, 0f);
            panel.neutralButton = CreateRow(panelObject.transform, template, AttackSlot.Neutral, "Neutral", LeftColumnX, -0.48f);
            panel.dashButton = CreateRow(panelObject.transform, template, AttackSlot.Dash, "Dash", RightColumnX, 0.48f);
            panel.needleStrikeButton = CreateRow(panelObject.transform, template, AttackSlot.NeedleStrike, "Strike", RightColumnX, 0f);
            panel.SetRowsActive(false);
            panel.RefreshNavigation();
            return panel;
        }

        private void RefreshNavigation()
        {
            InventoryItemSelectable crestEntry = crestList == null ? null : crestList.CurrentCrest;
            InventoryItemSelectable viewCrestsButton = FindViewCrestsButton();
            InventoryItemGrid toolGrid = FindToolGrid();
            InventoryItemSelectable toolEntry = FindToolEntry(toolGrid);
            InventoryToolCrestSlot highestSlot;
            InventoryToolCrestSlot lowestSlot;
            GetVerticalCrestSlots(out highestSlot, out lowestSlot);

            ConfigureButton(upButton, lowestSlot ?? crestEntry, downButton, viewCrestsButton ?? crestEntry, dashButton ?? toolEntry ?? crestEntry);
            ConfigureButton(downButton, upButton, neutralButton, viewCrestsButton ?? crestEntry, needleStrikeButton ?? toolEntry ?? crestEntry);
            ConfigureButton(neutralButton, downButton, highestSlot ?? crestEntry, viewCrestsButton ?? crestEntry, needleStrikeButton ?? toolEntry ?? crestEntry);
            ConfigureButton(dashButton, lowestSlot ?? crestEntry, needleStrikeButton, upButton ?? viewCrestsButton ?? crestEntry, toolEntry ?? crestEntry);
            ConfigureButton(needleStrikeButton, dashButton, highestSlot ?? crestEntry, downButton ?? viewCrestsButton ?? crestEntry, toolEntry ?? crestEntry);
            ConfigureCrestEntry(crestEntry);
            ConfigureReciprocalNavigation(viewCrestsButton, toolGrid, highestSlot, lowestSlot);
        }

        private static void ConfigureButton(
            InventoryAttackSourceButton button,
            InventoryItemSelectable up,
            InventoryItemSelectable down,
            InventoryItemSelectable left,
            InventoryItemSelectable right)
        {
            if (button == null)
            {
                return;
            }

            EnsureSelectableArrays(button);
            button.Selectables[(int)InventoryItemManager.SelectionDirection.Up] = up;
            button.Selectables[(int)InventoryItemManager.SelectionDirection.Down] = down;
            button.Selectables[(int)InventoryItemManager.SelectionDirection.Left] = left;
            button.Selectables[(int)InventoryItemManager.SelectionDirection.Right] = right;
        }

        private void ConfigureCrestEntry(InventoryItemSelectable escape)
        {
            InventoryItemSelectableDirectional directional = escape as InventoryItemSelectableDirectional;
            if (directional == null || upButton == null)
            {
                return;
            }

            EnsureSelectableArrays(directional);
            directional.Selectables[(int)InventoryItemManager.SelectionDirection.Down] = upButton;
        }

        private void ConfigureReciprocalNavigation(
            InventoryItemSelectable viewCrestsButton,
            InventoryItemGrid toolGrid,
            InventoryToolCrestSlot highestSlot,
            InventoryToolCrestSlot lowestSlot)
        {
            InventoryAttackSourceButton panelEntry = GetTopButton();
            InventoryAttackSourceButton verticalFromTopEntry = GetBottomButton();
            InventoryAttackSourceButton verticalFromBottomEntry = upButton ?? panelEntry;
            InventoryItemSelectableDirectional viewCrestsDirectional = viewCrestsButton as InventoryItemSelectableDirectional;
            if (viewCrestsDirectional != null)
            {
                EnsureSelectableArrays(viewCrestsDirectional);
                viewCrestsDirectional.Selectables[(int)InventoryItemManager.SelectionDirection.Right] = panelEntry;
            }

            if (toolGrid != null)
            {
                LinkToolGridLeftEdge(toolGrid, panelEntry);
            }

            if (highestSlot != null)
            {
                EnsureSelectableArrays(highestSlot);
                highestSlot.Selectables[(int)InventoryItemManager.SelectionDirection.Up] = verticalFromTopEntry;
            }

            if (lowestSlot != null)
            {
                EnsureSelectableArrays(lowestSlot);
                lowestSlot.Selectables[(int)InventoryItemManager.SelectionDirection.Down] = verticalFromBottomEntry;
            }
        }

        private InventoryAttackSourceButton GetPreferredPanelEntry()
        {
            return lastSelectedButton ?? downButton ?? upButton ?? neutralButton ?? dashButton ?? needleStrikeButton;
        }

        private InventoryItemSelectable FindViewCrestsButton()
        {
            InventoryItemSelectable button = CrestListChangeCrestButtonField == null || crestList == null
                ? null
                : CrestListChangeCrestButtonField.GetValue(crestList) as InventoryItemSelectable;
            if (button != null && button.gameObject.activeInHierarchy)
            {
                return button;
            }

            InventoryItemToolManager manager = crestList == null ? null : crestList.GetComponentInParent<InventoryItemToolManager>();
            button = ToolManagerChangeCrestButtonField == null || manager == null
                ? null
                : ToolManagerChangeCrestButtonField.GetValue(manager) as InventoryItemSelectable;
            return button != null && button.gameObject.activeInHierarchy ? button : null;
        }

        private InventoryItemGrid FindToolGrid()
        {
            InventoryItemToolManager manager = crestList == null ? null : crestList.GetComponentInParent<InventoryItemToolManager>();
            InventoryItemGrid toolGrid = ToolManagerToolListField == null || manager == null
                ? null
                : ToolManagerToolListField.GetValue(manager) as InventoryItemGrid;
            if (toolGrid == null || !toolGrid.gameObject.activeInHierarchy)
            {
                return null;
            }

            return toolGrid;
        }

        private InventoryItemSelectable FindToolEntry(InventoryItemGrid toolGrid)
        {
            if (toolGrid == null)
            {
                return null;
            }

            List<InventoryItemTool> tools = toolGrid.GetListItems<InventoryItemTool>(tool => tool != null && tool.gameObject.activeInHierarchy);
            if (tools.Count == 0)
            {
                return toolGrid;
            }

            InventoryItemTool naturalTarget = GetClosestToolByY(tools, transform.position.y);
            if (naturalTarget == null)
            {
                return toolGrid;
            }

            int naturalIndex = tools.IndexOf(naturalTarget);
            int higherIndex = naturalIndex - Mathf.Max(1, toolGrid.RowSplit);
            if (higherIndex >= 0 && higherIndex < tools.Count)
            {
                return tools[higherIndex];
            }

            InventoryItemTool higherByPosition = GetNearestToolAbove(tools, naturalTarget);
            return higherByPosition ?? naturalTarget;
        }

        private static InventoryItemTool GetClosestToolByY(List<InventoryItemTool> tools, float y)
        {
            InventoryItemTool closest = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < tools.Count; i++)
            {
                InventoryItemTool tool = tools[i];
                float distance = Mathf.Abs(tool.transform.position.y - y);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = tool;
                }
            }

            return closest;
        }

        private static InventoryItemTool GetNearestToolAbove(List<InventoryItemTool> tools, InventoryItemTool naturalTarget)
        {
            InventoryItemTool nearest = null;
            float nearestScore = float.MaxValue;
            Vector3 naturalPosition = naturalTarget.transform.position;
            for (int i = 0; i < tools.Count; i++)
            {
                InventoryItemTool tool = tools[i];
                Vector3 position = tool.transform.position;
                if (position.y <= naturalPosition.y + 0.001f)
                {
                    continue;
                }

                float score = Mathf.Abs(position.y - naturalPosition.y) * 4f + Mathf.Abs(position.x - naturalPosition.x);
                if (score < nearestScore)
                {
                    nearestScore = score;
                    nearest = tool;
                }
            }

            return nearest;
        }

        private static void LinkToolGridLeftEdge(InventoryItemGrid toolGrid, InventoryItemSelectable panelEntry)
        {
            if (toolGrid == null || panelEntry == null)
            {
                return;
            }

            List<InventoryItemTool> tools = toolGrid.GetListItems<InventoryItemTool>(null);
            for (int i = 0; i < tools.Count; i++)
            {
                InventoryItemTool tool = tools[i];
                if (tool == null || !tool.gameObject.activeInHierarchy)
                {
                    continue;
                }

                EnsureSelectableArrays(tool);
                if (tool.Selectables[(int)InventoryItemManager.SelectionDirection.Left] == null)
                {
                    tool.Selectables[(int)InventoryItemManager.SelectionDirection.Left] = panelEntry;
                }
            }
        }

        private void GetVerticalCrestSlots(out InventoryToolCrestSlot highestSlot, out InventoryToolCrestSlot lowestSlot)
        {
            highestSlot = null;
            lowestSlot = null;
            if (crestList == null)
            {
                return;
            }

            List<InventoryToolCrestSlot> slots = crestList.GetSlots()
                .Where(slot => slot != null && slot.gameObject.activeInHierarchy)
                .ToList();
            for (int i = 0; i < slots.Count; i++)
            {
                InventoryToolCrestSlot slot = slots[i];
                if (highestSlot == null || slot.transform.position.y > highestSlot.transform.position.y)
                {
                    highestSlot = slot;
                }

                if (lowestSlot == null || slot.transform.position.y < lowestSlot.transform.position.y)
                {
                    lowestSlot = slot;
                }
            }
        }

        private void RefreshSelectionDisplay()
        {
            InventoryAttackSourceButton visualSelection = selectedButton ?? lastSelectedButton ?? GetPreferredPanelEntry();
            bool panelSelected = visualSelection != null && IsAttackSwitcherVisible;
            upButton?.SetPanelSelectionState(panelSelected, visualSelection == upButton);
            downButton?.SetPanelSelectionState(panelSelected, visualSelection == downButton);
            neutralButton?.SetPanelSelectionState(panelSelected, visualSelection == neutralButton);
            dashButton?.SetPanelSelectionState(panelSelected, visualSelection == dashButton);
            needleStrikeButton?.SetPanelSelectionState(panelSelected, visualSelection == needleStrikeButton);
        }

        private InventoryAttackSourceButton GetTopButton()
        {
            return upButton ?? downButton ?? neutralButton ?? dashButton ?? needleStrikeButton;
        }

        private InventoryAttackSourceButton GetBottomButton()
        {
            return neutralButton ?? needleStrikeButton ?? downButton ?? dashButton ?? upButton;
        }

        private static void EnsureSelectableArrays(InventoryItemSelectableDirectional selectable)
        {
            int length = System.Enum.GetNames(typeof(InventoryItemManager.SelectionDirection)).Length;
            if (selectable.Selectables == null || selectable.Selectables.Length != length)
            {
                selectable.Selectables = new InventoryItemSelectable[length];
            }
        }

        private void FitSelectionFrameToPanel()
        {
            Bounds panelBounds;
            if (!TryGetTextBounds(out panelBounds))
            {
                SetSelectionFrame(new Vector2(0.25f, 0f), ColliderSize);
                return;
            }

            Vector2 size = new Vector2(
                Mathf.Max(ColliderSize.x, panelBounds.size.x + SelectionFramePadding.x * 2f),
                Mathf.Max(ColliderSize.y, panelBounds.size.y + SelectionFramePadding.y * 2f));
            Vector2 center = (Vector2)panelBounds.center + SelectionFrameNudge;
            SetSelectionFrame(center, size);
        }

        private bool TryGetTextBounds(out Bounds panelBounds)
        {
            panelBounds = new Bounds();
            bool hasBounds = false;
            TextMeshPro[] texts = GetComponentsInChildren<TextMeshPro>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshPro text = texts[i];
                if (text == null || string.IsNullOrEmpty(text.text))
                {
                    continue;
                }

                text.ForceMeshUpdate();
                Bounds textBounds = text.textBounds;
                if (textBounds.size.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                Vector3 worldMin = text.transform.TransformPoint(textBounds.min);
                Vector3 worldMax = text.transform.TransformPoint(textBounds.max);
                Vector3 localMin = transform.InverseTransformPoint(worldMin);
                Vector3 localMax = transform.InverseTransformPoint(worldMax);
                Bounds localBounds = new Bounds((localMin + localMax) * 0.5f, localMax - localMin);
                if (!hasBounds)
                {
                    panelBounds = localBounds;
                    hasBounds = true;
                }
                else
                {
                    panelBounds.Encapsulate(localBounds);
                }
            }

            return hasBounds;
        }

        private void SetSelectionFrame(Vector2 panelCenter, Vector2 panelSize)
        {
            SetButtonSelectionFrame(upButton, panelCenter, panelSize);
            SetButtonSelectionFrame(downButton, panelCenter, panelSize);
            SetButtonSelectionFrame(neutralButton, panelCenter, panelSize);
            SetButtonSelectionFrame(dashButton, panelCenter, panelSize);
            SetButtonSelectionFrame(needleStrikeButton, panelCenter, panelSize);
        }

        private void SetButtonSelectionFrame(InventoryAttackSourceButton button, Vector2 panelCenter, Vector2 panelSize)
        {
            if (button == null)
            {
                return;
            }

            BoxCollider2D collider = button.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = button.gameObject.AddComponent<BoxCollider2D>();
            }

            Vector3 worldCenter = transform.TransformPoint(panelCenter);
            Vector3 worldMin = transform.TransformPoint(panelCenter - panelSize * 0.5f);
            Vector3 worldMax = transform.TransformPoint(panelCenter + panelSize * 0.5f);
            Vector3 localCenter = button.transform.InverseTransformPoint(worldCenter);
            Vector3 localMin = button.transform.InverseTransformPoint(worldMin);
            Vector3 localMax = button.transform.InverseTransformPoint(worldMax);
            collider.offset = localCenter;
            collider.size = new Vector2(Mathf.Abs(localMax.x - localMin.x), Mathf.Abs(localMax.y - localMin.y));
            collider.isTrigger = true;
        }

        private static InventoryAttackSourceButton CreateRow(Transform parent, TextMeshPro template, AttackSlot slot, string label, float x, float y)
        {
            GameObject rowObject = new GameObject("CustomCrest" + label + "AttackSource");
            Transform rowTransform = rowObject.transform;
            rowTransform.SetParent(parent, false);

            rowTransform.localPosition = new Vector3(x, y, 0f);
            rowTransform.localRotation = Quaternion.identity;
            rowTransform.localScale = RowScale;

            TextMeshPro labelText = CreateText(rowTransform, template, "Label", new Vector3(LabelX, 0f, 0f), TextAlignmentOptions.Right, LabelBoxSize);
            TextMeshPro valueText = CreateText(rowTransform, template, "Value", new Vector3(ValueX, 0f, 0f), TextAlignmentOptions.Left, ValueBoxSize);
            SpriteRenderer arrowTemplate = FindMenuArrowTemplate(parent);
            SpriteRenderer leftArrow = CreateArrow(rowTransform, arrowTemplate, labelText, "LeftArrow", new Vector3(LeftArrowX, 0f, -0.05f), true);
            SpriteRenderer rightArrow = CreateArrow(rowTransform, arrowTemplate, labelText, "RightArrow", new Vector3(RightArrowX, 0f, -0.05f), false);

            BoxCollider2D collider = rowTransform.gameObject.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = rowTransform.gameObject.AddComponent<BoxCollider2D>();
            }

            collider.size = ColliderSize;
            collider.offset = new Vector2(0.25f, 0f);
            collider.isTrigger = true;

            InventoryAttackSourceButton button = rowTransform.gameObject.GetComponent<InventoryAttackSourceButton>();
            if (button == null)
            {
                button = rowTransform.gameObject.AddComponent<InventoryAttackSourceButton>();
            }

            button.Setup(slot, label, labelText, valueText, leftArrow, rightArrow);
            return button;
        }

        private static SpriteRenderer FindMenuArrowTemplate(Transform context)
        {
            SpriteRenderer best = null;
            int bestScore = int.MinValue;
            SpriteRenderer[] renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null)
                {
                    continue;
                }

                string objectName = renderer.gameObject.name ?? string.Empty;
                string spriteName = renderer.sprite.name ?? string.Empty;
                string path = GetTransformPath(renderer.transform);
                string haystack = (objectName + " " + spriteName + " " + path).ToLowerInvariant();
                if (haystack.Contains("customcrestattacksourcepanel") ||
                    haystack.Contains("menu_border") ||
                    haystack.Contains("/border/menu_border"))
                {
                    continue;
                }

                int score = 0;

                if (spriteName == "RT_LT_arrow" || spriteName == "rt_lt_arrow")
                {
                    score += 120;
                }

                if (haystack.Contains("panelistdisplay") || haystack.Contains("pane arrow"))
                {
                    score += 80;
                }

                if (haystack.Contains("pause"))
                {
                    score += 16;
                }

                if (haystack.Contains("option") || haystack.Contains("setting"))
                {
                    score += 12;
                }

                if (haystack.Contains("menu"))
                {
                    score += 8;
                }

                if (haystack.Contains("arrow"))
                {
                    score += 10;
                }

                if (haystack.Contains("fleur_arrow_selector"))
                {
                    score += 25;
                }

                if (haystack.Contains("controller_button_skins"))
                {
                    score -= 6;
                }

                Bounds bounds = renderer.sprite.bounds;
                if (bounds.size.x > bounds.size.y * 1.25f)
                {
                    score += 5;
                }

                if (renderer.color.a < 0.1f || bounds.size.sqrMagnitude <= 0.0001f)
                {
                    score -= 20;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = renderer;
                }
            }

            if (best != null && bestScore > 0)
            {
                Plugin.Log?.LogInfo("Custom Crest attack arrows using sprite '" + best.sprite.name + "' from '" + GetTransformPath(best.transform) + "'.");
                return best;
            }

            return null;
        }

        private static SpriteRenderer CreateArrow(Transform rowTransform, SpriteRenderer template, TextMeshPro sortTemplate, string name, Vector3 localPosition, bool flipX)
        {
            GameObject arrowObject = new GameObject(name);
            Transform arrowTransform = arrowObject.transform;
            arrowTransform.SetParent(rowTransform, false);

            arrowTransform.localPosition = localPosition;
            arrowTransform.localRotation = Quaternion.identity;
            arrowTransform.localScale = Vector3.one;

            SpriteRenderer arrowRenderer = arrowObject.AddComponent<SpriteRenderer>();
            Sprite fallbackSprite = null;
            if (template != null)
            {
                arrowRenderer.sprite = template.sprite;
                if (template.sharedMaterial != null)
                {
                    arrowRenderer.material = template.sharedMaterial;
                }
            }
            else
            {
                fallbackSprite = FindFallbackArrowSprite();
                arrowRenderer.sprite = fallbackSprite;
            }

            int renderLayer = sortTemplate != null
                ? sortTemplate.gameObject.layer
                : template != null
                    ? template.gameObject.layer
                    : rowTransform.gameObject.layer;
            SetLayerRecursive(arrowTransform.gameObject, renderLayer);

            arrowRenderer.flipX = (template != null && template.flipX) ^ flipX;

            Renderer sortRenderer = sortTemplate == null ? null : sortTemplate.GetComponent<Renderer>();
            if (sortRenderer != null)
            {
                arrowRenderer.sortingLayerID = sortRenderer.sortingLayerID;
                arrowRenderer.sortingOrder = sortRenderer.sortingOrder + 100;
            }
            else if (template != null)
            {
                arrowRenderer.sortingLayerID = template.sortingLayerID;
                arrowRenderer.sortingOrder = template.sortingOrder + 200;
            }

            arrowRenderer.color = Color.white;
            arrowRenderer.enabled = false;
            FitArrowHeight(arrowTransform, arrowRenderer);
            arrowTransform.gameObject.SetActive(false);
            if (arrowRenderer.sprite == null)
            {
                Plugin.Log?.LogWarning("Custom Crest attack arrow sprite was not found for " + name + ".");
            }
            else if (fallbackSprite != null)
            {
                Plugin.Log?.LogInfo("Custom Crest attack arrows using fallback sprite '" + fallbackSprite.name + "'.");
            }

            return arrowRenderer;
        }

        private static Sprite FindFallbackArrowSprite()
        {
            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            Sprite best = null;
            int bestScore = int.MinValue;
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null)
                {
                    continue;
                }

                string spriteName = sprite.name == null ? string.Empty : sprite.name.ToLowerInvariant();
                if (spriteName.Contains("menu_border"))
                {
                    continue;
                }

                int score = 0;
                if (spriteName == "rt_lt_arrow")
                {
                    score += 120;
                }

                if (spriteName.Contains("arrow"))
                {
                    score += 10;
                }

                if (spriteName.Contains("left") || spriteName.Contains("right"))
                {
                    score += 8;
                }

                if (spriteName.Contains("up") || spriteName.Contains("down"))
                {
                    score += 4;
                }

                if (spriteName.Contains("controller_button_skins"))
                {
                    score += 2;
                }

                Bounds bounds = sprite.bounds;
                if (bounds.size.x > bounds.size.y * 1.2f)
                {
                    score += 3;
                }

                if (bounds.size.sqrMagnitude <= 0.0001f)
                {
                    score -= 20;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = sprite;
                }
            }

            return bestScore > 0 ? best : null;
        }

        private void ToggleDebugSpriteGrid()
        {
            if (debugSpriteGrid != null)
            {
                debugSpriteGrid.SetActive(true);
                Plugin.Log?.LogInfo("Custom Crest sprite debug grid shown.");
                return;
            }

            debugSpriteGrid = new GameObject("CustomCrestSpriteDebugGrid");
            Transform gridTransform = debugSpriteGrid.transform;
            Transform debugAnchor = GetDebugGridAnchor();
            gridTransform.SetParent(debugAnchor, false);
            gridTransform.localPosition = DebugSpriteGridLocalPosition;
            gridTransform.localRotation = Quaternion.identity;
            gridTransform.localScale = Vector3.one;

            Renderer sortRenderer = FindRowSortRenderer();
            int renderLayer = sortRenderer == null ? gameObject.layer : sortRenderer.gameObject.layer;
            SetLayerRecursive(debugSpriteGrid, renderLayer);
            List<SpriteDebugCandidate> candidates = CollectDebugSpriteCandidates();
            int count = Mathf.Min(DebugSpriteGridMaxItems, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                SpriteDebugCandidate candidate = candidates[i];
                GameObject item = new GameObject("SpriteDebug_" + i + "_" + candidate.Sprite.name);
                item.layer = renderLayer;
                Transform itemTransform = item.transform;
                itemTransform.SetParent(gridTransform, false);
                int column = i % DebugSpriteGridColumns;
                int row = i / DebugSpriteGridColumns;
                itemTransform.localPosition = new Vector3(
                    column * DebugSpriteGridCellSize.x,
                    -row * DebugSpriteGridCellSize.y,
                    0f);
                itemTransform.localRotation = Quaternion.identity;
                itemTransform.localScale = Vector3.one;

                SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
                renderer.sprite = candidate.Sprite;
                if (candidate.Template != null && candidate.Template.sharedMaterial != null)
                {
                    renderer.material = candidate.Template.sharedMaterial;
                }

                if (sortRenderer != null)
                {
                    renderer.sortingLayerID = sortRenderer.sortingLayerID;
                    renderer.sortingOrder = sortRenderer.sortingOrder + 80;
                }
                else if (candidate.Template != null)
                {
                    renderer.sortingLayerID = candidate.Template.sortingLayerID;
                    renderer.sortingOrder = candidate.Template.sortingOrder + 400;
                }

                renderer.color = Color.white;
                FitDebugSprite(itemTransform, renderer);
                Plugin.Log?.LogInfo(
                    "Custom Crest sprite debug [" + i + "] score=" + candidate.Score +
                    " sprite='" + candidate.Sprite.name +
                    "' template='" + (candidate.Template == null ? "<none>" : GetTransformPath(candidate.Template.transform)) +
                    "' bounds=" + candidate.Sprite.bounds.size);
            }

            Plugin.Log?.LogInfo("Custom Crest sprite debug grid created with " + count + " sprites.");
        }

        private void RebuildDebugSpriteGrid()
        {
            if (debugSpriteGrid != null)
            {
                Destroy(debugSpriteGrid);
                debugSpriteGrid = null;
            }

            ToggleDebugSpriteGrid();
        }

        private void EnsureDebugSpriteGridShown()
        {
            if (debugSpriteGrid == null)
            {
                ToggleDebugSpriteGrid();
            }
            else if (!debugSpriteGrid.activeSelf)
            {
                debugSpriteGrid.SetActive(true);
            }
        }

        private Transform GetDebugGridAnchor()
        {
            InventoryAttackSourceButton anchorButton = selectedButton ?? lastSelectedButton ?? downButton ?? upButton ?? neutralButton ?? dashButton ?? needleStrikeButton;
            return anchorButton == null ? transform : anchorButton.transform;
        }

        private static void SetLayerRecursive(GameObject target, int layer)
        {
            if (target == null)
            {
                return;
            }

            target.layer = layer;
            Transform targetTransform = target.transform;
            for (int i = 0; i < targetTransform.childCount; i++)
            {
                SetLayerRecursive(targetTransform.GetChild(i).gameObject, layer);
            }
        }

        private Renderer FindRowSortRenderer()
        {
            TextMeshPro[] texts = GetComponentsInChildren<TextMeshPro>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshPro text = texts[i];
                if (text == null || string.IsNullOrEmpty(text.text))
                {
                    continue;
                }

                Renderer renderer = text.GetComponent<Renderer>();
                if (renderer != null)
                {
                    return renderer;
                }
            }

            return null;
        }

        private static List<SpriteDebugCandidate> CollectDebugSpriteCandidates()
        {
            Dictionary<Sprite, SpriteDebugCandidate> candidates = new Dictionary<Sprite, SpriteDebugCandidate>();
            SpriteRenderer[] renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null)
                {
                    continue;
                }

                int score = ScoreDebugSprite(renderer.sprite, renderer.gameObject.name + " " + GetTransformPath(renderer.transform));
                SpriteDebugCandidate existing;
                if (!candidates.TryGetValue(renderer.sprite, out existing) || score > existing.Score)
                {
                    candidates[renderer.sprite] = new SpriteDebugCandidate(renderer.sprite, renderer, score);
                }
            }

            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null || candidates.ContainsKey(sprite))
                {
                    continue;
                }

                int score = ScoreDebugSprite(sprite, sprite.name);
                candidates[sprite] = new SpriteDebugCandidate(sprite, null, score);
            }

            return candidates.Values
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Sprite.name)
                .ToList();
        }

        private static int ScoreDebugSprite(Sprite sprite, string context)
        {
            if (sprite == null)
            {
                return -100;
            }

            string haystack = ((sprite.name ?? string.Empty) + " " + (context ?? string.Empty)).ToLowerInvariant();
            int score = 0;
            if (haystack.Contains("arrow"))
            {
                score += 60;
            }

            if (haystack.Contains("pause") || haystack.Contains("option") || haystack.Contains("setting"))
            {
                score += 35;
            }

            if (haystack.Contains("cursor") || haystack.Contains("selection") || haystack.Contains("select"))
            {
                score += 25;
            }

            if (haystack.Contains("left") || haystack.Contains("right"))
            {
                score += 18;
            }

            if (haystack.Contains("up") || haystack.Contains("down"))
            {
                score += 10;
            }

            if (haystack.Contains("button") || haystack.Contains("controller"))
            {
                score += 6;
            }

            Bounds bounds = sprite.bounds;
            if (bounds.size.x > bounds.size.y * 1.25f)
            {
                score += 8;
            }

            if (bounds.size.sqrMagnitude <= 0.0001f)
            {
                score -= 100;
            }

            return score;
        }

        private static void FitDebugSprite(Transform itemTransform, SpriteRenderer renderer)
        {
            if (itemTransform == null || renderer == null || renderer.sprite == null)
            {
                return;
            }

            Vector3 size = renderer.sprite.bounds.size;
            float maxSize = Mathf.Max(size.x, size.y);
            if (Mathf.Approximately(maxSize, 0f))
            {
                return;
            }

            itemTransform.localScale = Vector3.one * (0.24f / maxSize);
        }

        private void DumpAttackArrowDebug()
        {
            InventoryAttackSourceButton[] buttons = GetComponentsInChildren<InventoryAttackSourceButton>(true);
            Plugin.Log?.LogInfo("Custom Crest arrow debug: buttons=" + buttons.Length + ", switcherOpen=" + IsAttackSwitcherVisible);
            for (int i = 0; i < buttons.Length; i++)
            {
                InventoryAttackSourceButton button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                SpriteRenderer[] renderers = button.GetComponentsInChildren<SpriteRenderer>(true);
                for (int j = 0; j < renderers.Length; j++)
                {
                    SpriteRenderer renderer = renderers[j];
                    if (renderer == null || (renderer.gameObject.name != "LeftArrow" && renderer.gameObject.name != "RightArrow"))
                    {
                        continue;
                    }

                    Plugin.Log?.LogInfo(
                        "Custom Crest arrow debug: button='" + button.gameObject.name +
                        "' arrow='" + renderer.gameObject.name +
                        "' activeSelf=" + renderer.gameObject.activeSelf +
                        " activeInHierarchy=" + renderer.gameObject.activeInHierarchy +
                        " enabled=" + renderer.enabled +
                        " sprite='" + (renderer.sprite == null ? "<null>" : renderer.sprite.name) +
                        "' color=" + renderer.color +
                        " sortingLayer=" + renderer.sortingLayerName +
                        " sortingOrder=" + renderer.sortingOrder +
                        " localPos=" + renderer.transform.localPosition +
                        " localScale=" + renderer.transform.localScale +
                        " path='" + GetTransformPath(renderer.transform) + "'");
                }
            }
        }

        private sealed class SpriteDebugCandidate
        {
            public readonly Sprite Sprite;
            public readonly SpriteRenderer Template;
            public readonly int Score;

            public SpriteDebugCandidate(Sprite sprite, SpriteRenderer template, int score)
            {
                Sprite = sprite;
                Template = template;
                Score = score;
            }
        }

        private static void FitArrowHeight(Transform arrowTransform, SpriteRenderer arrowRenderer)
        {
            if (arrowTransform == null || arrowRenderer == null || arrowRenderer.sprite == null)
            {
                return;
            }

            float spriteHeight = arrowRenderer.sprite.bounds.size.y;
            if (Mathf.Approximately(spriteHeight, 0f))
            {
                return;
            }

            arrowTransform.localScale = Vector3.one * (ArrowHeight / spriteHeight);
        }

        private static TextMeshPro CreateText(Transform rowTransform, TextMeshPro template, string name, Vector3 localPosition, TextAlignmentOptions alignment)
        {
            return CreateText(rowTransform, template, name, localPosition, alignment, null);
        }

        private static TextMeshPro CreateText(Transform rowTransform, TextMeshPro template, string name, Vector3 localPosition, TextAlignmentOptions alignment, Vector2? boxSize)
        {
            Transform textTransform;
            TextMeshPro text;
            if (template != null)
            {
                textTransform = Instantiate(template.transform, rowTransform, false);
                text = textTransform.GetComponent<TextMeshPro>();
            }
            else
            {
                GameObject textObject = new GameObject(name);
                textTransform = textObject.transform;
                textTransform.SetParent(rowTransform, false);
                text = textObject.AddComponent<TextMeshPro>();
                text.color = Color.white;
            }

            textTransform.gameObject.name = name;
            textTransform.localPosition = localPosition;
            textTransform.localRotation = Quaternion.identity;
            textTransform.localScale = Vector3.one;
            text.text = string.Empty;
            text.fontSize = 6f;
            text.alignment = alignment;
            if (boxSize != null)
            {
                RectTransform rectTransform = text.rectTransform;
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = boxSize.Value;
                }
            }
            MatchEffectiveScale(textTransform, template);
            return text;
        }

        private static void MatchEffectiveScale(Transform textTransform, TextMeshPro template)
        {
            if (template == null || textTransform == null || textTransform.parent == null)
            {
                return;
            }

            Vector3 desiredScale = template.transform.lossyScale;
            Vector3 parentScale = textTransform.parent.lossyScale;
            textTransform.localScale = new Vector3(
                DivideScale(desiredScale.x, parentScale.x),
                DivideScale(desiredScale.y, parentScale.y),
                DivideScale(desiredScale.z, parentScale.z));
        }

        private static float DivideScale(float desired, float parent)
        {
            return Mathf.Approximately(parent, 0f) ? 1f : desired / parent;
        }

        private static TextMeshPro FindTemplateText(InventoryToolCrestList crestList)
        {
            TextMeshPro viewCrestsText = FindViewCrestsText();
            if (viewCrestsText != null)
            {
                return viewCrestsText;
            }

            TextMeshPro[] texts = crestList.GetComponentsInChildren<TextMeshPro>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && !string.IsNullOrEmpty(texts[i].text))
                {
                    return texts[i];
                }
            }

            return texts.Length > 0 ? texts[0] : null;
        }

        private static TextMeshPro FindViewCrestsText()
        {
            TextMeshPro[] texts = Resources.FindObjectsOfTypeAll<TextMeshPro>();
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshPro text = texts[i];
                if (text == null || string.IsNullOrEmpty(text.text))
                {
                    continue;
                }

                if (!IsPromptText(text.text))
                {
                    continue;
                }

                return text;
            }

            return null;
        }

        private static bool IsPromptText(string text)
        {
            return text.IndexOf("Crest", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                (text.IndexOf("View", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("Change", System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
