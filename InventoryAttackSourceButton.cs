using GlobalEnums;
using TMProOld;
using UnityEngine;

namespace CrestLoadouts
{
    internal sealed class InventoryAttackSourceButton : InventoryItemSelectableDirectional
    {
        protected override bool IsAutoNavSelectable
        {
            get { return false; }
        }

        private AttackSlot attackSlot;
        private string label;
        private TextMeshPro labelText;
        private TextMeshPro valueText;
        private SpriteRenderer leftArrowRenderer;
        private SpriteRenderer rightArrowRenderer;
        private Color labelBaseColor = Color.white;
        private Color valueBaseColor = Color.white;
        private bool isPanelSelected;
        private bool isSelectedInPanel;
        private float panelAlpha = 1f;

        public void Setup(
            AttackSlot slot,
            string displayLabel,
            TextMeshPro labelTextMesh,
            TextMeshPro valueTextMesh,
            SpriteRenderer leftArrow,
            SpriteRenderer rightArrow)
        {
            attackSlot = slot;
            label = displayLabel;
            labelText = labelTextMesh;
            valueText = valueTextMesh;
            leftArrowRenderer = leftArrow;
            rightArrowRenderer = rightArrow;
            labelBaseColor = labelText == null ? Color.white : labelText.color;
            valueBaseColor = valueText == null ? Color.white : valueText.color;
            CursorGlowScale = Vector2.one;
            RefreshText();
            ApplySelectionDisplay();
        }

        public override string DisplayName
        {
            get { return "Crest Loadouts"; }
        }

        public override string Description
        {
            get { return "Change the attack source for this direction."; }
        }

        public override bool Submit()
        {
            if (Plugin.Instance == null)
            {
                return false;
            }

            if (!Plugin.Instance.CanChangeAttackSources())
            {
                ShowAttackBenchMessage();
                return false;
            }

            Plugin.Instance.CycleSaveAttackSource(GetSelectedCrestId(), attackSlot);
            PlayActivationSound();
            RefreshAllInParent();
            return true;
        }

        public bool Cycle(int direction)
        {
            if (Plugin.Instance == null)
            {
                return false;
            }

            if (!Plugin.Instance.CanChangeAttackSources())
            {
                ShowAttackBenchMessage();
                return false;
            }

            Plugin.Instance.CycleSaveAttackSource(GetSelectedCrestId(), attackSlot, direction);
            PlayActivationSound();
            RefreshAllInParent();
            return true;
        }

        public override void Select(InventoryItemManager.SelectionDirection? direction)
        {
            base.Select(direction);
            InventoryAttackSourcePanel panel = GetComponentInParent<InventoryAttackSourcePanel>(true);
            if (panel != null)
            {
                panel.SetSelectedButton(this);
            }
        }

        public override void Deselect()
        {
            base.Deselect();
            InventoryAttackSourcePanel panel = GetComponentInParent<InventoryAttackSourcePanel>(true);
            if (panel != null)
            {
                panel.ClearSelectedButton(this);
            }
        }

        public void SetPanelSelectionState(bool panelSelected, bool selectedInPanel)
        {
            isPanelSelected = panelSelected;
            isSelectedInPanel = selectedInPanel;
            ApplySelectionDisplay();
        }

        public void SetPanelAlpha(float alpha)
        {
            panelAlpha = Mathf.Clamp01(alpha);
            ApplySelectionDisplay();
        }

        public void RefreshText()
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            AttackSource source = Plugin.Instance.GetSaveAttackSource(GetSelectedCrestId(), attackSlot);
            if (labelText != null)
            {
                labelText.text = label + ":";
            }

            if (valueText != null)
            {
                valueText.text = Plugin.GetAttackSourceDisplayName(source);
            }

            ApplySelectionDisplay();
        }

        private void ApplySelectionDisplay()
        {
            Color labelColor = isPanelSelected && !isSelectedInPanel ? Dim(labelBaseColor) : labelBaseColor;
            Color valueColor = isPanelSelected && !isSelectedInPanel ? Dim(valueBaseColor) : valueBaseColor;
            labelColor.a = labelBaseColor.a * panelAlpha;
            valueColor.a = valueBaseColor.a * panelAlpha;
            if (labelText != null)
            {
                labelText.color = labelColor;
            }

            if (valueText != null)
            {
                valueText.color = valueColor;
            }

            Color arrowColor = labelBaseColor;
            arrowColor.a *= panelAlpha;
            SetArrowVisible(leftArrowRenderer, isPanelSelected && isSelectedInPanel && panelAlpha > 0.01f, arrowColor);
            SetArrowVisible(rightArrowRenderer, isPanelSelected && isSelectedInPanel && panelAlpha > 0.01f, arrowColor);
        }

        private static void SetArrowVisible(SpriteRenderer arrowRenderer, bool visible, Color color)
        {
            if (arrowRenderer == null)
            {
                return;
            }

            arrowRenderer.gameObject.SetActive(visible);
            arrowRenderer.enabled = visible;
            arrowRenderer.color = color;
        }

        private static Color Dim(Color color)
        {
            return new Color(color.r * 0.58f, color.g * 0.58f, color.b * 0.58f, color.a);
        }

        private string GetSelectedCrestId()
        {
            InventoryAttackSourcePanel panel = GetComponentInParent<InventoryAttackSourcePanel>(true);
            return panel == null ? Plugin.GetEquippedCrestId() : panel.SelectedCrestId;
        }

        private void PlayActivationSound()
        {
            InventoryItemToolManager manager = GetComponentInParent<InventoryItemToolManager>();
            if (manager != null)
            {
                manager.PlayMoveSound();
            }
        }

        private void ShowAttackBenchMessage()
        {
            InventoryItemToolManager manager = GetComponentInParent<InventoryItemToolManager>();
            if (manager == null)
            {
                return;
            }

            if (manager.ShowingCrestMsg)
            {
                manager.HideCrestEquipMsg(false);
                return;
            }

            Plugin.ShowAttackBenchMessage(manager);
        }

        private void RefreshAllInParent()
        {
            InventoryAttackSourcePanel panel = GetComponentInParent<InventoryAttackSourcePanel>(true);
            InventoryAttackSourceButton[] buttons = panel == null
                ? new[] { this }
                : panel.GetComponentsInChildren<InventoryAttackSourceButton>(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].RefreshText();
            }
        }
    }
}
