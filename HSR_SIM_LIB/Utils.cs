﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// some utility stuff
    /// </summary>
    internal static class Utils
    {
        private static string dataFolder = AppDomain.CurrentDomain.BaseDirectory + "DATA\\";

        public static string DataFolder { get => dataFolder; set => dataFolder = value; }
        public static Dictionary<String, Bitmap> imageCache = new Dictionary<String, Bitmap>();


        /// <summary>
        /// Create bitmap with convertation 
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
                using (var magickImages = new MagickImageCollection(fi))
                {
                    var ms = new MemoryStream();
                    if (magickImages.Count > 1)
                    {
                        magickImages.Write(ms, MagickFormat.Gif);
                    }
                    else
                    {
                        magickImages.Write(ms, MagickFormat.Png);
                    }
                    bitmap?.Dispose();
                    bitmap = new Bitmap(ms);
                    // keep MemoryStream from being garbage collected while Bitmap is in use
                    bitmap.Tag = ms;
                }
            }
            return bitmap;
        }
        /// <summary>
        /// Load file into bitmap. or get it from cache
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Bitmap LoadBitmap(string filename)
        {
            if (!imageCache.ContainsKey(filename))
                imageCache[filename] = NewBitmap(new FileInfo(getAvalableImageFile(filename)));
            return imageCache[filename];
        }
        /// <summary>
        /// we can load png or webp frames
        /// </summary>
        /// <param name="unitCode"></param>
        /// <returns></returns>
        public static string getAvalableImageFile(string unitCode)
        {
            string imageFileName = DataFolder + "Images\\" + unitCode;
            if (File.Exists(imageFileName + ".png"))
            {
                return imageFileName + ".png";
            }
            else if (File.Exists(imageFileName + ".webp"))
            {
                return imageFileName + ".webp";
            }
            return null;
        }
    }
}