
using System;
using System.IO;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public abstract class MaterialDesignIcons
{
    private static CodepointData[] _codepointsCollection;

    public static string GetIcon(string hassIconName, HassEntity entity)
    {
        if (_codepointsCollection == null)
            InitiateCodepointsCollection();

        string iconName;

        if (string.IsNullOrEmpty(hassIconName))
        {
            switch (entity.DeviceType)
            {
                case EDeviceType.DEFAULT:
                    iconName = "help";
                    break;
                case EDeviceType.LIGHT:
                    iconName = "lightbulb";
                    break;
                case EDeviceType.SWITCH:
                    iconName = "toggle-switch-variant-off";
                    break;
                case EDeviceType.BINARY_SENSOR or EDeviceType.SENSOR:
                    Enum.TryParse(entity.attributes.device_class, true, out ESensorDeviceClass sensorClass);
                    iconName = sensorClass switch
                    {
                        ESensorDeviceClass.ENERGY or ESensorDeviceClass.POWER => "lightning-bolt",
                        ESensorDeviceClass.TEMPERATURE => "thermometer",
                        _ => "radar"
                    };

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity), entity, null);
            }
        }
        else
        {
            iconName = hassIconName.Split(":")[1];
        }

        return (from data in _codepointsCollection where data.Name == iconName select data.Code).FirstOrDefault();
    }

    private static void InitiateCodepointsCollection()
    {
        TextAsset mdi = Resources.Load<TextAsset>("codepoints");
        string[] codepoints = mdi.text.Split('\n');
        _codepointsCollection = codepoints
            .Select(codepoint => new CodepointData(codepoint))
            .Where(data => data.Code != null) // Exclude invalid entries
            .ToArray();
    }
    
    [System.Serializable]
    public class CodepointData
    {
        public string Name { get; private set; }
        public string Hex { get; private set; }
        public string Code { get; private set; }

        public CodepointData(string codepoint)
        {
            string[] data = codepoint.Split(' ');
            Name = data[0];
            Hex = data[1];

            // Convert hex to a Unicode character using \U escape sequence
            int unicodeValue = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
            Code = char.ConvertFromUtf32(unicodeValue); // Supports supplementary planes
        }
    }
}
