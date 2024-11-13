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
        
        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        private void AddTextureBound(TextureBound textureBound)
        {
            _texturesBound.Add(textureBound);
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
                DrawSprite();
            }

            ImGui.End();
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
                    else
                    {
                        //Debug.Log($"Texture bound loaded");
                        ImGui.Image(textureBound.IntPtr,
                            new System.Numerics.Vector2(textureBound.Texture.Width, textureBound.Texture.Height));

                        //Debug.Log("Sprite Editor Toolbox: Texture bound changed!");
                    }
                }
            }
        }
        
        public void Open(Component component)
        {
            _component = component;
            _isOpen = true;
            DrawSprite();
        }
    }
}