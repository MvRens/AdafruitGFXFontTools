using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdafruitGFXFontsLib
{
    public static class AdafruitGFXFontWriter
    {
        public static void WriteFile(AdafruitGFXFont font, string path)
        {
            File.WriteAllText(path, WriteString(font));
        }


        public static string WriteString(AdafruitGFXFont font)
        {
            var output = new StringBuilder();

            WriteBitmaps(output, font);
            WriteGlyphs(output, font);
            WriteFont(output, font);

            return output.ToString();
        }


        private static void WriteBitmaps(StringBuilder output, AdafruitGFXFont font)
        {
            output
                .Append("const uint8_t ").Append(font.FontName).AppendLine("Bitmaps[] PROGMEM = {")
                .Append("  ");


            var rowCount = 0;

            for (var bitmapIndex = 0; bitmapIndex < font.Bitmap.Length; bitmapIndex++)
            {
                if (rowCount == 12)
                {
                    output.AppendLine().Append("  ");
                    rowCount = 0;
                }

                if (rowCount > 0)
                    output.Append(' ');

                output.Append("0x").Append(font.Bitmap[bitmapIndex].ToString("X2"));

                if (bitmapIndex < font.Bitmap.Length - 1)
                    output.Append(',');

                rowCount++;
            }


            output
                .AppendLine(" };")
                .AppendLine();
        }


        private static void WriteGlyphs(StringBuilder output, AdafruitGFXFont font)
        {
            output.Append("const GFXglyph ").Append(font.FontName).AppendLine("Glyphs[] PROGMEM = {");

            var currentChar = font.FirstChar;

            for (var glyphIndex = 0; glyphIndex < font.Glyphs.Count; glyphIndex++)
            {
                var glyph = font.Glyphs[glyphIndex];

                output.Append($"  {{ {glyph.bitmapOffset,5}, {glyph.width,3}, {glyph.height,3}, {glyph.xAdvance,3}, {glyph.xOffset,4}, {glyph.yOffset,4} }}");

                if (glyphIndex < font.Glyphs.Count - 1)
                    output.Append(",  ");
                else
                    output.Append(" };");

                output
                    .Append(" // 0x")
                    .Append(Convert.ToByte(currentChar).ToString("X2"))
                    .Append(" '")
                    .Append(currentChar)
                    .AppendLine("'");

                currentChar++;
            }

            output.AppendLine();
        }


        private static void WriteFont(StringBuilder output, AdafruitGFXFont font)
        {
            output
                .Append("const GFXfont ").Append(font.FontName).AppendLine(" PROGMEM = {")
                .Append("  (uint8_t  *)").Append(font.FontName).AppendLine("Bitmaps,")
                .Append("  (GFXglyph *)").Append(font.FontName).AppendLine("Glyphs,")
                .Append("  0x").Append(Convert.ToByte(font.FirstChar).ToString("X2")).Append(", ")
                    .Append("0x").Append(Convert.ToByte(font.LastChar).ToString("X2")).Append(", ")
                    .Append(font.YAdvance).AppendLine(" };")
                .AppendLine();

            output.AppendLine($"// Approx. {font.SizeEstimate} bytes");
        }
    }
}
