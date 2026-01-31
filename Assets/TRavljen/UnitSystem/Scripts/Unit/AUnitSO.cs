using System;
using System.Collections;

namespace UnitSystem
{

    /// <summary>
    /// Core scriptable object for units (producables that are spawned in Scene).
    /// Any producable that creates a game object should derive from this class.
    /// Unit components are primarily tied to this abstract class.
    /// To learn how to use this in custom iplementation check out <see cref="UnitSO"/>.
    /// </summary>
    public abstract class AUnitSO : ProducableSO
    {

        /// <summary>
        /// Implement this method to provide prefab for the unit. Interface
        /// is structured for async loading, but you can load prefab right away
        /// and invoke the argument unitLoaded.
        /// </summary>
        /// <param name="unitLoaded">
        /// Action for invoking once prefab is loaded.
        /// If this is never called, unit will never be spawned.
        /// </param>
        public abstract IEnumerator GetAssociatedPrefab(Action<Unit> unitLoaded);

    }

}