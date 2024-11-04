﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpFont;

namespace FNT
{
    public partial class FontLibrary
    {
        private class GlyphPage : IDisposable
        {
            public Texture2D Texture { get; }

            public GlyphPage(GraphicsDevice graphicsDevice, int textureSize)
            {
                TextureSize = textureSize;
                nodes.Add(new Rectangle(0, 0, textureSize, textureSize));
                colors = new uint[textureSize * textureSize];
                for (int i = textureSize * textureSize; i-- > 0;)
                    colors[i] = 0x00FFFFFF;

                Texture = new Texture2D(graphicsDevice, textureSize, textureSize, false, SurfaceFormat.Color);
            }

            public bool Pack(int w, int h, out Rectangle rect)
            {
                //  allocate an extra pixel on each side to prevent bleed
                w += 2;
                h += 2;

                for (int i = 0; i < nodes.Count; ++i)
                {
                    if (w <= nodes[i].Width && h <= nodes[i].Height)
                    {
                        var node = nodes[i];
                        nodes.RemoveAt(i);
                        rect = new Rectangle(node.X, node.Y, w, h);

                        if (node.X == 0)
                        {
                            nodes.Insert(i, new Rectangle(rect.Right, rect.Y, node.Right - rect.Right, rect.Height));
                            nodes.Add(new Rectangle(rect.X, rect.Bottom, node.Right, node.Bottom - rect.Bottom));
                        }
                        else
                        {
                            nodes.Insert(i, new Rectangle(rect.Right, rect.Y, node.Right - rect.Right, node.Height));
                        }

                        //  pad in for result
                        rect.X += 1;
                        rect.Y += 1;
                        rect.Width -= 1;
                        rect.Height -= 1;
                        return true;
                    }
                }

                rect = Rectangle.Empty;
                return false;
            }

            public void RenderGrayscaleGlyph(GlyphBitmap bitmap, int x, int y)
            {
                int height = bitmap.Rows;
                int width = bitmap.Width;
                int pitch = bitmap.Pitch;
                byte[] bufferData = bitmap.BufferData;

                for (int by = 0; by < height; by++)
                {
                    for (int bx = 0; bx < width; bx++)
                    {
                        var src = by * pitch + bx;
                        var dest = (by + y) * TextureSize + bx + x;
                        colors[dest] = (uint)(0x00FFFFFF | (bufferData[src] << 24));
                    }
                }

                Texture.SetData(colors);
            }

            public void RenderMonoGlyph(GlyphBitmap bitmap, int x, int y)
            {
                int height = bitmap.Rows;
                int width = bitmap.Width;
                int pitch = bitmap.Pitch;
                byte[] bufferData = bitmap.BufferData;

                for (int by = 0; by < height; by++)
                {
                    for (int bx = 0; bx < width; bx++)
                    {
                        int src = by * pitch + bx / 8;
                        int dest = (by + y) * TextureSize + bx + x;
                        bool isOpaque = (bufferData[src] & (1 << (7 - bx % 8))) != 0;
                        colors[dest] = isOpaque ? 0xFFFFFFFF : 0x00FFFFFF;
                    }
                }

                Texture.SetData(colors);
            }

            public void Dispose()
            {
                Texture.Dispose();
            }

            private readonly int TextureSize;
            private readonly uint[] colors;
            private readonly List<Rectangle> nodes = new List<Rectangle>();
        }

        private class RenderGlyph
        {
            public RenderGlyph(Texture2D texture, Rectangle bounds, Vector2 position)
            {
                Texture = texture;
                Bounds = bounds;
                Position = position;
            }

            public Texture2D Texture { get; }
            public Rectangle Bounds { get; }
            public Vector2 Position { get; }
        }

        private struct GlyphInfo
        {
            public uint Index;
            public int Width;
            public int Height;
            public int Advance;
            public int BearingX;
            public int BearingY;
            public char Character;
            public GlyphBitmap Bitmap;
            public int BitmapLeft;

            public GlyphInfo(char c, uint index, int bitmapLeft, GlyphMetrics metrics, FTBitmap bitmap)
            {
                BitmapLeft = bitmapLeft;
                Index = index;
                Character = c;
                Width = (int)metrics.Width;
                Height = (int)metrics.Height;
                Advance = (int)metrics.HorizontalAdvance;
                BearingX = (int)metrics.HorizontalBearingX;
                BearingY = (int)metrics.HorizontalBearingY;
                Bitmap = new GlyphBitmap(bitmap);
            }
        }

        private struct GlyphBitmap
        {
            public byte[] BufferData;
            public int Pitch;
            public int Rows;
            public int Width;
            public PixelMode PixelMode;

            public GlyphBitmap(FTBitmap bitmap)
            {
                Pitch = bitmap.Pitch;
                Rows = bitmap.Rows;
                Width = bitmap.Width;
                PixelMode = bitmap.PixelMode;
                BufferData = Pitch == 0 ? new byte[0] : bitmap.BufferData;
            }
        }
    }
}