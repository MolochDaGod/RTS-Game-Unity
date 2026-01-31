using UnityEngine;
using UnityEngine.AI;
using UnitSystem;


namespace UnitSystem.Demo
{

    /// <summary>
    /// Basic implementation example for grounded unit that can be moved on the
    /// ground and garrisoned within a garrison unit.
    /// Movement is achieved with <see cref="IControlUnit"/> interface implementation
    /// and garrisoning unit is achieved with <see cref="IGarrisonableUnit"/>.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class GroundUnit : MonoBehaviour, IGarrisonableUnit, IControlUnit
    {

        #region Properties

        private NavMeshAgent agent;

        /// <summary>
        /// Current garrison target that the unit should be moving towards.
        /// </summary>
        public IGarrisonUnit TargetGarrison { get; private set; }

        #endregion

        #region IControlUnit

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public virtual void SetTargetPosition(Vector3 groundPosition)
        {
            // Set new destination and reset garrison as movement
            // was instructed on for ground terrain.
            agent.destination = groundPosition;
            TargetGarrison = null;
        }

        #endregion

        #region IGarrisonable

        public void EnterGarrison()
        {
            // Simply disable game object without animation, we can use
            // scale down animation here or something more complex and THEN
            // disable the object.
            gameObject.SetActive(false);

            TargetGarrison = null;
        }

        public void ExitGarrison()
        {
            // Simply activate game object again
            gameObject.SetActive(true);
        }

        public void SetTargetGarrison(IGarrisonUnit garrison)
        {
            // Set new target garrison and it's entrance as target position.
            TargetGarrison = garrison;
            agent.destination = garrison.GarrisonEntrancePosition;
        }

        #endregion
    }

}