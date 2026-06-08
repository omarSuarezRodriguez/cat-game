using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerHaven.Utils
{
    public static class Extensions
    {
        // --- Collections ---
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // --- Transform ---
        public static void DestroyAllChildren(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
        }

        // --- Color ---
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        // --- Double Math ---
        public static double Clamp(double value, double min, double max)
            => Math.Max(min, Math.Min(max, value));

        // --- String ---
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }

        // --- Canvases / UI ---
        public static void SetInteractable(this CanvasGroup cg, bool interactable, bool changeAlpha = true)
        {
            cg.interactable = interactable;
            cg.blocksRaycasts = interactable;
            if (changeAlpha) cg.alpha = interactable ? 1f : 0.5f;
        }
    }
}
