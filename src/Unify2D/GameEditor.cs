using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.ImGuiRenderer;
using Unify2D.Toolbox;
using Num = System.Numerics;

namespace Unify2D
{
    /// <summary>
    /// The main editor of Unify2D
    /// </summary>
    public class GameEditor : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer.Renderer _imGuiRenderer;

        private Texture2D _xnaTexture;
        private Texture2D _texture;
        private IntPtr _imGuiTexture;
        SpriteBatch spriteBatch;

        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();


        public GameEditor()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _toolboxes.Add(new AssetsToolbox());

            foreach (var item in _toolboxes)
            {
                item.Initialize();
            }

            _imGuiRenderer = new ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 60;
            _graphics.ApplyChanges();

            _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Texture loading example
            _texture = Content.Load<Texture2D>("Assets/joconde.png");

            // First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
            _xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
            {
                var red = (pixel % 300) / 2;
                return new Color(red, 1, 1);
            });

            // Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
            _imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            base.LoadContent();
        }

        IntPtr _renderTargetId = IntPtr.Zero;
        RenderTarget2D _sceneRenderTarget;


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_sceneRenderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(_texture, new Vector2(10, 10), Color.White);

            spriteBatch.End();

            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();

            //base.Draw(gameTime);

            _renderTargetId = _imGuiRenderer.BindTexture(_sceneRenderTarget);

            ImGui.Begin("GAME", ImGuiWindowFlags.None);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
            ImGui.Image(_renderTargetId, ImGui.GetContentRegionAvail());
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("DND_DEMO_CELL");
                    if (ptr.NativePtr != null)
                    {

                    }
                }


            }
            ImGui.EndDragDropTarget();
            ImGui.PopStyleVar();
            ImGui.End();

    

                    GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();

        }


        protected virtual void ImGuiLayout()
        {
            foreach (var item in _toolboxes)
            {
                item.Show();
            }

            ImGui.ShowDemoWindow();
        }

        public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
        {
            //initialize a texture
            var texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for (var pixel = 0; pixel < data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = paint(pixel);
            }

            //set the color
            texture.SetData(data);

            return texture;
        }
    }
}



