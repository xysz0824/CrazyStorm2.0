/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Windows;
using SlimDX.Direct3D9;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace CrazyStorm_Player.DirectX
{
    abstract class DirectXFramework : IDisposable
    {
        #region Private Members
        bool disposed;
        bool deviceLost;
        bool formResizing;
        Form form;
        FormConfig windowConfig;
        DeviceContext context;
        Sprite sprite;
        long ticks;
        float deltaTime;
        float timeAccumulator;
        float frameCount;
        float fps;
        #endregion

        #region Public Members
        public Device Device { get { return context.Device; } }
        public Sprite Sprite { get { return sprite; } }
        public string WindowTitle
        {
            get { return windowConfig.WindowTitle; }
            set { windowConfig.WindowTitle = value; }
        }
        public int WindowWidth
        {
            get { return windowConfig.WindowWidth; }
            set { windowConfig.WindowWidth = value; }
        }
        public int WindowHeight
        {
            get { return windowConfig.WindowHeight; }
            set { windowConfig.WindowHeight = value; }
        }
        public bool Windowed { get; set; }
        #endregion

        #region Constructor
        public DirectXFramework()
        {
            windowConfig = new FormConfig
            {
                WindowTitle = string.Empty,
                WindowWidth = 800,
                WindowHeight = 600
            };
            Windowed = true;
        }
        #endregion

        #region Destructor
        ~DirectXFramework()
        {
            Dispose(false);
        }
        #endregion

        #region Private Methods
        void CreateWindow()
        {
            form = new RenderForm(windowConfig.WindowTitle)
            {
                ClientSize = new Size(windowConfig.WindowWidth, windowConfig.WindowHeight),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.CenterScreen,
                Icon = Properties.Resources.logo,
                MaximizeBox = false
            };
        }
        void InitializeDevice()
        {
            var settings = new DeviceSettings
            {
                Adapter = 0,
                CreateFlags = CreateFlags.HardwareVertexProcessing,
                Width = windowConfig.WindowWidth,
                Height = windowConfig.WindowHeight,
                Windowed = Windowed
            };
            context = new DeviceContext(form.Handle, settings);
            sprite = new Sprite(context.Device);
        }
        void HandleResize(object sender, EventArgs e)
        {
            if (form.WindowState == FormWindowState.Minimized)
                return;

            OnUnLoad();
            if (context != null)
            {
                context.PresentParameters.BackBufferWidth = 0;
                context.PresentParameters.BackBufferHeight = 0;
                context.Device.Reset(context.PresentParameters);
            }
            OnLoad();
        }
        void HandleResize()
        {
            var currentWindowState = form.WindowState;
            form.ResizeBegin += (o, args) => { formResizing = true; };
            form.Resize += (o, args) =>
                {
                    if (form.WindowState != currentWindowState)
                        HandleResize(o, args);

                    currentWindowState = form.WindowState;
                };
            form.ResizeEnd += (o, args) => { formResizing = false; HandleResize(o, args); };
        }
        void HandleAltEnter()
        {
            form.KeyUp += (o, args) =>
                {
                    if (args.Alt && args.KeyCode == Keys.Enter)
                    {
                        OnUnLoad();
                        Windowed = !Windowed;
                        if (context != null)
                        {
                            context.PresentParameters.BackBufferWidth = windowConfig.WindowWidth;
                            context.PresentParameters.BackBufferHeight = windowConfig.WindowHeight;
                            context.PresentParameters.Windowed = !Windowed;
                            if (Windowed)
                                form.MaximizeBox = true;

                            context.Device.Reset(context.PresentParameters);
                        }
                        OnLoad();
                    }
                };
        }
        void MeasureDeltaTime()
        {
            long lastTicks = ticks;
            ticks = Stopwatch.GetTimestamp();
            deltaTime = (float)(ticks - lastTicks) / Stopwatch.Frequency;
        }
        void CountFrame()
        {
            timeAccumulator += deltaTime;
            ++frameCount;
            if (timeAccumulator >= 1.0f)
            {
                fps = frameCount / timeAccumulator;
                timeAccumulator = 0;
                frameCount = 0;
            }
        }
        void Update()
        {
            MeasureDeltaTime();
            OnUpdate();
        }
        void Draw()
        {
            if (deviceLost)
            {
                if (context.Device.TestCooperativeLevel() == ResultCode.DeviceNotReset)
                {
                    context.Device.Reset(context.PresentParameters);
                    deviceLost = false;
                    OnLoad();
                }
                else
                {
                    Thread.Sleep(100);
                    return;
                }
            }
            CountFrame();
            try
            {
                context.Device.BeginScene();
                OnDraw();
                context.Device.EndScene();
                context.Device.Present();
            }
            catch (Direct3D9Exception ex)
            {
                if (ex.ResultCode == ResultCode.DeviceLost)
                {
                    OnUnLoad();
                    deviceLost = true;
                    return;
                }
                throw;
            }
        }
        #endregion

        #region Protected Methods
        protected void ClearScreen(ClearFlags clearFlags, Color4 color, float zdepth, int stencil)
        {
            context.Device.Clear(clearFlags, color, zdepth, stencil);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                context.Dispose();
                form.Dispose();
            }
            disposed = true;
        }
        #endregion

        #region Abstract Methods
        protected abstract void OnInitialize();
        protected virtual void OnLoad()
        {
            sprite.OnResetDevice();
        }
        protected virtual void OnUnLoad()
        {
            sprite.OnLostDevice();
        }
        protected abstract void OnUpdate();
        protected abstract void OnDraw();
        #endregion

        #region Public Methods
        public void Run()
        {
            OnInitialize();
            CreateWindow();
            InitializeDevice();
            HandleResize();
            HandleAltEnter();
            OnLoad();
            MessagePump.Run(form, () =>
                {
                    Update();
                    if (!formResizing)
                        Draw();
                });
            OnUnLoad();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
