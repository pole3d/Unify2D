using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Unify2D.Core;

namespace Unify2D.Toolbox;

public class Texture2DPropertyViewer : AssetTypePropertyViewer<Texture2D>
{
    protected override Texture2D GetAssetFromPath(string path)
    {
        using (FileStream filestream = new FileStream(path, FileMode.Open))
        {
            Texture2D texture = Texture2D.FromStream(GameCore.Current.GraphicsDevice, filestream);
            return texture;
        }
    }

    protected override string GetPropertyName() => "Sprite";
    protected override string GetAssetExtension() => ".png";

    public override Texture2D GetInitializeAsset()
    {
        Texture2D baseRectangle = new Texture2D(GameCore.Current.GraphicsDevice, 100, 100);

        Color[] colors = new Color[100 * 100];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.White;
        }
        
        baseRectangle.SetData(colors);
        
        return baseRectangle;
    }

    public override (string name, string path) GetBaseAsset()
    {
        return ("Rectangle", "null");
    }
}