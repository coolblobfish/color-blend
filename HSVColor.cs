using System.Drawing;

namespace ColorBlend
{
    public readonly struct HSVColor(float h, float s, float v)
    {
        public readonly float H { get; } = h;
        public readonly float S { get; } = s;
        public readonly float V { get; } = v;

        public static HSVColor FromColor(Color color)
        {
            return RGBColor.FromColor(color).ToHSV();
        }

        public Color ToColor()
        {
            return ToRGB().ToColor();
        }

        public RGBColor ToRGB()
        {
            float maxVal = V * 255;
            if (S == 0)
            {
                byte colorAmount = (byte)(maxVal + 0.5);
                return new RGBColor(colorAmount, colorAmount, colorAmount);
            }

            float range = S * maxVal;
            float minVal = maxVal - range;

            float[] rgbNormalized;
            if (H <= 60)
                rgbNormalized = [1, H / 60, 0];
            else if (H <= 120)
                rgbNormalized = [(120 - H) / 60, 1, 0];
            else if (H <= 180)
                rgbNormalized = [0, 1, (H - 120) / 60];
            else if (H <= 240)
                rgbNormalized = [0, (240 - H) / 60, 1];
            else if (H <= 300)
                rgbNormalized = [(H - 240) / 60, 0, 1];
            else
                rgbNormalized = [1, 0, (360 - H) / 60];

            return new RGBColor(
                (byte)(rgbNormalized[0] * range + minVal + 0.5),
                (byte)(rgbNormalized[1] * range + minVal + 0.5),
                (byte)(rgbNormalized[2] * range + minVal + 0.5));
        }

        public HSVColor Blend(HSVColor other, float t)
        {
            float hue1;
            float hue2;
            if (Math.Abs(other.H - H) <= 180)
            {
                hue1 = H;
                hue2 = other.H;
            }
            else
            {
                if (H > other.H)
                {
                    hue1 = H;
                    hue2 = other.H + 360;
                }
                else
                {
                    hue1 = H + 360;
                    hue2 = other.H;
                }
            }

            float hueWeight1 = S * (1 - t);
            float hueWeight2 = other.S * t;
            float hue;
            if (hueWeight1 + hueWeight2 == 0)
            {
                hue = 0;
            }
            else
            {
                hue = (hue1 * hueWeight1 + hue2 * hueWeight2) / (hueWeight1 + hueWeight2);
                if (hue > 360)
                    hue -= 360;
            }

            float satWeight1 = V * (1 - t);
            float satWeight2 = other.V * t;
            float saturation;
            if (satWeight1 + satWeight2 == 0)
            {
                saturation = S + (other.S - S) * t;
            }
            else
            {
                saturation = (S * satWeight1 + other.S * satWeight2) / (satWeight1 + satWeight2);
            }

            return new HSVColor(hue, saturation, satWeight1 + satWeight2);
        }

        public override string ToString() => $"({H:F0}, {S:F3}, {V:F3})";
    }
}
