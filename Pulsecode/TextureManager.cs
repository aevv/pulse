using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Reflection;
using System.Resources;
using Pulse.UI;

namespace Pulse
{
    public static class TextureManager
    {
        static Dictionary<string, int> masks = new Dictionary<string, int>();
        static Dictionary<string, int> maskCount = new Dictionary<string, int>();
        static Dictionary<string, int> textures = new Dictionary<string, int>();
        static Dictionary<string, int> refCount = new Dictionary<string, int>();
        public static int loadFromBitmap(Bitmap b, out SizeF bSize)
        {
            return loadFromBitmap(b, false, "", out bSize);
        }
        public static int loadFromBitmap(Bitmap b, String key, out SizeF bSize)
        {
            return loadFromBitmap(b, true, key, out bSize);
        }
        public static int loadFromBitmap(Bitmap b, bool store, String key, out SizeF bSize)
        {
            bSize = new SizeF();
            if (textures.ContainsKey(key) && store)
            {
                refCount[key]++;
                return textures[key];
            }
            int texture = genFromBitmap(b, out bSize);
            if (store)
            {
                textures.Add(key, texture);
                refCount.Add(key, 1);
            }
            return texture;

        }
        public static int loadFromResx(String key, out SizeF bSize)
        {
            bSize = new SizeF();
            if (textures.ContainsKey(key))
            {
                refCount[key]++;
                return textures[key];
            }
            ResourceManager rm = new ResourceManager(typeof(DefaultSkin));
            Bitmap b = (Bitmap)rm.GetObject(key);
            int texture = genFromBitmap(b, out bSize);
            textures.Add(key, texture);
            refCount.Add(key, 1);
            return texture;
        }
        public static int loadImage(string path, out SizeF bSize)
        {
            return loadImage(path, true, out bSize);
        }
        /// <summary>
        /// Loads a texture into OpenGL and returns the int handle
        /// </summary>
        /// <param name="path">String path of the image to be loaded</param>
        /// <param name="store">Boolean determining whether to cache the reference or not</param>
        /// <returns></returns>
        public static int loadImage(string path, bool store, out SizeF bSize)
        {
            bSize = new SizeF();
            string[] split = path.Split('.');
            if (textures.ContainsKey(split[0]) && store)
            {
                refCount[split[0]]++;
                return textures[split[0]];
            }
            else
            {
                int texture;
                Bitmap b;
                if (File.Exists(path))
                {
                    b = new Bitmap(Bitmap.FromFile(path));
                }
                else
                {
                    Type type = typeof(Pulse.UI.DefaultSkin);
                    PropertyInfo pi = type.GetProperty(Path.GetFileNameWithoutExtension(path));
                    if (pi == null)
                    {
                        return -1;
                    }
                    b = (Bitmap)pi.GetValue(new object(), null);
                }
                texture = genFromBitmap(b, out bSize);
                //Console.WriteLine("Generating " + path);
                if (store)
                {
                    textures.Add(split[0], texture);
                    refCount.Add(split[0], 1);
                }
                return texture;
            }
        }
        public static int loadMask(string path, out SizeF bSize)
        {
            bSize = new SizeF();
            string[] split = path.Split('.');
            if (masks.ContainsKey(split[0]))
            {
                maskCount[split[0]]++;
                return masks[split[0]];
            }
            else
            {
                int texture;
                Bitmap b;
                if (File.Exists(path))
                {
                    b = new Bitmap(Bitmap.FromFile(path));
                    for (int x = 0; x < b.Width; x++)
                    {
                        for (int y = 0; y < b.Height; y++)
                        {
                            Color c = b.GetPixel(x, y);
                            
                            int z = c.R;
                            if (c.G > z)
                                z = c.G;
                            if (c.B > z)
                                z = c.B;
                            b.SetPixel(x, y, Color.FromArgb(255, 255 - z, 255 - z, 255 - z));
                        }
                    }                   
                }
                else
                {
                    Type type = typeof(Pulse.UI.DefaultSkin);
                    PropertyInfo pi = type.GetProperty(Path.GetFileNameWithoutExtension(path));
                    if (pi == null)
                    {
                        return -1;
                    }
                    b = (Bitmap)pi.GetValue(new object(), null);
                }
                texture = genFromBitmap(b, out bSize);
                //Console.WriteLine("Generating " + path);
                masks.Add(split[0], texture);
                maskCount.Add(split[0], 1);
                return texture;
            }
        }
        private static int genFromBitmap(Bitmap b, out SizeF bSize)
        {
            bSize = new SizeF();
            try
            {
                int texture;
                using (Bitmap bitmap = b)
                {
                    GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
                    GL.GenTextures(1, out texture);
                    GL.BindTexture(TextureTarget.Texture2D, texture);
                    BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.Finish();
                    bitmap.UnlockBits(data);
                    bitmap.Dispose();
                }
                GC.Collect();
                return texture;
            }
            catch (Exception)
            {
                return -1;
            }
        }
       
        /// <summary>
        /// Removes a reference of this image, and if the references == 0, deletes the image from OpenGLs memory
        /// </summary>
        /// <param name="path"></param>
        public static void removeImage(string path)
        {
            string[] split = path.Split('.');
            if (refCount.ContainsKey(split[0]))
            {
                if (--refCount[split[0]] == 0)
                {
                    int temp = textures[split[0]];
                    GL.DeleteTextures(1, ref temp);
                    refCount.Remove(split[0]);
                    textures.Remove(split[0]);
                }
            }
        }
    }
}
