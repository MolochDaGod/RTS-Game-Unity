using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Production interface used for updating current production with time/progress.
    /// You can use time passed (example: Time.deltaTime) for realtime production
    /// or a custom delta value that can represent a turn based progress
    /// (example: 1 for when player ends his turn).
    /// </summary>
    public interface IProduce
    {

        /// <summary>
        /// Called each time production needs to apply the delta.
        /// Implement this to handle production progress of components.
        /// </summary>
        /// <param name="delta">
        /// Value change that will be applied to production.
        /// If its realtime it will probably be <see cref="Time.deltaTime"/>,
        /// otherwise it can be turn based like 1, 2, etc...
        /// </param>
        public void Produce(float delta);

    }

}