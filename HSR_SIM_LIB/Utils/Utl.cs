using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Utils.Utils;
using ImageMagick;

namespace HSR_SIM_LIB.Utils;

/// <summary>
///     some utility stuff
/// </summary>
public static class Utl
{
    private static readonly Dictionary<string, Bitmap> ImageCache = new();
    public static string DataFolder { get; set; } = AppDomain.CurrentDomain.BaseDirectory + "DATA\\";

    public static bool IsList(object o)
    {
        if (o == null) return false;
        return o is IList &&
               o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
    }

    /// <summary>
    ///     Get one item from list by random
    /// </summary>
    /// <param name="items"></param>
    /// <param name="wrk">link to Worker</param>
    /// <returns></returns>
    public static object GetRandomObject(object items, Worker wrk = null)
    {
        if (wrk.DevMode)
            return DevModeUtils.GetFixedObject(items, wrk);
        var rNextDouble = new MersenneTwister().NextDouble(); //get rand [0-1)
        object res = null;
        if (items is IEnumerable ie)
        {
            object[] arr = { };
            var i = 0;
            foreach (var element in ie)
            {
                Array.Resize(ref arr, i + 1);
                arr[i] = element;
                i = i + 1;
            }

            if (arr.Length > 0)
                res = arr[(int)Math.Floor(rNextDouble * arr.Length)];
        }
        else
        {
            throw new NotImplementedException();
        }

        return res;
    }

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