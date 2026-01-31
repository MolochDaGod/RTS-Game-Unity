using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnitSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitSystem.Utility;

namespace UnitSystem.Demo
{

    /// <summary>
    /// Demo component for controlling UI of the town hall and barracks,
    /// default actions that do not require production queue.
    /// In this demo you can build Town hall and Barrack each one time, if one is
    /// destroyed, it cannot be rebuilt.
    /// </summary>
    public class StructuresTabUI : MonoBehaviour
    {
        #region Properties

        [SerializeField]
        private Player player;

        /// <summary>
        /// Specifies structure object used for townhall. Expected to train workers.
        /// </summary>
        [SerializeField]
        private ProductionUnit townHall;

        /// <summary>
        /// Specifies structure object used for barracks. Expected to train soldiers.
        /// </summary>
        [SerializeField]
        private ProductionUnit barracks;

        /// <summary>
        /// Specifies the button for building town hall.
        /// </summary>
        [SerializeField]
        private Button buildTownHallButton;

        /// <summary>
        /// Specifies the button for building barracks.
        /// </summary>
        [SerializeField]
        private Button buildBarracksButton;

        /// <summary>
        /// Specifies the text use to display current production in form of a text.
        /// </summary>
        [SerializeField]
        private Text productionText;

        /// <summary>
        /// Specifies the primary visual component. This is disable when a unit
        /// is selected.
        /// </summary>
        [SerializeField]
        private Transform view;

        /// <summary>
        /// Specifies the unit selector component.
        /// </summary>
        [SerializeField]
        private UnitSelector unitSelector;

        private AUnitSO townHallData => townHall.Data;

        private AUnitSO barracksData => barracks.Data;

        /// <summary>
        /// Action invoked when town hall has been built.
        /// </summary>
        public Action TownHallBuilt;

        /// <summary>
        /// Action invoked when barrack has been built.
        /// </summary>
        public Action BarracksBuilt;

        #endregion

        #region Lifecycle

        private void Start()
        {
            // Disable interaction until town hall is built.
            buildBarracksButton.interactable = false;
        }

        private void Update()
        {
            productionText.text = GetStructureProducablesText();
        }

        private void OnEnable()
        {
            player.OnUnitRemoved += HandleUnitRemoved;
            unitSelector.OnUnitSelectionChange += SelectionChanged;
        }

        private void OnDisable()
        {
            player.OnUnitRemoved -= HandleUnitRemoved;
            unitSelector.OnUnitSelectionChange -= SelectionChanged;
        }

        #endregion

        private void HandleUnitRemoved(Unit unit)
        {
            if (unit == townHall)
            {
                buildBarracksButton.interactable = false;
            }
        }

        private void SelectionChanged(Unit selectedUnit)
        {
            // Show this tab when there is no structure selected.
            view.gameObject.SetActive(selectedUnit == null);
        }

        #region Actions

        public void BuildTownHall()
        {
            var cost = townHallData.Cost;

            if (player.ResourceManager.HasEnoughResources(cost))
            {
                var unit = townHall.GetComponent<Unit>();
                player.ResourceManager.ConsumeResources(cost);
                player.AddUnit(unit);
                player.AddProducable(unit.Data);
                townHall.gameObject.SetActive(true);

                StartCoroutine(RaiseUp(townHall.transform));
                TownHallBuilt?.Invoke();

                buildTownHallButton.interactable = false;
                buildBarracksButton.interactable = true;
            }
            else
            {
                InfoLogger.Log("Not enough resources to produce a TOWN HALL structure.");
                return;
            }
        }

        public void BuildBarracks()
        {
            var cost = barracksData.Cost;

            if (player.ResourceManager.HasEnoughResources(cost))
            {
                player.ResourceManager.ConsumeResources(cost);

                barracks.gameObject.SetActive(true);
                StartCoroutine(RaiseUp(barracks.transform));

                buildBarracksButton.interactable = false;

                var unit = barracks.GetComponent<Unit>();
                player.AddUnit(unit);
                player.AddProducable(unit.Data);
                BarracksBuilt?.Invoke();
            }
            else
            {
                InfoLogger.Log("Not enough resources to produce a TOWN HALL structure.");
            }
        }

        #endregion

        #region Convenience

        public string GetStructureProducablesText()
        {
            var text = "";

            ProductionUnit unitProduction;

            if (unitSelector.SelectedUnit != null &&
                unitSelector.SelectedUnit is ProductionUnit)
            {
                unitProduction = (ProductionUnit)unitSelector.SelectedUnit;
            }
            else
            {
                return text;
            }

            var productionUnitData = (ProductionUnitSO)unitProduction.Data;
            if (productionUnitData.ProducesResource.Count > 0)
            {
                text += "RESOURCES (per second): \n\n";

                foreach (var resource in productionUnitData.ProducesResource)
                {
                    text += resource.Producable.Name + ": " + resource.Quantity + "\n";
                }

                text += "\n";
            }

            text += GetCurrentProductionProgress(unitProduction);

            List<ProducableSO> remainingQueue = new List<ProducableSO>(
                unitProduction.ProductionQueue.Queue.Select(item => item.Producable));

            // First remove the one in progress
            if (remainingQueue.Count > 0)
            {
                remainingQueue.RemoveAt(0);
            }

            // Check if remainder is more than 0, then add name and count
            // to the text
            if (remainingQueue.Count > 0)
            {
                // Remove first as its in progress

                text += "\nQUEUED: \n";

                var queuedItems =
                    from producable in remainingQueue
                    group producable by producable.Name into Name
                    select new { Name = Name.Key, Count = Name.Count() };

                var size = queuedItems.Count();
                for (int index = 0; index < size; index++)
                {
                    var item = queuedItems.ElementAt(index);
                    text += " (" + item.Count + ") " + item.Name;

                    if (index + 1 != size)
                    {
                        text += ",\n";
                    }
                }
            }

            return text;
        }

        /// <summary>
        /// Animations transform Vector3.up for 2.0f.
        /// This is just to demonstrate building with minor animation.
        /// For your game instantiate the game object before building it and not
        /// before to avoid unnecessary memory consumption.
        /// </summary>
        private IEnumerator RaiseUp(Transform transform)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = transform.position + Vector3.up * 2.0f;

            const float maxTime = 0.3f;
            const float interval = 0.016f;
            float time = 0f;

            while (time < maxTime)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, time / maxTime);

                yield return new WaitForSeconds(interval);
                time += interval;
            }

            // Wait remaining time and move to target
            yield return new WaitForSeconds(maxTime - time);
            transform.position = targetPos;
        }

        private string GetCurrentProductionProgress(ProductionUnit unitProduction)
        {
            var progress = unitProduction.ProductionQueue.CurrentProductionProgress;
            var queue = unitProduction.ProductionQueue.Queue;

            // Check if factory is producing
            if (progress != -1 && queue.Length > 0)
            {
                // Then display its name and progress
                return queue[0].Producable.Name + " (" + queue[0].Quantity + ")" + ": " + Mathf.Round(progress * 100) + "%\n";
            }

            return "";
        }

        #endregion

    }

}