using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Interface used for communicating commands to units that a new target
    /// position has been assigned to them.
    /// </summary>
    public interface IControlUnit
    {

        /// <summary>
        /// Implement this to set new target position for the unit.
        /// Each unit might respond differently when it receives
        /// this message.
        /// </summary>
        /// <param name="position">New target position.</param>
        void SetTargetPosition(Vector3 position);

    }

}