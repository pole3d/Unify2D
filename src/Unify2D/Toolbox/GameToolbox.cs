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
    /// <summary>
    /// The <see cref="GameToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface to visualize the game world, it provide a <see cref="CameraEditor">,
    /// and provides functionality to interact with gameobjects.
    /// </summary>
    internal class GameToolbox : Toolbox
    {
        static readonly List<Vector2> s_resolutions = new List<Vector2>()
        {
            new Vector2(1920, 1080),
            new Vector2(1366, 768 ),
            new Vector2(1440, 900 ),
            new Vector2(1280, 720 ),
            new Vector2(1280, 1024),
        };

        readonly Num.Vector2 _bottomOffset   = new Num.Vector2(0, 20);
        Vector2 _resolution = new Vector2(1920, 1080);

        public Vector2 Position { get; private set; }
        public Num.Vector2 UIPosition { get => new Num.Vector2(Position.X, Position.Y); private set => Position = new Vector2(value.X, value.Y); }
        public Vector2 CameraSize { get; private set; }
        public Num.Vector2 UICameraSize { get => new Num.Vector2(CameraSize.X, CameraSize.Y); private set => CameraSize = new Vector2(value.X, value.Y); }

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


        private void SetResolution(Vector2 resolution)
        {
            // set local and camera resolution
            _resolution = resolution;
            _gameCamera.Viewport = resolution;

            _editor.UnbindTexture(_sceneRenderTarget, _renderTargetId);

            // new texture
            _sceneRenderTarget = new RenderTarget2D(_editor.GraphicsDevice, (int)resolution.X, (int)resolution.Y);
            _renderTargetId = _editor.GuiRenderer.BindTexture(_sceneRenderTarget);

        }
        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            
            _gameCamera = new CameraEditor(new Vector2(_resolution.X, _resolution.Y), new Vector2(0, 0));

            _sceneRenderTarget = new RenderTarget2D(editor.GraphicsDevice, (int)_resolution.X, (int)_resolution.Y);
            _renderTargetId = _editor.GuiRenderer.BindTexture(_sceneRenderTarget);

            _gridTexture = new Texture2D(_editor.GraphicsDevice, 1, 1);
            _gridTexture.SetData(new Color[] { new Color(1, 1, 1, .5f) });

            _smallGridTexture = new Texture2D(_editor.GraphicsDevice, 1, 1);
            _smallGridTexture.SetData(new Color[] { new Color(1, 1, 1, .1f) });
        }
        public override void Update(GameTime gameTime)
        {
            Selection.Update(gameTime);
        }
        public override void Draw()
        {
            // Render target
            _editor.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _editor.GraphicsDevice.Clear(_gameCamera.Background);

            // clear la texture de render de la scéne
            ImGui.Begin("GAME", ImGuiWindowFlags.None);

            // Declare style
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, Num.Vector2.One);


            if (ImGui.MenuItem($"Resolution {_resolution}"))
            {
                ImGui.SetWindowPos("Resolution", ImGui.GetMousePos());
                _editor.ShowPopup(new ValuePopup<Vector2>("Resolution", s_resolutions, SetResolution));
            }

            // begin child because stupid ImGui doesn't calculate Menu height....
            ImGui.BeginChild("Camera", new Num.Vector2(), false, ImGuiWindowFlags.NoScrollbar);

            UIPosition = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();

            #region Camera Size

            Num.Vector2 space = ImGui.GetContentRegionAvail();

            float spaceX = space.X / _resolution.X;
            float spaceY = space.Y / _resolution.Y;

            if (spaceY >= spaceX)
            {
                space.Y = _resolution.Y * spaceX;
            }
            else
            {
                space.X = _resolution.X * spaceY;
            }
            UICameraSize = space;
            #endregion

            // Bind and give pointer to Scene render texture
            ImGui.Image(_renderTargetId, space);

            // Circle Gizmo around selected Game Object
            Selection.CircleSelected();

            #region Drag & Drop Asset
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("ASSET");
                    if (ptr.NativePtr != null)
                    {
                        Asset asset = Clipboard.Content as Asset;
                        GameObject go = GameObject.Create();
                        go.Name = asset.Name;
                        
                        go.Position = GetMousePosition();


                        Selection.SelectObject(go);

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

            // Write camera data
            ImGui.Text($"(X.{_lastMousePosition.X} Y.{_lastMousePosition.Y}) (Zoom.{MathF.Round(_gameCamera.ZoomLevel * 100) / 100}) (Angle.{_rotationAngle}°) (Pixel / Square : {_pixelPerGridSquare})");

            // Close
            ImGui.EndChild();

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
            Vector2 position = new Vector2(mouseState.X, mouseState.Y);

            position -= new Vector2(Position.X, Position.Y);
            position /= CameraSize;

            return position.X >= 0 && position.X <= 1 && position.Y >= 0 && position.Y <= 1;
        }
        public Vector2 GetMousePosition()
        {
            var mouseState = Mouse.GetState();

            float mouseX = mouseState.X;
            float mouseY = mouseState.Y;

            mouseX -= Position.X;
            mouseX /= CameraSize.X;
            mouseX *= _resolution.X;

            mouseY -= Position.Y;
            mouseY /= CameraSize.Y;
            mouseY *= _resolution.Y;

            Vector2 localPosition = _gameCamera.LocalToWorld(new Vector2(mouseX, mouseY));

            return new Vector2(MathF.Round(localPosition.X), MathF.Round(localPosition.Y));
        }
        public Num.Vector2 WorldToUI(Vector2 world)
        {
            Vector2 uiPos = _gameCamera.WorldToViewport(world);

            uiPos /= _resolution;
            uiPos *= CameraSize;
            uiPos += Position;

            return new Num.Vector2(uiPos.X, uiPos.Y);
        }
        internal void GoTo(Vector2 position)
        {
            _gameCamera.Position = position;
        }
    }
}
