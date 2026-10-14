# Vendored ImGuiColorTextEditNet

Source: https://github.com/csinkers/ImGuiColorTextEditNet
Commit: 2a6d4feb9f1a0f68acae15012d22be3ca7c74497
License: MIT (see LICENSE, also copied to application output).

The C# sources are built against the same ImGui.NET.Branches.Docking 1.88.0 as Unify2D.
Local changes:
- Use the older BeginChild overload and string text drawing overloads.
- Expose caret screen position, focus state, explicit focus requests and replaceable keyboard input.
- Scrolling to a line no longer steals focus.
- Cursor scrolling uses its column and compares horizontal distances in pixels.
- Advance the selection/caret to the end of inserted text in ReplaceSelection (including undo state).
- Return normalized LF text, matching the editor's internal line representation and Roslyn offsets.

C# syntax coloring and Roslyn completion integration live in Unify2D, outside this library.
