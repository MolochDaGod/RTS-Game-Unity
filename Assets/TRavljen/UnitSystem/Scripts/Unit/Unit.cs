using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Core Unit component used for attaching unit
    /// data and player owner to the unit. This way
    /// unit can access it's own data and it's owner
    /// for various events.
    /// </summary>
    [DisallowMultipleComponent]
    public class Unit: MonoBehaviour
    {

        /// <summary>
        /// Specifies the player owner of the unit.
        /// </summary>
        public APlayer Owner;

        /// <summary>
        /// Specifies the data information about the Unit.
        /// </summary>
        public AUnitSO Data;

    }

}