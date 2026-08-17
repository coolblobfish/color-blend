namespace ColorBlend
{
    public abstract class CustomColor
    {
        public abstract CustomColor Blend(CustomColor color1, CustomColor color2, float t);
    }
}
