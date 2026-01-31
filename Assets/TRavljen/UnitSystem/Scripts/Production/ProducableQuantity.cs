using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Representation of producable quantities with <see cref="float"/>.
    /// </summary>
    [Serializable]
    public struct ProducableQuantity
    {
        /// <summary>
        /// Specifies the producable.
        /// </summary>
        public ProducableSO Producable;

        /// <summary>
        /// Specifies quantity of the producable.
        /// </summary>
        public float Quantity;

        public ProducableQuantity(ProducableSO producable, float quantity)
        {
            Producable = producable;
            Quantity = quantity;
        }
    }

}