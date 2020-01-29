using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using AdafruitGFXFontsLib;

namespace AdafruitGFXFontToBitmap
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Adafruit GFX Fonts to Bitmap converter");
                Console.WriteLine("  by Mark van Renswoude");
                Console.WriteLine("  https://github.com/MvRens/AdafruitGFXFontTools");
                Console.WriteLine("");

                if (args.Length == 0)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  AdafruitGFXFontToBitmap.exe [<file name of mask>, ...]");
                    Console.WriteLine("");
                    Console.WriteLine("Examples:");
                    Console.WriteLine("");
                    Console.WriteLine("  AdafruitGFXFontToBitmap.exe FreeMono9pt7b.h");
                    Console.WriteLine("  AdafruitGFXFontToBitmap.exe FreeSans9pt7b.h FreeSans12pt7b.h");
                    Console.WriteLine("  AdafruitGFXFontToBitmap.exe Fonts\\*.h");
                    Console.WriteLine("");
                    return 2;
                }

                try
                {
                    var fileCount = 0;
               
                    foreach (var arg in args)
                    {
                        var basePath = Path.GetDirectoryName(arg);
                        var fileMask = Path.GetFileName(arg);

                        if (string.IsNullOrEmpty(basePath))
                            basePath = Directory.GetCurrentDirectory();

                        if (string.IsNullOrEmpty(fileMask))
                            fileMask = "*.h";

                        foreach (var filename in Directory.GetFiles(basePath, fileMask))
                        {
                            Console.WriteLine($"Converting {filename} > {filename}.bmp");
                            ProcessFile(filename, filename + ".bmp");
                            fileCount++;
                        }
                    }

                    Console.WriteLine($"{fileCount} file(s) converted");

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




        private static void ProcessFile(string inputFilename, string outputFilename)
        {
            var font = AdafruitGFXFontParser.ReadFile(inputFilename);

            var minYOffset = font.Glyphs.Min(g => g.yOffset);
            var maxYOffset = font.Glyphs.Max(g => g.height + g.yOffset);

            var totalWidth = font.Glyphs.Sum(g => g.ActualWidth) + (font.Glyphs.Count - 1);
            var maxHeight = maxYOffset - minYOffset;
            var cursorY = -minYOffset;

            var outputBitmap = new Bitmap(totalWidth, maxHeight);
            var graphics = Graphics.FromImage(outputBitmap);
            graphics.Clear(Color.Black);

            var glyphX = 0;

            foreach (var glyph in font.Glyphs)
            {
                var bitmapOffset = glyph.bitmapOffset;
                var bitmapBit = 0;
                var yOffset = cursorY + glyph.yOffset;

                for (var y = glyph.Bounds.Top + yOffset; y < glyph.Bounds.Bottom + yOffset; y++)
                {
                    for (var x = glyph.Bounds.Left; x < glyph.Bounds.Right; x++)
                    {
                        var pixel = font.Bitmap[bitmapOffset] & (1 << (7 - bitmapBit));
                        if (pixel > 0)
                            outputBitmap.SetPixel(glyphX + x, y, Color.White);

                        bitmapBit++;

                        // ReSharper disable once InvertIf
                        if (bitmapBit > 7)
                        {
                            bitmapOffset++;
                            bitmapBit = 0;
                        }
                    }
                }


                glyphX += glyph.ActualWidth;
                if (glyphX >= outputBitmap.Width) 
                    continue;


                // Draw separator line
                for (var y = 0; y < maxHeight; y++)
                    outputBitmap.SetPixel(glyphX, y, Color.DimGray);

                glyphX++;
            }

            outputBitmap.Save(outputFilename, ImageFormat.Bmp);
        }
    }
}
