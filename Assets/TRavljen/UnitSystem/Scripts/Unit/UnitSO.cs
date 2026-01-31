using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Scriptable object deriving from <see cref="AUnitSO"/> and contains
    /// implementation for the basic prefab approach. Prefab is required for
    /// spawning units (game objects) within world scene.
    /// If more customisation is needed on the scriptable object, modify this one
    /// or create your own subclass of <see cref="AUnitSO"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New unit", menuName = "Unit System/Units/Unit")]
    public class UnitSO : AUnitSO
    {

        /// <summary>
        /// Specifies the game object prefab reference used for instantiating
        /// this unit. This can be quickly change for something like addressable key.
        /// </summary>
        [Header("Unit"), SerializeField, Tooltip("Specifies the game object " +
            "prefab used for spawning the unit within the scene.")]
        private Unit associatedPrefab;

        public override IEnumerator GetAssociatedPrefab(Action<Unit> unitLoaded)
        {
            unitLoaded?.Invoke(associatedPrefab);
            yield return null;
        }

    }

}
