using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLab
{
    class CodeLabTemp
    {
        bool IsCancelRequested = false;

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            // Delete any of these lines you don't need
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
            int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
            int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
            ColorBgra PrimaryColor = EnvironmentParameters.PrimaryColor;
            ColorBgra SecondaryColor = EnvironmentParameters.SecondaryColor;
            int BrushWidth = (int)EnvironmentParameters.BrushWidth;

            ColorBgra CurrentPixel;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    CurrentPixel = src[x, y];
                    // TODO: Add pixel processing code here
                    // Access RGBA values this way, for example:
                    // CurrentPixel.R = PrimaryColor.R;
                    // CurrentPixel.G = PrimaryColor.G;
                    // CurrentPixel.B = PrimaryColor.B;
                    // CurrentPixel.A = PrimaryColor.A;
                    dst[x, y] = CurrentPixel;
                }
            }
        }
    }

    class Surface
    {
        public Rectangle Bounds { get; set; }

        public ColorBgra this[int x, int y]
        {
            get { throw new Exception(); }
            set { throw new Exception(); }
        }
    }

    struct Rectangle
    {
        public int Bottom { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
    }

    struct ColorBgra { }

    class PdnRegion
    {
        public Rectangle GetBoundsInt()
        {
            throw new NotImplementedException();
        }
    }

    class EnvironmentParameters
    {
        public static float BrushWidth { get; set; }
        public static ColorBgra PrimaryColor { get; set; }
        public static ColorBgra SecondaryColor { get; set; }

        public static PdnRegion GetSelection(Rectangle bounds)
        {
            throw new NotImplementedException();
        }
    }
}
