﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace HSR_SIM_LIB.Utils;

/// <summary>
///     some utility stuff
/// </summary>
public static class Utl
{
    private static readonly Dictionary<string, Bitmap> ImageCache = new();
    public static string DataFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory + "DATA\\";


    /// <summary>
    ///     Create bitmap with convertation
    /// </summary>
    /// <param name="fi"></param>
    /// <returns></returns>
    public static Bitmap NewBitmap(this FileInfo fi)
    {
        Bitmap bitmap = null;
        try
        {
            bitmap = new Bitmap(fi.FullName);
        }
        catch (Exception)
        {
            // use 'MagickImage()' if you want just the 1st frame of an animated image. 
            // 'MagickImageCollection()' returns all frames
            using var magickImages = new MagickImageCollection(fi);
            var ms = new MemoryStream();
            if (magickImages.Count > 1)
                magickImages.Write(ms, MagickFormat.Gif);
            else
                magickImages.Write(ms, MagickFormat.Png);
            bitmap?.Dispose();
            bitmap = new Bitmap(ms)
            {
                Tag = ms
            };
        }

        return bitmap;
    }

    /// <summary>
    ///     Load file into bitmap. or get it from cache
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static Bitmap LoadBitmap(string filename)
    {
        if (!ImageCache.ContainsKey(filename))
            ImageCache[filename] = new FileInfo(GetAvailableImageFile(filename)).NewBitmap();
        return ImageCache[filename];
    }

    /// <summary>
    ///     we can load png or webp frames
    /// </summary>
    /// <param name="unitCode"></param>
    /// <returns></returns>
    public static string GetAvailableImageFile(string unitCode)
    {
        var imageFileName = DataFolder + "Images\\" + unitCode;
        if (File.Exists(imageFileName + ".png"))
            return imageFileName + ".png";
        if (File.Exists(imageFileName + ".webp")) return imageFileName + ".webp";
        return null;
    }
}