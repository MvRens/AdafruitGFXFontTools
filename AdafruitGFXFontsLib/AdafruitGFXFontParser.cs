using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdafruitGFXFontsLib
{
    public static class AdafruitGFXFontParser
    {
        private static readonly Regex MatchBitmapsArray = new Regex("const uint8_t .*? {(.+?)};", RegexOptions.Singleline);
        private static readonly Regex MatchBitmapValue = new Regex("0x([0-9A-F]{2})");

        private static readonly Regex MatchGlyphsArray = new Regex("const GFXglyph .*? {(.+?)};", RegexOptions.Singleline);
        private static readonly Regex MatchGlyphValue = new Regex(@"{\s*(?<bitmapOffset>\d+),\s*(?<width>\d+),\s*(?<height>\d+),\s*(?<xAdvance>\d+),\s*(?<xOffset>-{0,1}\d+),\s*(?<yOffset>-{0,1}\d+)");

        private static readonly Regex MatchFont = new Regex(@"const GFXfont (?<fontName>.+?) PROGMEM = {\s*\(uint8_t\s*\*\)(?<bitmapsName>.+?)Bitmaps,\s*\(GFXglyph\s*\*\)(?<glyphsName>.+?)Glyphs,\s+0x(?<firstChar>[0-9A-F]+),\s*0x(?<lastChar>[0-9A-F]+),\s*(?<yAdvance>\d+)\s*};", RegexOptions.Singleline);


        public static AdafruitGFXFont ReadFile(string path)
        {
            return ReadString(File.ReadAllText(path));
        }


        public static AdafruitGFXFont ReadString(string headerFileContents)
        {
            var fontMatch = MatchFont.Match(headerFileContents);
            if (!fontMatch.Success)
                throw new IOException("Font definition not found");


            return new AdafruitGFXFont
            {
                FontName = fontMatch.Groups["fontName"].Value,
                FirstChar = HexToChar(fontMatch.Groups["firstChar"].Value),
                LastChar = HexToChar(fontMatch.Groups["lastChar"].Value),
                YAdvance = int.Parse(fontMatch.Groups["yAdvance"].Value),

                Bitmap = GetBitmap(headerFileContents),
                Glyphs = GetGlyphs(headerFileContents).ToList()
            };
        }


        private static char HexToChar(string value)
        {
            return Convert.ToChar(Convert.ToUInt32(value, 16));
        }


        private static byte[] GetBitmap(string headerFileContents)
        {
            var arrayMatch = MatchBitmapsArray.Match(headerFileContents);
            if (!arrayMatch.Success)
                throw new IOException("Bitmap array not found");

            var valueMatches = MatchBitmapValue.Matches(arrayMatch.Groups[1].Value);
            if (valueMatches.Count == 0)
                throw new IOException("Bitmap array values not found");

            return valueMatches
                .Cast<Match>()
                .Select(m => Convert.ToByte(m.Groups[1].Value, 16))
                .ToArray();
        }


        private static IEnumerable<GFXglyph> GetGlyphs(string headerFileContents)
        {
            var arrayMatch = MatchGlyphsArray.Match(headerFileContents);
            if (!arrayMatch.Success)
                throw new IOException("Glyphs array not found");

            var valueMatches = MatchGlyphValue.Matches(arrayMatch.Groups[1].Value);
            if (valueMatches.Count == 0)
                throw new IOException("Glyph array values not found");

            return valueMatches
                .Cast<Match>()
                .Select(m => new GFXglyph
                {
                    bitmapOffset = int.Parse(m.Groups["bitmapOffset"].Value),
                    width = int.Parse(m.Groups["width"].Value),
                    height = int.Parse(m.Groups["height"].Value),
                    xAdvance = int.Parse(m.Groups["xAdvance"].Value),
                    xOffset = int.Parse(m.Groups["xOffset"].Value),
                    yOffset = int.Parse(m.Groups["yOffset"].Value)
                });
        }
    }
}
