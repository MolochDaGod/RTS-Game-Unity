using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Entrance for <see cref="Garrison"/> that handles adding
    /// units to the garrison once they are within range.
    /// <para>
    /// Setup the position and size of the entrance for units to be picked up
    /// when in range. Make sure the entrace collision detection is not inside
    /// garrison units' main collider.
    /// 
    /// Make sure to select a desired <see cref="collisionDetectionMode"/> in
    /// order to check collisions. Support is for collider trigger or manual
    /// detection of overlapping colliders.
    /// </para>
    /// </summary>
    public class GarrisonEntrance : MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Type of collision detection for units entering the garrison.
        /// Overlapping mode has simple box collision detection, to use more
        /// complex one a trigerring collider is more fitting.
        /// </summary>
        [SerializeField, Tooltip("Collision detection mode determines how units for entering garrison are detected.")]
        private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.OverlapBox;

        /// <summary>
        /// Box size used to calculate overlapping colliders when the collider
        /// detection is set to 'OverlapBox'.
        /// </summary>
        [SerializeField, Tooltip("Box size used to calculate overlapping colliders " +
            "when the collider detection is set to 'OverlapBox'.")]
        private Vector3 overlapBoxSize = new Vector3(1, 2, 1);

        [SerializeField, Tooltip("Specifies the garrison that owns this entrance. " +
            "This component will add units to garrison when they are within range.")]
        private Garrison garrison;

        #endregion

        #region Lifecycle

        public void Update()
        {
            if (collisionDetectionMode == CollisionDetectionMode.OverlapBox)
            {
                CheckCollisionsWithOverlapBox();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collisionDetectionMode != CollisionDetectionMode.ColliderTriggers)
            {
                EnterGarrisonIfNeeded(other);
            }
        }

        private void OnDrawGizmos()
        {
            if (collisionDetectionMode == CollisionDetectionMode.OverlapBox)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Vector3.zero, overlapBoxSize);
            }
        }

        #endregion

        #region Helpers

        private void CheckCollisionsWithOverlapBox()
        {
            // Detect all colliders within the garrison collider.
            var colliders = Physics.OverlapBox(
                transform.position,
                overlapBoxSize / 2f,
                transform.rotation);

            // Then check for each if it needs to enter garrison.
            foreach (var collider in colliders)
            {
                EnterGarrisonIfNeeded(collider);
            }
        }

        /// <summary>
        /// Checks whether the collider contains necessary components
        /// and correct <see cref="IGarrisonableUnit.TargetGarrison"/>
        /// for entering the garrison.
        /// </summary>
        /// <param name="collider">Collider within the range of entering.</param>
        private void EnterGarrisonIfNeeded(Collider collider)
        {
            if (collider.gameObject.TryGetComponent(out IGarrisonableUnit garrisonable) &&
                collider.gameObject.TryGetComponent(out Unit unit) &&
                Object.ReferenceEquals(garrisonable.TargetGarrison, garrison))
            {
                if (garrison.AddUnit(unit))
                {
                    garrisonable.EnterGarrison();
                }
            }
        }

        #endregion

        #region ColliderDetection

        enum CollisionDetectionMode
        {
            /// <summary>
            /// Requires component to have a trigger collider attached to it.
            /// <see cref="OnTriggerEnter(Collider)"/> is used to detect units
            /// near the entrance. As always, do not forget that collision will
            /// not be triggered if one of the game objects does not have
            /// <see cref="Rigidbody"/> or <see cref="Rigidbody2D"/>. To avoid
            /// this limitation use <see cref="OverlapBox"/> mode.
            /// </summary>
            ColliderTriggers,

            /// <summary>
            /// Uses built in <see cref="Physics.OverlapBox(Vector3, Vector3, Quaternion)"/>
            /// to detect units near the entrance. This means that only BOX
            /// is supported with the position, scale and rotation.
            /// To add extra or custom collision shapes, add new mode here.
            /// </summary>
            OverlapBox
        }

        #endregion
    }

}