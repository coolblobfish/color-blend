namespace ColorBlend
{
    public class ColorRegion
    {
        private HybridColor[] colors;
        private float[,] positions;
        private float[] weights;
        private int dimensions;

        public ColorRegion(HybridColor[] colors, float[,] positions, float[]? weights)
        {
            int minLength = Math.Min(colors.Length, positions.GetLength(0));
            if (weights != null)
                minLength = Math.Min(minLength, weights.Length);

            if (minLength == 0)
            {
                this.colors = colors.Length > 0 ? [colors[0]] : [new HybridColor(0, 0, 0)];
                this.positions = new float[1, 2];
                this.weights = [1];
                return;
            }

            dimensions = positions.GetLength(1);

            this.colors = new HybridColor[minLength];
            this.positions = new float[minLength, dimensions];
            this.weights = new float[minLength];

            for (int i = 0; i < minLength; i++)
            {
                this.colors[i] = colors[i];
                this.weights[i] = weights != null ? weights[i] : 1;
                for (int j = 0; j < dimensions; j++)
                    this.positions[i, j] = positions[i, j];
            }
        }

        public ColorRegion(HybridColor[] colors, float[,] positions) : this(colors, positions, null) { }

        public void Add(HybridColor color, float[] position, float weight)
        {
            if (colors.Length == 0)
            {
                colors = [color];
                positions = new float[1, position.Length];
                weights = [weight];
                for (int i = 0; i < position.Length; i++)
                    positions[0, i] = position[i];
                dimensions = position.Length;
                return;
            }

            int newLength = colors.Length + 1;
            HybridColor[] newColors = new HybridColor[newLength];
            float[,] newPositions = new float[newLength, dimensions];
            float[] newWeights = new float[newLength];

            for (int i = 0; i < colors.Length; i++)
            {
                newColors[i] = colors[i];
                newWeights[i] = weights[i];
                for (int j = 0; j < dimensions; j++)
                    newPositions[i, j] = positions[i, j];
            }

            newColors[colors.Length] = color;
            newWeights[colors.Length] = weight;
            for (int i = 0; i < dimensions; i++)
                newPositions[colors.Length, i] = i < position.Length ? position[i] : 0;

            colors = newColors;
            weights = newWeights;
            positions = newPositions;
        }

        public void Add(HybridColor color, float[] position)
        {
            Add(color, position, 1);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= colors.Length)
                return;

            int newLength = colors.Length - 1;
            HybridColor[] newColors = new HybridColor[newLength];
            float[,] newPositions = new float[newLength, dimensions];
            float[] newWeights = new float[newLength];

            for (int i = 0; i < newLength; i++)
            {
                int j = i < index ? i : i + 1;
                newColors[i] = colors[j];
                newWeights[i] = weights[j];
                for (int coord = 0; coord < dimensions; coord++)
                    newPositions[i, coord] = positions[j, coord];
            }

            colors = newColors;
            weights = newWeights;
            positions = newPositions;
        }

        public void Remove(HybridColor color)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] == color)
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        public float[] SecureDimensions(float[] position)
        {
            float[] safePosition = new float[dimensions];
            for (int i = 0; i < dimensions; i++)
                safePosition[i] = i < position.Length ? position[i] : 0;
            return safePosition;
        }

        public RGBColor GetRGB(float[] targetPos)
        {
            if (targetPos.Length != dimensions)
                targetPos = SecureDimensions(targetPos);
            return HybridColor.BlendMulti(colors, positions, targetPos, weights);
        }

        public RGBColor GetLinearRGB(float[] targetPos)
        {
            if (colors.Length == 1)
                return colors[0].RGB;

            if (targetPos.Length != dimensions)
                targetPos = SecureDimensions(targetPos);

            float numerator = 0;
            float denominator = 0;
            for (int i = 0; i < dimensions; i++)
            {
                float directionCoord = positions[1, i] - positions[0, i];
                numerator += directionCoord * (targetPos[i] - positions[0, i]);
                denominator += directionCoord * directionCoord;
            }
            return colors[0].Blend(colors[1], Math.Clamp(numerator / denominator, 0, 1));
        }

        public override string ToString()
        {
            string result = "ColorRegion:\n";
            for (int i = 0; i < colors.Length; i++)
            {
                result += $"[{i}]:\n  color={colors[i]}\n  position=(";
                for (int j = 0; j < dimensions; j++)
                {
                    result += positions[i, j];
                    if (j < dimensions - 1)
                        result += ", ";
                }
                result += $")\n  weight={weights[i]}\n";
            }
            return result;
        }
    }
}
