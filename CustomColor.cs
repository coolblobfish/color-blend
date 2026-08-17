namespace ColorBlend
{
    public delegate CustomColor BlendMethod(CustomColor color1, CustomColor color2, float t);

    public abstract class CustomColor
    {
        public abstract RGBColor ToRGB();
        public abstract HSVColor ToHSV();
        public abstract HybridColor ToHybrid();

        public static RGBColor BlendMulti(CustomColor[] colors, float[,] positions, float[] target,
            float[] colorWeights, BlendMethod blendMethod)
        {
            if (colors.Length == 0)
                return new RGBColor(0, 0, 0);

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
                            if (target[coord] != positions[j, coord])
                            {
                                samePosition = false;
                                break;
                            }
                        }
                        if (samePosition)
                            indices.Add(j);
                    }
                    CustomColor[] newColors = new CustomColor[indices.Count];
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
                return colors[0].ToRGB();

            RGBColor[] blends = new RGBColor[colors.Length * (colors.Length - 1) / 2];
            float[] finalWeights = new float[blends.Length];
            float weightSum = 0;

            int index = 0;
            for (int i = 0; i < colors.Length - 1; i++)
            {
                for (int j = i + 1; j < colors.Length; j++)
                {
                    blends[index] = blendMethod(colors[i], colors[j], weights[j] / (weights[i] + weights[j])).ToRGB();
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
    }
}
