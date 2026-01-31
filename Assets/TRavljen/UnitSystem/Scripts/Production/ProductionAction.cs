using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Representation of production actions group. A group will define a list
    /// of actions under a certain name.
    /// Example: Research, Units, Default or for workers a groups like Housing,
    /// Food Production structures, etc.
    /// </summary>
    [System.Serializable]
    public struct ProductionActionGroup
    {
        /// <summary>
        /// Specifies the name of the production action group.
        /// </summary>
        public string Name;

        /// <summary>
        /// Specifies the list of actions in the group.
        /// </summary>
        public List<ProductionAction> Actions;

    }

    /// <summary>
    /// Represents a production action result and its quantity.
    /// Quantity specifies number of producables to be produced and consequently
    /// how much resources is required for the production.
    /// </summary>
    [System.Serializable]
    public struct ProductionAction
    {
        /// <summary>
        /// Producable item that will be the result of production.
        /// </summary>
        public ProducableSO Producable;

        /// <summary>
        /// Quantity of the 'Producable' this action produces.
        /// </summary>
        [Range(0.1f, 99_999f)] public float Quantity;
    }

}
