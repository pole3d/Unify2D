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
using Microsoft.Xna.Framework;

using Num = System.Numerics;

namespace Unify2D.Toolbox
{
    internal class GameToolbox : Toolbox
    {
        readonly Num.Vector2 _gameResolution = new Num.Vector2(1920, 1080);
        readonly Num.Vector2 _bottomOffset   = new Num.Vector2(0, 20);

        public readonly Num.Vector2 WindowOffset = new Num.Vector2(8, 27);

        public Num.Vector2 Position { get; private set; }
        public Num.Vector2 Size { get; private set; }

        private RenderTarget2D _sceneRenderTarget;
        private Camera2D _gameCamera;


        private bool _movingCamera;
        private bool _dragInput;
        private Vector2 _lastMousePosition;

        private int _lastMouseSroll;
        private int _zoomLevel = 10;
        private int _rotationAngle;

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            
            _sceneRenderTarget = new RenderTarget2D(editor.GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);

            _gameCamera = new Camera2D(new Vector2(_gameResolution.X, _gameResolution.Y), new Vector2(_gameResolution.X / 2, _gameResolution.Y / 2));
        }
        public override void Draw()
        {
            // Render target
            _editor.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _editor.GraphicsDevice.Clear(Color.CornflowerBlue);

            // clear la texture de render de la scéne
            ImGui.Begin("GAME", ImGuiWindowFlags.None);

            // Windows property
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin() - _bottomOffset;

            // Declare style
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
            
            // Bind and give pointer to Scene render texture
            IntPtr renderTargetId = _editor.Renderer.BindTexture(_sceneRenderTarget);
            ImGui.Image(renderTargetId, ImGui.GetContentRegionAvail() - _bottomOffset);
            
            // Circle Gizmo around selected Game Object
            _editor.CircleSelected();

            #region Drag & Drop Asset
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("ASSET");
                    if (ptr.NativePtr != null)
                    {
                        Asset asset = Clipboard.DragContent as Asset;
                        asset?.AssetContent.OnDragDroppedInGame(_editor);
                        //
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
                    if(_zoomLevel < 0) _zoomLevel = 0;


                    // cache zoom and position of mous before zooming
                    float lastZoom = _gameCamera.Zoom;
                    Vector2 targetMove = GetMousePosition();

                    // zoom
                    _gameCamera.Zoom = (_zoomLevel < 1 ? -1f / (_zoomLevel - 2) : _zoomLevel) * 0.1f;

                    // difference between last zoom
                    float difference = (_gameCamera.Zoom - lastZoom) / lastZoom;

                    //relative to camera
                    targetMove -= _gameCamera.Position;

                    //move is equivalent to zoom
                    targetMove *= difference;

                    _gameCamera.Move(targetMove);
                }
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
            Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);
            mousePosition -= (Position + WindowOffset);

            Num.Vector2 result = new Num.Vector2(mousePosition.X, mousePosition.Y);
            result /= Size;

            return result.X >= 0 && result.X <= 1 && result.Y >= 0 && result.Y <= 1;
        }
        public Vector2 GetMousePosition()
        {
            var mouseState = Mouse.GetState();

            Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);

            mousePosition -= (Position + WindowOffset);
            mousePosition /= Size;
            mousePosition *= _gameResolution;

            Vector2 localPosition = _gameCamera.LocalToWorld(mousePosition);

            return new Vector2(MathF.Round(localPosition.X), MathF.Round(localPosition.Y));
        }
        public Num.Vector2 WorldToUI(Vector2 world)
        {
            Num.Vector2 uiPos = _gameCamera.WorldToViewport(world);

            uiPos /= _gameResolution;
            uiPos *= Size;
           
            return Position + WindowOffset + uiPos;
        }
    }
}
