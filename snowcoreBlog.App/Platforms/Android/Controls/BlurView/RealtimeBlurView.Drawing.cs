using System.Diagnostics;
using Android.Graphics;
using Color = Android.Graphics.Color;
using RectF = Android.Graphics.RectF;

namespace Sharpnado.MaterialFrame.Droid;

public partial class RealtimeBlurView
{
    public override void Draw(Canvas canvas)
    {
        if (mIsRendering)
        {
            Debug.WriteLine($"BlurView@{GetHashCode()}", () => $"Draw() => skipping during blur capture");

            // Don't call base.Draw() - just skip this view entirely during capture
            return;
        }

        if (RENDERING_COUNT > 0)
        {
            Debug.WriteLine($"BlurView@{GetHashCode()}", () => $"Draw() => Doesn't support blurview overlap on another blurview");

            // Doesn't support blurview overlap on another blurview
            // Also skip drawing to prevent infinite recursion
        }
        else
        {
            Debug.WriteLine($"BlurView@{GetHashCode()}", () => $"Draw() => calling base draw");
            base.Draw(canvas);
        }
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        Debug.WriteLine($"BlurView@{GetHashCode()}", () => $"OnDraw(formsId: {_formsId})");
        DrawRoundedBlurredBitmap(canvas, mDisplayedBlurredBitmap, mOverlayColor);
    }

    private void DrawRoundedBlurredBitmap(Canvas canvas, Bitmap blurredBitmap, int overlayColor)
    {
        if (blurredBitmap != null)
        {
            Debug.WriteLine($"BlurView@{GetHashCode()}", () => $"DrawRoundedBlurredBitmap( mCornerRadius: {mCornerRadius}, mOverlayColor: {mOverlayColor} )");

            var mRectF = new RectF { Right = Width, Bottom = Height };

            mPaint.Reset();
            mPaint.AntiAlias = true;
            BitmapShader shader = new BitmapShader(blurredBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
            Matrix matrix = new Matrix();
            matrix.PostScale(mRectF.Width() / blurredBitmap.Width, mRectF.Height() / blurredBitmap.Height);
            shader.SetLocalMatrix(matrix);
            mPaint.SetShader(shader);
            canvas.DrawRoundRect(mRectF, mCornerRadius, mCornerRadius, mPaint);

            mPaint.Reset();
            mPaint.AntiAlias = true;
            mPaint.Color = new Color(overlayColor);
            canvas.DrawRoundRect(mRectF, mCornerRadius, mCornerRadius, mPaint);
        }
    }
}
