using System.Collections;
using System.Collections.Generic;
using UnitSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UnitSystem.Demo
{

    /// <summary>
    /// Simple class for rendering text information about current resource state.
    /// </summary>
    public class ResourceUI : MonoBehaviour
    {

        #region Private Properties

        /// <summary>
        /// Specifies the list of texts used for displaying resources, one by one.
        /// Number of available resources for the player and texts should match!
        /// </summary>
        [SerializeField]
        private List<Text> resourceTextList;

        /// <summary>
        /// Specifies the player component used for retrieving <see cref="ResourceManager"/>.
        /// </summary>
        [SerializeField]
        private Player player;

        [SerializeField]
        private PopulationManager populationManager;

        #endregion

        #region Lifecycle

        void Start()
        {
            UpdateResourcesUI();
        }

        private void OnEnable()
        {
            player.ResourceManager.OnResourceUpdate += OnResourceUpdated;
        }

        private void OnDisable()
        {
            player.ResourceManager.OnResourceUpdate -= OnResourceUpdated;
        }

        private void Update()
        {
            // For demo purposes I avoided to use callback, because it would not
            // update UI if tester of the demo scene would toggle MaxPopulationEnabled
            // on the population controller via editor. Otherwise to use action
            // for updates is preferred approach, same as for resources.
            UpdatePopulationUI(
                populationManager.CurrentPopulation,
                populationManager.MaxPopulation);
        }

        #endregion

        #region Convenience

        private void UpdateResourcesUI()
        {
            var resources = player.ResourceManager.GetResources();

            for (int index = 0; index < resources.Length; index++)
            {
                var resourceQuantity = resources[index];
                string name = resourceQuantity.Producable.Name;
                int maxQuantity = (int)player
                    .ResourceManager
                    .GetMaxResourceQuantity(resourceQuantity.Producable.UUID);
                var quantity = (int)resourceQuantity.Quantity;

                resourceTextList[index].text = name + ": " + quantity;
            }
        }

        private void UpdatePopulationUI(int current, int max)
        {
            string text = name + ": ";
            if (populationManager.MaxPopulationEnabled)
            {
                resourceTextList[3].text = "Population: " + current + "/" + max;
            }
            else
            {
                // No maximal population
                resourceTextList[3].text = "Population: " + current;
            }
        }

        private void OnResourceUpdated(ProducableSO resource)
        {
            UpdateResourcesUI();
        }

        #endregion

    }

}