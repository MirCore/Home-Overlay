using System;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public static class Helpers
    {
        public static string GetDisplayName(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DisplayNameAttribute attribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
            return attribute != null ? attribute.Name : value.ToString();
        }
    }
    
    internal sealed class DisplayNameAttribute : Attribute
    {
        public string Name { get; }

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}