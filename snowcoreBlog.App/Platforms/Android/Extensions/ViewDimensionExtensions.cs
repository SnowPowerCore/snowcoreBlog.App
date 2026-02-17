using Android.Util;
using AndroidContent = Android.Content;

namespace snowcoreBlog.App.Platforms.Android.Extensions;

public static class ViewDimensionExtensions
{
	public static float DpToPx(AndroidContent.Context context, float dp)
	{
		var metrics = context.Resources?.DisplayMetrics
			?? context.ApplicationContext?.Resources?.DisplayMetrics;

		if (metrics is default(DisplayMetrics))
			return dp;

		return TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, metrics);
	}
}