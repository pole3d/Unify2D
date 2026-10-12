using System;
using System.Linq;
using Unify2D.Assets;
using Unify2D.Core;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Unify2D.Runtime;
using Num = System.Numerics;

namespace Unify2D.Toolbox
{
    internal sealed class PlayToolbox : Toolbox
    {
        private PlaySession _session;
        private RenderTarget2D _target;
        private IntPtr _textureId;
        private bool _focused;
        private bool _hovered;
        private bool _requestFocus;
        private Num.Vector2 _imagePosition;
        private Num.Vector2 _imageSize;
        private string _error;
        private GameCore _editingCore;
        private GameCoreViewer _editingGameView;
        private GameCoreViewer _editingHierarchyView;
        private object _editingSelection;

        internal GameCore EditingCore => _editingCore;

        public bool IsPlaying => _session != null;
        public bool HasInputFocus => IsPlaying && _focused && _editor.IsActive;

        public void TogglePlay()
        {
            if (IsPlaying) Stop();
            else Start();
        }

        private void Start()
        {
            Stop();
            _error = null;
            try
            {
                var selected = Selection.Selected;
                _session = new PlaySession(_editor, _editor.SceneEditorManager.CurrentScene, selected as GameObject);
                _target = new RenderTarget2D(_editor.GraphicsDevice, 1920, 1080);
                _textureId = _editor.GuiRenderer.BindTexture(_target);
                _editingCore = GameCore.Current;
                _editingSelection = selected;
                _editingGameView = _editor.GameToolbox?.Tag as GameCoreViewer;
                _editingHierarchyView = _editor.HierarchyToolbox?.Tag as GameCoreViewer;
                var viewer = new GameCoreViewer(_session.Core, ".scene");
                _editor.SpriteEditorToolbox?.Close();
                _editor.GameToolbox?.SetCore(viewer);
                _editor.HierarchyToolbox?.SetCore(viewer);
                GameCore.SetCurrent(_session.Core);
                Selection.UnSelectObject();
                Selection.SelectObject(selected is GameObject ? _session.InitialSelection : selected);
                _requestFocus = true;
            }
            catch (Exception exception)
            {
                Stop();
                ReportError(exception);
            }
        }

        public void Stop()
        {
            if (_editingCore != null)
            {
                _editor.SpriteEditorToolbox?.Close();
                Selection.UnSelectObject();
                _editor.GameToolbox?.SetCore(_editingGameView);
                _editor.HierarchyToolbox?.SetCore(_editingHierarchyView);
                GameCore.SetCurrent(_editingCore);
                Selection.SelectObject(_editingSelection);
                _editingCore = null;
                _editingGameView = _editingHierarchyView = null;
                _editingSelection = null;
            }
            _session?.Dispose();
            _session = null;
            _focused = _hovered = false;
            if (_target != null)
            {
                _editor.UnbindTexture(_target, _textureId);
                _target = null;
                _textureId = IntPtr.Zero;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_session == null) return;
            try
            {
                var keyboard = HasInputFocus ? Keyboard.GetState() : default;
                var mouse = new MouseState(-1, -1, 0, ButtonState.Released, ButtonState.Released,
                    ButtonState.Released, ButtonState.Released, ButtonState.Released);
                if (HasInputFocus && _hovered && _imageSize.X > 0 && _imageSize.Y > 0)
                {
                    var position = (ImGui.GetMousePos() - _imagePosition) / _imageSize;
                    var state = Mouse.GetState();
                    mouse = new MouseState((int)(position.X * _target.Width), (int)(position.Y * _target.Height),
                        state.ScrollWheelValue, state.LeftButton, state.MiddleButton, state.RightButton,
                        state.XButton1, state.XButton2);
                }
                _session.Update((float)gameTime.ElapsedGameTime.TotalSeconds, keyboard, mouse);
                if (Selection.Selected is GameObject selected &&
                    _session.Core.SceneManager.CurrentScene?.GameObjectsWithChildren.Contains(selected) != true)
                    Selection.UnSelectObject();
            }
            catch (Exception exception)
            {
                _session.Paused = true;
                ReportError(exception);
            }
        }

        public override void Draw()
        {
            if (_requestFocus)
            {
                ImGui.SetNextWindowFocus();
                _requestFocus = false;
            }
            bool visible = ImGui.Begin("Play", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            _focused = visible && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
            _hovered = false;
            if (visible)
            {
                if (_session == null)
                {
                    if (ImGui.Button("Play (F5)")) Start();
                }
                else
                {
                    if (ImGui.Button("Stop (F5)")) Stop();
                    if (_session != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button(_session.Paused ? "Resume" : "Pause"))
                            _session.Paused = !_session.Paused;
                    }
                }

                if (_error != null) ImGui.TextWrapped(_error);
                if (_session != null)
                {
                    try { _session.Render(_target); }
                    catch (Exception exception)
                    {
                        Stop();
                        ReportError(exception);
                    }
                    if (_target != null)
                    {
                        var available = ImGui.GetContentRegionAvail();
                        float scale = Math.Min(available.X / _target.Width, available.Y / _target.Height);
                        if (scale > 0)
                        {
                            _imageSize = new Num.Vector2(_target.Width, _target.Height) * scale;
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (available.X - _imageSize.X) / 2);
                            _imagePosition = ImGui.GetCursorScreenPos();
                            ImGui.Image(_textureId, _imageSize);
                            _hovered = ImGui.IsItemHovered();
                        }
                    }
                }
            }
            ImGui.End();
        }

        private void ReportError(Exception exception)
        {
            _error = exception.GetBaseException().Message;
            Debug.LogError("Play: " + exception);
        }
    }
}
