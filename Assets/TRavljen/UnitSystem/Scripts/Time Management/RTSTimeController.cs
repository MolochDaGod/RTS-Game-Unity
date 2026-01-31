using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Component for updating production for real time strategy.
    /// Each player has a collection of units and if any of units components
    /// implement <see cref="IProduce"/ then production time will be applied
    /// to them each frame.
    /// </summary>
    public class RTSTimeController : MonoBehaviour
    {

        /// <summary>
        /// Specifies if the game is running. Set this to 'false' when game
        /// is paused and production will be paused as well.
        /// </summary>
        public bool IsGameRunning = true;

        public List<APlayer> Players = new List<APlayer>();

        protected virtual void FixedUpdate()
        {
            if (IsGameRunning)
            {
                foreach (APlayer player in Players)
                {
                    // Update Production units for each player
                    ProduceForTime(player, Time.fixedDeltaTime);
                }
            }
        }

        protected void ProduceForTime(APlayer player, float time)
        {
            foreach (Unit unit in player.GetUnits())
            {
                var components = unit.GetComponents<IProduce>();

                foreach (IProduce component in components)
                {
                    component.Produce(time);
                }
            }
        }

    }

}