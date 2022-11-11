using UnityEngine;

public static class ColorExtensions
{
	public static Color SetAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}
	
	public static Color SetRGB(this Color color, Color rgb)
	{
		return new Color(rgb.r, rgb.g, rgb.b, color.a);
	}
}
