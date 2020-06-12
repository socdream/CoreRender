using CoreRender.Geometry;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CoreRender.Text
{
    public class Font : IDisposable
    {
        public static string FontsPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Resources\Fonts\");
        public static string CharSheet = "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ1234567890¡!@#$%^&*()-=_+[]{}\\|;:'\".,<>/¿?`~· ";

        public static int CharSheetTexture { get; set; }

        public List<Character> Characters { get; set; } = new List<Character>();
        public int Texture { get; set; }
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();
        public Shaders.TextShader Shader { get; set; } = Shaders.ShaderManager.LoadShader<Shaders.TextShader>();
        public Mesh FontMesh { get; set; }
        public Color Color
        {
            get => Shader.Color;
            set => Shader.Color = value;
        }

        public Font(int screenWidth, int screenHeight)
        {
            var media = GetMediaSize(screenWidth);
            var paints = GetPaints(media);

            var path = $"{FontsPath}{media}.png";

            if (!File.Exists(path))
                CreateFontAtlasTexture(screenWidth);

            var png = new CoreImaging.PNG.PngImage(path);
            
            Texture = TextureManager.LoadTexture(png);

            Characters = GetCharacters(paints);
            
            for (int i = 0; i < Characters.Count; i++)
            {
                Meshes.Add(CreateCharMesh(Characters[i], screenWidth, screenHeight, png.Width, png.Height));
            }

            FontMesh = CreateFontMesh();
        }

        private Mesh CreateFontMesh()
        {
            var vertices = new List<MeshHelper.PositionUV0Vertex>()
            {
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = 0,
                    PosY = 0,
                    PosZ = 0,
                    UvX = 0,
                    UvY = 1
                },
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = 2,
                    PosY = 0,
                    PosZ = 0,
                    UvX = 1,
                    UvY = 1
                },
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = 2,
                    PosY = 1,
                    PosZ = 0,
                    UvX = 1,
                    UvY = 0
                },
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = 0,
                    PosY = 1,
                    PosZ = 0,
                    UvX = 0,
                    UvY = 0
                }
            };

            var indices = new List<int>
            {
                0, 1, 2,
                0, 2, 3
            };

            return new Mesh(MeshHelper.FromVertices(vertices, indices))
            {
                Shader = Shader,
                Texture = Texture
            };
        }

        private Mesh CreateCharMesh(Character character, int screenWidth, int screenHeight, int atlasWidth, int atlasHeight)
        {
            // create vao for each char
            var h = character.Height / (screenHeight / 2f);
            var w = character.Width / (screenWidth / 2f);

            // create texture coordinates
            var x1 = (float)character.X / atlasWidth;
            var x2 = (float)(character.X + character.Width) / atlasWidth;
            var y1 = (float)character.Y / atlasHeight;
            var y2 = (float)(character.Y + character.Height) / atlasHeight;

            var vertices = new List<MeshHelper.PositionUV0Vertex>()
            {
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = 0,
                    PosY = 0,
                    PosZ = 0,
                    UvX = x1,
                    UvY = y2
                },
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = w,
                    PosY = 0,
                    PosZ = 0,
                    UvX = x2,
                    UvY = y2
                },
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = w,
                    PosY = h,
                    PosZ = 0,
                    UvX = x2,
                    UvY = y1
                },
                new MeshHelper.PositionUV0Vertex()
                {
                    PosX = 0,
                    PosY = h,
                    PosZ = 0,
                    UvX = x1,
                    UvY = y1
                }
            };

            var indices = new List<int> 
            {
                0, 1, 2,
                0, 2, 3
            };

            return new Mesh(MeshHelper.FromVertices(vertices, indices))
            {
                Shader = Shader,
                Texture = Texture
            };
        }

        public void Draw(string text, float x, float y, Size size, int screenWidth, int screenHeight, Camera camera)
        {
            Shader.Texture = Texture;
            var offset = 0;

            foreach (var c in text)
                offset += Draw(c, x + offset, y, size, screenWidth, screenHeight, camera);
        }

        private int Draw(char value, float x, float y, Size size, int screenWidth, int screenHeight, Camera camera)
        {
            var index = Characters.TakeWhile(a => a.Name != value || a.Size != size).Count();

            if (index == Characters.Count)
                return 0;
            
            // Convert to pixel Co-ordinates
            x = x / (screenWidth / 2) - 1;
            y = (screenHeight - y - Characters[index].Height) / (screenHeight / 2) - 1;

            Shader.Offset = new float[] { x, y };

            // Draw
            Meshes[index].Draw(camera, null);

            return Characters[index].Width;
        }

        public PointF GetTextSize(string text, Size size)
        {
            var width = 0f;
            var height = 0f;

            foreach (var c in text)
            {
                var index = Characters.TakeWhile(a => a.Name != c || a.Size != size).Count();

                if (index == Characters.Count)
                    continue;

                width += Characters[index].Width;
                height = Characters[index].Height;
            }

            return new PointF(width, height);
        }

        public static List<Character> GetCharacters(List<SKPaint> paints)
        {
            var chars = GetCharacters(Size.Small, 0, paints[0]).ToList();
            chars.AddRange(GetCharacters(Size.Medium, (int)(paints[0].FontMetrics.Bottom - paints[0].FontMetrics.Top), paints[1]));
            chars.AddRange(GetCharacters(Size.Large, (int)(paints[0].FontMetrics.Bottom - paints[0].FontMetrics.Top +
                    paints[1].FontMetrics.Bottom - paints[1].FontMetrics.Top), paints[2]));

            return chars;
        }

        public static IEnumerable<Character> GetCharacters(Size size, int yOffset, SKPaint paint)
        {
            var x = 0;

            var rect = new SKRect(0, 0, CharSheet.Length * (int)paint.TextSize, (int)paint.TextSize * 2);

            for (var i = 0; i < CharSheet.Length; i++)
            {
                var width = (int)paint.MeasureText(CharSheet[i].ToString(), ref rect);

                yield return new Character()
                {
                    Name = CharSheet[i],
                    X = x,
                    Y = yOffset,
                    Height = (int)(paint.FontMetrics.Bottom - paint.FontMetrics.Top),
                    Width = width,
                    Size = size
                };

                x += width + 1;
            }
        }

        private static List<SKPaint> GetPaints(MediaSize media)
        {
            return new List<SKPaint>()
            {
                new SKPaint()
                {
                    TextSize = GetFontSize(Size.Small, media),
                    IsAntialias = true,
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill
                },
                new SKPaint()
                {
                    TextSize = GetFontSize(Size.Medium, media),
                    IsAntialias = true,
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill
                },
                new SKPaint()
                {
                    TextSize = GetFontSize(Size.Large, media),
                    IsAntialias = true,
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill
                }
            };
        }

        public static string CreateFontAtlasTexture(int width)
        {
            var media = GetMediaSize(width);
            var paints = GetPaints(media);

            var size = paints[0].FontMetrics.Bottom - paints[0].FontMetrics.Top +
                paints[1].FontMetrics.Bottom - paints[1].FontMetrics.Top +
                paints[2].FontMetrics.Bottom - paints[2].FontMetrics.Top;

            var rect = new SKRect(0, 0, CharSheet.Length * (int)paints[2].TextSize, size);

            var textwidth = paints[2].MeasureText(CharSheet, ref rect);

            var bitmap = new SKBitmap((int)textwidth + CharSheet.Length, (int)size);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            var chars = GetCharacters(paints);

            foreach (var c in chars)
            {
                var paint = c.Size == Size.Large ? paints[2] : c.Size == Size.Medium ? paints[1] : paints[0];

                canvas.DrawText(c.Name.ToString(), c.X, c.Y - paint.FontMetrics.Top, paint);
            }

            canvas.Save();

            // create an image and then get the PNG (or any other) encoded data
            if (!Directory.Exists(FontsPath))
                Directory.CreateDirectory(FontsPath);

            var path = $"{FontsPath}{media}.png";

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                // save the data to a stream
                using (var stream = File.OpenWrite(path))
                {
                    data.SaveTo(stream);

                    return path;
                }
            }
        }

        public class Character
        {
            /// <summary>
            /// Actual character being represented
            /// </summary>
            public char Name { get; set; }
            /// <summary>
            /// X Position in the atlas
            /// </summary>
            public int X { get; set; }
            /// <summary>
            /// Y Position in the atlas
            /// </summary>
            public int Y { get; set; }
            /// <summary>
            /// Width in pixels
            /// </summary>
            public int Width { get; set; }
            /// <summary>
            /// Height in pixels
            /// </summary>
            public int Height { get; set; }
            /// <summary>
            /// Size of the font
            /// </summary>
            public Size Size { get; set; }
        }
        
        public static int GetFontSize(Size size, MediaSize media)
        {
            switch (size)
            {
                case Size.Small:
                    switch (media)
                    {
                        case MediaSize.ExtraSmall:
                            return 5;
                        case MediaSize.Small:
                            return 8;
                        case MediaSize.Medium:
                            return 13;
                        case MediaSize.Large:
                            return 21;
                        case MediaSize.ExtraLarge:
                            return 34;
                    }
                    break;
                case Size.Medium:
                    switch (media)
                    {
                        case MediaSize.ExtraSmall:
                            return 8;
                        case MediaSize.Small:
                            return 13;
                        case MediaSize.Medium:
                            return 21;
                        case MediaSize.Large:
                            return 34;
                        case MediaSize.ExtraLarge:
                            return 55;
                    }
                    break;
                case Size.Large:
                    switch (media)
                    {
                        case MediaSize.ExtraSmall:
                            return 13;
                        case MediaSize.Small:
                            return 21;
                        case MediaSize.Medium:
                            return 34;
                        case MediaSize.Large:
                            return 55;
                        case MediaSize.ExtraLarge:
                            return 89;
                    }
                    break;
            }

            return 13;
        }

        public static MediaSize GetMediaSize(int width)
        {
            if (width < 576)
                return MediaSize.ExtraSmall;
            if (width >= 576 && width < 768)
                return MediaSize.Small;
            if (width >= 768 && width < 992)
                return MediaSize.Medium;
            if (width >= 992 && width < 1200)
                return MediaSize.Large;
            if (width >= 1200)
                return MediaSize.ExtraLarge;

            return MediaSize.Medium;
        }

        public enum MediaSize : int
        {
            // Extra small devices (portrait phones, less than 576px)
            ExtraSmall,
            // Small devices (landscape phones, 576px and up)
            Small,
            // Medium devices (tablets, 768px and up)
            Medium,
            // Large devices (desktops, 992px and up)
            Large,
            // Extra large devices (large desktops, 1200px and up)
            ExtraLarge
        }

        public enum Size : int
        {
            Small,
            Medium,
            Large
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    foreach (var item in Meshes)
                        item.Dispose();
                }
                
                disposedValue = true;
            }
        }
        
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
