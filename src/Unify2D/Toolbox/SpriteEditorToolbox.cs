using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Unify2D.Core;

namespace Unify2D.Toolbox;

public class SpriteEditorToolbox : Toolbox
{
    private static bool _isOpen = false;

    public GameObject GameObject => _gameObject;
    private GameObject _gameObject;

    public Color Color { get; set; } = Color.White;
    public float LayerDepth { get; set; } = 0f;
    [JsonIgnore] public GameAsset Asset => _asset;

    [JsonProperty] GameAsset _asset;
    Texture2D _texture;

    public void Initialize(Game game, GameObject go, string texturePath)
    {
        _gameObject = go;
        try
        {
            _texture = game.Content.Load<Texture2D>($"./Assets/{texturePath}");
            _gameObject.BoundingSize = new Vector2(_texture.Width, _texture.Height);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public override void Draw()
    {
        if (!_isOpen)
            return;

        if (ImGui.Begin("Sprite Editor Toolbox", ref _isOpen))
        {
        }

        if (_texture != null)
        {
            GameCore.Current.SpriteBatch.Draw(_texture, _gameObject.Position,
                null, Color, _gameObject.Rotation, _gameObject.BoundingSize / 2, _gameObject.Scale,
                SpriteEffects.None, LayerDepth);
        }

        ImGui.End();
    }

    public static void Open()
    {
        _isOpen = true;
    }
}