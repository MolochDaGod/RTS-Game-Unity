using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Component responsible for managing population of a player.
    /// Many RTS games use population to control the number of units
    /// that can be spawned in a game.
    /// <para>
    /// This component handles attributes of units! If you do not use maximal
    /// population and do not want to limit max population, then you may add
    /// only an attribute on the unit definition for population consumption
    /// (with this attribute you can easily track players current population
    /// count - <see cref="populationConsumptionAttributeUUID"/>).
    /// If the player can increase maximal population by constructing or
    /// producing other units, then make sure to set
    /// <see cref="populationCapacityAttributeUUID"/> for manager to handle this
    /// automatically.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(APlayer))]
    public class PopulationManager : MonoBehaviour
    {

        /// <summary>
        /// Specifies the UUID of the population consumption attribute used
        /// for updating population count.
        /// </summary>
        [SerializeField, Tooltip("Specifies the UUID of the population " +
            "consumption attribute used for updating population count.")]
        private string populationConsumptionAttributeUUID;

        /// <summary>
        /// Specifies the UUID of the population capacity attribute used for
        /// maximal population capacity of units.
        /// </summary>
        [SerializeField, Tooltip("Specifies the UUID of the population capacity " +
            "attribute used for maximal population capacity of units.")]
        private string populationCapacityAttributeUUID;

        /// <summary>
        /// Specifies maximal population count allowed.
        /// </summary>
        [SerializeField, Tooltip("Specifies maximal population count allowed.")]
        private float maxPopulation = 10;

        /// <summary>
        /// Set this to 'false' if population should not be capped at
        /// MAX population.
        /// </summary>
        [SerializeField, Tooltip("Set this to 'false' if population " +
            "should not be capped at MAX population.")]
        private bool maxPopulationEnabled = true;

        /// <summary>
        /// Returns true if the maximal population is enabled, which means
        /// population is capped.
        /// </summary>
        public bool MaxPopulationEnabled => maxPopulationEnabled;

        /// <summary>
        /// Specifies the current population.
        /// </summary>
        private float currentPopulation = 0;

        /// <summary>
        /// Player for which population is being managed.
        /// </summary>
        private APlayer player;

        /// <summary>
        /// Maximal population allowed. If maximal population is disabled (not
        /// used), then '-1' will be returned.
        /// </summary>
        public int MaxPopulation {
            get
            {
                if (maxPopulationEnabled)
                {
                    return (int)maxPopulation;
                }

                return -1;
            }
        }

        /// <summary>
        /// Current population count.
        /// </summary>
        public int CurrentPopulation => (int)currentPopulation;

        /// <summary>
        /// Action invoked when population is updated, either current or maximal.
        /// </summary>
        public System.Action<int, int> OnPopulationUpdate;

        #region Lifecycle

        private void Awake()
        {
            player = GetComponent<APlayer>();
        }

        private void OnEnable()
        {
            player.OnProducableAdded += HandleProducableAdded;
            player.OnProducableRemoved += HandleProducableRemoved;
        }

        private void OnDisable()
        {
            player.OnProducableAdded -= HandleProducableAdded;
            player.OnProducableRemoved -= HandleProducableRemoved;
        }

        #endregion

        #region Handle producables

        private void HandleProducableAdded(ProducableSO producable, float quantity)
        {
            // When unit is added, it may contain population attributes.

            if (GetPopulationCapacityAttribute(producable, out AttributeValue capacity))
            {
                int value = (int)(capacity.Value * quantity);
                ModifyMaxPopulation(value);
            }

            if (GetPopulationConsumptionAttribute(producable, out AttributeValue consumption))
            {
                ModifyPopulation(consumption.Value * quantity);
            }
        }

        private void HandleProducableRemoved(ProducableSO producable)
        {
            // If removed producable has a population consumption, it will be returned
            // here to the Player.
            if (GetPopulationConsumptionAttribute(producable, out AttributeValue attribute))
            {
                ModifyPopulation(-(int)attribute.Value);
            }

            // If removed producable has a population attribute, we should modify
            // max population capacity.
            if (GetPopulationCapacityAttribute(producable, out AttributeValue capacity))
            {
                ModifyMaxPopulation(-(int)capacity.Value);
            }
        }

        #endregion

        #region Population Helpers

        private bool GetPopulationCapacityAttribute(ProducableSO producable, out AttributeValue attribute)
        {
            attribute = producable.Attributes
                .Find(match => match.Attribute.UUID == populationCapacityAttributeUUID);

            return attribute.Attribute != null;
        }

        private bool GetPopulationConsumptionAttribute(ProducableSO producable, out AttributeValue attributeValue)
        {
            attributeValue = producable.Attributes
                .Find(match => match.Attribute.UUID == populationConsumptionAttributeUUID);

            return attributeValue.Attribute != null;
        }

        private void ModifyPopulation(float value)
        {
            currentPopulation += value;
            if (maxPopulationEnabled)
            {
                currentPopulation = Mathf.Max(currentPopulation, 0);
            }

            InvokePopulationUpdatedAction();
        }

        private void ModifyMaxPopulation(float value)
        {
            maxPopulation += value;
            InvokePopulationUpdatedAction();
        }

        private void InvokePopulationUpdatedAction() =>
            OnPopulationUpdate?.Invoke(CurrentPopulation, MaxPopulation);

        /// <summary>
        /// Checks if production should be finished for a certain producable
        /// regarding the population consumption attribute.
        /// </summary>
        /// <param name="producableQuantity">Producable to produce</param>
        /// <returns>
        /// Returns 'false' if population requirements are not fulfilled (not
        /// enough population capacity for the producables quantity).
        /// </returns>
        public bool ShouldFinishProductionFor(ProducableQuantity producableQuantity)
        {
            if (maxPopulationEnabled &&
                GetPopulationConsumptionAttribute(
                    producableQuantity.Producable, out AttributeValue attributeValue))
            {
                float fullValue = attributeValue.Value * producableQuantity.Quantity;
                return fullValue + CurrentPopulation <= MaxPopulation;
            }

            // No population consumption attribute
            return true;
        }

        #endregion

    }

}
