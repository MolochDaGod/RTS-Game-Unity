using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Definition for spawning point on the unit, where other units
    /// can be spawned or respawned. Generally spawn points have
    /// option to set target location for the spawned units.
    /// </summary>
    public abstract class AUnitSpawnPoint : MonoBehaviour
    {

        /// <summary>
        /// Implement this to spawn a unit from prefab.
        /// </summary>
        /// <param name="prefab">Prefab used for spawning the new unit.</param>
        /// <returns>Returns newly instantiated unit.</returns>
        public abstract Unit SpawnUnit(Unit prefab);

        /// <summary>
        /// Implement this to respawn a unit.
        /// </summary>
        /// <param name="sceneUnit">Instance of the unit that needs to be respawned.</param>
        public abstract void RespawnUnit(Unit sceneUnit);

        /// <summary>
        /// Implement this to set the target position of the spawn.
        /// Commonly units spawn on spawn point, are then moved to the target
        /// spawn position.
        /// </summary>
        /// <param name="target">New target position for the unit.</param>
        public abstract void SetTargetPosition(Vector3 target);

    }

}