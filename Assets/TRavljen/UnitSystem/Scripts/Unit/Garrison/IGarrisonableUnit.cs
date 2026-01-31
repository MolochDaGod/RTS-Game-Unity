using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Interface used for units that can be garrisoned into
    /// a garrison unit (<see cref="IGarrisonUnit"/>).
    /// </summary>
    public interface IGarrisonableUnit
    {

        /// <summary>
        /// Current target garrison for the unit. When unit
        /// is moving towards a garrison this field should
        /// hold a reference to that garrison.
        /// </summary>
        IGarrisonUnit TargetGarrison { get; }

        /// <summary>
        /// Method called when unit has entered a proximity of the
        /// target garrison and can enter it.
        /// </summary>
        void EnterGarrison();

        /// <summary>
        /// Method called when unit has to exit the garrison
        /// in which it is garrisoned.
        /// </summary>
        void ExitGarrison();

        /// <summary>
        /// Implement this to set the <see cref="TargetGarrison"/> field
        /// and handle movement towards it.
        /// </summary>
        void SetTargetGarrison(IGarrisonUnit garrison);

    }

}