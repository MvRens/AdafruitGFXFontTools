using System.Collections.Generic;

namespace AdafruitGFXFontsLib
{
    public class AdafruitGFXFont
    {
        public string FontName { get; set; }

        public char FirstChar { get; set; }
        public char LastChar { get; set; }
        public int YAdvance { get; set; }

        public byte[] Bitmap { get; set; }
        public List<GFXglyph> Glyphs { get; set; }


        // Based on the calculation in fontconvert.c
        public int SizeEstimate => Bitmap.Length + (Glyphs.Count * 7) + 7;
    }
}
