using CoreImaging;
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

        public static int LoadTexture(Image image)
        {
            var texture = OpenTK.Graphics.OpenGL4.GL.GenTexture();

            OpenTK.Graphics.OpenGL4.GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture);

            OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackAlignment, 1);
            OpenTK.Graphics.OpenGL4.GL.PixelStore(OpenTK.Graphics.OpenGL4.PixelStoreParameter.UnpackRowLength, image.Width);

            if (image.DataStructure == CoreImaging.Image.ImageDataStructure.Rgb)
                OpenTK.Graphics.OpenGL4.GL.TexImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                    0, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgb, image.Width, image.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, image.Data);
            else if (image.DataStructure == CoreImaging.Image.ImageDataStructure.Rgba)
                OpenTK.Graphics.OpenGL4.GL.TexImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                    0, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, image.Data);

            OpenTK.Graphics.OpenGL4.GL.TexParameter(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                OpenTK.Graphics.OpenGL4.TextureParameterName.TextureMinFilter,
                (int)OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear);

            OpenTK.Graphics.OpenGL4.GL.TexParameter(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                OpenTK.Graphics.OpenGL4.TextureParameterName.TextureMagFilter,
                (int)OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear);

            OpenTK.Graphics.OpenGL4.GL.GenerateMipmap(OpenTK.Graphics.OpenGL4.GenerateMipmapTarget.Texture2D);

            return texture;
        }

        public static int LoadTexture(string path)
        {
            if (_textures.ContainsKey(path))
                return _textures[path];

            var texture = 0;
            var extension = System.IO.Path.GetExtension(path);

            if (extension == ".png")
            {
                //get texture data
                texture = LoadTexture(new CoreImaging.PNG.PngImage(path));
            }
            else if (extension == ".tif" || extension == ".tiff")
            {
                //get texture data
                texture = LoadTexture(new CoreImaging.Tiff.TiffImage(path));
            }
            else if (extension == ".tga")
            {
                //get texture data
                texture = LoadTexture(new CoreImaging.TGA.TgaImage(path)
                {
                    DataStructure = CoreImaging.Image.ImageDataStructure.Rgba
                });
            }

            _textures.Add(path, texture);

            return texture;
        }

    }
}
