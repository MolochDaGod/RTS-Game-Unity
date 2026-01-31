using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Base producable object, it is primarily used to define producables for
    /// the project.. Any gameObject/unit should use this class or derive from
    /// this class in order to utilise <see cref="UnitSystem"/> components.
    /// <para>
    /// All producables should originate from this scriptable object and should
    /// primarily differentiate between other producables with the UUID.
    /// You may either subclass this to define new types (like structures) or
    /// you may also add a new field like `Type` with an enum to differentiate
    /// between such units (like workers vs structures).
    /// </para>
    /// </summary>
    public abstract class ProducableSO : BasicSO
    {

        /// <summary>
        /// Specifies the name of the producable, examples: "Solider", "House".
        /// </summary>
        [Header("General")]
        public string Name;

        /// <summary>
        /// Specifies the description of the producable.
        /// </summary>
        public string Description;

        /// <summary>
        /// Specifies producable attributes.
        /// </summary>
        public List<AttributeValue> Attributes;

        /// <summary>
        /// Specifies producable objects requirements, other than cost.
        /// These should not be consumed before production starts.
        /// </summary>
        public List<ProducableQuantity> Requirements;

        /// <summary>
        /// Specifies the resource requirements to produce this. These will be
        /// consumed before the production starts.
        /// </summary>
        public List<ProducableQuantity> Cost;

        /// <summary>
        /// Specifies the time required to produce this (examples:
        /// training or construction). Values allowed should be positive
        /// numbers, max on inspector range is 99_999 and it can be
        /// increased here if needed.
        /// </summary>
        [Range(0, 99_999)]
        public float ProductionDuration;

    }

}