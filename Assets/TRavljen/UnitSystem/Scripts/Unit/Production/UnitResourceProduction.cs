using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Simple component for producing resources. Resources to produce are
    /// retrieved from units DATA on <see cref="ProductionUnitSO"/> and the
    /// production of those resources is handled with implementation of the
    /// <see cref="IProduce"/> interface.
    /// <para>
    /// It can support realtime or turn based production, depending on time
    /// manager behaviour.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Unit))]
    public class UnitResourceProduction : MonoBehaviour, IProduce
    {

        /// <summary>
        /// Unit component attached to the game object.
        /// </summary>
        protected Unit unit;

        /// <summary>
        /// List of resources and its quantity for production.
        /// </summary>
        private List<ProducableQuantity> productionResourceQuantities;

        public void Start()
        {
            unit = GetComponent<Unit>();
            productionResourceQuantities = GetProducingResources();
        }

        /// <summary>
        /// Retrieves production resources from <see cref="Unit.Data"/>, it
        /// expects a type of <see cref="ProductionUnitSO"/>.
        /// Override this method to return custom production resources. 
        /// </summary>
        /// <returns>Returns resources to produce by the unit.</returns>
        public virtual List<ProducableQuantity> GetProducingResources()
        {
            return unit.Data switch
            {
                ProductionUnitSO Data => Data.ProducesResource,
                _ => new List<ProducableQuantity>(),
            };
        }

        public void Produce(float delta)
        {
            foreach (ProducableQuantity resourceQuantity in productionResourceQuantities)
            {
                float quantity = resourceQuantity.Quantity * delta;
                unit.Owner.AddProducable(resourceQuantity.Producable, quantity);
            }
        }

    }

}