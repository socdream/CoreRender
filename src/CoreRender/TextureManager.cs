using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender
{
    public static class TextureManager
    {
        public static string TexturesPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Resources\Textures\");
        private static Dictionary<string, int> _textures = new Dictionary<string, int>();

        public static int LoadTexture(string path)
        {
            if (_textures.ContainsKey(path))
                return _textures[path];

            var texture = 0;
            var extension = System.IO.Path.GetExtension(path);

            if (extension == ".png")
            {
                //get texture data
                var png = new CoreImaging.PNG.PngImage(path);

                texture = OpenTK.Graphics.OpenGL4.GL.GenTexture();

                OpenTK.Graphics.OpenGL4.GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture);

                OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackAlignment, 1);
                OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackRowLength, png.Width);

                if (png.DataStructure == CoreImaging.Image.ImageDataStructure.Rgb)
                    OpenTK.Graphics.OpenGL4.GL.TexImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                        0, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgb, png.Width, png.Height, 0,
                        OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, png.Data);
                else if (png.DataStructure == CoreImaging.Image.ImageDataStructure.Rgba)
                    OpenTK.Graphics.OpenGL4.GL.TexImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                        0, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgba, png.Width, png.Height, 0,
                        OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, png.Data);
            }
            else if (extension == ".tif" || extension == ".tiff")
            {
                //get texture data
                var tif = new CoreImaging.Tiff.TiffImage(path);

                texture = OpenTK.Graphics.OpenGL4.GL.GenTexture();

                OpenTK.Graphics.OpenGL4.GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture);

                OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackAlignment, 1);
                OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackRowLength, tif.Width);

                OpenTK.Graphics.OpenGL4.GL.TexImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                    0, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgb, tif.Width, tif.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, tif.Data);
            }
            else if (extension == ".tga")
            {
                //get texture data
                var tga = new CoreImaging.TGA.TgaImage(path)
                {
                    DataStructure = CoreImaging.Image.ImageDataStructure.Rgba
                };

                texture = OpenTK.Graphics.OpenGL4.GL.GenTexture();

                OpenTK.Graphics.OpenGL4.GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture);

                OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackAlignment, 1);
                OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackRowLength, tga.Width);

                OpenTK.Graphics.OpenGL4.GL.TexImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                    0, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgba, tga.Width, tga.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, tga.Data);
            }
            
            OpenTK.Graphics.OpenGL4.GL.TexParameter(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                OpenTK.Graphics.OpenGL4.TextureParameterName.TextureMinFilter,
                (int)OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear);

            OpenTK.Graphics.OpenGL4.GL.TexParameter(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                OpenTK.Graphics.OpenGL4.TextureParameterName.TextureMagFilter,
                (int)OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear);

            OpenTK.Graphics.OpenGL4.GL.GenerateMipmap(OpenTK.Graphics.OpenGL4.GenerateMipmapTarget.Texture2D);

            return texture;
        }

    }
}
