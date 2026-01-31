using System.Collections;
using System.Collections.Generic;
using UnitSystem.Utility;
using UnityEngine;

namespace UnitSystem.Demo
{

    /// <summary>
    /// Player component responsible for invoking production actions for the player
    /// and is also updating UI when production is finished. Generally this should
    /// be avoided in the player controller, as game manager, ui management and
    /// player controller should be decoupled.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {

        /// <summary>
        /// Specifies the player for control.
        /// </summary>
        public Player Player;

        /// <summary>
        /// Specifies the DEMO selected unit UI that displays actions of the
        /// currently selected unit.
        /// </summary>
        public SelectedUnitUI SelectedUnitUI;

        /// <summary>
        /// Specify UI container for the GARRISON button action.
        /// This is used to show the container once Town Hall is built.
        /// </summary>
        public GameObject GarrisonActionContainer;

        public UnitSelector UnitSelector;

        /// <summary>
        /// Specifies action hotkeys for controlling unit production with keyboard.
        /// </summary>
        [SerializeField]
        private List<KeyCode> ActionHotKeys = new List<KeyCode>();

        /// <summary>
        /// Specifies turn based time controller. If this is active then
        /// turn base hotkey is available (<see cref="KeyCode.D"/>.
        /// </summary>
        [SerializeField]
        private TBSTimeController tbsTimeController;

        private ResearchManager researchManager => Player.ResearchManager;
        private ResourceManager resourceManager => Player.ResourceManager;

        private void OnEnable()
        {
            SelectedUnitUI.OnProductionActionTriggered += HandleProductionAction;
            researchManager.OnResearchFinished += HandleResearchFinished;
            Player.OnProducableAdded += HandleProducableAdded;
            Player.OnProducableRemoved += HandleProducableRemoved;
            Player.OnUnitRemoved += HandleUnitRemoved;
        }

        private void OnDisable()
        {
            SelectedUnitUI.OnProductionActionTriggered -= HandleProductionAction;
            researchManager.OnResearchFinished -= HandleResearchFinished;
            Player.OnProducableAdded -= HandleProducableAdded;
            Player.OnProducableRemoved -= HandleProducableRemoved;
            Player.OnUnitRemoved -= HandleUnitRemoved;
        }

        private void Update()
        {
            if (SelectedUnitUI.DisplayedUnit != null)
            {
                // If structure is selected, check if any hotkeys are pressed
                // to initiate actions from keyboard.
                TriggerUnitActionHotKeysIfPressed(SelectedUnitUI.DisplayedUnit);
            }

            if (tbsTimeController.gameObject.activeSelf &&
                Input.GetKeyDown(KeyCode.D))
            {
                tbsTimeController.EndTurnForCurrentPlayer();
            }
        }

        private void TriggerUnitActionHotKeysIfPressed(Unit unit)
        {
            var unitData = unit.Data;
            if (!(unitData is ProductionUnitSO)) return;

            var productionUnit = (ProductionUnitSO)unitData;

            var productionActions = productionUnit.GetAllProductionActions();
            for (int index = 0; index < productionActions.Count; index++)
            {
                var action = productionActions[index];
                if (index < ActionHotKeys.Count && Input.GetKeyDown(ActionHotKeys[index]))
                {
                    switch (action.Producable)
                    {
                        case ResearchSO research:
                            // In case research is complete, ignore the action.
                            if (researchManager.IsResearchComplete(research))
                                return;
                            break;

                        default:
                            // Others do not get disabled in the demo.
                            break;
                    }

                    HandleProductionAction(action);
                }
            }
        }

        private void HandleProducableAdded(ProducableSO producable, float quantity)
        {
            if (producable.Name == "Barracks")
                SelectedUnitUI.ReloadUI();

            if (producable.Name == "Town Hall")
                GarrisonActionContainer.SetActive(true);
        }

        private void HandleProducableRemoved(ProducableSO producable)
        {
            if (producable.Name == "Town Hall")
                GarrisonActionContainer.SetActive(false);
        }

        private void HandleResearchFinished(ProducableSO research)
            => SelectedUnitUI.ReloadUI();

        private void HandleProductionAction(ProductionAction action)
        {
            var structure = SelectedUnitUI.DisplayedUnit;
            var productionStructure = structure.GetComponent<ProductionUnit>();

            var producable = action.Producable;

            // Check if producable is research & if research production is already in progress
            if (producable is ResearchSO && productionStructure.ProductionQueue.IsProducing(producable))
            {
                // Cancel production and refund resources
                productionStructure.ProductionQueue.CancelProductionOrder(producable);
                resourceManager.AddResources(producable.Cost.ToArray());

                SelectedUnitUI.ReloadUI();
                return;
            }

            // Otherwise if it fulfills requirements & cost, start production
            if (Player.FulfillsRequirements(producable, action.Quantity))
            {
                List<ProducableQuantity> fullCost = new List<ProducableQuantity>();

                foreach (var producableQuantity in action.Producable.Cost)
                {
                    fullCost.Add(new ProducableQuantity(
                        producableQuantity.Producable,
                        producableQuantity.Quantity * action.Quantity));
                }

                // Make sure that full cost of the action is calculate because if
                // you only use action.Producable.Cost and action.Quantity is not
                // equal 1, then check for enough resources and consuming them won't
                // be the correct quantity.
                if (resourceManager.HasEnoughResources(fullCost))
                {
                    resourceManager.ConsumeResources(fullCost);
                    productionStructure.StartProduction(action, false);

                    SelectedUnitUI.ReloadUI();
                }
                else
                {
                    InfoLogger.Log("Not enough resources to produce a WORKER unit.");
                }
            }
            else
            {
                InfoLogger.Log("Town hall does not have this action for this time period and index!");
            }
        }

        private void HandleUnitRemoved(Unit unit)
        {
            // If town hall was destroyed, remove option to garrison units in DEMO scene.
            if (unit.Data.Name == "Town Hall")
            {
                GarrisonActionContainer.gameObject.SetActive(false);
            }

            // Selected unit is about to be destroyed, remove its reference.
            if (unit == UnitSelector.SelectedUnit)
            {
                UnitSelector.SetSelectedUnit(null);
            }

            List<ProducableQuantity> resourcesToRefund = new List<ProducableQuantity>();

            // If unit that will be destroyed is a production unit, then expected result
            // would be for production to cancel all orders and refund player with
            // resources consumed by those orders.
            if (unit.TryGetComponent(out ProductionUnit unitProduction))
            {
                var queue = unitProduction.ProductionQueue.Queue;
                foreach (var item in queue)
                {
                    foreach (var cost in item.Producable.Cost)
                    {
                        // Multiply each resource with item quantity
                        resourcesToRefund.Add(
                            new ProducableQuantity(cost.Producable, cost.Quantity * item.Quantity));
                    }
                }

                unitProduction.CancelProduction();
            }

            // Refund player with resources consumed by destroyed production.
            // This can of course differentiate based on the type of game you
            // are making. A punishing mechanic would be not to refund resources.   
            resourceManager.AddResources(resourcesToRefund.ToArray());
        }
    }

}