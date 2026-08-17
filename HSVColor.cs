namespace ColorBlend
{
    public class HSVColor(float h, float s, float v) : CustomColor
    {
        public float H { get; } = h;
        public float S { get; } = s;
        public float V { get; } = v;

        public override HSVColor ToHSV() => this;

        public override HybridColor ToHybrid() => new(ToRGB(), this);

        public override RGBColor ToRGB()
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

        private static (float, float) FixWrapAround(float hue1, float hue2)
        {
            if (Math.Abs(hue1 - hue2) <= 180)
                return (hue1, hue2);
            if (hue1 < hue2)
                return (hue1 + 360, hue2);
            return (hue1, hue2 + 360);
        }

        public static HSVColor Blend(CustomColor color1, CustomColor color2, float t)
        {
            if (color1 is not HSVColor hsv1) hsv1 = color1.ToHSV();
            if (color2 is not HSVColor hsv2) hsv2 = color2.ToHSV();

            float satWeight1 = hsv1.V * (1 - t);
            float satWeight2 = hsv2.V * t;
            float saturation;
            if (satWeight1 + satWeight2 == 0)
            {
                saturation = hsv1.S + (hsv2.S - hsv1.S) * t;
            }
            else
            {
                saturation = (hsv1.S * satWeight1 + hsv2.S * satWeight2) / (satWeight1 + satWeight2);
            }

            float hueWeight1 = hsv1.S * satWeight1;
            float hueWeight2 = hsv2.S * satWeight2;
            float hue;
            if (hueWeight1 + hueWeight2 == 0)
            {
                hue = 0;
            }
            else
            {
                (float hue1, float hue2) = FixWrapAround(hsv1.H, hsv2.H);
                hue = (hue1 * hueWeight1 + hue2 * hueWeight2) / (hueWeight1 + hueWeight2);
                if (hue > 360)
                    hue -= 360;
            }

            return new HSVColor(hue, saturation, satWeight1 + satWeight2);
        }

        public static HSVColor BlendRaw(CustomColor color1, CustomColor color2, float t)
        {
            if (color1 is not HSVColor hsv1) hsv1 = color1.ToHSV();
            if (color2 is not HSVColor hsv2) hsv2 = color2.ToHSV();

            (float hue1, float hue2) = FixWrapAround(hsv1.H, hsv2.H);

            float hue = hue1 + (hue2 - hue1) * t;
            if (hue > 360)
                hue -= 360;

            return new HSVColor(hue, hsv1.S + (hsv2.S - hsv1.S) * t, hsv1.V + (hsv2.V - hsv1.V) * t);
        }

        public override string ToString() => $"HSV=({H:F0}, {S:F3}, {V:F3})";
    }
}
