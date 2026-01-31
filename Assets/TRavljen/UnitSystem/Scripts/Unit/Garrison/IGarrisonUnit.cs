using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Interface used for units that have capacity
    /// to garrison other units. 
    /// </summary>
    public interface IGarrisonUnit
    {

        /// <summary>
        /// Entrance position of the garrison. Point where
        /// units should move to in order to enter the garrison.
        /// </summary>
        Vector3 GarrisonEntrancePosition { get; }

        /// <summary>
        /// Max capacity that this unit can garrison.
        /// </summary>
        int MaxCapacity { get; }

        /// <summary>
        /// Units currently garrisoned within the unit.
        /// </summary>
        List<Unit> GarrisonUnits { get; }

        /// <summary>
        /// Implement this to remove all units from garrison.
        /// </summary>
        void RemoveAllUnits();

        /// <summary>
        /// Implement this to add a unit to garrison.
        /// </summary>
        /// <param name="unit">Unit to add to garrison.</param>
        /// <returns>Returns `true` if unit was added.</returns>
        bool AddUnit(Unit unit);

        /// <summary>
        /// Implement this to remove a unit from garrison.
        /// </summary>
        /// <param name="unit">Unit to remove from garrison.</param>
        /// <returns>Returns `true` if unit was removed.</returns>
        bool RemoveUnit(Unit unit);

    }

}
