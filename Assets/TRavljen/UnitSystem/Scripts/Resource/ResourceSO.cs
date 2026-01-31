using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Specifies a default resource scriptable object.
    /// The <see cref="ResourceManager"/> is dependent on the <see cref="ProducableSO"/>
    /// so this is just a convenience for type casting in <see cref="Player"/> for
    /// specific behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "New resource", menuName = "Unit System/Resource")]
    public class ResourceSO : ProducableSO
    {

    }

}