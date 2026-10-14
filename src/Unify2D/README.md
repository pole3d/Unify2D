## Unify 2D is an educationnal 2D game engine with a purpose on fast production : fast iteration and fast creation

## FNA
- Download FNA from https://github.com/FNA-XNA/FNA/releases and add a reference to it
- Download the native FNA dependencies from https://github.com/FNA-XNA/FNA/wiki/3:-Distributing-FNA-Games and copy them to the project output directory
- Replace the marked MonoGame-specific parts of the code with their commented FNA counterparts (search for "FNA-specific")

## Script editor

User scripts implicitly have the aliases `Input = Microsoft.Xna.Framework.Input`, `Keys = Microsoft.Xna.Framework.Input.Keys` and `Vector2 = Microsoft.Xna.Framework.Vector2`, in completion, Play and exported games. For example, `Input.Keyboard.GetState().IsKeyDown(Keys.Space)` and `new Vector2(10, 20)` need no imports. Existing files are not modified. The Visual Studio project template supplies the same global aliases; redundant explicit aliases can be removed when working in Visual Studio.

The editor uses the vendored C# [ImGuiColorTextEditNet](../ImGuiColorTextEditNet/README.md) component with our existing ImGui 1.88 integration. It provides line numbers, indentation, selection, clipboard and undo/redo. Roslyn supplies C# syntax colors, including multiline comments and strings.
Use **Save** or **Ctrl+S** while the code editor is focused to save the script and request recompilation. **Ctrl+Z / Ctrl+Y** undo and redo, including inserted completions. Text is normalized to LF line endings.

The embedded script editor uses Roslyn's C# CompletionService for context-aware suggestions, including local variables, inferred types, keywords and accessible members.
Suggestions show the symbol kind and type. The adjacent details panel shows Roslyn descriptions and XML documentation, plus signatures with return types, parameter names/types, optional defaults and overloads. Accepting a suggestion applies Roslyn's suggested text change and caret position, including escaped identifiers.
Suggestions appear in a floating list beside the text cursor after a short typing delay; **Ctrl+Space** requests them immediately.
Use **Up/Down** to select, **Tab / Enter** or a click to insert, and **Escape** to dismiss. Tab still indents when no suggestions are shown.
Analysis runs in the background using the current buffer and the other project scripts; obsolete requests are cancelled as the user types. Completion does not save the file or move focus when selecting a script asset.

## Play in the editor

- Open a scene, then use **Play** or **F5** to run its current in-memory state in the Play toolbox. Saving or exporting is not required.
- **Pause / Resume** controls simulation. **Stop** or **F5** discards the runtime scene; the editing scene remains unchanged.
- During Play, the hierarchy, inspector and GAME editing view all use the running scene. Transform/component edits affect the live objects, and simulated values appear directly in the inspector. Stop restores the original scene and selection, including unsaved edits from before Play.
- Scene saving is ignored during Play. Opening a scene or prefab, or building, stops Play first.
- Click the Play view to send keyboard and mouse input to the game. When it loses focus, simulation continues with neutral input. Mouse coordinates are mapped to the 1920 × 1080 game view.
- Each run loads a fresh copy of the script assembly. Script edits made during Play are used on the next run. Compilation errors prevent a new run; runtime update exceptions pause it and appear in the console.
- A camera is created for the preview if the starting scene has none. **File → Build** keeps the standalone export workflow.

Play runs in the editor process. A script that blocks the game loop also blocks the editor; an assembly load context is not process isolation.

### Play regression checks (Windows / PowerShell)

Run from the repository root. These checks create a hidden FNA graphics device and verify scene/script isolation, pause, rendering, input restoration, restart, and assembly unloading.

```powershell
dotnet build tests/Unify2D.PlayTests/Unify2D.PlayTests.csproj -p:Platform=AnyCPU "-p:SolutionDir=$pwd\src\" "-p:OutDir=$pwd\bin\PlayCheck\"
dotnet bin/PlayCheck/Unify2D.PlayTests.dll
```
