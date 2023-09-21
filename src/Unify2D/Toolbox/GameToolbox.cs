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
        readonly Vector2 _bottomOffset = new Vector2(0, 20);
        public readonly Vector2 WindowOffset = new Vector2(8, 27);

        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }

        public override void Show()
        {
            //todo separate SceneRenderTarget from GameEditor
            IntPtr renderTargetId = _editor.Renderer.BindTexture(_editor.SceneRenderTarget);

            ImGui.Begin("GAME", ImGuiWindowFlags.None);

            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin() - _bottomOffset;
            

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.Image(renderTargetId, ImGui.GetContentRegionAvail() - _bottomOffset);
            _editor.Circle();

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
            var mouseState = GetMousePosition();
            var mouse = Mouse.GetState();
            ImGui.Text($" {mouseState.X}:{mouseState.Y}");
            ImGui.PopStyleVar();

            ImGui.End();
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

            float x = mousePosition.X / Size.X;
            float y = mousePosition.Y / Size.Y;

            x = XnaF.MathHelper.Clamp(x, 0, 1);
            y = XnaF.MathHelper.Clamp(y, 0, 1);

            x *= _editor.GameResolution.X;
            y *= _editor.GameResolution.Y;

            x = MathF.Round(x);
            y = MathF.Round(y);

            return new XnaF.Vector2(x, y);
        }

    }
}
