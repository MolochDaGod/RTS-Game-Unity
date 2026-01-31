using System;
using UnitSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UnitSystem.Demo
{
    /// <summary>
    /// Component responsible for controlling UI of the selected unit.
    /// </summary>
    public class SelectedUnitUI : MonoBehaviour
    {

        [SerializeField]
        private UnitSelector unitSelector;

        [SerializeField]
        private HorizontalLayoutGroup container;

        [SerializeField]
        private UnitProductionView unitProductionViewPrefab;

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Button destroyUnitButton;

        /// <summary>
        /// Action invoked when production action is clicked.
        /// </summary>
        public Action<ProductionAction> OnProductionActionTriggered;

        /// <summary>
        /// Currently dsplayed unit.
        /// </summary>
        public Unit DisplayedUnit { private set; get; }

        private void OnEnable()
        {
            unitSelector.OnUnitSelectionChange += SelectionChanged;
        }

        private void OnDisable()
        {
            unitSelector.OnUnitSelectionChange -= SelectionChanged;
        }

        private void SelectionChanged(Unit selectedUnit)
        {
            // Apply changes only if there was a change to apply
            if (DisplayedUnit != selectedUnit)
            {
                DisplayedUnit = selectedUnit;

                // If structure was selected build new UI
                if (DisplayedUnit != null)
                {
                    ReloadUI();
                    titleText.text = selectedUnit.Data.Name;
                }
                // Else structure was deselected, clear UI
                else
                {
                    titleText.text = "Default player actions";
                    ClearUI();
                }
            }

            destroyUnitButton.gameObject.SetActive(selectedUnit != null);
        }

        /// <summary>
        /// Destroys selected unit and removes it from the unit owner (player).
        /// </summary>
        public void DestroySelectedUnit()
        {
            var unit = unitSelector.SelectedUnit;
            if (unit != null)
            {
                // Release all units if player is destroying a garrison
                if (unit.TryGetComponent(out IGarrisonUnit garrisonUnit))
                {
                    garrisonUnit.RemoveAllUnits();
                }

                unit.Owner.RemoveProducable(unit.Data);
                unit.Owner.RemoveUnit(unit);

                Destroy(unit.gameObject);
                unitSelector.SetSelectedUnit(null);
                ClearUI();
            }
        }

        /// <summary>
        /// Reconstructs visual elements for the selected unit.
        /// </summary>
        public void ReloadUI()
        {
            ClearUI();

            if (DisplayedUnit != null)
            {
                BuildUI();
            }
        }

        private void BuildUI()
        {
            if (!(DisplayedUnit.Data is ProductionUnitSO)) return;

            var Data = (ProductionUnitSO)DisplayedUnit.Data;

            foreach (ProductionAction action in Data.GetAllProductionActions())
            {
                var view = Instantiate(unitProductionViewPrefab, container.transform);
                var state = GetButtonState(action);

                view.Configure(action, state);
                view.OnActionButtonClicked += action =>
                {
                    OnProductionActionTriggered.Invoke(action);
                };
            }
        }

        /// <summary>
        /// Creates button state based on action producable where it checks
        /// various states of the producable and if it fulfills requirements
        /// </summary>
        private UnitProductionView.ProductionActionButtonState GetButtonState(
            ProductionAction action)
        {
            var producable = action.Producable;
            var cancellable = producable is ResearchSO && unitSelector
                    .SelectedUnit
                    .GetComponent<ProductionUnit>()
                    .ProductionQueue
                    .IsProducing(producable);

            var fulfillsRequirements = DisplayedUnit.Owner
                .FulfillsRequirements(producable, action.Quantity);

            var complete = !cancellable && producable is ResearchSO && DisplayedUnit
                .Owner
                .ContainsProducable(producable);

            if (complete)
            {
                return UnitProductionView.ProductionActionButtonState.Completed;
            }
            else if (cancellable)
            {
                return UnitProductionView.ProductionActionButtonState.Cancellable;
            }
            else if (fulfillsRequirements)
            {
                return UnitProductionView.ProductionActionButtonState.Default;
            }

            return UnitProductionView.ProductionActionButtonState.Disabled;
        }

        private void ClearUI()
        {
            for (int i = container.transform.childCount - 1; i >= 0; --i)
            {
                var child = container.transform.GetChild(i).gameObject;
                Destroy(child);
            }
        }

    }

}