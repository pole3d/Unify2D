using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Unify2D.Core;

namespace Unify2D
{
    public class GameCoreEditor : GameCore
    {
        private List<PrefabInstance> _prefabInstances = new List<PrefabInstance>();

        public GameCoreEditor(Game game) : base(game) { }
    
        // public void AddPrefabInstance(PrefabInstance pi)
        // {
        //     _prefabInstances.Add(pi);
        //     if (pi.LinkedGameObject == null) {
        //         pi.InstantiateAndLinkGameObject();
        //     }
        // }
        //
        // public override void DestroyImmediate(GameObject item) {
        //     base.DestroyImmediate(item);
        //     if (item.PrefabInstance != null)
        //         _prefabInstances.Remove(item.PrefabInstance);
        // }
        //
        // public override void LoadScene(Game game, SceneData data) {
        //     base.LoadScene(game, data);
        //
        //     _prefabInstances.Clear();
        //     foreach (PrefabInstance item in data.PrefabInstances)
        //     {
        //         AddPrefabInstance(item);
        //     }
        // }
        //
        // public SceneData GetSceneData() {
        //     // Set Name property of prefab instances
        //     foreach (PrefabInstance prefabInstance in _prefabInstances)
        //     {
        //         if (string.IsNullOrEmpty(prefabInstance.Name) && prefabInstance.LinkedGameObject != null)
        //             prefabInstance.Name = prefabInstance.LinkedGameObject.Name;
        //     }
        //     // Don't serialize gameObjects from prefab instances
        //     List<GameObject> gameObjects = new List<GameObject>(_gameObjects);
        //     for (int i = gameObjects.Count - 1; i >= 0; i--) {
        //         if (gameObjects[i].PrefabInstance != null)
        //             gameObjects.RemoveAt(i);
        //     }
        //     
        //     return new SceneData(gameObjects, _prefabInstances);
        // }
    }
}