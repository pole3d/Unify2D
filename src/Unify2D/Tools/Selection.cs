using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Unify2D.Assets;
using Unify2D.Core;
using UnifyCore;

namespace Unify2D
{
    /// <summary>
    /// The <see cref="Selection"/> class provides various Utility fonctions for selecting
    /// <see cref="GameObject"/>s or <see cref="GameAsset"/>s in the editor.
    /// </summary>
    public static class Selection
    {
        enum SelectedState
        {
            None,
            Empty,
            Select
        }
        private static SelectedState _selectState = SelectedState.None;

        public static object Selected
        {
            get
            {
                return _gameObject;
            }
            private set
            {
                _gameObject = value as GameObject;
                _asset = value as Asset;
            }
        }
        static GameObject _gameObject;
        static Asset _asset;
        static Vector2 _dragOffset;
        private static float _timeAtLastClick;

        public static bool TryGameObject(out GameObject go)
        {
            go = _gameObject;
            return go != null;
        }
        internal static bool TryAsset(out Asset asset)
        {
            asset = _asset;
            return asset != null;
        }
        public static void SelectObject(object obj)
        {
            if (Selected == obj) return;

            Selected = obj;

            if (obj == null)
            {
                UnSelectObject();
                return;
            }

            if (_asset != null)
            {
                if (_asset.AssetContent is ScriptAssetContent)
                {
                    GameEditor.Instance.ScriptToolbox.SetObject(_asset);
                    return;
                }
            }

            if (_gameObject != null)
            {

            }

            if (GameEditor.Instance.InspectorToolbox != null)
            {
                GameEditor.Instance.InspectorToolbox.SetObject(obj);
            }
        }
        public static void UnSelectObject()
        {
            Selected = null;

            if (GameEditor.Instance.InspectorToolbox != null)
            {
                GameEditor.Instance.InspectorToolbox.SetObject(null);
            }
        }

        internal static void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            if (_selectState == SelectedState.Empty && mouseState.LeftButton == ButtonState.Released)
            {
                _selectState = SelectedState.None;
            }
            else if (_selectState == SelectedState.None && mouseState.LeftButton == ButtonState.Pressed && GameEditor.Instance.IsMouseInGameWindow())
            {
                Vector2 mouseWorldPosition = GameEditor.Instance.GetWorldMousePosition();
                if (SceneManager.Instance.CurrentScene != null)
                {

                    foreach (var item in SceneManager.Instance.CurrentScene.GameObjectsWithChildren)
                    {
                        if (item.IsPointInBounds(mouseWorldPosition))
                        {
                            if (item == _gameObject && gameTime.TotalGameTime.Seconds - _timeAtLastClick < 0.5f)
                            {
                                // Double click, focus
                                GameEditor.Instance.GameToolbox.GoTo(item.Position);
                            }

                            SelectObject(item);
                            _selectState = SelectedState.Select;
                            _dragOffset = GameEditor.Instance.GetWorldMousePosition() - item.Position;

                            _timeAtLastClick = gameTime.TotalGameTime.Seconds;
                            break;
                        }
                    }
                }

                if (_selectState == SelectedState.None)
                {
                    _selectState = SelectedState.Empty;
                }
            }
            else if (_selectState == SelectedState.Select)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && _gameObject != null)
                {
                    _gameObject.Position = GameEditor.Instance.GetWorldMousePosition() - _dragOffset;
                }
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    _selectState = SelectedState.None;
                }
            }
        }

        public static void CircleSelected()
        {
            if (!TryGameObject(out GameObject go)) return;

            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(p0, p1);

            uint color;

            if (_selectState == SelectedState.Select)
            {
                color = ToolsUI.ToColor32(255, 255, 50, 255);
            }
            else
            {
                color = ToolsUI.ToColor32(50, 255, 50, 255);
            }

            drawList.AddCircle(GameEditor.Instance.GameToolbox.WorldToUI(go.Position),
                      8, color, 64, 3);
            drawList.PopClipRect();
        }
    }
}



