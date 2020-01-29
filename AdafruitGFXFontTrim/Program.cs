using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AdafruitGFXFontsLib;

namespace AdafruitGFXFontTrim
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Adafruit GFX Fonts Trimmer");
                Console.WriteLine("  by Mark van Renswoude");
                Console.WriteLine("  https://github.com/MvRens/AdafruitGFXFontTools");
                Console.WriteLine("");


                if (args.Length < 2)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  AdafruitGFXFontTrim.exe <Characters to keep (regex)> [<file name of mask>, ...]");
                    Console.WriteLine("");
                    Console.WriteLine("Examples:");
                    Console.WriteLine("");
                    Console.WriteLine("  AdafruitGFXFontTrim.exe [0-9] FreeMono9pt7b.h");
                    Console.WriteLine("  AdafruitGFXFontTrim.exe [\\w] FreeSans9pt7b.h FreeSans12pt7b.h");
                    Console.WriteLine("  AdafruitGFXFontTrim.exe \"[0-9,\\.:]\" Fonts\\*.h");
                    Console.WriteLine("");
                    return 2;
                }


                try
                {
                    var keepPattern = args[0];
                    var keep = new Regex(keepPattern);
                    var fileCount = 0;

                    foreach (var arg in args.Skip(1))
                    {
                        var basePath = Path.GetDirectoryName(arg);
                        var fileMask = Path.GetFileName(arg);

                        if (string.IsNullOrEmpty(basePath))
                            basePath = Directory.GetCurrentDirectory();

                        if (string.IsNullOrEmpty(fileMask))
                            fileMask = "*.h";

                        foreach (var filename in Directory.GetFiles(basePath, fileMask))
                        {
                            var outputFilename = Path.ChangeExtension(filename, ".trimmed.h");

                            Console.WriteLine($"Trimming {filename} > {outputFilename}");
                            ProcessFile(filename, outputFilename, keep, keepPattern);
                            fileCount++;
                        }
                    }

                    Console.WriteLine($"{fileCount} file(s) trimmed");

                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error:");
                    Console.WriteLine(e);
                    Console.WriteLine("");
                    return 1;
                }
            }
            finally
            {
                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
        }



        private static void ProcessFile(string inputFilename, string outputFilename, Regex keep, string keepPattern)
        {
            var inputFont = AdafruitGFXFontParser.ReadFile(inputFilename);

            var bitmap = new MemoryStream();
            var glyphs = new List<GFXglyph>();

            var currentChar = inputFont.FirstChar;
            char? firstChar = null;
            char? lastChar = null;
            var matchedGlyphsCount = 0;
            var matchedGlyphsLastCount = 0;


            foreach (var glyph in inputFont.Glyphs)
            {
                if (keep.IsMatch(currentChar.ToString()))
                {
                    glyphs.Add(new GFXglyph
                    {
                        bitmapOffset = (int)bitmap.Length,
                        width = glyph.width,
                        height = glyph.height,
                        xAdvance = glyph.xAdvance,
                        xOffset = glyph.xOffset,
                        yOffset = glyph.yOffset
                    });

                    var bitmapSize = (int)Math.Ceiling((glyph.width * glyph.height) / 8.0);
                    bitmap.Write(inputFont.Bitmap, glyph.bitmapOffset, bitmapSize);


                    if (!firstChar.HasValue)
                        firstChar = currentChar;

                    lastChar = currentChar;

                    matchedGlyphsCount++;
                    matchedGlyphsLastCount = glyphs.Count;
                }
                else if (firstChar.HasValue)
                {
                    // No gaps are allowed, insert dummy glyph
                    glyphs.Add(new GFXglyph
                    {
                        bitmapOffset = 0,
                        width = 0,
                        height = 0,
                        xAdvance = 0,
                        xOffset = 0,
                        yOffset = 0
                    });
                }

                currentChar++;
            }


            if (!firstChar.HasValue)
                throw new ArgumentException("No characters match the specified filter");


            // Remove the empty glyphs at the end
            if (glyphs.Count > matchedGlyphsLastCount)
                glyphs.RemoveRange(matchedGlyphsLastCount, glyphs.Count - matchedGlyphsLastCount);


            var outputFont = new AdafruitGFXFont
            {
                FontName = inputFont.FontName + "Trimmed",
                FirstChar = firstChar.Value,
                LastChar = lastChar.Value,
                YAdvance = inputFont.YAdvance,

                Bitmap = bitmap.ToArray(),
                Glyphs = glyphs
            };


            Console.WriteLine($"         Kept {matchedGlyphsCount} of {inputFont.Glyphs.Count} glyphs, ~ {inputFont.SizeEstimate} > {outputFont.SizeEstimate} bytes");
            Console.WriteLine();


            var output = AdafruitGFXFontWriter.WriteString(outputFont);

            using (var file = new StreamWriter(outputFilename))
            {
                file.WriteLine("/*");
                file.WriteLine("");
                file.WriteLine("  Generated by AdafruitGFXFontToBitmap");
                file.WriteLine("  https: //github.com/MvRens/AdafruitGFXFontTools");
                file.WriteLine("");
                file.WriteLine("  Source: " + Path.GetFileName(inputFilename));
                file.WriteLine("  Filter: " + CommentSafeRegex(keepPattern));
                file.WriteLine("");
                file.WriteLine("*/");
                file.WriteLine("");

                file.Write(output);
            }
        }


        private static string CommentSafeRegex(string value)
        {
            // It is unlikely that any / will appear in the pattern considering this use case,
            // but just to be sure we'll filter anything out but keep it a valid regular expression
            return value.Replace("/", "\\x2F");
        }
    }
}
