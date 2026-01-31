using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnitSystem
{

    /// <summary>
    /// Simple research managment component, responsible for keeping track of currently
    /// completed researches for a player. These are generally less complex than for
    /// resource management. Actual production of research should be processed as part of production.
    /// </summary>
    public class ResearchManager: MonoBehaviour
    {

        [SerializeField, Tooltip("List of completed researches. Add through " +
            "inspector for default values.")]
        protected List<ProducableSO> completedResearches = new List<ProducableSO>();

        /// <summary>
        /// Action invoked when research is added to the collection via
        /// <see cref="AddFinishedResearch(ProducableSO)"/>.
        /// </summary>
        public Action<ProducableSO> OnResearchFinished;

        /// <summary>
        /// Adds new research to the <see cref="completedResearches"/> collection
        /// and invokes the <see cref="OnResearchFinished"/> action.
        /// </summary>
        /// <param name="research">Research to be added.</param>
        public void AddFinishedResearch(ProducableSO research)
        {
            completedResearches.Add(research);
            OnResearchFinished?.Invoke(research);
        }

        /// <summary>
        /// Checks if the <see cref="completedResearches"/> collection contains
        /// a research with the same UUID.
        /// </summary>
        /// <param name="research">Research used for matching UUIDs.</param>
        /// <returns>Returns 'true' if collection contains research.</returns>
        public bool IsResearchComplete(ProducableSO research)
        {
            for (int index = 0; index < completedResearches.Count; index++)
            {
                if (completedResearches[index].UUID == research.UUID)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes research from <see cref="completedResearches"/> collection
        /// if it contains one. Matching is done with UUIDs.
        /// </summary>
        /// <param name="research">Research to be removed.</param>
        /// <returns>
        /// Returns 'true' when research is removed.
        /// Returns 'false' if collection does not contain the research
        /// and therefore could not be removed.
        /// Only produced/completed are present.
        /// </returns>
        public bool RemoveCompletedResearch(ProducableSO research)
        {
            bool isPresent = false;
            for (int index = completedResearches.Count; index >= 0; index--)
            {
                if (completedResearches[index].UUID != research.UUID)
                {
                    completedResearches.RemoveAt(index);
                    isPresent = true;
                }
            }

            return isPresent;
        }

    }

}
