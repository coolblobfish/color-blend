using System.Drawing;

namespace ColorBlend
{
    public class HybridColor
    {
        public RGBColor RGB { get; }
        public HSVColor HSV { get; }

        public HybridColor(RGBColor rgb, HSVColor hsv)
        {
            RGB = rgb;
            HSV = hsv;
        }

        public HybridColor(RGBColor rgb)
        {
            RGB = rgb;
            HSV = rgb.ToHSV();
        }

        public HybridColor(HSVColor hsv)
        {
            RGB = hsv.ToRGB();
            HSV = hsv;
        }

        public HybridColor(byte r, byte g, byte b)
        {
            RGB = new RGBColor(r, g, b);
            HSV = RGB.ToHSV();
        }

        public HybridColor(float h, float s, float v)
        {
            HSV = new HSVColor(h, s, v);
            RGB = HSV.ToRGB();
        }

        public HybridColor(byte r, byte g, byte b, float h, float s, float v)
        {
            RGB = new RGBColor(r, g, b);
            HSV = new HSVColor(h, s, v);
        }

        public static HybridColor FromColor(Color color)
        {
            RGBColor rgb = RGBColor.FromColor(color);
            return new HybridColor(rgb, rgb.ToHSV());
        }

        public Color ToColor()
        {
            return RGB.ToColor();
        }

        public RGBColor Blend(HybridColor other, float t)
        {
            float rgbWeight = Math.Abs(HSV.H - other.HSV.H) / 180;
            if (rgbWeight > 1)
                rgbWeight = 2 - rgbWeight;
            rgbWeight = 0.5f * (1 - (float)Math.Cos(Math.PI * rgbWeight));
            RGBColor rgbBlend = RGB.Blend(other.RGB, t);
            RGBColor hsvBlend = HSV.Blend(other.HSV, t).ToRGB();
            return hsvBlend.Blend(rgbBlend, rgbWeight);
        }

        public static RGBColor BlendMulti(HybridColor[] colors, float[,] positions, float[] target, float[]? colorWeights)
        {
            float[] weights = new float[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                float distanceSquared = 0;
                for (int coord = 0; coord < target.Length; coord++)
                {
                    float coordinateDiff = target[coord] - positions[i, coord];
                    distanceSquared += coordinateDiff * coordinateDiff;
                }

                if (distanceSquared == 0)
                {
                    List<int> indices = [i];
                    for (int j = i + 1; j < colors.Length; j++)
                    {
                        bool samePosition = true;
                        for (int coord = 0; coord < target.Length; coord++)
                        {
                            if (target[coord] == positions[j, coord])
                            {
                                samePosition = false;
                                break;
                            }
                        }
                        if (samePosition)
                            indices.Add(j);
                    }
                    HybridColor[] newColors = new HybridColor[indices.Count];
                    weights = new float[indices.Count];
                    for (int j = 0; j < indices.Count; j++)
                    {
                        newColors[j] = colors[indices[j]];
                        weights[j] = 1;
                    }
                    colors = newColors;
                    break;
                }

                weights[i] = (colorWeights == null ? 1 : colorWeights[i]) / (float)Math.Sqrt(distanceSquared);
            }

            if (colors.Length == 1)
                return colors[0].RGB;

            RGBColor[] blends = new RGBColor[colors.Length * (colors.Length - 1) / 2];
            float[] finalWeights = new float[blends.Length];
            float weightSum = 0;

            int index = 0;
            for (int i = 0; i < colors.Length - 1; i++)
            {
                for (int j = i + 1; j < colors.Length; j++)
                {
                    blends[index] = colors[i].Blend(colors[j], weights[j] / (weights[i] + weights[j]));
                    finalWeights[index] = weights[i] * weights[j];
                    weightSum += finalWeights[index];
                    index++;
                }
            }

            for (int i = 0; i < finalWeights.Length; i++)
                finalWeights[i] /= weightSum;

            float[] resultRGB = [0, 0, 0];
            for (int i = 0; i < finalWeights.Length; i++)
            {
                resultRGB[0] += blends[i].R * finalWeights[i];
                resultRGB[1] += blends[i].G * finalWeights[i];
                resultRGB[2] += blends[i].B * finalWeights[i];
            }

            return new RGBColor(
                (byte)(resultRGB[0] + 0.5),
                (byte)(resultRGB[1] + 0.5),
                (byte)(resultRGB[2] + 0.5));
        }

        public override string ToString() => $"[{RGB}, {HSV}]";
    }
}
