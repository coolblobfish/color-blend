using System.Drawing;

namespace ColorBlend
{
    public readonly struct RGBColor(byte r, byte g, byte b)
    {
        // Coefficients for luminance: R = 0.2126, G = 0.7152, B = 0.0722
        static readonly float[] lightnessWeights = [0.2126f, 0.7152f, 0.0722f];

        public readonly byte R { get; } = r;
        public readonly byte G { get; } = g;
        public readonly byte B { get; } = b;

        public static RGBColor FromColor(Color color)
        {
            return new RGBColor(color.R, color.G, color.B);
        }

        public int ToArgb()
        {
            return (255 << 24) | (R << 16) | (G << 8) | B;
        }

        public Color ToColor()
        {
            return Color.FromArgb(R, G, B);
        }

        public HSVColor ToHSV()
        {
            int[] rgbArray = [R, G, B];
            int maxVal = rgbArray[0];
            int minVal = rgbArray[0];
            int maxIndex = 0;
            int minIndex = 0;

            for (int i = 1; i <= 2; i++)
            {
                if (rgbArray[i] > maxVal)
                {
                    maxVal = rgbArray[i];
                    maxIndex = i;
                }
                else if (rgbArray[i] < minVal)
                {
                    minVal = rgbArray[i];
                    minIndex = i;
                }
            }

            int range = maxVal - minVal;
            if (range == 0)
                return new HSVColor(0, 0, (float)maxVal / 255);

            int medIndex = 3 - maxIndex - minIndex;
            float[] rgbNormalized = new float[3];
            rgbNormalized[maxIndex] = 1;
            rgbNormalized[minIndex] = 0;
            rgbNormalized[medIndex] = (float)(rgbArray[medIndex] - minVal) / range;

            float hue;
            if (maxIndex == 0)
            {
                if (rgbNormalized[1] > 0)
                    hue = 60 * rgbNormalized[1];
                else
                    hue = 360 - 60 * rgbNormalized[2];
            }
            else if (maxIndex == 1)
                hue = 120 + 60 * (rgbNormalized[2] - rgbNormalized[0]);
            else
                hue = 240 + 60 * (rgbNormalized[0] - rgbNormalized[1]);

            return new HSVColor(hue, (float)range / maxVal, (float)maxVal / 255);
        }

        public RGBColor Blend(RGBColor other, float t)
        {
            return new RGBColor(
                (byte)(R + ((other.R - R) * t + 0.5)),
                (byte)(G + ((other.G - G) * t + 0.5)),
                (byte)(B + ((other.B - B) * t + 0.5)));
        }

        public static float GetLuminance(float r, float g, float b)
        {
            return 0.2126f * r * r + 0.7152f * g * g + 0.0722f * b * b;
        }

        public static float GetLightness(float r, float g, float b)
        {
            float luminance = GetLuminance(r, g, b);
            if (luminance <= 216 / 24389f)
                return luminance * 24389 / 27f;
            return 116 * (float)Math.Cbrt(luminance) - 16;
        }

        public float GetLightness()
        {
            return GetLightness(R / 255f, G / 255f, B / 255f);
        }

        public RGBColor ChangeLightness(float targetLightness)
        {
            float[] rgbArray = [R / 255f, G / 255f, B / 255f];
            float maxVal = rgbArray[0];
            float minVal = rgbArray[0];
            int maxIndex = 0;
            int minIndex = 0;

            for (int i = 1; i <= 2; i++)
            {
                if (rgbArray[i] > maxVal)
                {
                    maxVal = rgbArray[i];
                    maxIndex = i;
                }
                else if (rgbArray[i] < minVal)
                {
                    minVal = rgbArray[i];
                    minIndex = i;
                }
            }

            if (maxVal == 0)
            {
                for (int i = 0; i < rgbArray.Length; i++)
                    rgbArray[i] = 1;
                maxVal = 1;
                minVal = 1;
            }

            float maxLightness = GetLightness(rgbArray[0] / maxVal, rgbArray[1] / maxVal, rgbArray[2] / maxVal);

            float luminance = targetLightness <= 8 ? targetLightness * (27 / 24389f)
                : (float)Math.Pow((targetLightness + 16) / 116, 3);

            if (targetLightness <= maxLightness)
            {
                float scale = 255 * (float)Math.Sqrt(luminance / GetLuminance(rgbArray[0], rgbArray[1], rgbArray[2]));
                return new RGBColor(
                    (byte)(rgbArray[0] * scale + 0.5),
                    (byte)(rgbArray[1] * scale + 0.5),
                    (byte)(rgbArray[2] * scale + 0.5));
            }

            int medIndex = 3 - maxIndex - minIndex;
            float t = (rgbArray[medIndex] - minVal) / (maxVal - minVal);
            float constantTerm = lightnessWeights[maxIndex] + lightnessWeights[medIndex] * t * t
                - luminance;
            float linearCoef = lightnessWeights[medIndex] * (1 - t) * (1 - t) + lightnessWeights[minIndex];
            float quadraticCoef = lightnessWeights[medIndex] * (1 - t) * t;

            float minColorAmount = ((float)Math.Sqrt(quadraticCoef * quadraticCoef - linearCoef * constantTerm)
                - quadraticCoef) / linearCoef;
            float medColorAmount = minColorAmount + t * (1 - minColorAmount);

            float[] finalColors = new float[3];
            finalColors[maxIndex] = 255;
            finalColors[medIndex] = 255 * medColorAmount;
            finalColors[minIndex] = 255 * minColorAmount;

            return new RGBColor(
                    (byte)(finalColors[0] + 0.5),
                    (byte)(finalColors[1] + 0.5),
                    (byte)(finalColors[2] + 0.5));
        }

        public RGBColor ChangeValue(RGBColor valueColor)
        {
            HSVColor thisColor = ToHSV();
            return new HSVColor(thisColor.H, thisColor.S, valueColor.ToHSV().V).ToRGB();
        }

        public override string ToString() => $"RGB=({R}, {G}, {B})";
    }
}
