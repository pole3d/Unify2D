using System.Reflection;
using ImGuiNET;
using Unify2D.Scripting;
using Unify2D.Toolbox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ImGuiColorTextEditNet;

internal static class CompletionChecks
{
    private static ScriptCompletion.Result Complete(string marked, params KeyValuePair<string, string>[] others)
    {
        int cursor = marked.IndexOf('|');
        return ScriptCompletion.Get(marked.Remove(cursor, 1), cursor, "Current.cs", others);
    }

    public static void Run(Action<bool, string> check)
    {
        var signatures = Complete("class Test { int Move(float distance, int steps = 2) => 0; void Move(ref int count) {} void Run() { Mo| } }");
        var method = signatures.Details["Move"];
        check(method.Kind == "method" && method.Signatures.Length == 2 &&
            method.Signatures.Any(s => s.Contains("int Test.Move(float distance, [int steps = 2])")) &&
            method.Signatures.Any(s => s.Contains("void Test.Move(ref int count)")),
            "Completion describes return types, named parameters, defaults, ref and overloads: " + string.Join("; ", method.Signatures));
        var localDetails = Complete("class Test { void Run() { int speed = 1; spe| } }").Details["speed"];
        check(localDetails.Type == "int" && localDetails.Kind == "local", "Completion describes variable types");
        var propertyDetails = Complete("class Test { public string Title { get; set; } void Run() { Ti| } }").Details["Title"];
        check(propertyDetails.Type == "string" && propertyDetails.Kind == "property", "Completion describes property types");
        check(Complete("class Test { void Run() { Input.| } }").Items.Contains("Keyboard"),
            "Completion resolves the implicit Input alias");
        check(Complete("class Test { void Run() { Input.Keyboard.| } }").Items.Contains("GetState"),
            "Completion resolves static members through Input");
        check(Complete("class Test { void Run() { Vector2.| } }").Items.Contains("Zero"),
            "Completion resolves the implicit Vector2 alias");
        check(Complete("class Test { void Run() { Ke| } }").Items.Contains("Keys"),
            "Completion suggests the implicit Keys alias");
        check(Complete("class Test { void Run() { Input.Keyboard.GetState().IsKeyDown(Keys.Ri|); } }").Items.Contains("Right"),
            "Completion resolves Keys enum values inside method arguments");
        check(Complete("using Keys = Microsoft.Xna.Framework.Input.Keys; class Test { void Run() { Keys.Sp| } }").Items.Contains("Space"),
            "An explicit Keys alias preserves completion offsets");
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(path));
        var aliases = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("AliasCheck",
            ScriptCompilationDefaults.WithImports(new[] {
                ScriptCompilationDefaults.Parse("using Input = Microsoft.Xna.Framework.Input; using Vector2 = Microsoft.Xna.Framework.Vector2; class Explicit { Vector2 Value = new Vector2(); object Read() => Input.Keyboard.GetState(); }"),
                ScriptCompilationDefaults.Parse("namespace Nested { class Implicit { Vector2 Value = Vector2.Zero; object Read() => Input.Keyboard.GetState().IsKeyDown(Keys.Right); } }"),
                ScriptCompilationDefaults.Parse("using Keys = Microsoft.Xna.Framework.Input.Keys; class ExplicitKeys { Keys Value = Keys.Space; }")
            }), references, new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
        var errors = aliases.GetDiagnostics().Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToArray();
        check(errors.Length == 0, "Implicit aliases compile alongside explicit aliases: " + string.Join("; ", errors.Select(d => d.ToString())));
        check(Complete("class Test { void Run() { ret| } }").Items.Contains("return"), "Completion suggests C# keywords");
        check(Complete("class Test { void Run() { int speed = 1; spe| } }").Items.Contains("speed"),
            "Completion includes local variables from unsaved text");
        var instance = Complete("using Unify2D.Core; class Test : Component { void Run() { GameObject.Pos| } }");
        check(instance.Items.Contains("Position"), "Completion resolves inherited GameObject member access");
        var statics = Complete("using Microsoft.Xna.Framework; class Test { void Run() { Vector2.| } }");
        check(statics.Items.Contains("Zero") && !statics.Items.Contains("Length"), "Completion distinguishes static and instance members");
        var project = Complete("class Test { void Run() { var other = new Other(); other.Spe| } }",
            new KeyValuePair<string, string>("Other.cs", "public class Other { public float Speed; private int Secret; }"));
        check(project.Items.Contains("Speed"), "Completion resolves types from other project scripts");
        check(Complete("class Test { void Run() { // GameObject.Pos|\n } }").Items.Length == 0 &&
            Complete("class Test { string s = \"GameObject.Pos|\"; }").Items.Length == 0,
            "Completion stays hidden inside comments and strings");
        check(!Complete("class Test { void Run() { var other = new Other(); other.Se| } }",
            new KeyValuePair<string, string>("Other.cs", "public class Other { private int Secret; }")).Items.Contains("Secret"),
            "Completion does not expose inaccessible private members");

        const string marked = "// été\n\tclass Test { void Run() { ret|; } }";
        int caret = marked.IndexOf('|');
        string source = marked.Remove(caret, 1);
        var result = Complete(marked);
        var widget = new ScriptCompletionInput();
        void Set(string name, object value) => typeof(ScriptCompletionInput)
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(widget, value);
        Set("_result", result);
        Set("_resultSource", source);
        Set("_resultCursor", caret);
        widget.Editor.AllText = source;
        widget.Editor.CursorPosition = ScriptCompletionInput.CoordinatesAt(source, caret, widget.Editor.Options.TabSize);
        check(widget.Accept("return") && widget.Editor.AllText == source.Insert(caret, "urn"),
            "Completion preserves accented text, tabs and the suffix");
        check(ScriptCompletionInput.OffsetAt(widget.Editor.AllText, widget.Editor.CursorPosition, widget.Editor.Options.TabSize) == caret + 3,
            "Completion advances the caret to the end of the inserted identifier");
        widget.Editor.Undo();
        check(widget.Editor.AllText == source, "Completion can be undone in one step");
        widget.Editor.Redo();
        check(widget.Editor.AllText == source.Insert(caret, "urn"), "Completion can be redone");
        widget.Reset();
        check(typeof(ScriptCompletionInput).GetField("_result", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(widget) == null, "Changing scripts clears completion suggestions");
        var highlighter = new CSharpSyntaxHighlighter();
        const string code = "class Test { int x = 42; /* début\nfin */ string s = @\"été\nencore\"; }";
        highlighter.Update(code);
        object state = null!;
        var colored = new List<Glyph[]>();
        foreach (var line in code.Split('\n'))
        {
            var glyphs = line.Select(c => new Glyph(c, PaletteIndex.Default)).ToArray();
            state = highlighter.Colorize(glyphs, state);
            colored.Add(glyphs);
        }
        check(colored[0][0].ColorIndex == PaletteIndex.Keyword && colored[0][code.IndexOf("42")].ColorIndex == PaletteIndex.Number,
            "Roslyn colors C# keywords and numeric literals");
        check(colored[1][0].ColorIndex == PaletteIndex.Comment && colored[2][0].ColorIndex == PaletteIndex.String,
            "Coloring follows multiline comments and verbatim strings");
    }

    public static void RunMouseCheck(Unify2D.ImGuiRenderer.Renderer gui, Action<bool, string> check)
    {
        var widget = new ScriptCompletionInput();
        string text = "ret";
        void Set(string name, object value) => typeof(ScriptCompletionInput)
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(widget, value);
        void Suggest(params string[] items)
        {
            Set("_result", new ScriptCompletion.Result(0, text.Length, items) {
                Details = items.ToDictionary(name => name, name => new ScriptCompletion.Description("method", "int",
                    new[] { "int " + name + "(float distance, int steps = 2)", "int " + name + "(ref int count)" }))
            });
            Set("_resultSource", text);
            Set("_resultCursor", typeof(ScriptCompletionInput).GetField("_cursor", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(widget)!);
            Set("_selected", 0);
        }
        System.Numerics.Vector2 listPosition = default;
        void Frame(ButtonState button, bool overList)
        {
            Mouse.StateOverrideEXT = new MouseState(overList ? (int)listPosition.X + 15 : -1,
                overList ? (int)listPosition.Y + 12 : -1, 0, button, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released);
            gui.BeforeLayout(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1d / 60)));
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 400));
            ImGui.Begin("Completion test");
            widget.Draw(ref text, new System.Numerics.Vector2(550, 320), "Test.cs",
                () => Array.Empty<KeyValuePair<string, string>>());
            listPosition = widget.SuggestionPosition;
            ImGui.End();
            gui.AfterLayout();
        }
        try
        {
            Frame(ButtonState.Released, false);
            widget.Editor.Renderer.RequestFocus = true;
            Frame(ButtonState.Released, false);
            Suggest("return");
            Frame(ButtonState.Released, false);
            check(System.Numerics.Vector2.Distance(listPosition, widget.Editor.Renderer.CursorScreenPosition) < 60,
                "Suggestions appear beside the actual text caret");
            Frame(ButtonState.Released, true);
            Frame(ButtonState.Pressed, true);
            Frame(ButtonState.Released, true);
            Frame(ButtonState.Released, true);
            Frame(ButtonState.Released, true);
            check(text == "return", "Clicking a completion refocuses the editor and inserts it");
            Suggest("return", "returnValue");
            Keyboard.StateOverrideEXT = new KeyboardState(Keys.Down);
            Frame(ButtonState.Released, false);
            Keyboard.StateOverrideEXT = new KeyboardState(Keys.Enter);
            Frame(ButtonState.Released, false);
            check(text == "returnValue", "Arrow navigation and Enter insert the selected suggestion without a newline");
            Keyboard.StateOverrideEXT = default(KeyboardState);
            Frame(ButtonState.Released, false);
            Suggest("returnValueOther");
            Keyboard.StateOverrideEXT = new KeyboardState(Keys.Escape);
            Frame(ButtonState.Released, false);
            check(text == "returnValue", "Escape dismisses suggestions without reverting the script");
            Keyboard.StateOverrideEXT = new KeyboardState(Keys.LeftControl, Keys.S);
            Frame(ButtonState.Released, false);
            check(widget.SaveRequested, "Ctrl+S requests script saving in the focused code editor");
            Keyboard.StateOverrideEXT = default(KeyboardState);
            text = new string('x', 300) + "\nclass Demo { public string Message = \"Bonjour\"; }";
            Frame(ButtonState.Released, false);
            widget.Editor.CursorPosition = new Coordinates(0, 300);
            Frame(ButtonState.Released, false);
            Frame(ButtonState.Released, false);
            check(widget.Editor.Renderer.CursorScreenPosition.X < 570,
                "Long-line caret scrolling keeps completion placement inside the editor");
        }
        finally { Mouse.StateOverrideEXT = null; Keyboard.StateOverrideEXT = null; }
    }
}
