using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Unify2D;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.Physics;
using Unify2D.Runtime;
using Unify2D.Scripting;
using UnifyCore.Core.Tweens;
using Unify2D.Toolbox;
using ImGuiNET;
using Unify2D.Assets;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS " + message);
}
static void Set(object target, string field, object value) =>
    typeof(GameEditor).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

var project = Path.Combine(Path.GetTempPath(), "UnifyPlayTests-" + Guid.NewGuid());
Directory.CreateDirectory(Path.Combine(project, "Assets"));
File.WriteAllText(Path.Combine(project, "Assets", "Mover.cs"), """
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Unify2D.Core;
public class Mover : Component
{
    public static int Starts;
    public int Speed = 3;
    public GameObject Target;
    public bool Throw;
    public override void Load(Game game) { Starts++; }
    public override void Update(GameCore game)
    {
        if (Throw) throw new Exception("Script failure");
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
            GameObject.LocalPosition += new Vector2(Speed, 0);
    }
}
"""
);

// The FNA window remains hidden: no Game.Run() or platform show-window call.
using var editor = new GameEditor();
var settings = new GameEditorSettings();
typeof(GameEditorSettings).GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic)!
    .SetValue(settings, new GameEditorSettingsData { CurrentProjectPath = project });
Set(editor, "_settings", settings);
editor.Content.RootDirectory = project;
((IGraphicsDeviceManager)editor.Services.GetService(typeof(IGraphicsDeviceManager))).CreateDevice();
var editorCore = new GameCore(editor);
GameCore.SetCurrent(editorCore);
editorCore.Initialize(editor.GraphicsDevice);
var scene = new Scene("Unsaved scene", Path.Combine(project, "unsaved.scene"));
editorCore.SceneManager.SetActiveScene(scene);
var scripts = new Scripting();
Set(editor, "_scripting", scripts);
scripts.Load(editor);
var moverType = scripts.GetTypes().Single(type => type.Name == "Mover");
var go = GameObject.Create();
go.Name = "Player";
var sprite = go.AddComponent<SpriteRenderer>();
sprite.Color = Color.Red;
scripts.TryAddComponent(go, moverType);
var mover = go.Components.Single(component => component.GetType() == moverType);
moverType.GetField("Speed")!.SetValue(mover, 9);
var child = GameObject.CreateChild(go);
child.Name = "Child";
moverType.GetField("Target")!.SetValue(mover, child);
var camera = GameObject.Create().AddComponent<Camera>();
camera.Background = Color.Blue;
var falling = GameObject.Create();
falling.Name = "Falling";
scripts.TryAddComponent(falling, scripts.GetTypes().Single(type => type.Name == "Rigidbody"));
var editorWorld = PhysicsSettings.World;
var editorTweens = TweenManager.Instance;

[MethodImpl(MethodImplOptions.NoInlining)]
static WeakReference RunSession(GameEditor editor, GameCore editorCore, Scene scene, GameObject original, Type editorMoverType)
{
    using var session = new PlaySession(editor, scene);
    Check(GameCore.Current == editorCore && SceneManager.Instance.CurrentScene == scene, "Play restores editor context after startup");
    var copy = session.Core.SceneManager.CurrentScene;
    var runtimeObject = copy.GameObjects.Single(obj => obj.Name == "Player");
    var runtimeMover = runtimeObject.Components.Single(component => component.GetType().Name == "Mover");
    var runtimeType = runtimeMover.GetType();
    Check(runtimeObject != original && runtimeType != editorMoverType, "Scene and script types are isolated");
    Check(runtimeType.GetField("Starts")!.GetValue(null) is 1, "Play starts with fresh script statics");
    Check(editorMoverType.GetField("Starts")!.GetValue(null) is 0, "Editor script statics are untouched");
    Check(runtimeType.GetField("Speed")!.GetValue(runtimeMover) is 9, "Unsaved component fields are copied");
    Check(runtimeType.GetField("Target")!.GetValue(runtimeMover) == runtimeObject.Children.Single(), "Object references point into the copied scene");
    Check(runtimeObject.Children.Single().Parent == runtimeObject, "Child hierarchy survives cloning");
    using (session.Enter())
    {
        Check(SceneManager.Instance != editorCore.SceneManager, "Scene manager is isolated");
        Check(Camera.Main != editorCore.MainCamera, "Main camera is isolated");
        Check(TweenManager.Instance != editorCore.Tweens, "Tweens are isolated");
        Check(Unify2D.Builder.GameSettings.Instance != editorCore.Settings, "Scene catalog is isolated");
        var playWorld = PhysicsSettings.World;
        using (new RestoreCore(editorCore))
            Check(PhysicsSettings.World != playWorld, "Physics worlds are isolated");
        TweenManager.Move(runtimeObject, new Vector2(20, 0), 1);
    }
    session.Update(1f / 60f, new KeyboardState(Keys.Right), default);
    Check(runtimeObject.Position.X != 0 && original.Position.X == 0, "Only runtime objects move");
    Check(!Keyboard.StateOverrideEXT.HasValue && !Mouse.StateOverrideEXT.HasValue, "Input overrides are restored");
    var beforePause = runtimeObject.Position;
    var falling = copy.GameObjects.Single(obj => obj.Name == "Falling");
    for (int i = 0; i < 4; i++) session.Update(1f / 60f, default, default);
    Check(falling.Position.Y > 0, "Runtime physics simulates bodies");
    falling.LocalPosition = new Vector2(75, -100);
    falling.LocalRotation = 0.4f;
    session.Update(1f / 60f, default, default);
    Check(Vector2.Distance(falling.Position, new Vector2(75, -100)) < 0.01f &&
        Math.Abs(falling.Rotation - 0.4f) < 0.001f, "Manual transform edits move the live physics body");
    beforePause = runtimeObject.Position;
    var beforePauseBody = falling.Position;
    session.Paused = true;
    session.Update(1, new KeyboardState(Keys.Right), default);
    Check(runtimeObject.Position == beforePause, "Pause freezes scripts and tweens");
    Check(falling.Position == beforePauseBody, "Pause freezes physics");
    using var target = new RenderTarget2D(editor.GraphicsDevice, 320, 180);
    session.Render(target);
    Check(runtimeObject.Position == beforePause, "Rendering does not advance paused simulation");
    var pixels = new Color[320 * 180];
    target.GetData(pixels);
    Check(pixels.Contains(Color.Red) && pixels.Contains(Color.Blue), "Camera and sprite render into the embedded view");
    session.Paused = false;
    runtimeType.GetField("Throw")!.SetValue(runtimeMover, true);
    try { session.Update(1f / 60f, default, default); throw new Exception("Expected script failure"); }
    catch (Exception exception) when (exception.Message == "Script failure") { }
    Check(GameCore.Current == editorCore && !Keyboard.StateOverrideEXT.HasValue && !Mouse.StateOverrideEXT.HasValue,
        "Script exception restores editor context and input");
    return new WeakReference(AssemblyLoadContext.GetLoadContext(runtimeType.Assembly));
}

var context = RunSession(editor, editorCore, scene, go, moverType);
Check(Camera.Main == camera && PhysicsSettings.World == editorWorld && TweenManager.Instance == editorTweens,
    "Stop preserves editor camera, physics and tweens");
Check(scene.GameObjects.Contains(go) && go.Position == Vector2.Zero && !File.Exists(scene.Path),
    "Stop preserves unsaved scene without writing it to disk");
RunSession(editor, editorCore, scene, go, moverType);
for (int i = 0; i < 10 && context.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }
Check(!context.IsAlive, "Stopped script assembly can be unloaded");

var gui = new Unify2D.ImGuiRenderer.Renderer(editor);
gui.RebuildFontAtlas();
Set(editor, "_imGuiRenderer", gui);
var playToolbox = new PlayToolbox();
playToolbox.Initialize(editor);
var gameToolbox = new GameToolbox();
gameToolbox.SetCore(new GameCoreViewer(editorCore, ".scene"));
gameToolbox.Initialize(editor);
Set(editor, "<GameToolbox>k__BackingField", gameToolbox);
Set(editor, "<PlayToolbox>k__BackingField", playToolbox);
var hierarchy = new HierarchyToolbox();
hierarchy.Initialize(editor);
hierarchy.SetCore((GameCoreViewer)gameToolbox.Tag);
Set(editor, "<HierarchyToolbox>k__BackingField", hierarchy);
var inspector = new InspectorToolbox();
inspector.Initialize(editor);
Set(editor, "<InspectorToolbox>k__BackingField", inspector);
Selection.SelectObject(go);
playToolbox.TogglePlay();
Check(playToolbox.IsPlaying, "Play toolbox starts an embedded session");
Check(GameCore.Current.IsPlaySession && ((GameCoreViewer)gameToolbox.Tag).GameCore == GameCore.Current &&
    ((GameCoreViewer)hierarchy.Tag).GameCore == GameCore.Current, "Hierarchy and editing view use the live Play core");
Check(inspector.GameObject == Selection.Selected && inspector.GameObject != go && inspector.GameObject.Name == go.Name,
    "Selection transfers to the same live object used by the inspector");
inspector.GameObject.LocalPosition = new Vector2(100, 20);
Check(SceneManager.Instance.CurrentScene.GameObjects.Contains(inspector.GameObject) && go.Position == Vector2.Zero,
    "Inspector edits the live scene directly while the original stays unchanged");
var liveCore = GameCore.Current;
var liveScene = SceneManager.Instance.CurrentScene;
var runtimeMoverType = scripts.GetTypes().Single(type => type.Name == "Mover");
var created = GameObject.Create();
Check(scripts.TryAddComponent(created, runtimeMoverType) && runtimeMoverType.GetField("Starts")!.GetValue(null) is 2,
    "Components added during Play use the runtime assembly and run Load");
File.AppendAllText(Path.Combine(project, "Assets", "Mover.cs"), "\n// Saved during Play\n");
scripts.Reload();
Check(GameCore.Current == liveCore && scripts.GetTypes().Contains(runtimeMoverType) &&
    go.Components.Single(component => component.GetType().Name == "Mover").GetType() != moverType,
    "Script reload updates the editing scene without replacing live script instances");
editor.GameEditorUI.SaveCurrentScene();
Check(!File.Exists(scene.Path), "Saving during Play does not write runtime changes to disk");
gui.BeforeLayout(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1d / 60)));
hierarchy.Draw();
inspector.Draw();
ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 400));
gameToolbox.Draw();
var gameTarget = (RenderTarget2D)typeof(GameToolbox)
    .GetField("_sceneRenderTarget", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(gameToolbox)!;
var gamePixelsBeforePlay = new Color[gameTarget.Width * gameTarget.Height];
gameTarget.GetData(gamePixelsBeforePlay);
ImGui.SetNextWindowPos(new System.Numerics.Vector2(400, 0));
ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 400));
playToolbox.Draw();
var gamePixelsAfterPlay = new Color[gamePixelsBeforePlay.Length];
gameTarget.GetData(gamePixelsAfterPlay);
Check(gamePixelsBeforePlay.SequenceEqual(gamePixelsAfterPlay), "Game view pixels survive rendering Play alongside it");
Check(editor.GraphicsDevice.GetRenderTargets().Length == 0, "Both toolboxes restore the backbuffer after drawing");
editor.GraphicsDevice.SetRenderTarget(null);
gui.AfterLayout();
Check(playToolbox.IsPlaying, "Play toolbox renders its ImGui controls and game image");
Check(inspector.GameObject.LocalPosition == new Vector2(100, 20), "Drawing the live inspector preserves the current transform");
liveScene.DestroyImmediate((GameObject)Selection.Selected);
playToolbox.Update(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1d / 60)));
Check(Selection.Selected == null && inspector.GameObject == null, "Destroyed runtime selection is cleared from the inspector");
playToolbox.TogglePlay();
Check(!playToolbox.IsPlaying, "Play toolbox stops the session");
Check(GameCore.Current == editorCore && Selection.Selected == go && inspector.GameObject == go &&
    ((GameCoreViewer)gameToolbox.Tag).GameCore == editorCore && ((GameCoreViewer)hierarchy.Tag).GameCore == editorCore,
    "Stop restores the original scene, views and selection");
Check(go.Position == Vector2.Zero && !scene.GameObjects.Contains(created), "Live edits and newly created objects are discarded on Stop");

using (var emptyPlay = new PlaySession(editor, new Scene("Empty", "")))
{
    using (emptyPlay.Enter()) Check(Camera.Main != null, "Empty scenes receive a preview camera");
}
File.WriteAllText(Path.Combine(project, "Assets", "Broken.cs"), "invalid C#");
try { using var broken = new PlaySession(editor, scene); throw new Exception("Expected compile failure"); }
catch (InvalidOperationException exception) when (exception.Message.Contains("compilation")) { }
Check(GameCore.Current == editorCore && SceneManager.Instance.CurrentScene == scene,
    "Failed startup leaves the editing scene intact");
scripts.Unload();
editorCore.ResourcesManager.Dispose();
editorCore.SpriteBatch.Dispose();
Console.WriteLine("All Play session checks passed.");

sealed class RestoreCore : IDisposable
{
    readonly GameCore previous = GameCore.Current;
    public RestoreCore(GameCore core) => GameCore.SetCurrent(core);
    public void Dispose() => GameCore.SetCurrent(previous);
}
