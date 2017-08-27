/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
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
using SlimDX.DirectInput;
using Device = SlimDX.Direct3D9.Device;

namespace CrazyStorm_Player.DirectX
{
    abstract class DirectXFramework : IDisposable
    {
        #region Private Members
        bool disposed;
        bool deviceLost;
        Form form;
        FormConfig windowConfig;
        DeviceContext context;
        Sprite sprite;
        DirectInput input;
        Keyboard keyboard;
        KeyboardState keyboardState;
        long ticks;
        float deltaTime;
        float timeAccumulator;
        float frameCount;
        float fps;
        #endregion

        #region Public Members
        public Device Device { get { return context.Device; } }
        public Sprite Sprite { get { return sprite; } }
        public KeyboardState KeyboardState { get { return keyboardState; } }
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
            input = new DirectInput();
            keyboard = new Keyboard(input);
            keyboard.SetCooperativeLevel(form, CooperativeLevel.Nonexclusive | CooperativeLevel.Background);
            keyboard.Acquire();
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
            keyboardState = keyboard.GetCurrentState();
            MeasureDeltaTime();
            OnUpdate();
        }
        void Draw()
        {
            if (deviceLost)
            {
                if (context.Device.TestCooperativeLevel() == SlimDX.Direct3D9.ResultCode.DeviceNotReset)
                {
                    context.Device.Reset(context.PresentParameters);
                    deviceLost = false;
                    OnReset();
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
                if (ex.ResultCode == SlimDX.Direct3D9.ResultCode.DeviceLost)
                {
                    OnLost();
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
        protected virtual void OnReset()
        {
            sprite.OnResetDevice();
        }
        protected virtual void OnLost()
        {
            sprite.OnLostDevice();
        }
        protected abstract void OnInitialize();
        protected abstract void OnLoad();
        protected abstract void OnUpdate();
        protected abstract void OnDraw();
        #endregion

        #region Public Methods
        public void Run()
        {
            OnInitialize();
            CreateWindow();
            InitializeDevice();
            OnLoad();
            MessagePump.Run(form, () =>
                {
                    Update();
                    Draw();
                });
            OnLost();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
