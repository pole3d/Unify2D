using System;
using System.Collections.Generic;
using System.Reflection;
using Unify2D.Core;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Unify2D.Toolbox
{
    public class SpriteEditorToolbox : Toolbox
    {
        private bool _isOpen = false;
        private GameAsset _gameAsset = null;
        private Component _component = null;

        private readonly List<TextureBound> _texturesBound = new List<TextureBound>();

        int _pixelSizeX = 64;
        int _pixelSizeY = 64;

        private ColorPixel[,] _texture;
        private List<ColorPixel[,]> _slicedTextures = new List<ColorPixel[,]>();

        // Debug
        private bool _hasSlicedTextures = false;
        //End Debug

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        public void Open(Component component)
        {
            _component = component;
            AddTextureBound(GetSpriteTextureBoundToSlice());
            _isOpen = true;
        }
        
        public override void Draw()
        {
            if (!_isOpen)
                return;

            if (ImGui.Begin("Sprite Editor Toolbox", ref _isOpen))
            {
                if (ImGui.Button("Slice"))
                {
                    SliceTexture();
                }
                
                DrawTexturePreviewWithGrid();

                //Debug
                if (_hasSlicedTextures)
                {
                    ImGui.Separator();
                    DrawSlicedTextures();
                }
                //End Debug
            }

            ImGui.End();
        }
        
        private void DrawTexturePreviewWithGrid()
        {
            ImGui.InputInt("Pixel Size X", ref _pixelSizeX, 1, 1);
            ImGui.InputInt("Pixel Size Y", ref _pixelSizeY, 1, 1);
            _pixelSizeX = Math.Max(1, _pixelSizeX);
            _pixelSizeY = Math.Max(1, _pixelSizeY);

            ImGui.Image(_texturesBound[0].IntPtr,
                new System.Numerics.Vector2(_texturesBound[0].Texture.Width, _texturesBound[0].Texture.Height));

            var imageMin = ImGui.GetItemRectMin();
            var imageMax = ImGui.GetItemRectMax();
            var drawList = ImGui.GetWindowDrawList();

            uint redColor = ImGui.GetColorU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            float lineThickness = 1.0f;

            // Calcul de la grille
            int lastVerticalLine =
                (_texturesBound[0].Texture.Width / _pixelSizeX) * _pixelSizeX; // Dernière position verticale valide
            int lastHorizontalLine =
                (_texturesBound[0].Texture.Height / _pixelSizeY) * _pixelSizeY; // Dernière position horizontale valide

            // Lignes verticales
            for (int x = 0; x < _texturesBound[0].Texture.Width; x += _pixelSizeX)
            {
                if (x + _pixelSizeX > lastVerticalLine)
                    break;

                drawList.AddLine(
                    new System.Numerics.Vector2(imageMin.X + x, imageMin.Y),
                    new System.Numerics.Vector2(imageMin.X + x, imageMax.Y),
                    redColor,
                    lineThickness
                );
            }

            // Lignes horizontales
            for (int y = 0; y < _texturesBound[0].Texture.Height; y += _pixelSizeY)
            {
                if (y + _pixelSizeY > lastHorizontalLine)
                    break;

                drawList.AddLine(
                    new System.Numerics.Vector2(imageMin.X, imageMin.Y + y),
                    new System.Numerics.Vector2(imageMax.X, imageMin.Y + y),
                    redColor,
                    lineThickness
                );
            }
        }

        // Slice Texture
        private void SliceTexture()
        {
            if (_texturesBound.Count == 0)
            {
                Debug.Log("No texture bound to slice.");
                return;
            }

            Texture2D texture = _texturesBound[0].Texture;
            byte[] imageData = new byte[4 * texture.Width * texture.Height];
            texture.GetData(imageData);

            _texture = ConvertToColorArray(imageData, texture.Width, texture.Height);

            int textureWidth = _texture.GetLength(0);
            int textureHeight = _texture.GetLength(1);

            int columns = textureWidth / _pixelSizeX;
            int rows = textureHeight / _pixelSizeY;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int startX = x * _pixelSizeX;
                    int startY = y * _pixelSizeY;

                    ColorPixel[,] cell = new ColorPixel[_pixelSizeX, _pixelSizeY];

                    for (int i = 0; i < _pixelSizeX; i++)
                    {
                        for (int j = 0; j < _pixelSizeY; j++)
                        {
                            cell[i, j] = _texture[startX + i, startY + j];
                        }
                    }

                    _slicedTextures.Add(cell);
                }
            }

            _hasSlicedTextures = true;
            Debug.Log(
                $"Slicing complete. {_slicedTextures.Count} cells created."); // Print the number of sub-textures created
        }

        private ColorPixel[,] ConvertToColorArray(byte[] imageData, int width, int height)
        {
            ColorPixel[,] colorArray = new ColorPixel[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;

                    float r = imageData[index] / 255f;
                    float g = imageData[index + 1] / 255f;
                    float b = imageData[index + 2] / 255f;
                    float a = imageData[index + 3] / 255f;

                    colorArray[x, y] = new ColorPixel(r, g, b, a);
                }
            }

            return colorArray;
        }

        // Draw SubTextures
        private void DrawSlicedTextures()
        {
            if (_slicedTextures.Count == 0)
            {
                ImGui.Text("No sliced textures to display.");
                return;
            }

            ImGui.Text("Sliced Textures Preview:");

            float cellSizeX = _pixelSizeX;
            float cellSizeY = _pixelSizeY;
            float padding = 10.0f;
            float windowWidth = ImGui.GetContentRegionAvail().X;
            float cursorX = 0.0f;

            foreach (var cell in _slicedTextures)
            {
                Texture2D cellTexture = CreateTextureFromColorArray(cell);
                IntPtr cellPtr = GameEditor.Instance.GuiRenderer.BindTexture(cellTexture);

                ImGui.Image(cellPtr, new System.Numerics.Vector2(cellSizeX, cellSizeY));

                cursorX += _pixelSizeX + padding;
                if (cursorX + _pixelSizeX + padding > windowWidth)
                {
                    cursorX = 0.0f;
                    ImGui.NewLine();
                }
            }
        }

        private Texture2D CreateTextureFromColorArray(ColorPixel[,] colorArray)
        {
            int width = colorArray.GetLength(0);
            int height = colorArray.GetLength(1);

            Texture2D texture = new Texture2D(GameEditor.Instance.GraphicsDevice, width, height);

            Color[] colorData = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = colorArray[x, y];
                    colorData[y * width + x] = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            texture.SetData(colorData);
            return texture;
        }

        // Utilities
        private void AddTextureBound(TextureBound textureBound)
        {
            _texturesBound.Add(textureBound);
        }

        private TextureBound GetSpriteTextureBoundToSlice()
        {
            PropertyInfo[] properties = _component.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                _gameAsset = property.GetValue(_component) as GameAsset;

                if (_gameAsset != null)
                {
                    Texture2D texture = _gameAsset.Asset as Texture2D;
                    TextureBound textureBound = GetGameAssetTextureBound(texture);

                    if (textureBound == null)
                    {
                        // TODO : handle unbind
                        IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

                        textureBound = new TextureBound { IntPtr = ptr, Texture = texture };
                        return textureBound;
                    }
                }
            }

            return null;
        }

        private TextureBound GetGameAssetTextureBound(Texture2D texture)
        {
            foreach (var item in _texturesBound)
            {
                if (item.Texture == texture)
                    return item;
            }

            return null;
        }
    }

    public struct ColorPixel
    {
        public float R, G, B, A;

        public ColorPixel(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}