using System;
using UnityEngine;

namespace Assets.Scripts.Assets.Utils
{
    public static class ColorUtils
    {
        public static Color ShiftSaturation(Color c, float shift)
        {
            // Clamp the shift to ensure it stays within the expected bounds
            shift = Math.Clamp(shift, -1.0f, 1.0f);

            // Calculate standard Rec. 709 luminance for accurate grayscale perception
            float luminance = (c.r * 0.2126f) + (c.g * 0.7152f) + (c.b * 0.0722f);

            // If shift > 0, we blend toward grayscale. 
            // If shift < 0, we push away from grayscale (increasing contrast/saturation).
            float newR = c.r + (shift * (luminance - c.r));
            float newG = c.g + (shift * (luminance - c.g));
            float newB = c.b + (shift * (luminance - c.b));

            // Clamp the output to prevent color clipping artifacts, especially when increasing contrast
            return new Color(
                Math.Clamp(newR, 0.0f, 1.0f),
                Math.Clamp(newG, 0.0f, 1.0f),
                Math.Clamp(newB, 0.0f, 1.0f),
                1
            );
        }
    }
}
