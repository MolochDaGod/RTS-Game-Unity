using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Specifies a default research scriptable object.
    /// The <see cref="ResearchManager"/> is dependent on the <see cref="ProducableSO"/>
    /// so this is just a convenience for type casting in <see cref="Player"/> for
    /// specific behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "New research", menuName = "Unit System/Research")]
    public class ResearchSO : ProducableSO
    {

    }

}