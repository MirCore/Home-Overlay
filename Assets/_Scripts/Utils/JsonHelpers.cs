using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using UnityEngine;

namespace Utils
{
    public static class JsonHelpers
    {
        /// <summary>
        /// Converts an RGB array (0-255) to a Unity Color (0.0-1.0).
        /// </summary>
        /// <param name="rgb">Array containing R, G, and B values (0-255).</param>
        /// <returns>Unity Color object.</returns>
        public static Color RGBToUnityColor(int[] rgb)
        {
            if (rgb is not { Length: 3 })
            {
                throw new ArgumentException("RGB array must have exactly 3 elements.");
            }

            float r = rgb[0] / 255f;
            float g = rgb[1] / 255f;
            float b = rgb[2] / 255f;

            return new Color(r, g, b);
        }
        public static string DictToJsonString(Dictionary<string, string> dict)
        {
            return JsonHelper.ArrayToJsonString(dict.Select(pair => new DictStruct { key = pair.Key, value = pair.Value }).ToArray());
        }
        
        [Serializable]
        public struct DictStruct
        {
            public string key;
            public string value;
        }
    }
}