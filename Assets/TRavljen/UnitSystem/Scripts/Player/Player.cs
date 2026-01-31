using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Next to responsibilities of <see cref="APlayer"/> this component
    /// also connects various management components that allow player to
    /// track it's stats from a single component. Each player in game should
    /// have a component that implements <see cref="APlayer"/> in order to
    /// attach information to Units spawned in scene.
    ///
    /// <para>
    /// If you wish to customise behaviour of the player completely, derive
    /// <see cref="APlayer"/> to use it with the <see cref="UnitSystem"/>
    /// components.
    /// </para>
    /// </summary>
    public class Player : APlayer, IProductionQueueDelegate
    {

        #region Properties

        /// <summary> Specifies the research manager for the player. </summary>
        [Tooltip("Specifies the research manager for the player.")]
        public ResearchManager ResearchManager;

        /// <summary> Specifies the resource manager for the player. </summary>
        [Tooltip("Specifies the resource manager for the player.")]
        public ResourceManager ResourceManager;

        /// <summary> Specifies the population manager for the player. </summary>
        [Tooltip("Specifies the population manager for the player.")]
        public PopulationManager PopulationManager;

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            OnProducableAdded += HandleProducableAdded;
        }

        private void OnDisable()
        {
            OnProducableAdded -= HandleProducableAdded;
        }

        private void HandleProducableAdded(ProducableSO producable, float quantity)
        {
            // Basic handling for research and resources. To change behaviour adjust
            // this code or create a new Player class to your own expectations.
            switch (producable)
            {
                case ResearchSO research:
                    for (int index = 0; index < quantity; index++)
                        ResearchManager.AddFinishedResearch(research);
                    break;

                case ResourceSO resource:
                    ResourceManager.AddResource(new ProducableQuantity(resource, quantity));
                    break;
            }
        }

        #endregion

        #region Units

        public override void AddUnit(Unit unit)
        {
            base.AddUnit(unit);

            if (unit.TryGetComponent(out ProductionUnit unitProduction))
            {
                unitProduction.ProductionQueue.Delegate = this;
            }
        }

        public override void RemoveUnit(Unit unit)
        {
            base.RemoveUnit(unit);

            if (unit.TryGetComponent(out ProductionUnit unitProduction))
            {
                unitProduction.ProductionQueue.Delegate = null;
            }
        }

        public float AddResource(ProducableSO producable, float quantity)
        {
            switch (producable)
            {
                case ResourceSO resource:
                    return ResourceManager.AddResource(new ProducableQuantity(resource, quantity));

                default: return -1; // should not be added, not a resource.
            }
        }

        public override bool ContainsProducable(ProducableQuantity producableQuantity)
        {
            string producableUUID = producableQuantity.Producable.UUID;

            switch (producableQuantity.Producable)
            {
                case ResearchSO research:
                    return ResearchManager.IsResearchComplete(research);

                case AUnitSO unit:
                    int quantity = 0;

                    foreach (var _unit in units)
                        if (_unit.Data.UUID == producableUUID)
                            quantity++;

                    return quantity >= producableQuantity.Quantity;

                default: return false;
            }
        }

        #endregion

        #region IProductionQueueDelegate

        bool IProductionQueueDelegate.ShouldFinishProductionFor(ProducableQuantity producableQuantity)
        {
            // First check if population is valid for this producable
            if (PopulationManager != null &&
                !PopulationManager.ShouldFinishProductionFor(producableQuantity))
            {
                return false;
            }

            // Second check requirements of the producable
            if (!FulfillsRequirements(producableQuantity.Producable, producableQuantity.Quantity))
            {
                return false;
            }

            // Finally in case of a resource production, check if it has storage for it.
            switch (producableQuantity.Producable)
            {
                case ResourceSO resource:
                    return ResourceManager.HasEnoughStorage(resource, producableQuantity.Quantity);
                default: return true;
            }
        }

        #endregion

    }

}