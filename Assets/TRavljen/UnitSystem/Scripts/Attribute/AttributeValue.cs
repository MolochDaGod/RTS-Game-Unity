using System;

namespace UnitSystem
{

    /// <summary>
    /// Representation of attribute, value and its value type (<see cref="ValueType"/>).
    /// <para>
    /// This allows relative values (example: increasing population capacity for 2),
    /// percentage value (example: where damage versus buildings is increased for 20%),
    /// and absolute value which is expected to override/replace the current value
    /// (example: new health for a unit is 200 regardless of the previous value).
    /// </para>
    /// </summary>
    [Serializable]
    public struct AttributeValue
    {

        /// <summary>
        /// Specifies the attribute for the value.
        /// </summary>
        public AttributeSO Attribute;

        /// <summary>
        /// Specifies the value of the attribute.
        /// </summary>
        public float Value;

        /// <summary>
        /// Specifies the type of value and how it is applied.
        /// </summary>
        public ValueType ValueType;

    }

    public enum ValueType
    {

        /// <summary>
        /// When value should manipulate current unit attribute value.
        /// </summary>
        Relative,

        /// <summary>
        /// When value should replace current unit attribute value.
        /// </summary>
        Absolute,

        /// <summary>
        /// When value should manipulate current unit attribute with
        /// percentage value.
        /// </summary>
        Percentage
    }

}
