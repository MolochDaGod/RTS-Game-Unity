using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Basic implementation of the <see cref="AUnitSpawnPoint"/> abstract
    /// component. This spawn point supports spawning new units from prefab,
    /// respawning existing units and moving them to target position using the
    /// <see cref="IControlUnit"/> interface.
    /// </summary>
    public class UnitSpawnPoint : AUnitSpawnPoint
    {

        #region Properties

        /// <summary>
        /// Specifies the location that units will move to
        /// once they are produced.
        /// </summary>
        [SerializeField, Tooltip("Set default spawn position (scene position).")]
        private Vector3 spawnedUnitTargetPosition = Vector3.zero;

        /// <summary>
        /// Specifies if the Gizmo for the <see cref="spawnedUnitTargetPosition"/>
        /// should be drawn.
        /// </summary>
        [SerializeField]
        private bool targetGizmoEnabled = true;

        /// <summary>
        /// Specifies the color of the gizmo on target position.
        /// </summary>
        [SerializeField]
        public Color gizmoColor = Color.red;

        #endregion

        #region Lifecycle

        private void OnDrawGizmosSelected()
        {
            if (targetGizmoEnabled && spawnedUnitTargetPosition != Vector3.zero)
            {
                Gizmos.color = gizmoColor;
                var size = new Vector3(.1f, 1.2f, .1f);
                Gizmos.DrawCube(spawnedUnitTargetPosition + size / 2, size);
            }
        }

        #endregion

        #region AUnitSpawnPoint

        public override Unit SpawnUnit(Unit prefab)
        {
            var newUnit = Instantiate(prefab, transform.position, Quaternion.identity);

            if (spawnedUnitTargetPosition != Vector3.zero)
            {
                MoveUnitToTargetPosition(newUnit);
            }

            return newUnit;
        }

        public override void RespawnUnit(Unit sceneUnit)
        {
            sceneUnit.gameObject.SetActive(true);
            MoveUnitToTargetPosition(sceneUnit);
        }

        public override void SetTargetPosition(Vector3 target)
        {
            spawnedUnitTargetPosition = target;
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Retrieves component that implements <see cref="IControlUnit"/>
        /// and sets the target position.
        /// </summary>
        /// <param name="unit">
        /// Unit that contains component with implementation of <see cref="IControlUnit"/>.
        /// </param>
        private void MoveUnitToTargetPosition(Unit unit)
        {
            if (unit.TryGetComponent(out IControlUnit controlUnit))
            {
                controlUnit.SetTargetPosition(spawnedUnitTargetPosition);
            }
            else
            {
                Debug.LogWarning("`UnitSpawnPoint` attempted to set target position " +
                    "for `" + gameObject + "`, but did not find any component " +
                    "implementing `IControlUnit`");
            }
        }

        #endregion

    }

}