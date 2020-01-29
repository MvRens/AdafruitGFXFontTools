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
    }
}
