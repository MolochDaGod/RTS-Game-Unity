using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Definition of main players component that is referenced by units in
    /// order to communicate events from unit components such as
    /// <see cref="ProductionUnit"/>, where player is used to add newly
    /// produced units to the player.
    /// </summary>
    public abstract class APlayer : MonoBehaviour
    {

        #region Properties

        /// <summary>
        /// Collection of all player units. Protected field to avoid unwanted
        /// modification of the list which can lead to unexpected behaviour.
        /// </summary>
        [SerializeField]
        protected List<Unit> units = new List<Unit>();

        /// <summary>
        /// Public getter for all the players units. This converts list to an
        /// array so do not call it too often. 
        /// </summary>
        public Unit[] GetUnits() => units.ToArray();

        /// <summary>
        /// Action invoked once a unit has been added to the <see cref="Units"/> collection.
        /// </summary>
        public Action<Unit> OnUnitAdded;

        /// <summary>
        /// Action invoked once a unit has been removed from the <see cref="Units"/> collection.
        /// </summary>
        public Action<Unit> OnUnitRemoved;

        /// <summary>
        /// Action invoked once a producable has been added to the player.
        /// </summary>
        public Action<ProducableSO, float> OnProducableAdded;

        /// <summary>
        /// Action invoked once a producable has been removed from the player.
        /// </summary>
        public Action<ProducableSO> OnProducableRemoved;

        #endregion

        /// <summary>
        /// Adds unit to the player by assigning ownership and storing
        /// the reference into <see cref="Units"/> collection. This
        /// allows quick access and matching for players units.
        /// </summary>
        /// <param name="unit">
        /// New unit that will to be added to the player.
        /// </param>
        public virtual void AddUnit(Unit unit)
        {
            unit.Owner = this;
            units.Add(unit);

            OnUnitAdded?.Invoke(unit);
        }

        /// <summary>
        /// Removes unit from the player by removing ownership and
        /// removing the unit from <see cref="Units"/> collection.
        /// 'ArgumentException' will be thrown if player does not
        /// contain this unit.
        /// </summary>
        /// <param name="unit">
        /// Unit to be removed, which will no longer be under ownership of
        /// it's current player.
        /// </param>
        public virtual void RemoveUnit(Unit unit)
        {
            if (!ContainsUnit(unit))
            {
                throw new ArgumentException("Attempting to remove unit that does not belong to this Player");
            }

            unit.Owner = null;
            units.Remove(unit);

            OnUnitRemoved?.Invoke(unit);
        }

        /// <summary>
        /// Add a producable of quantity 1 to the player.
        /// </summary>
        /// <param name="producable">Producable to be added.</param>
        public virtual void AddProducable(ProducableSO producable)
            => AddProducable(producable, 1);

        /// <summary>
        /// Adds a producable with specified quantity to the player.
        /// Primarily this method only triggers action for other components
        /// to respond to, with the use of <see cref="OnProducableAdded"/>.
        /// Player production units use this interface to produce new units
        /// or resources for the unit owner (player).
        /// </summary>
        /// <param name="producable">Producable to be added.</param>
        /// <param name="quantity">Quantity of producables to be added.</param>
        public virtual void AddProducable(ProducableSO producable, float quantity)
        {
            OnProducableAdded?.Invoke(producable, quantity);
        }

        /// <summary>
        /// Remove a producable from the player. Primarily this method only
        /// triggers action for other components to respod to, with the use of
        /// <see cref="OnProducableRemoved"/>.
        /// </summary>
        /// <param name="producable">Producable to be removed.</param>
        public virtual void RemoveProducable(ProducableSO producable)
        {
            OnProducableRemoved?.Invoke(producable);
        }

        /// <summary>
        /// Invokes method <see cref="ContainsProducable(ProducableQuantity)"/>
        /// with quantity of 1.
        /// </summary>
        /// <param name="producable">Producable to be used for check.</param>
        /// <returns>
        /// Returns true if contains a producable of at least 1 quantity.
        /// </returns>
        public virtual bool ContainsProducable(ProducableSO producable)
            => ContainsProducable(new ProducableQuantity(producable, 1));

        /// <summary>
        /// Implement this method to check whether a player contains the specified
        /// producable with quantity equal or larger.
        /// </summary>
        /// <param name="producableQuantity">
        /// Producable quantity to be used for checking
        /// </param>
        /// <returns></returns>
        public abstract bool ContainsProducable(ProducableQuantity producableQuantity);

        /// <summary>
        /// Checks whether the unit exists within the <see cref="Units"/> collection.
        /// </summary>
        /// <returns></returns>
        public bool ContainsUnit(Unit unit)
        {
            return units.Find(match => match == unit) != null;
        }

        /// <summary>
        /// Checks if the requirements for a producable are fullfilled, by
        /// checking if the player contains all the requirements defined
        /// on producable with the help of existing method <see cref="ContainsProducable(ProducableQuantity)"/>.
        /// To customise this override the method.
        /// </summary>
        /// <param name="producable">Producable with requirements</param>
        /// <param name="quantity">
        /// Quantity of the producable, this will be multipled requirements quantity.
        /// </param>
        /// <returns>If there are no requirements or they are fullfilled TRUE
        /// is returned, otherwise FALSE.</returns>
        public virtual bool FulfillsRequirements(ProducableSO producable, float quantity)
        {
            // Check if any requirements are set
            if (producable.Requirements.Count == 0) return true;

            var requirements = producable.Requirements;

            // Goes through requirements and checks if any are missing.
            // If any does miss, the loop will return early 'false'.
            foreach (ProducableQuantity requirement in requirements)
            {
                var fullRequirement = new ProducableQuantity(
                    requirement.Producable,
                    requirement.Quantity * quantity);
                if (!ContainsProducable(fullRequirement))
                {
                    return false;
                }
            }

            return true;
        }

    }
}