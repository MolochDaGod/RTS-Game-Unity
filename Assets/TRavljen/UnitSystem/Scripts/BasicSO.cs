using UnityEngine;
using System;

namespace UnitSystem
{
    using Utility;

    /// <summary>
    /// Basic ScriptableObject that contains UUID for it. Identifiers are good
    /// practice for matching items, to avoid comparing multiple properties,
    /// structures or even references.
    /// </summary>
    public class BasicSO : ScriptableObject
    {

        /// <summary>
        /// Unique identifier generated when instance is created.
        /// It is a private field and not editable in Unity Editor,
        /// it may only be copied from Editor to retrieve the UUID.
        /// <para>
        /// <c>Hint:</c> If replacing this UUID is really needed for the SO,
        /// then enable debug view in Inspector and edit option will appear.
        /// </para>
        /// </summary>
        [SerializeField, DisableInInspector, Tooltip("Unique ID for the " +
            "ScriptableObject. Do not change this manually if you have used " +
            "this UUID somewhere, unless you update it manually everywhere. " +
            "Manual override can be done in Debug mode.")]
        private string _uuid;

        /// <summary>
        /// Public getter for UUID.
        /// </summary>
        public string UUID => _uuid;

        protected BasicSO()
        {
            _uuid = Guid.NewGuid().ToString();
        }

    }
}
