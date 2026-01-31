using UnityEngine;
using UnityEngine.UI;
using UnitSystem;


namespace UnitSystem.Demo
{

    /// <summary>
    /// Control component for garrison present in the scene - takes care of a single
    /// garrison unit for DEMO. It will be updating button text based on garrison
    /// state and handle it's action (garrion/release).
    /// </summary>
    public class GarrisonUI : MonoBehaviour
    {

        /// <summary>
        /// Specifies the garrison unit used for control.
        /// </summary>
        public Unit Garrison;

        private IGarrisonUnit garrisonUnit;

        /// <summary>
        /// Specifies the button text that will change based on state of the
        /// garrisoned units. If present "RELEASE UNITS" is set, otherwise
        /// "GARRISON" is shown.
        /// </summary>
        public Text GarrisonButtonText;

        private void Start()
        {
            garrisonUnit = Garrison.GetComponent<IGarrisonUnit>();
        }

        private void Update()
        {
            GarrisonButtonText.text = garrisonUnit.GarrisonUnits.Count == 0 ? "GARRISON" : "RELEASE UNITS";
        }

        /// <summary>
        /// Simple demonstration logic for working with garrison.
        /// This does nothing if there are ground units active in the
        /// scene (<see cref="GroundUnit"/>).
        /// </summary>
        public void GarrisonButtonClicked()
        {
            if (garrisonUnit.GarrisonUnits.Count == 0)
            {
                var characters = FindObjectsOfType<GroundUnit>();
                var calledInCharactersCount = 0;
                foreach (var character in characters)
                {
                    character.SetTargetGarrison(garrisonUnit);
                    calledInCharactersCount += 1;

                    // Recall only as many as building can take.
                    if (calledInCharactersCount == garrisonUnit.MaxCapacity)
                    {
                        break;
                    }
                }
            }
            else
            {
                // Removes all units from garrison
                garrisonUnit.RemoveAllUnits();
            }
        }

    }

}