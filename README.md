# AdafruitGFXFontTools
A small collection of command-line utilities to work with the Adafruit GFX font format.

Written in C# and targets .NET 4.7. A port to .NET Standard would probably be quite easy but I couldn't be bothered. Windows builds are available on the [Releases](https://github.com/MvRens/AdafruitGFXFontTools/releases) page.

All contents are released under the [Unlicense](https://unlicense.org/). Help yourself!


Note that all utilities assume the font files are generated in a format compatible with Adafruit's fontconvert tool. Tested against all the standard included fonts, your mileage may vary for other fonts.


## Usage
##### AdafruitGFXFontToBitmap
Produces a Windows bitmap (.bmp) file for one or more font (.h) files. Useful for creating UI mockups.

The output bitmap contains all glyphs in order from left to right as white text on a black background, with grey lines separating each glyph.

Multiple files can be specified on the command line, as well as wildcards. The output filename will have the ".bmp" extension added to the original filename.

__Examples__
```
  AdafruitGFXFontToBitmap.exe FreeMono9pt7b.h
  AdafruitGFXFontToBitmap.exe FreeSans9pt7b.h FreeSans12pt7b.h
  AdafruitGFXFontToBitmap.exe Fonts\*.h
```
<br>


#### AdafruitGFXFontTrim
Creates a "trimmed" version of a font, where only the data for the characters you are interested in will be kept. For example, if you want to use FreeSansBold24pt7b but only need the digits, the trimmed version of the font will save around 7kb of storage (from approximately 8815 bytes to 1034 bytes).

The first parameter is a regular expression to match the characters to keep. All remaining parameters are filenames, and may include wildcards.

Be aware that this can reduce the range of valid characters! Attempting to display characters outside of the range will use whatever program memory is at that location and the results are unpredictable at best! Intermediate characters are encoded as having a width and height of 0 and are relatively safe.

__Examples__
```
  AdafruitGFXFontTrim.exe [0-9] FreeMono9pt7b.h
  AdafruitGFXFontTrim.exe [\w] FreeSans9pt7b.h FreeSans12pt7b.h
  AdafruitGFXFontTrim.exe "[0-9,\.:]" Fonts\\*.h
```
