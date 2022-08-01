using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Common
{
    class ImageUtils
    {
        public static BitmapSource GetImage(IntPtr bm)
        {
            BitmapSource bmSource = Imaging.CreateBitmapSourceFromHBitmap(
                bm,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return bmSource;
        }
    }
}
