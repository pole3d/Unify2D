using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal class PrefabAssetContent : AssetContent
    {
        private string _serializedText;
        
        public PrefabAssetContent(Asset asset)
        {
            _asset = asset;
        }
        Asset _asset;


        // public override OnDragDroppedInGame(GameEditor editor)
        public void OnDragDroppedInGame(GameEditor editor)
        {
            PrefabInstance pi = new PrefabInstance(_asset.FullPath);;
            ((GameCoreEditor)GameCore.Current).AddPrefabInstance(pi);
            Selection.SelectObject(pi.LinkedGameObject);
        }


        internal void Save(GameObject gameObject)
        {
            // Make so type name should be written in serialized data
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            // Write serialized data to file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText($"{_asset.FullPath}", _serializedText);
            Console.WriteLine($"Prefab {gameObject.Name} saved to {Path.GetFullPath(_asset.FullPath)}");
        }
    }
}