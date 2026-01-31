using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Garrison component is designed to be used for units that have
    /// garrison attribute which allows other units to enter this one.
    /// Max capacity is determined by the attribute attached to the
    /// <see cref="Unit.Data"/> and its UUID is matched by
    /// <see cref="Garrison.GarrisonCapacityAttributeUUID"/>. This
    /// allows you to create your own capacity attribute and still use
    /// this component.
    /// </summary>
    [RequireComponent(typeof(Unit), typeof(Collider))]
    public class Garrison : MonoBehaviour, IGarrisonUnit
    {

        #region Properties

        /// <summary>
        /// Get maximal capacity available for this garrison.
        /// </summary>
        public int MaxCapacity => GetGarrisonCapacity();

        /// <summary>
        /// Position of the garrison entrance.
        /// </summary>
        public Vector3 GarrisonEntrancePosition => entrance.transform.position;

        /// <summary>
        /// Specifies the list of units currently occupying space within
        /// this unit. Maximal number of units is defined by the garrison
        /// attribute on the unit.
        /// </summary>
        public List<Unit> GarrisonUnits { get; private set; } = new List<Unit>();

        /// <summary>
        /// Specifies the 'garrison' attribute reference. Set this in order
        /// to compare with attribute on units. If this is left on 'null',
        /// it will not work.
        /// </summary>
        [Tooltip("Specifies the UUID of the garrison capacity attribute.")]
        public string GarrisonCapacityAttributeUUID;

        /// <summary>
        /// Action invoked when unit enters the garrison.
        /// </summary>
        public System.Action<Unit> OnUnitEnter;

        /// <summary>
        /// Action invoked when unit exits the garrison.
        /// </summary>
        public System.Action<Unit> OnUnitExit;

        [SerializeField, Tooltip("Specifies the entrance of the garrison where " +
            "other units should approach in order to enter it.")]
        private GarrisonEntrance entrance;

        /// <summary>
        /// Specifies the unit spawn point for the units that will exit the garrison.
        /// </summary>
        [SerializeField]
        private AUnitSpawnPoint spawnPoint = null;

        /// <summary>
        /// Attached Unit component.
        /// </summary>
        private Unit unit;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        #endregion

        #region Manage Units

        public virtual bool AddUnit(Unit unit)
        {
            // Add unit to the list only if capacity has not been reached.
            if (GarrisonUnits.Count < MaxCapacity)
            {
                GarrisonUnits.Add(unit);
                OnUnitEnter?.Invoke(unit);
                return true;
            }

            return false;
        }

        public bool RemoveUnit(Unit unit)
        {
            // Nothing to remove if the list does not contain the unit.
            if (!GarrisonUnits.Contains(unit)) return false;

            GarrisonUnits.Remove(unit);

            if (unit.gameObject.TryGetComponent(out IGarrisonableUnit garrisonable))
            {
                garrisonable.ExitGarrison();
                OnUnitExit?.Invoke(unit);
            }

            spawnPoint.RespawnUnit(unit);

            return true;
        }

        public void RemoveAllUnits()
        {
            for (int index = GarrisonUnits.Count-1; index >= 0; index--)
            {
                RemoveUnit(GarrisonUnits[index]);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Retrieves the garrison capacity value from the
        /// attributed attached to the unit data <see cref="Unit.Data"/>.
        /// </summary>
        /// <returns>
        /// Returns 0 if the attribute is missing/not set. Otherwise
        /// it will return value of the attribute, converted from `float` to
        /// `int`.
        /// </returns>
        private int GetGarrisonCapacity()
        {
            if (GarrisonCapacityAttributeUUID == null) return 0;

            var attribute = unit.Data.Attributes
                .Find(match => match.Attribute.UUID == GarrisonCapacityAttributeUUID);

            if (attribute.Attribute == null) return 0;

            return (int)attribute.Value;
        }

        #endregion

    }

}