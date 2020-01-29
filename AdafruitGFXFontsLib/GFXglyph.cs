using System;
using System.Drawing;

namespace AdafruitGFXFontsLib
{
    // ReSharper disable InconsistentNaming - naming is consistent with Adafruit's gfxfont.h
    public struct GFXglyph
    {
        public int bitmapOffset;
        public int width;
        public int height;
        public int xAdvance;
        public int xOffset;
        public int yOffset;


        public Rectangle Bounds => new Rectangle(
            xOffset,
            0,
            width,
            height
        );

        public int ActualWidth => Math.Max(xAdvance, width + xOffset);
    }
    // ReSharper restore InconsistentNaming}
}
