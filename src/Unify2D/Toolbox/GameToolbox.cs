using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Assets;
using Unify2D.Core.Graphics;
using Unify2D.Core;
using Unify2D.Tools;
using System.Numerics;

using XnaF = Microsoft.Xna.Framework;

namespace Unify2D.Toolbox
{
    internal class GameToolbox : Toolbox
    {
        readonly Vector2 _gameResolution = new Vector2(1920, 1080);
        readonly Vector2 _bottomOffset = new Vector2(0, 20);

        public readonly Vector2 WindowOffset = new Vector2(8, 27);

        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }

        private RenderTarget2D _sceneRenderTarget;
        private Camera2D _gameCamera;


        private bool _movingCamera;
        private bool _dragInput;
        private XnaF.Vector2 _lastMousePosition;

        private int _lastMouseSroll;
        private int _zoomLevel;
        private int _rotationAngle;

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            
            _sceneRenderTarget = new RenderTarget2D(editor.GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);
            _gameCamera = new Camera2D(editor.GraphicsDevice, new XnaF.Vector2(0, 0));
        }
        public override void Draw()
        {
            // Render target
            _editor.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _editor.GraphicsDevice.Clear(XnaF.Color.CornflowerBlue);

            // clear la texture de render de la scéne
            ImGui.Begin("GAME", ImGuiWindowFlags.None);

            // Windows property
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin() - _bottomOffset;

            // Declare style
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            
            // Bind and give pointer to Scene render texture
            IntPtr renderTargetId = _editor.Renderer.BindTexture(_sceneRenderTarget);
            ImGui.Image(renderTargetId, ImGui.GetContentRegionAvail() - _bottomOffset);
            
            // Circle Gizmo around selected Game Object
            _editor.CircleSelected();

            //Draw Component Gizmos for selected Game Object
            _editor.DrawComponentGizmoSelected();

            #region Drag & Drop Asset
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("ASSET");
                    if (ptr.NativePtr != null)
                    {
                        Asset asset = Clipboard.Content as Asset;
                        GameObject go = new GameObject() { Name = asset.Name };
                        _editor.SelectObject(go);
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.Initialize(_editor, go, asset.FullPath);
                    }
                }
            }
            ImGui.EndDragDropTarget();
            #endregion

            UpdateCamera();

            // Write position in world
            ImGui.Text($"(X.{_lastMousePosition.X} Y.{_lastMousePosition.Y}) (Zoom.{MathF.Round(_gameCamera.Zoom * 100) / 100}) (Angle.{_rotationAngle}°)");

            // Draw all game assets
            _editor.GameCore.Draw(_gameCamera.Matrix);

            // Close
            ImGui.PopStyleVar();
            ImGui.End();
        }
        private void UpdateCamera()
        {
            MouseState mouseState = Mouse.GetState();

            #region Move Camera
            
            if (mouseState.RightButton == ButtonState.Pressed | mouseState.MiddleButton == ButtonState.Pressed)
            {
                // si on viens de clicker et qu'on est dans la fenêtre
                if (!_dragInput && IsMouseInWindow())
                {
                    _movingCamera = true;
                    _lastMousePosition = GetMousePosition();
                }
                _dragInput = true;
            }
            else
            {
                // reset state vars
                _dragInput = false;
                _movingCamera = false;
            }

            if (_movingCamera)
            {
                // move camera
                _gameCamera.Move(_lastMousePosition - GetMousePosition());
            }

            #endregion

            #region Zoom Camera

            if (mouseState.ScrollWheelValue != _lastMouseSroll & ! Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                if (IsMouseInWindow())
                {
                    _zoomLevel += (mouseState.ScrollWheelValue - _lastMouseSroll) / 120;
                }

                _gameCamera.Zoom = _zoomLevel < 1 ? -1f / (_zoomLevel - 2) : _zoomLevel;
            }

            #endregion

            #region Rotation Camera

            if (mouseState.ScrollWheelValue != _lastMouseSroll & Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                if (IsMouseInWindow())
                {
                    _rotationAngle += (mouseState.ScrollWheelValue - _lastMouseSroll) / 120 * 15;
                }

                _gameCamera.RotationEuleur = _rotationAngle;
            }

            #endregion

            #region Update Last

            _lastMousePosition = GetMousePosition();
            _lastMouseSroll = mouseState.ScrollWheelValue;

            #endregion
        }

        public bool IsMouseInWindow()
        {
            var mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            mousePosition -= (Position + WindowOffset);

            Vector2 result = new Vector2(mousePosition.X, mousePosition.Y);
            result /= Size;

            return result.X >= 0 && result.X <= 1 && result.Y >= 0 && result.Y <= 1;
        }
        public XnaF.Vector2 GetMousePosition()
        {
            var mouseState = Mouse.GetState();

            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            mousePosition -= (Position + WindowOffset);
            mousePosition /= Size;
            mousePosition *= _gameResolution;

            XnaF.Vector2 localPosition = _gameCamera.LocalToWorld(mousePosition);

            return new XnaF.Vector2(MathF.Round(localPosition.X), MathF.Round(localPosition.Y));
        }
        public Vector2 WorldToUI(XnaF.Vector2 world)
        {
            Vector2 uiPos = _gameCamera.WorldToLocal(world);

            uiPos /= _gameResolution;
            uiPos *= Size;
           
            return Position + WindowOffset + uiPos;
        }
    }
}
