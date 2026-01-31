using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Component used for handling unit production with <see cref="ProductionQueue"/>.
    /// It will take care of spawning units after they are produced with the use
    /// of <see cref="spawnPoint"/>.
    /// <para>
    /// It will also attempt to garrison units if <see cref="GarrisonUnitsAfterProduction"/>
    /// is enabled and if <see cref="garrisonUnit"/> is not 'NULL'.
    /// </para>
    /// </summary>
    public class ProductionUnit : Unit, IControlUnit, IProduce
    {

        #region Properties

        /// <summary>
        /// Specifies behaviour for produced units. If this is set to `true`
        /// then <see cref="IGarrisonUnit"/> (if present) will be used to
        /// garrison units after production.
        /// </summary>
        [Tooltip("Set this to true if unit should garrison produced units. " +
            "Which means that they will need to be manually spawned." +
            "Garrison maximal capacity is still used.")]
        public bool GarrisonUnitsAfterProduction = false;

        /// <summary>
        /// Queue for producing units, resources, etc.
        /// </summary>
        public readonly ProductionQueue ProductionQueue = new ProductionQueue();

        /// <summary>
        /// Specifies the spawn point component required for spawning
        /// units after they are produced.
        /// </summary>
        [SerializeField]
        private AUnitSpawnPoint spawnPoint;

        private IGarrisonUnit garrisonUnit;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            garrisonUnit = GetComponent<IGarrisonUnit>();
        }

        private void OnEnable()
        {
            ProductionQueue.OnProductionFinished += HandleFinishedProduction;
        }

        private void OnDisable()
        {
            ProductionQueue.OnProductionFinished -= HandleFinishedProduction;
        }

        #endregion

        #region Production

        /// <summary>
        /// Queues production action on the unit or splits it into multiple
        /// productions.
        /// </summary>
        /// <param name="action">Production action to be queued</param>
        /// <param name="queueMultipleOrders">Split production quantity into multiple orders</param>
        public void StartProduction(ProductionAction action, bool queueMultipleOrders = false)
        {
            ProductionQueue.AddProductionOrder(action.Producable, action.Quantity, queueMultipleOrders);
        }

        /// <summary>
        /// Queues the production of producable with quantity or splits it into
        /// multiple productions.
        /// </summary>
        /// <param name="producable">Producable to be produced</param>
        /// <param name="quantity">Number of producables</param>
        /// <param name="queueMultipleOrders">Split production quantity into multiple orders</param>
        public void StartProduction(ProducableSO producable, float quantity, bool queueMultipleOrders = false)
        {
            ProductionQueue.AddProductionOrder(producable, quantity, queueMultipleOrders);
        }

        /// <summary>
        /// Convenience method to cancel all production orders. Specific production
        /// can be cancelled on <see cref="ProductionQueue"/> directly.
        /// </summary>
        public void CancelProduction() => ProductionQueue.CancelProduction();

        private void HandleFinishedProduction(ProducableQuantity item)
        {
            // Update player
            Owner.AddProducable(item.Producable, item.Quantity);

            // Spawn unit
            switch (item.Producable)
            {
                case AUnitSO unit:
                    for (int index = 0; index < item.Quantity; index++)
                    {
                        SpawnProducedUnit(unit);
                    }

                    break;

                default:
                    break;
            }
        }

        private void SpawnProducedUnit(AUnitSO unit)
        {
            // Start coroutine to load the prefab as it may happen async.
            StartCoroutine(unit.GetAssociatedPrefab(prefab =>
            {
                if (prefab == null)
                {
                    Debug.LogWarning("Prefab was returned NULL for AUnitSO named '" + unit.Name + "' so unit cannot be spawned.");
                    return;
                }

                var newUnit = spawnPoint.SpawnUnit(prefab);
                Owner.AddUnit(newUnit);

                GarrisonSpawnedUnitIfNeeded(newUnit);
            }));
        }

        /// <summary>
        /// Garrison unit after spawning it & disable it immediately
        /// so that the unit does not appear in scene. Reference of
        /// the disabled unit will remain within the garrison.
        /// </summary>
        /// <param name="newUnit">Newly spawned unit.</param>
        private void GarrisonSpawnedUnitIfNeeded(Unit newUnit)
        {
            if (garrisonUnit != null && GarrisonUnitsAfterProduction
                && newUnit.TryGetComponent(out IGarrisonableUnit garrisonableUnit))
            {
                if (garrisonUnit.GarrisonUnits.Count < garrisonUnit.MaxCapacity)
                {
                    garrisonUnit.AddUnit(newUnit);
                    garrisonableUnit.EnterGarrison();
                }
            }
        }

        #endregion

        #region IProduce

        /// <summary>
        /// Updates production time on production queue.
        /// </summary>
        /// <param name="delta">Amount of progression for the queue</param>
        public void Produce(float delta)
        {
            ProductionQueue.Produce(delta);
        }

        #endregion

        #region IControlUnit

        /// <summary>
        /// Sets the spawn point target position.
        /// </summary>
        /// <param name="groundPosition">New target position for spawned units.</param>
        public void SetTargetPosition(Vector3 groundPosition)
        {
            spawnPoint.SetTargetPosition(groundPosition);
        }

        #endregion
    }

}
