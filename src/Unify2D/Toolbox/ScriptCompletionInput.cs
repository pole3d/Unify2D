using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using ImGuiColorTextEditNet;
using ImGuiColorTextEditNet.Editor;
using ImGuiColorTextEditNet.Input;
using Unify2D.Scripting;

namespace Unify2D.Toolbox
{
    internal sealed class ScriptCompletionInput : ITextEditorKeyboardInput
    {
        internal TextEditor Editor { get; private set; }
        private StandardKeyboardInput _keyboard;
        private readonly CSharpSyntaxHighlighter _highlighter = new();
        private string _source;
        private int _cursor;
        private long _queryAt;
        private bool _force;
        private int _selected;
        private bool _scrollSelection;
        private ScriptCompletion.Result _result;
        private Task<ScriptCompletion.Result> _query;
        private string _querySource;
        private string _queryPath;
        private int _queryCursor;
        private string _resultSource;
        private int _resultCursor;
        internal Vector2 SuggestionPosition { get; private set; }
        internal bool SaveRequested { get; private set; }

        public ScriptCompletionInput() => Reset();

        internal void Reset()
        {
            Editor = new TextEditor();
            Editor.Renderer.IsShowingWhitespace = false;
            _keyboard = (StandardKeyboardInput)Editor.Renderer.KeyboardInput;
            Editor.Renderer.KeyboardInput = this;
            Editor.SetColor(PaletteIndex.Background, 0xff1e1e1e);
            Editor.SetColor(PaletteIndex.Keyword, 0xffd69c56);
            Editor.SetColor(PaletteIndex.String, 0xff7891ce);
            Editor.SetColor(PaletteIndex.CharLiteral, 0xff7891ce);
            Editor.SetColor(PaletteIndex.Comment, 0xff55996a);
            Editor.SetColor(PaletteIndex.Number, 0xffa8ceb5);
            Editor.SetColor(PaletteIndex.KnownIdentifier, 0xffb0c94e);
            _source = _querySource = _resultSource = null;
            _result = null;
            _queryAt = 0;
            _selected = _cursor = 0;
            _force = SaveRequested = false;
        }

        private void Colorize()
        {
            _highlighter.Update(_source);
            Editor.SyntaxHighlighter = _highlighter;
        }

        public bool Draw(ref string source, Vector2 size, string path, Func<KeyValuePair<string, string>[]> sources)
        {
            SaveRequested = false;
            if (_source != source)
            {
                Reset();
                Editor.AllText = source;
                _source = Editor.AllText;
                source = _source;
                Colorize();
            }
            if (_query?.IsCompleted == true)
            {
                if (_query.IsCompletedSuccessfully && _source == _querySource && _cursor == _queryCursor && path == _queryPath)
                {
                    _result = _query.Result;
                    _resultSource = _querySource;
                    _resultCursor = _queryCursor;
                    _selected = 0;
                    _scrollSelection = true;
                }
                _ = _query.Exception;
                _query = null;
            }
            long version = Editor.Version;
            Editor.Render("##source", new Vector2(size.X, Math.Max(40, size.Y)));
            bool changed = Editor.Version != version;
            if (changed)
            {
                source = _source = Editor.AllText;
                Colorize();
            }
            int cursor = OffsetAt(_source, Editor.CursorPosition, Editor.Options.TabSize);
            if (changed || cursor != _cursor || Editor.Selection.HasSelection)
            {
                _cursor = cursor;
                _result = null;
                _queryAt = Editor.Selection.HasSelection ? 0 : Environment.TickCount64 + 150;
            }
            if (Editor.Renderer.IsFocused && _query == null && _queryAt != 0 && Environment.TickCount64 >= _queryAt)
            {
                _querySource = _source;
                _queryCursor = _cursor;
                _queryPath = path;
                var text = _source;
                bool force = _force;
                var snapshot = sources();
                _query = Task.Run(() => ScriptCompletion.Get(text, cursor, path, snapshot, force));
                _queryAt = 0;
                _force = false;
            }
            if (DrawSuggestions())
            {
                source = _source = Editor.AllText;
                _cursor = OffsetAt(_source, Editor.CursorPosition, Editor.Options.TabSize);
                Colorize();
                changed = true;
            }
            return changed;
        }

        public void HandleKeyboardInputs()
        {
            if (!ImGui.IsWindowFocused()) return;
            var io = ImGui.GetIO();
            io.WantCaptureKeyboard = io.WantTextInput = true;
            if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.S)) { SaveRequested = true; return; }
            if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.Space))
            {
                _force = true;
                _queryAt = Environment.TickCount64;
                return;
            }
            if (_result?.Items.Length > 0 && _source == _resultSource &&
                OffsetAt(_source, Editor.CursorPosition, Editor.Options.TabSize) == _resultCursor && !Editor.Selection.HasSelection)
            {
                if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                {
                    _result = null;
                    _querySource = null;
                    _queryAt = 0;
                    return;
                }
                if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) || ImGui.IsKeyPressed(ImGuiKey.UpArrow))
                {
                    int direction = ImGui.IsKeyPressed(ImGuiKey.DownArrow) ? 1 : -1;
                    _selected = (_selected + direction + _result.Items.Length) % _result.Items.Length;
                    _scrollSelection = true;
                    return;
                }
                if (ImGui.IsKeyPressed(ImGuiKey.Tab) || ImGui.IsKeyPressed(ImGuiKey.Enter))
                {
                    Accept(_result.Items[_selected]);
                    return;
                }
            }
            _keyboard.HandleKeyboardInputs();
        }

        internal bool Accept(string item)
        {
            if (_result == null || Editor.AllText != _resultSource) return false;
            var start = CoordinatesAt(_resultSource, _result.Start, Editor.Options.TabSize);
            var end = CoordinatesAt(_resultSource, _result.Start + _result.Length, Editor.Options.TabSize);
            Editor.CursorPosition = end;
            Editor.Selection.Select(start, end);
            TextEditorModify.ReplaceSelection(Editor, item);
            Editor.ScrollToLine(start.Line);
            _result = null;
            _querySource = null;
            _queryAt = 0;
            return true;
        }

        private bool DrawSuggestions()
        {
            if (_result?.Items.Length > 0 && _source == _resultSource)
            {
                var viewport = ImGui.GetWindowViewport();
                float rowHeight = ImGui.GetTextLineHeightWithSpacing();
                var popupSize = Vector2.Min(new Vector2(380, rowHeight * Math.Min(8, _result.Items.Length) + 16), viewport.WorkSize);
                var caret = Editor.Renderer.CursorScreenPosition;
                var position = caret + new Vector2(0, Editor.Renderer.LineHeight + 3);
                var limit = viewport.WorkPos + viewport.WorkSize;
                if (position.Y + popupSize.Y > limit.Y) position.Y = caret.Y - popupSize.Y - 3;
                position = Vector2.Clamp(position, viewport.WorkPos, Vector2.Max(viewport.WorkPos, limit - popupSize));
                SuggestionPosition = position;
                ImGui.SetNextWindowPos(position);
                ImGui.SetNextWindowSize(popupSize);
                ImGui.SetNextWindowViewport(viewport.ID);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6, 6));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 3);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.12f, 0.12f, 0.14f, 1));
                ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.12f, 0.32f, 0.53f, 1));
                ImGui.Begin("##script_suggestions", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoDocking |
                    ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.Tooltip);
                string accepted = null;
                for (int i = 0; i < _result.Items.Length; i++)
                {
                    string name = _result.Items[i];
                    _result.Details.TryGetValue(name, out var detail);
                    string label = detail == null ? name : $"{name}    {detail.Type}  ({detail.Kind})";
                    if (ImGui.Selectable(label + "##" + name, i == _selected)) accepted = name;
                    if (ImGui.IsItemHovered() && ImGui.GetIO().MouseDelta != Vector2.Zero) _selected = i;
                    if (_scrollSelection && i == _selected) ImGui.SetScrollHereY();
                }
                _scrollSelection = false;
                bool hovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
                ImGui.End();
                if (_result.Details.TryGetValue(_result.Items[_selected], out var selected))
                {
                    var detailSize = Vector2.Min(new Vector2(440, Math.Min(280,
                        rowHeight * (selected.Signatures.Length * 3 + 2) + 16)), viewport.WorkSize);
                    var detailPosition = position + new Vector2(popupSize.X + 4, 0);
                    if (detailPosition.X + detailSize.X > limit.X)
                        detailPosition.X = position.X - detailSize.X - 4;
                    if (detailPosition.X < viewport.WorkPos.X)
                    {
                        detailPosition.X = position.X;
                        detailPosition.Y = position.Y + popupSize.Y + 4;
                        if (detailPosition.Y + detailSize.Y > limit.Y)
                            detailPosition.Y = position.Y - detailSize.Y - 4;
                    }
                    detailPosition = Vector2.Clamp(detailPosition, viewport.WorkPos,
                        Vector2.Max(viewport.WorkPos, limit - detailSize));
                    ImGui.SetNextWindowPos(detailPosition);
                    ImGui.SetNextWindowSize(detailSize);
                    ImGui.SetNextWindowViewport(viewport.ID);
                    ImGui.Begin("##script_signature", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
                        ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoDocking |
                        ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.Tooltip);
                    ImGui.TextDisabled(selected.Kind + (selected.Signatures.Length > 1 ? $" — {selected.Signatures.Length} overloads" : ""));
                    ImGui.PushTextWrapPos(0);
                    for (int i = 0; i < selected.Signatures.Length; i++)
                    {
                        if (i > 0) ImGui.Separator();
                        ImGui.TextUnformatted(selected.Signatures[i]);
                    }
                    ImGui.PopTextWrapPos();
                    hovered |= ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
                    ImGui.End();
                }
                ImGui.PopStyleColor(2);
                ImGui.PopStyleVar(2);
                if (accepted != null)
                {
                    Editor.Renderer.RequestFocus = true;
                    return Accept(accepted);
                }
                if (!Editor.Renderer.IsFocused && !hovered) _result = null;
            }
            return false;
        }

        internal static Coordinates CoordinatesAt(string text, int offset, int tabSize)
        {
            int line = 0, column = 0;
            for (int i = 0; i < Math.Min(offset, text.Length); i++)
            {
                if (text[i] == '\n') { line++; column = 0; }
                else if (text[i] == '\t') column = (column / tabSize + 1) * tabSize;
                else column++;
            }
            return new Coordinates(line, column);
        }

        internal static int OffsetAt(string text, Coordinates position, int tabSize)
        {
            int line = 0, column = 0, i = 0;
            for (; i < text.Length; i++)
            {
                if (line == position.Line && (column >= position.Column || text[i] == '\n')) break;
                if (text[i] == '\n') { line++; column = 0; }
                else if (text[i] == '\t') column = (column / tabSize + 1) * tabSize;
                else column++;
            }
            return i;
        }
    }
}
