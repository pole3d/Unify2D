using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Unify2D.Assets;
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
        private Camera2D _gameCamera;

        private bool _movingCamera;
        private bool _dragInput;
        private Vector2 _lastMousePosition;

        private int _lastMouseSroll;
        private int _zoomLevel = 10;
        private int _rotationAngle;

        private int _pixelPerGridSquare;
        
        SelectedState _selectState;
        enum SelectedState
        {
            None,
            Select,
            Drag
        }
        
        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            
            _sceneRenderTarget = new RenderTarget2D(editor.GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);

            _gameCamera = new Camera2D(new Vector2(_gameResolution.X, _gameResolution.Y), new Vector2(_gameResolution.X / 2, _gameResolution.Y / 2));

            _sceneRenderTarget = new RenderTarget2D(_editor.GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);
            _renderTargetId = _editor.GuiRenderer.BindTexture(_sceneRenderTarget);
        }
        
        public void SetCore(GameCoreInfo coreInfo)
        {
            _tag = coreInfo;
        }

        public override void Update()
        {
            var mouseState = Mouse.GetState();

            if (_selectState == SelectedState.None && IsMouseInWindow())
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    Vector2 worldPosition = GetMousePosition();

                    foreach (var item in ((GameCoreInfo)_tag).GameCore.GameObjects)
                    {
                        if (worldPosition.X >= item.Position.X - item.BoundingSize.X / 2 && worldPosition.X <= item.Position.X + item.BoundingSize.X / 2
                            && worldPosition.Y >= item.Position.Y - item.BoundingSize.Y / 2 && worldPosition.Y <= item.Position.Y + item.BoundingSize.Y / 2)
                        {
                            _editor.SelectObject(item);

                            Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);
                            Num.Vector2 goPosition = WorldToUI(item.Position);

                            Num.Vector2 direction = mousePosition - goPosition;
                            if (direction.Length() < 10)
                            {
                                _selectState = SelectedState.Drag;
                            }
                        }
                    }
                }
            }
            else if (_selectState == SelectedState.Drag)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && _editor.Selected != null)
                {
                    _editor.Selected.Position = GetMousePosition();
                }
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    _selectState = SelectedState.None;
                }
            }
        }

        public void CircleSelected()
        {
            if (_editor.Selected == null)
                return;

            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(p0, p1);

            uint color = ToolsUI.ToColor32(50, 255, 50, 255);

            if (_selectState == SelectedState.Drag)
            {
                color = ToolsUI.ToColor32(255, 255, 50, 255);
            }

            drawList.AddCircle(WorldToUI(_editor.Selected.Position),
                      8, color, 64, 3);
            drawList.PopClipRect();
        }
        
        public override void Draw()
        {
            // Render target
            _editor.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _editor.GraphicsDevice.Clear(Color.CornflowerBlue);

            // clear la texture de render de la scéne
            ImGui.Begin($"GAME - {((GameCoreInfo)_tag).AssetPath}", ImGuiWindowFlags.None);

            // Windows property
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin() - _bottomOffset;

            // Declare style
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
            
            // Bind and give pointer to Scene render texture
            ImGui.Image(_renderTargetId, ImGui.GetContentRegionAvail() - _bottomOffset);
            
            // Circle Gizmo around selected Game Object
            CircleSelected();

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
                    }
                }
                ImGui.EndDragDropTarget();
            }
            #endregion

            //updates camera movement ect...
            UpdateCamera();

            #region Drawing
            ((GameCoreInfo)_tag).GameCore.BeginDraw(_gameCamera.Matrix);

            //Draw the editor only grid
            DrawGrid();

            // Draw all game assets
            ((GameCoreInfo)_tag).GameCore.Draw();

            ((GameCoreInfo)_tag).GameCore.EndDraw();
            #endregion

            // Write position in world
            ImGui.Text($"(X.{_lastMousePosition.X} Y.{_lastMousePosition.Y}) (Zoom.{MathF.Round(_gameCamera.Zoom * 100) / 100}) (Angle.{_rotationAngle}°) (Pixel / Square : {_pixelPerGridSquare})");
            
            // Close
            ImGui.PopStyleVar();
            ImGui.End();
        }

        private void DrawGrid()
        {
            Vector2 viewPort = _gameCamera.Resolution / _gameCamera.Zoom;

            int step = 20;
            int width = 1;

            int rowX = (int)MathF.Round(viewPort.X / step);
            int rowY = (int)MathF.Round(viewPort.Y / step);

            int lowRowX = 0;
            int lowRowY = 0;
            int lowStep = 0;
            int lowWidth = 0;

            int multiple = 5;

            while (rowX > 16 || rowY > 9)
            {
                lowRowX = rowX + 1;
                lowRowY = rowY + 1;
                lowStep = step;
                lowWidth = width;

                step *= multiple;
                width *= multiple;
                
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

                DrawGrid(1 - (lowRowX / (32f * multiple)), lowStartX, lowStartY, lowRowX, lowRowY, lowStep, lowWidth, (int)viewPort.X, (int)viewPort.Y, multiple);
            }
            DrawGrid(rowX / 16f, startX, startY, rowX, rowY, step, width, (int)viewPort.X, (int)viewPort.Y);
        }
        private void DrawGrid(float opacity, int startX, int startY, int rowX, int rowY, int step, int width, int viewX, int viewY, int ignore = 1)
        {
            Texture2D texture1px = new Texture2D(_editor.GraphicsDevice, 1, 1);
            texture1px.SetData(new Color[] { new Color(1, 1, 1, opacity) });

            for (int x = 0; x <= rowX; x++)
            {
                if(ignore == 1 || (x + startX) % ignore != 0)
                {
                    Rectangle rectangle = new Rectangle(((startX + x) * step) - (width / 2), (startY - 1) * step, width, viewY + step * 2);
                    GameCore.Current.SpriteBatch.Draw(texture1px, rectangle, Color.Gray);
                }
            }

            for (int y = 0; y <= rowY; y++)
            {
                if (ignore == 1 || (y + startY) % ignore != 0)
                {
                    Rectangle rectangle = new Rectangle((startX - 1) * step, ((startY + y) * step) - (width / 2), viewX + step * 2, width);
                    GameCore.Current.SpriteBatch.Draw(texture1px, rectangle, Color.Gray);
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
