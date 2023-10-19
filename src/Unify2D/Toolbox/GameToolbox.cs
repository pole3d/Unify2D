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
        IntPtr _renderTargetId;
        private CameraEditor _gameCamera;


        private bool _movingCamera;
        private bool _dragInput;
        private Vector2 _lastMousePosition;

        private int _lastMouseSroll;
        private int _rotationAngle;


        private int _pixelPerGridSquare;
        private Texture2D _gridTexture;
        private Texture2D _smallGridTexture;
        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            
            _sceneRenderTarget = new RenderTarget2D(editor.GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);

            _gameCamera = new CameraEditor(new Vector2(_gameResolution.X, _gameResolution.Y), new Vector2(_gameResolution.X / 2, _gameResolution.Y / 2));

            _sceneRenderTarget = new RenderTarget2D(_editor.GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);
            _renderTargetId = _editor.Renderer.BindTexture(_sceneRenderTarget);

            _gridTexture = new Texture2D(_editor.GraphicsDevice, 1, 1);
            _gridTexture.SetData(new Color[] { new Color(1, 1, 1, .5f) });

            _smallGridTexture = new Texture2D(_editor.GraphicsDevice, 1, 1);
            _smallGridTexture.SetData(new Color[] { new Color(1, 1, 1, .1f) });
        }
        public override void Draw()
        {
            // Render target
            _editor.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _editor.GraphicsDevice.Clear(_gameCamera.Background);

            // clear la texture de render de la scéne
            ImGui.Begin("GAME", ImGuiWindowFlags.None);

            // Windows property
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin() - _bottomOffset;

            // Declare style
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
            
            // Bind and give pointer to Scene render texture
            ImGui.Image(_renderTargetId, ImGui.GetContentRegionAvail() - _bottomOffset);
            
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
                        Asset asset = Clipboard.Content as Asset;
                        GameObject go = new GameObject() { Name = asset.Name };
                        go.Position = GetMousePosition();

                        _editor.SelectObject(go);
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.Initialize(_editor, go, asset.FullPath);
                    }
                }
            }
            ImGui.EndDragDropTarget();
            #endregion

            //updates camera movement ect...
            UpdateCamera();

            #region Drawing
            _editor.GameCore.BeginDraw(_gameCamera.Matrix);

            //Draw the editor only grid
            DrawGrid();

            // Draw all game assets
            _editor.GameCore.Draw();
            // Draw all debutg gizmo
            _editor.GameCore.DrawGizmo();

            _editor.GameCore.EndDraw();
            #endregion

            // Write position in world
            ImGui.Text($"(X.{_lastMousePosition.X} Y.{_lastMousePosition.Y}) (Zoom.{MathF.Round(_gameCamera.ZoomLevel * 100) / 100}) (Angle.{_rotationAngle}°) (Pixel / Square : {_pixelPerGridSquare})");

            // Close
            ImGui.PopStyleVar();
            ImGui.End();
        }

        private void DrawGrid()
        {
            Vector2 viewPort = _gameCamera.Viewport / _gameCamera.Zoom;

            int step = 50;
            int width = (int)MathF.Round(5 * MathF.Max(_gameCamera.ZoomLevel, 0.2f));

            int rowX = (int)MathF.Round(viewPort.X / step);
            int rowY = (int)MathF.Round(viewPort.Y / step);

            int lowRowX = 0;
            int lowRowY = 0;
            int lowStep = 0;

            int multiple = 5;

            while (rowX > 16 || rowY > 9)
            {
                lowRowX = rowX + 1;
                lowRowY = rowY + 1;
                lowStep = step;

                step *= multiple;
                
                rowX /= multiple;
                rowY /= multiple;
            }
            _pixelPerGridSquare = step;
            rowX++;
            rowY++;


            int startX = (int)MathF.Round((_gameCamera.Position.X - viewPort.X / 2) / step);
            int startY = (int)MathF.Round((_gameCamera.Position.Y - viewPort.Y / 2) / step);

            if (lowRowX != 0 && lowRowX < 80)
            {
                int lowStartX = (int)MathF.Round((_gameCamera.Position.X - viewPort.X / 2) / lowStep);
                int lowStartY = (int)MathF.Round((_gameCamera.Position.Y - viewPort.Y / 2) / lowStep);

                DrawGrid(_smallGridTexture, lowStartX, lowStartY, lowRowX, lowRowY, lowStep, width, (int)viewPort.X, (int)viewPort.Y, multiple);
            }
            DrawGrid(_gridTexture, startX, startY, rowX, rowY, step, width, (int)viewPort.X, (int)viewPort.Y);
        }
        private void DrawGrid(Texture2D texture, int startX, int startY, int rowX, int rowY, int step, int width, int viewX, int viewY, int ignore = 1)
        {
            for (int x = 0; x <= rowX; x++)
            {
                if(ignore == 1 || (x + startX) % ignore != 0)
                {
                    Rectangle rectangle = new Rectangle(((startX + x) * step) - (width / 2), (startY - 1) * step, width, viewY + step * 2);
                    GameCore.Current.SpriteBatch.Draw(texture, rectangle, Color.Gray);
                }
            }

            for (int y = 0; y <= rowY; y++)
            {
                if (ignore == 1 || (y + startY) % ignore != 0)
                {
                    Rectangle rectangle = new Rectangle((startX - 1) * step, ((startY + y) * step) - (width / 2), viewX + step * 2, width);
                    GameCore.Current.SpriteBatch.Draw(texture, rectangle, Color.Gray);
                }
            }
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
                _gameCamera.Position += _lastMousePosition - GetMousePosition();
            }

            #endregion

            #region Zoom Camera

            if (mouseState.ScrollWheelValue != _lastMouseSroll & ! Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                if (IsMouseInWindow())
                {
                    // cache zoom and position of mous before zooming
                    float lastZoom = _gameCamera.Zoom;
                    Vector2 targetMove = GetMousePosition();

                    // zoom
                    float zoomLevel = _gameCamera.ZoomLevel;
                    float delta = (mouseState.ScrollWheelValue - _lastMouseSroll) / 120 * 0.1f;
                    if (zoomLevel < 1)
                    {
                        delta *= zoomLevel * zoomLevel;
                    }
                    _gameCamera.ZoomLevel -= delta;

                    // difference between last zoom
                    float difference = (_gameCamera.Zoom - lastZoom) / lastZoom;

                    //relative to camera
                    targetMove -= _gameCamera.Position;

                    //move is equivalent to zoom
                    targetMove *= difference;

                    _gameCamera.Position += targetMove;
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
