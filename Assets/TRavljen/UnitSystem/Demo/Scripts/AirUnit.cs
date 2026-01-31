using System.Collections;
using System.Collections.Generic;
using UnitSystem;
using UnityEngine;
using UnityEngine.AI;

namespace UnitSystem.Demo
{

    /// <summary>
    /// Basic implementation example for air unit that can be moved on the
    /// walkable ground but not garrisoned.
    /// Movement is achieved with <see cref="IControlUnit"/> interface implementation.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AirUnit : MonoBehaviour, IControlUnit
    {

        #region Properties

        private NavMeshAgent agent;

        #endregion

        #region IControlUnit

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public virtual void SetTargetPosition(Vector3 groundPosition)
        {
            agent.destination = groundPosition;
        }

        #endregion
    }

}