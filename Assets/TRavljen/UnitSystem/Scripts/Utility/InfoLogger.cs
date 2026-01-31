using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem.Utility
{

    /// <summary>
    /// Class responsible for printing info logs in the console produced by the
    /// <see cref="UnitSystem"/> logic. It is using <see cref="Debug.Log(object)"/>
    /// when flag <see cref="Enabled"/> is true.
    /// </summary>
    public static class InfoLogger
    {

        /// <summary>
        /// Logging by default is enabled. To disable it set this to false.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// Print info text in the console.
        /// </summary>
        /// <param name="log">Text to print</param>
        public static void Log(string log)
        {
            if (Enabled)
            {
                Debug.Log(log);
            }
        }
    }
}