using System;
using System.Collections.Generic;
using System.Reflection;
using Unify2D.Core;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Unify2D.Assets;

namespace Unify2D.Toolbox
{
    public class SpriteEditorToolbox : Toolbox
    {
        private bool _isOpen = false;
        private GameAsset _gameAsset = null;
        private Component _component = null;

        private readonly List<TextureBound> _texturesBound = new List<TextureBound>();

        private int _pixelSizeX = 64;
        private int _pixelSizeY = 64;

        private ColorPixel[,] _texture;
        private List<ColorPixel[,]> _slicedTextures = new List<ColorPixel[,]>();

        private enum SliceMode
        {
            ByPixelSize,
            ByCellCount
        }

        private SliceMode _sliceMode = SliceMode.ByPixelSize;
        private int _pixelCountX = 64;
        private int _pixelCountY = 64;
        private int _cellCountX = 8;
        private int _cellCountY = 8;

        private int _spriteWidth = 0;
        private int _spriteHeight = 0;

        private float _imageOffsetY = 50.0f;

        private bool _isScaleLocked = false;
        private float _lockedScale = 1.0f;
        private float _zoomLevel = 1.0f;
        private float _minZoom = 0.1f;
        private float _maxZoom = 10.0f;

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
            _pixelCountX = _pixelSizeX;
            _pixelCountY = _pixelSizeY;
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
                    SaveSlicedTexture();
                }

                ImGui.SameLine();
                ImGui.Text("by :");
                
                ImGui.SameLine();
                if (ImGui.RadioButton("Pixel Size", _sliceMode == SliceMode.ByPixelSize))
                {
                    _sliceMode = SliceMode.ByPixelSize;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("Cell Count", _sliceMode == SliceMode.ByCellCount))
                {
                    _sliceMode = SliceMode.ByCellCount;
                }

                if (_sliceMode == SliceMode.ByPixelSize)
                {
                    ImGui.InputInt("Pixel Size X", ref _pixelCountX, 1, 1);
                    ImGui.InputInt("Pixel Size Y", ref _pixelCountY, 1, 1);
                    _pixelSizeX = Math.Max(1, _pixelCountX);
                    _pixelSizeY = Math.Max(1, _pixelCountY);
                }
                else if (_sliceMode == SliceMode.ByCellCount)
                {
                    ImGui.InputInt("Cell Count X", ref _cellCountX, 1, 1);
                    ImGui.InputInt("Cell Count Y", ref _cellCountY, 1, 1);
                    _cellCountX = Math.Max(1, _cellCountX);
                    _cellCountY = Math.Max(1, _cellCountY);

                    _pixelSizeX = _spriteWidth / _cellCountX;
                    _pixelSizeY = _spriteHeight / _cellCountY;
                }


                ImGui.Separator();

                DrawTexturePreviewWithGrid();

                if (_hasSlicedTextures)
                {
                    ImGui.Separator();
                    DrawSlicedTextures();
                }
            }

            ImGui.End();
        }

        private void DrawTexturePreviewWithGrid()
        {
            _spriteWidth = _texturesBound[0].Texture.Width;
            _spriteHeight = _texturesBound[0].Texture.Height;

            var availableSize = ImGui.GetContentRegionAvail();

            float scale = _isScaleLocked
                ? _lockedScale
                : Math.Min(availableSize.X / _spriteWidth, (availableSize.Y - _imageOffsetY) / _spriteHeight);

            scale *= _zoomLevel;

            float scaledWidth = _spriteWidth * scale;
            float scaledHeight = _spriteHeight * scale;

            if (ImGui.Button("-"))
            {
                _zoomLevel = Math.Max(_minZoom, _zoomLevel - 0.1f);
            }

            ImGui.SameLine();
            if (ImGui.Button("+"))
            {
                _zoomLevel = Math.Min(_maxZoom, _zoomLevel + 0.1f);
            }

            ImGui.SameLine();
            ImGui.Text($"Zoom: {_zoomLevel * 100:0}%");

            if (ImGui.Button("Lock Scale"))
            {
                _isScaleLocked = !_isScaleLocked;
                if (_isScaleLocked)
                {
                    _lockedScale = scale / _zoomLevel;
                }
            }

            ImGui.SameLine();
            ImGui.Text(_isScaleLocked ? "Scale Locked" : "Scale Unlocked");

            ImGui.Image(_texturesBound[0].IntPtr, new System.Numerics.Vector2(scaledWidth, scaledHeight));

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + _imageOffsetY);

            var imageMin = ImGui.GetItemRectMin();
            var imageMax = ImGui.GetItemRectMax();
            var drawList = ImGui.GetWindowDrawList();

            uint redColor = ImGui.GetColorU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            float lineThickness = 1.0f;

            float scaledPixelSizeX = _pixelSizeX * scale;
            float scaledPixelSizeY = _pixelSizeY * scale;

            for (int x = 0; x <= scaledWidth; x += (int)scaledPixelSizeX)
            {
                drawList.AddLine(
                    new System.Numerics.Vector2(imageMin.X + x, imageMin.Y),
                    new System.Numerics.Vector2(imageMin.X + x, imageMin.Y + scaledHeight),
                    redColor,
                    lineThickness
                );
            }

            for (int y = 0; y <= scaledHeight; y += (int)scaledPixelSizeY)
            {
                drawList.AddLine(
                    new System.Numerics.Vector2(imageMin.X, imageMin.Y + y),
                    new System.Numerics.Vector2(imageMin.X + scaledWidth, imageMin.Y + y),
                    redColor,
                    lineThickness
                );
            }
        }

        // Save Slice Texture
        private void SaveSlicedTexture()
        {
            for (var i = 0; i < _slicedTextures.Count; i++)
            {
                // Convert to texture2D
                Texture2D cellTexture = CreateTextureFromColorArray(_slicedTextures[i]);

                string subtextureAssetName = _gameAsset.Name.Split('.')[0];
                if (subtextureAssetName[0] == '\\')
                    subtextureAssetName = subtextureAssetName.Substring(1);
                
                Asset subTextureAsset = GameEditor.Instance.AssetManager.CreateAsset<SubTextureAssetContent>($"{subtextureAssetName}_{i}");
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

            int columns, rows;

            if (_sliceMode == SliceMode.ByPixelSize)
            {
                columns = textureWidth / _pixelCountX;
                rows = textureHeight / _pixelCountY;
            }
            else
            {
                columns = _cellCountX;
                rows = _cellCountY;

                _pixelSizeX = textureWidth / _cellCountX;
                _pixelSizeY = textureHeight / _cellCountY;
            }

            _slicedTextures.Clear();

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
            Debug.Log($"Slicing complete. {_slicedTextures.Count} cells created.");
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