using UnityEngine;

namespace UnitSystem
{

    /// <summary>
    /// Defines base attribute for producables. To add additional fields
    /// or to add methods/functionality to attributes, derive from this class.
    /// Changing original files will result in override (lost changes) if
    /// package is ever updated.
    /// </summary>
    [CreateAssetMenu(fileName = "New attribute", menuName = "Unit System/Attribute")]
    public class AttributeSO : BasicSO
    {

        /// <summary>
        /// Specifies the attribute name, such as "Health", "DamageToArchers", etc.
        /// </summary>
        public string Name;

        /// <summary>
        /// Specifies the description of the attribute. Generally this is also
        /// the description that player may read.
        /// </summary>
        public string Description;

    }

}
