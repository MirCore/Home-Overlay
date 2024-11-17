using System;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public static class Helpers
    {
        /// <summary>
        /// Retrieves the display name of an enum value using the DisplayNameAttribute.
        /// If the attribute is not present, returns the enum value as a string.
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DisplayNameAttribute attribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
            return attribute != null ? attribute.Name : value.ToString();
        }
    }
    
    /// <summary>
    /// An attribute used to specify a display name for an enum value.
    /// </summary>
    internal sealed class DisplayNameAttribute : Attribute
    {
        public string Name { get; }

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}