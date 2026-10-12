using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Unify2D.Builder;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.Physics;

namespace Unify2D.Runtime
{
    internal sealed class PlaySession : IDisposable
    {
        private PlayScriptAssembly _scripts;
        public GameCore Core { get; private set; }
        public bool Paused { get; set; }
        public GameObject InitialSelection { get; private set; }

        public PlaySession(GameEditor editor, Scene source, GameObject selected = null)
        {
            if (source == null) throw new InvalidOperationException("Open a scene before starting Play.");
            try
            {
                _scripts = editor.Scripting.CreatePlayAssembly();
                var settings = new GameSettings();
                settings.ScenesInGame.AddRange(GameSettings.Instance.ScenesInGame);
                settings.ScenesSave.AddRange(GameSettings.Instance.ScenesSave.Select(scene =>
                    new SceneInfo(scene.Name, scene.Path) { BuildIndex = scene.BuildIndex }));
                Core = new GameCore(editor, new SceneManager(), settings)
                {
                    IsPlaySession = true,
                    SerializationBinder = _scripts
                };

                using (Enter())
                {
                    Core.Initialize(editor.GraphicsDevice);
                    Core.AssetsManager.Initialize(editor.AssetsPath);
                    var cloner = new SceneCloner(_scripts);
                    var copy = new Scene(source.Name, source.Path);
                    copy.GameObjects.AddRange((List<GameObject>)cloner.Copy(source.GameObjects));
                    InitialSelection = cloner.FindCopy(selected) as GameObject;
                    Core.SceneManager.SetActiveScene(copy);
                    copy.Initialize();
                    if (Camera.Main == null)
                    {
                        var camera = GameObject.Create();
                        camera.Name = "Play Camera";
                        camera.AddComponent<Camera>();
                    }
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal static Scene CloneScene(Scene source, ISerializationBinder binder)
        {
            var cloner = new SceneCloner(binder);
            var objects = (List<GameObject>)cloner.Copy(source.GameObjects);
            var copy = new Scene(source.Name, source.Path);
            copy.GameObjects.AddRange(objects);
            return copy;
        }

        private sealed class SceneCloner
        {
            private readonly ISerializationBinder _binder;
            private readonly DefaultContractResolver _resolver = new DefaultContractResolver();
            private readonly Dictionary<object, object> _copies = new(ReferenceEqualityComparer.Instance);

            public SceneCloner(ISerializationBinder binder) => _binder = binder;
            public object FindCopy(object source) => source != null && _copies.TryGetValue(source, out var copy) ? copy : null;

            public object Copy(object source)
            {
                if (source == null) return null;
                var sourceType = source.GetType();
                if (sourceType.IsPrimitive || source is string || source is decimal || source is DateTime ||
                    source is TimeSpan || source is Guid) return source;
                if (sourceType.IsValueType && sourceType.Assembly == typeof(Vector2).Assembly)
                    return source;
                if (_copies.TryGetValue(source, out var existing)) return existing;

                var type = _binder.BindToType(sourceType.Assembly.GetName().Name, sourceType.FullName);
                if (sourceType.IsEnum) return Enum.ToObject(type, source);
                if (source is GameAsset asset)
                {
                    var copy = new GameAsset(asset.GUID, null, asset.Name, asset.Path);
                    _copies[source] = copy;
                    return copy;
                }
                if (source is Array array)
                {
                    if (array.Rank != 1) throw new InvalidOperationException("Play supports one-dimensional serialized arrays.");
                    var copy = Array.CreateInstance(type.GetElementType(), array.Length);
                    _copies[source] = copy;
                    for (int i = 0; i < array.Length; i++) copy.SetValue(Copy(array.GetValue(i)), i);
                    return copy;
                }

                var target = type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null
                    ? Activator.CreateInstance(type) : RuntimeHelpers.GetUninitializedObject(type);
                _copies[source] = target;
                if (source is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary)
                        ((IDictionary)target).Add(Copy(entry.Key), Copy(entry.Value));
                }
                else if (source is IList list)
                {
                    foreach (var item in list) ((IList)target).Add(Copy(item));
                }
                else
                {
                    // Inspect only editor types: Json.NET caches must never see collectible Play types.
                    var contract = (JsonObjectContract)_resolver.ResolveContract(sourceType);
                    foreach (var property in contract.Properties.Where(p => p.Readable && p.Writable && !p.Ignored))
                    {
                        var value = Copy(property.ValueProvider.GetValue(source));
                        var declaringType = _binder.BindToType(property.DeclaringType.Assembly.GetName().Name,
                            property.DeclaringType.FullName);
                        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                        var field = declaringType.GetField(property.UnderlyingName, flags);
                        if (field != null) field.SetValue(target, value);
                        else declaringType.GetProperty(property.UnderlyingName, flags)?.SetValue(target, value);
                    }
                }
                return target;
            }
        }

        public IDisposable Enter() => new CoreScope(Core);

        public void Update(float elapsed, KeyboardState keyboard, MouseState mouse)
        {
            if (Paused) return;
            using (Enter())
            {
                var oldKeyboard = Keyboard.StateOverrideEXT;
                var oldMouse = Mouse.StateOverrideEXT;
                try
                {
                    Keyboard.StateOverrideEXT = keyboard;
                    Mouse.StateOverrideEXT = mouse;
                    // Avoid large simulation jumps after a breakpoint or a slow compilation.
                    elapsed = Math.Clamp(elapsed, 0, 1f / 15f);
                    Core.SetDeltaTime(elapsed);
                    Core.SceneManager.CurrentScene?.Update(elapsed);
                }
                finally
                {
                    Keyboard.StateOverrideEXT = oldKeyboard;
                    Mouse.StateOverrideEXT = oldMouse;
                }
            }
        }

        public void Render(RenderTarget2D target)
        {
            using (Enter())
            {
                var device = Core.GraphicsDevice;
                var previousTargets = device.GetRenderTargets();
                var viewport = device.Viewport;
                try
                {
                    device.SetRenderTarget(target);
                    var camera = Camera.Main;
                    device.Clear(camera?.Background ?? Color.CornflowerBlue);
                    if (camera == null) return;
                    camera.Viewport = new Vector2(target.Width, target.Height);
                    Core.BeginDraw(camera.Matrix);
                    try { Core.SceneManager.CurrentScene?.Draw(); }
                    finally { Core.EndDraw(); }
                }
                finally
                {
                    device.SetRenderTargets(previousTargets);
                    device.Viewport = viewport;
                }
            }
        }

        public void Dispose()
        {
            InitialSelection = null;
            if (Core != null)
            {
                using (Enter())
                {
                    var scene = Core.SceneManager.CurrentScene;
                    if (scene != null)
                    {
                        foreach (var go in scene.GameObjectsWithChildren.ToList())
                        {
                            try { go.ClearComponents(); }
                            catch (Exception exception) { Debug.LogError(exception.ToString()); }
                        }
                        scene.ClearScene();
                    }
                    Core.Tweens.Clear();
                    PhysicsSettings.World = null;
                    Camera.Main = null;
                    Core.ResourcesManager?.Dispose();
                    Core.SpriteBatch?.Dispose();
                    Core.SerializationBinder = null;
                }
                Core = null;
            }
            _scripts?.Dispose();
            _scripts = null;
        }

        private sealed class CoreScope : IDisposable
        {
            private readonly GameCore _previous;
            public CoreScope(GameCore core)
            {
                _previous = GameCore.Current;
                GameCore.SetCurrent(core);
            }
            public void Dispose() => GameCore.SetCurrent(_previous);
        }
    }
}
