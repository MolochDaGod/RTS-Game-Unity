using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace UnitSystem
{

    /// <summary>
    /// Component derives from <see cref="UnitSO"/> and it is intended to be
    /// used in combination with the <see cref="ProductionUnit"/>. Production
    /// units can produce other producables (units, resources and more).
    /// If more complex class is required, modify this one or create your own
    /// subclass of either <see cref="UnitSO"/> or <see cref="AUnitSO"/>.
    /// <see cref="ProductionUnit"/> can be used completely independently even
    /// if this SO is not used.
    /// </summary>
    [CreateAssetMenu(fileName = "New unit", menuName = "Unit System/Units/Production Unit")]
    public class ProductionUnitSO : UnitSO
    {

        /// <summary>
        /// Specifies group of production actions available on this unit.
        /// This can include any <see cref="ProducableSO"/> like resource,
        /// unit, research, etc.
        /// </summary>
        [Header("Production"), Tooltip("Action groups of the unit. This way " +
            "you can separate units by context or type.")]
        public List<ProductionActionGroup> GroupedActions;

        /// <summary>
        /// Specifies the resources this unit produces per game defined period.
        /// This can be per one second or per turn, or any other custom time
        /// behaviour.
        /// Multiple resources are supported.
        /// </summary>
        [Tooltip("List of resources this unit produces and their quantity per " +
            "second, per turn or per custom time based period.")]
        public List<ProducableQuantity> ProducesResource;

        /// <summary>
        /// Iterates through <see cref="GroupedActions"/>
        /// and creates a flat list of the actions of all groups.
        /// </summary>
        /// <returns>
        /// Returns a flat list of all production actions within the
        /// actions groups.
        /// </returns>
        public List<ProductionAction> GetAllProductionActions()
        {
            List<ProductionAction> allActions = new List<ProductionAction>();
            var actions = GroupedActions.ConvertAll(group => group.Actions);
            foreach (var groupActions in actions)
            {
                allActions.AddRange(groupActions);
            }
            return allActions;
        }

    }

}