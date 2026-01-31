using UnityEngine;
using UnitSystem;

namespace UnitSystem.Demo
{
    /// <summary>
    /// UI Turn base Strategy component for controlling Input key for ending
    /// turn and clicking end turn.
    /// </summary>
    public class TurnBaseStrategyUI : MonoBehaviour
    {

        public TBSTimeController GameManager;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                EndTurnClicked();
            }
        }

        public void EndTurnClicked()
        {
            GameManager.EndTurnForCurrentPlayer();
        }

    }

}