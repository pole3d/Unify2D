using System;
using System.Collections.Generic;
using System.Reflection;
using Unify2D.Core;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

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

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        private void AddTextureBound(TextureBound textureBound)
        {
            _texturesBound.Add(textureBound);
            Debug.Log(_texturesBound.Count.ToString());
        }

        private TextureBound GetTextureBound(Texture2D texture)
        {
            foreach (var item in _texturesBound)
            {
                if (item.Texture == texture)
                    return item;
            }

            return null;
        }

        public override void Draw()
        {
            if (!_isOpen)
                return;

            if (ImGui.Begin("Sprite Editor Toolbox", ref _isOpen))
            {
                //DrawInputField();

                DrawSprite();
                if (ImGui.Button("Slice"))
                {
                    //SliceTexture();
                }

                DrawTexturePreviewWithGrid();
            }

            ImGui.End();
        }

        private void DrawInputField()
        {
            ImGui.InputInt("Pixel Size X", ref _pixelSizeX, 1, 1);
            ImGui.InputInt("Pixel Size Y", ref _pixelSizeY, 1, 1);

            _pixelSizeX = Math.Max(1, _pixelSizeX);
            _pixelSizeY = Math.Max(1, _pixelSizeY);

            if (ImGui.Button("Slice"))
            {
                //SliceTexture();
            }
        }

        private void DrawSprite()
        {
            PropertyInfo[] properties = _component.GetType().GetProperties();
            //Debug.Log($"Properties loaded");

            foreach (PropertyInfo property in properties)
            {
                _gameAsset = property.GetValue(_component) as GameAsset;

                //Debug.Log($"Game Asset loaded");

                if (_gameAsset != null)
                {
                    Texture2D texture = _gameAsset.Asset as Texture2D;
                    // if (texture != null)
                    //     Debug.Log($"Texture loaded");
                    // else
                    //     Debug.LogError($"Texture loading failed");

                    TextureBound textureBound = GetTextureBound(texture);

                    if (textureBound == null)
                    {
                        // TODO : handle unbind
                        IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

                        textureBound = new TextureBound { IntPtr = ptr, Texture = texture };
                        AddTextureBound(textureBound);
                    }
                    // else
                    // {
                    //     //Debug.Log($"Texture bound loaded");
                    //     ImGui.Image(textureBound.IntPtr,
                    //         new System.Numerics.Vector2(textureBound.Texture.Width, textureBound.Texture.Height));
                    //
                    //     //Debug.Log("Sprite Editor Toolbox: Texture bound changed!");
                    // }
                }
            }
        }

        // private void SliceTexture()
        // {
        //     byte[] imageData = LoadImageAsColorArray();
        //     
        //     _texture = ConvertToColorArray(imageData, _texturesBound[0].Texture.Width, _texturesBound[0].Texture.Height);
        //
        //     int textureWidth = _texture.GetLength(0);
        //     int textureHeight = _texture.GetLength(1);
        //
        //     int columns = textureWidth / _pixelSizeX;
        //     int rows = textureHeight / _pixelSizeY;
        //
        //     for (int y = 0; y < rows; y++)
        //     {
        //         for (int x = 0; x < columns; x++)
        //         {
        //             int startX = x * _pixelSizeX;
        //             int startY = y * _pixelSizeY;
        //
        //             ColorPixel[,] cell = new ColorPixel[_pixelSizeX, _pixelSizeY];
        //     
        //             for (int i = 0; i < _pixelSizeX; i++)
        //             {
        //                 for (int j = 0; j < _pixelSizeY; j++)
        //                 {
        //                     cell[i, j] = _texture[startX + i, startY + j];
        //                 }
        //             }
        //             _slicedTextures.Add(cell);
        //         }
        //     }
        //}

        public void Open(Component component)
        {
            _component = component;
            _isOpen = true;
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

        public ColorPixel[,] ConvertToColorArray(byte[] imageData, int width, int height)
        {
            // Initialise le tableau 2D pour stocker les couleurs de chaque pixel
            ColorPixel[,] colorArray = new ColorPixel[width, height];

            // Parcours des données brutes de l'image (4 octets par pixel : R, G, B, A)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4; // 4 canaux RGBA

                    // Convertit les octets (0-255) en valeurs flottantes (0-1)
                    float r = imageData[index] / 255f;
                    float g = imageData[index + 1] / 255f;
                    float b = imageData[index + 2] / 255f;
                    float a = imageData[index + 3] / 255f;

                    // Crée et stocke la couleur dans le tableau
                    colorArray[x, y] = new ColorPixel(r, g, b, a);
                }
            }

            return colorArray;
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