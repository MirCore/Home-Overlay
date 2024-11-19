
using System;
using System.IO;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public abstract class MaterialDesignIcons
{
    private static CodepointData[] _codepointsCollection;

    public static string GetIcon(string hassIconName, EDeviceType entityStateDeviceType)
    {
        if (_codepointsCollection == null)
            InitiateCodepointsCollection();

        string iconName;

        if (string.IsNullOrEmpty(hassIconName))
        {
            iconName = entityStateDeviceType switch
            {
                EDeviceType.DEFAULT => "help",
                EDeviceType.LIGHT => "lightbulb",
                EDeviceType.SWITCH => "toggle-switch-variant-off",
                _ => throw new ArgumentOutOfRangeException(nameof(entityStateDeviceType), entityStateDeviceType, null)
            };
        }
        else
        {
            iconName = hassIconName.Split(":")[1];
        }

        return (from data in _codepointsCollection where data.Name == iconName select data.Code).FirstOrDefault();
    }

    private static void InitiateCodepointsCollection()
    {
        string[] codepoints = File.ReadAllLines(Path.Combine(Application.dataPath, "mdi/codepoints"));
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
