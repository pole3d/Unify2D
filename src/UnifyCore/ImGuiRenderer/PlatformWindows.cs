using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using ImGuiNET;
using SDL2;

namespace Unify2D.ImGuiRenderer
{
    // SDL windows share FNA's graphics device. Callbacks stay rooted until shutdown.
    internal sealed unsafe class PlatformWindows : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void WindowAction(ImGuiViewport* viewport);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SetVector(ImGuiViewport* viewport, Vector2 value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void GetVector(ImGuiViewport* viewport, Vector2* value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate byte GetBool(ImGuiViewport* viewport);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SetTitle(ImGuiViewport* viewport, IntPtr title);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SetAlpha(ImGuiViewport* viewport, float alpha);

        private int _mouseButtons;
        private bool _mouseCaptured;
        internal uint MouseButtons => (uint)Volatile.Read(ref _mouseButtons);

        internal void UpdateMouseCapture(uint pressedButtons)
        {
            // A release outside our windows may not generate an SDL button-up event.
            Interlocked.And(ref _mouseButtons, (int)pressedButtons);
            bool capture = IsActive && MouseButtons != 0;
            if (!IsActive) Interlocked.Exchange(ref _mouseButtons, 0);
            if (capture == _mouseCaptured) return;
            SDL.SDL_CaptureMouse(capture ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
            _mouseCaptured = capture;
        }

        private readonly IntPtr _mainWindow;
        private readonly uint _mainWindowId;
        private readonly List<Delegate> _callbacks = new();
        private readonly ConcurrentDictionary<uint, IntPtr> _windows = new();
        private readonly ConcurrentQueue<SDL.SDL_WindowEvent> _events = new();
        private readonly SDL.SDL_EventFilter _filter;
        private readonly SDL.SDL_EventFilter _previousFilter;
        private readonly IntPtr _previousUserData;

        public PlatformWindows(IntPtr mainWindow, bool showWindows)
        {
            _mainWindow = mainWindow;
            _mainWindowId = SDL.SDL_GetWindowID(mainWindow);
            var platform = ImGui.GetPlatformIO();
            platform.Platform_CreateWindow = Keep<WindowAction>(Create);
            platform.Platform_DestroyWindow = Keep<WindowAction>(Destroy);
            platform.Platform_ShowWindow = Keep<WindowAction>(v => { if (showWindows) SDL.SDL_ShowWindow((IntPtr)v->PlatformHandle); });
            platform.Platform_SetWindowPos = Keep<SetVector>((v, p) => SDL.SDL_SetWindowPosition((IntPtr)v->PlatformHandle, (int)p.X, (int)p.Y));
            platform.Platform_SetWindowSize = Keep<SetVector>((v, s) => SDL.SDL_SetWindowSize((IntPtr)v->PlatformHandle, Math.Max(1, (int)s.X), Math.Max(1, (int)s.Y)));
            // cimgui's adapters avoid the platform-dependent ABI of returning ImVec2 by value.
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowPos(platform.NativePtr, Keep<GetVector>((v, p) => {
                SDL.SDL_GetWindowPosition((IntPtr)v->PlatformHandle, out int x, out int y); *p = new Vector2(x, y);
            }));
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowSize(platform.NativePtr, Keep<GetVector>((v, s) => {
                SDL.SDL_GetWindowSize((IntPtr)v->PlatformHandle, out int w, out int h); *s = new Vector2(w, h);
            }));
            platform.Platform_SetWindowFocus = Keep<WindowAction>(v => SDL.SDL_RaiseWindow((IntPtr)v->PlatformHandle));
            platform.Platform_GetWindowFocus = Keep<GetBool>(v => (byte)(SDL.SDL_GetKeyboardFocus() == (IntPtr)v->PlatformHandle ? 1 : 0));
            platform.Platform_GetWindowMinimized = Keep<GetBool>(v => (byte)((SDL.SDL_GetWindowFlags((IntPtr)v->PlatformHandle) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0 ? 1 : 0));
            platform.Platform_SetWindowTitle = Keep<SetTitle>((v, t) => SDL.SDL_SetWindowTitle((IntPtr)v->PlatformHandle, Marshal.PtrToStringUTF8(t)));
            platform.Platform_SetWindowAlpha = Keep<SetAlpha>((v, a) => SDL.SDL_SetWindowOpacity((IntPtr)v->PlatformHandle, a));
            var mainViewport = ImGui.GetMainViewport();
            mainViewport.PlatformHandle = mainWindow;
            SDL.SDL_GetEventFilter(out _previousFilter, out _previousUserData);
            _filter = Filter;
            SDL.SDL_SetEventFilter(_filter, _previousUserData);
            UpdateMonitors();
            ImGui.GetIO().BackendFlags |= ImGuiBackendFlags.PlatformHasViewports | ImGuiBackendFlags.RendererHasViewports;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        }

        private IntPtr Keep<T>(T callback) where T : Delegate
        {
            _callbacks.Add(callback);
            return Marshal.GetFunctionPointerForDelegate(callback);
        }

        public bool IsActive
        {
            get
            {
                var focus = SDL.SDL_GetKeyboardFocus();
                return focus != IntPtr.Zero && (focus == _mainWindow || _windows.ContainsKey(SDL.SDL_GetWindowID(focus)));
            }
        }

        private void Create(ImGuiViewport* v)
        {
            var flags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            flags |= (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_mainWindow) &
                (SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN);
            if ((v->Flags & ImGuiViewportFlags.NoDecoration) != 0) flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            if ((v->Flags & ImGuiViewportFlags.NoTaskBarIcon) != 0) flags |= SDL.SDL_WindowFlags.SDL_WINDOW_SKIP_TASKBAR;
            if ((v->Flags & ImGuiViewportFlags.TopMost) != 0) flags |= SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP;
            var window = SDL.SDL_CreateWindow("Unify2D", (int)v->Pos.X, (int)v->Pos.Y,
                Math.Max(1, (int)v->Size.X), Math.Max(1, (int)v->Size.Y), flags);
            v->PlatformHandle = (void*)window;
            if (window != IntPtr.Zero) _windows[SDL.SDL_GetWindowID(window)] = (IntPtr)v;
            else v->PlatformRequestClose = 1;
        }

        private void Destroy(ImGuiViewport* v)
        {
            var window = (IntPtr)v->PlatformHandle;
            if (window != IntPtr.Zero && window != _mainWindow)
            {
                _windows.TryRemove(SDL.SDL_GetWindowID(window), out _);
                SDL.SDL_DestroyWindow(window);
            }
            v->PlatformHandle = null;
            v->PlatformUserData = null;
        }

        private int Filter(IntPtr userData, IntPtr eventPtr)
        {
            var evt = (SDL.SDL_Event*)eventPtr;
            // Only client-area button events may start an ImGui drag. Global button
            // polling also sees Windows title-bar/border drags, whose capture we must not steal.
            if ((evt->type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN || evt->type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP) &&
                (evt->button.windowID == _mainWindowId || _windows.ContainsKey(evt->button.windowID)))
            {
                int mask = 1 << (evt->button.button - 1);
                if (evt->type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN) Interlocked.Or(ref _mouseButtons, mask);
                else Interlocked.And(ref _mouseButtons, ~mask);
            }
            if (evt->type == SDL.SDL_EventType.SDL_WINDOWEVENT && evt->window.windowID == _mainWindowId)
            {
                _events.Enqueue(evt->window);
                // The renderer sizes the shared backbuffer from the current native bounds.
                // FNA's resize handler calls ApplyChanges/EndScreenDeviceChange, which can
                // restore and recenter the OS window while the user is moving or resizing it.
                if (evt->window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                    return 0;
            }
            if (evt->type == SDL.SDL_EventType.SDL_WINDOWEVENT && _windows.ContainsKey(evt->window.windowID))
            {
                _events.Enqueue(evt->window);
                // FNA assumes window events belong to its main window (resize, focus, expose).
                return 0;
            }
            return _previousFilter?.Invoke(userData, eventPtr) ?? 1;
        }

        public void Update()
        {
            while (_events.TryDequeue(out var evt))
            {
                ImGuiViewport* v;
                if (evt.windowID == _mainWindowId) v = ImGui.GetMainViewport().NativePtr;
                else if (_windows.TryGetValue(evt.windowID, out var pointer)) v = (ImGuiViewport*)pointer;
                else continue;
                if (evt.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE) v->PlatformRequestClose = 1;
                if (evt.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED) v->PlatformRequestMove = 1;
                if (evt.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED || evt.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED) v->PlatformRequestResize = 1;
            }
            UpdateMonitors();
        }

        private void UpdateMonitors()
        {
            int count = SDL.SDL_GetNumVideoDisplays();
            if (count <= 0) return;
            var platform = ImGui.GetPlatformIO().NativePtr;
            if (platform->Monitors.Capacity < count)
            {
                ImGui.MemFree(platform->Monitors.Data);
                platform->Monitors = new ImVector(count, count, ImGui.MemAlloc((uint)(count * sizeof(ImGuiPlatformMonitor))));
            }
            platform->Monitors = new ImVector(count, platform->Monitors.Capacity, platform->Monitors.Data);
            var monitors = (ImGuiPlatformMonitor*)platform->Monitors.Data;
            for (int i = 0; i < count; i++)
            {
                SDL.SDL_GetDisplayBounds(i, out var bounds);
                if (SDL.SDL_GetDisplayUsableBounds(i, out var work) != 0) work = bounds;
                monitors[i] = new ImGuiPlatformMonitor {
                    MainPos = new Vector2(bounds.x, bounds.y), MainSize = new Vector2(bounds.w, bounds.h),
                    WorkPos = new Vector2(work.x, work.y), WorkSize = new Vector2(work.w, work.h), DpiScale = 1f
                };
            }
        }

        public void Dispose()
        {
            if (_mouseCaptured) SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_FALSE);
            ImGui.DestroyPlatformWindows();
            SDL.SDL_SetEventFilter(_previousFilter, _previousUserData);
            var p = ImGui.GetPlatformIO();
            p.Platform_CreateWindow = p.Platform_DestroyWindow = p.Platform_ShowWindow = IntPtr.Zero;
            p.Platform_SetWindowPos = p.Platform_GetWindowPos = p.Platform_SetWindowSize = p.Platform_GetWindowSize = IntPtr.Zero;
            p.Platform_SetWindowFocus = p.Platform_GetWindowFocus = p.Platform_GetWindowMinimized = IntPtr.Zero;
            p.Platform_SetWindowTitle = p.Platform_SetWindowAlpha = IntPtr.Zero;
            ImGui.GetIO().BackendFlags &= ~(ImGuiBackendFlags.PlatformHasViewports | ImGuiBackendFlags.RendererHasViewports);
            ImGui.GetIO().ConfigFlags &= ~ImGuiConfigFlags.ViewportsEnable;
            _callbacks.Clear();
        }
    }
}
