# ColorBlend

## A C# class library for smoothly blending colors using combinations of RGB and HSV.

This library blends colors by automatically combining RGB and HSV algorithms to reduce the dullness of intermediate colors.

Main features:
* Blend colors with a hybrid RGB and HSV algorithm
* Blend individual RGB and HSV colors
* Create any-dimensional "color regions" that contain multiple color points to find the blended color of any position in between
* Find the perceived lightness of a color
* Construct colors from a given hue and perceived lightness

\
**Note:**\
All RGB values must be integers between 0 and 255.\
Hue must be a float between 0 and 360, inclusive, and saturation and value must be floats between 0 and 1, inclusive.
When finding a color in a ColorRegion, the target position should have the same number of dimensions as all of the positions in the region.

\
In the following image, the top half is shaded using simple RGB blending, and the bottom half uses the hybrid algorithm.
<img width="1280" height="360" alt="rgb vs hybrid gradient" src="https://github.com/user-attachments/assets/8eff3b8d-c5b2-444e-b22e-4bc0d31c80af" />

The middle range of the hybrid gradient is noticeably more vibrant than that of the RGB gradient.

\
For blending HSV colors, the **hue and saturation are influenced by the saturation and value**, respectively, to negate any unpredictable behavior. For example, when blending a saturated green with a color that has 0 saturation, the hue will be green throughout the entire gradient because the other color does not really have a hue, so interpolating between green and the numerical hue of the second color would be illogical. Likewise, blending a saturated green with a color with 0 value (which will always be black), the saturation of the color throughout the gradient will be equal to the saturation of the original green. For all saturations and values in between, the weights for the resulting hues and saturations take into account the ratio of saturations and values between the two colors when blending.
