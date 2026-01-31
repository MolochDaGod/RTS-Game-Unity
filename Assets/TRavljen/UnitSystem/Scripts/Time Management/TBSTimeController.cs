using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// Component for updating production for turn based strategy.
    /// Each player has a collection of units and if any of units components
    /// implement <see cref="IProduce"/ then production time will be applied
    /// to them on end of turn.
    /// </summary>
    public class TBSTimeController : MonoBehaviour
    {

        /// <summary>
        /// Index of the current player.
        /// </summary>
        private int currentPlayerIndex = 0;

        public List<APlayer> Players = new List<APlayer>();

        /// <summary>
        /// Current player on turn. If there are no players in the
        /// collection (<see cref="Players"/>), this getter will crash.
        /// </summary>
        public APlayer CurrentPlayer => Players[currentPlayerIndex];

        /// <summary>
        /// Delta applied for a single production cycle (one turn).
        /// </summary>
        public float singleTurnTimeIncrement = 1f;

        /// <summary>
        /// Action invoked when all players have ended their turn.
        /// </summary>
        public Action AllTurnsEnded;

        /// <summary>
        /// Call this method when current player ends its turn.
        /// </summary>
        public void EndTurnForCurrentPlayer()
        {
            currentPlayerIndex += 1;

            // Check if last player ended his turn.
            // If it did, handle production.
            if (currentPlayerIndex == Players.Count)
            {
                currentPlayerIndex = 0;
                AllTurnsEnded?.Invoke();

                foreach (APlayer player in Players)
                {
                    // Update Production units for each player
                    ProduceForTurns(player, singleTurnTimeIncrement);
                }
            }
        }

        protected virtual void ProduceForTurns(APlayer player, float turns)
        {
            foreach (Unit unit in player.GetUnits())
            {
                var components = unit.GetComponents<IProduce>();

                foreach (IProduce component in components)
                {
                    component.Produce(turns);
                }
            }
        }

    }

}
