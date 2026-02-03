using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using CrazyStorm;
using System.Text;

namespace MonoGame.Framework.WpfInterop
{
    /// <summary>
    /// Host a Direct3D 11 scene.
    /// Low level abstraction, should not be used (use <see cref="T:MonoGame.Framework.WpfInterop.WpfGame" /> instead).
    /// </summary>
    public abstract class D3D11Host : Image, IDisposable
    {
        private static readonly object GraphicsDeviceLock = new object();
        private static bool _useASingleSharedGraphicsDevice = false;
        private double _dpiScalingFactor = 1.0;
        private List<IDisposable> _toBeDisposedNextFrame = new List<IDisposable>();
        private bool _isRendering;
        private static GraphicsDevice _staticGraphicsDevice;
        private GraphicsDevice _graphicsDevice;
        private static bool? _isInDesignMode;
        private static int _referenceCount;
        private D3D11Image _d3D11Image;
        private bool _disposed;
        private TimeSpan _lastRenderingTime;
        private bool _loaded;
        private bool GraphicsDeviceInitialized;
        /// <summary>Shared between WPF and monogame.</summary>
        private RenderTarget2D _sharedRenderTarget;
        /// <summary>
        /// Actual rendertarget that monogame will draw into.
        /// Once a draw call is finished it then copies its content to <see cref="F:MonoGame.Framework.WpfInterop.D3D11Host._sharedRenderTarget" />.
        /// This prevents flickering of the screen when WPF decides to draw the rendertarget to screen while monogame is midway populating it.
        /// </summary>
        private RenderTarget2D _cachedRenderTarget;
        private bool _resetBackBuffer;
        private bool _dpiChanged;
        private bool _isActive;
        private SpriteBatch _spriteBatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MonoGame.Framework.WpfInterop.D3D11Host" /> class.
        /// </summary>
        protected D3D11Host()
        {
            Stretch = Stretch.Fill;
            Loaded += new RoutedEventHandler(OnLoaded);
        }

        /// <summary>
        /// <para>
        /// Must be set before the first instance is created.
        /// </para>
        /// <para>
        /// Defaults to false, (prior 2.0 the behaviour was as if this was set to true).
        /// If set to false each WpfGame instance gets its own graphicsdevice.
        /// If set to true, A single graphics device is shared across all instances of WpfGame.
        /// </para>
        /// <para>
        /// Currently there is bug in monogame when setting this to false AND using MSAA (Disposing rendertargets will crash, so your only option is to not dispose them, causing memoryleaks).
        /// Meanwhile if this is true, you can Dispose rendertargets just fine.
        /// </para>
        /// </summary>
        public static bool UseASingleSharedGraphicsDevice
        {
            get
            {
                return _useASingleSharedGraphicsDevice;
            }
            set
            {
                if (_referenceCount > 0)
                {
                    throw new InvalidOperationException("UseASingleSharedGraphicsDevice must be set before the first instance is created and cannot be changed during runtime.");
                }
                _useASingleSharedGraphicsDevice = value;
            }
        }

        /// <summary>
        /// Determines whether the game runs in fixed timestep or not.
        /// The current implementation always calls Update and Draw after each other continuously.
        /// Since the rendering is based on the WPF render thread the exact times at which it will be called cannot be guaranteed.
        /// Therefore this value is always false.
        /// </summary>
        public bool IsFixedTimeStep
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the target time between two updates. Defaults to 60fps.
        /// WPF is limiting its rendering to 60 FPS max, therefore setting a target value higher than 60 fps (lower than TimeSpan.FromSeconds(1 / 60.0)) will have no effect.
        /// </summary>
        public TimeSpan TargetElapsedTime { get; set; } = TimeSpan.FromTicks(160000L);

        /// <summary>
        /// Gets a value indicating whether the controls runs in the context of a designer (e.g.
        /// Visual Studio Designer or Expression Blend).
        /// </summary>
        /// <value>
        /// <see langword="true" /> if controls run in design mode; otherwise,
        /// <see langword="false" />.
        /// </value>
        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    _isInDesignMode = new bool?((bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue);
                }
                return _isInDesignMode.Value;
            }
        }

        /// <summary>
        /// Gets whether the current control is active.
        /// This property will be true, when the control is active (parent window has focus).
        /// Note that if the game is inside a tab, then this property will only be true if the window has focus and the tab is active.
        /// If either the window loses focus or the tab is switched, this property will be false.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            private set
            {
                if (_isActive == value) return;
                _isActive = value;
                if (_disposed) return;
                if (IsActive)
                {
                    EventHandler<EventArgs> activated = Activated;
                    if (activated == null) return;
                    activated(this, EventArgs.Empty);
                }
                else
                {
                    EventHandler<EventArgs> deactivated = Deactivated;
                    if (deactivated == null) return;
                    deactivated(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the scaling factor that is applied to the attached gamecontrol.
        /// For legacy compatibility this always defaults to a factor of 1.
        /// If your monitor is scaled at 200%, then this will cause the game to render at only half the size.
        /// In order to render at full native resolution, set this value to the correct <see cref="!:SystemDpiScalingFactor" />.
        /// </summary>
        public double DpiScalingFactor
        {
            get
            {
                return _dpiScalingFactor;
            }
            set
            {
                if (value <= 0.0) throw new ArgumentOutOfRangeException(nameof(DpiScalingFactor), "value must be positive");
                if (_dpiScalingFactor == value) return;
                _dpiScalingFactor = value;
                _dpiChanged = true;
            }
        }

        /// <summary>Gets the graphics device.</summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (!UseASingleSharedGraphicsDevice) return _graphicsDevice;
                return _staticGraphicsDevice;
            }
        }

        /// <summary>Default services collection.</summary>
        public GameServiceContainer Services { get; } = new GameServiceContainer();

        /// <summary>
        /// Event is invoked when the game is activated (window receives focus).
        /// Note for game instances inside a tab control this will fire only when the tab is activated (or the tab was active and the window is focused).
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        /// <summary>
        /// Event is invoked when the game is deactivated (window loses focus).
        /// Note for game instances inside a tab control this will fire only when the tab is deactivated (or the tab was active and the window loses focus).
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;

        public void Dispose()
        {
            StopRendering();
            UnitializeImageSource();
            DisposeRenderTargetsFromPreviousFrames();
            if (GraphicsDeviceInitialized)
            {
                UninitializeGraphicsDevice();
                GraphicsDeviceInitialized = false;
            }
            if (_disposed) return;
            _disposed = true;
            Activated = null;
            Deactivated = null;
            if (_spriteBatch != null)
            {
                _spriteBatch.Dispose();
                _spriteBatch = null;
            }
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);

        protected virtual void Initialize()
        {
            _disposed = false;
            IGraphicsDeviceManager service = (IGraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
            if (service == null) throw new NotSupportedException("Services must contain a IGraphicsDeviceManager instance");
            service.CreateDevice();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified
        /// information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _resetBackBuffer = true;
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected virtual void Render(GameTime time)
        {
        }

        private void InitializeGraphicsDevice()
        {
            lock (GraphicsDeviceLock)
            {
                ++_referenceCount;
                if (_referenceCount != 1 && UseASingleSharedGraphicsDevice) return;
                GraphicsDevice sharedGraphicsDevice = CreateSharedGraphicsDevice(new PresentationParameters()
                {
                    DeviceWindowHandle = IntPtr.Zero
                });
                if (UseASingleSharedGraphicsDevice) _staticGraphicsDevice = sharedGraphicsDevice;
                else _graphicsDevice = sharedGraphicsDevice;
            }
        }

        private static GraphicsDevice CreateSharedGraphicsDevice(PresentationParameters presentationParameters)
        {
            return new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);
        }

        /// <summary>
        /// Helper that allows applying new presentation parameters.
        /// </summary>
        /// <param name="pp"></param>
        public void RecreateGraphicsDevice(PresentationParameters pp)
        {
            lock (GraphicsDeviceLock)
            {
                if (GraphicsDevice == null)
                {
                    throw new NotSupportedException("Can only recreate graphicsdevice when one already exists. Initalize one first!");
                }
                CreateGraphicsDeviceDependentResources(pp);
            }
        }

        private void UninitializeGraphicsDevice()
        {
            lock (GraphicsDeviceLock)
            {
                --_referenceCount;
                if (_referenceCount != 0 && UseASingleSharedGraphicsDevice) return;
                GraphicsDevice.PresentationParameters.DeviceWindowHandle = new IntPtr(1);
                GraphicsDevice.Reset();
                GraphicsDevice.Dispose();
                if (UseASingleSharedGraphicsDevice) _staticGraphicsDevice = null;
                else _graphicsDevice = null;
            }
        }

        private void CreateBackBuffer()
        {
            _d3D11Image.SetBackBuffer(null);
            if (_sharedRenderTarget != null)
                _toBeDisposedNextFrame.Add(_sharedRenderTarget);
            int num1 = 0;
            if (_cachedRenderTarget != null)
            {
                num1 = _cachedRenderTarget.MultiSampleCount;
                _toBeDisposedNextFrame.Add(_cachedRenderTarget);
            }
            int num2 = Math.Max((int)ActualWidth, 1);
            int num3 = Math.Max((int)ActualHeight, 1);
            CreateGraphicsDeviceDependentResources(new PresentationParameters()
            {
                BackBufferWidth = num2,
                BackBufferHeight = num3,
                MultiSampleCount = num1
            });
        }

        private void CreateGraphicsDeviceDependentResources(PresentationParameters pp)
        {
            int backBufferWidth = pp.BackBufferWidth;
            int backBufferHeight = pp.BackBufferHeight;
            int multiSampleCount = pp.MultiSampleCount;
            _sharedRenderTarget = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Bgr32, DepthFormat.None, 0, RenderTargetUsage.DiscardContents, true);
            _d3D11Image.SetBackBuffer(_sharedRenderTarget);
            _cachedRenderTarget = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Bgr32, DepthFormat.None, multiSampleCount, RenderTargetUsage.DiscardContents, false);
        }

        private void InitializeImageSource()
        {
            _d3D11Image = new D3D11Image();
            _d3D11Image.IsFrontBufferAvailableChanged += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);
            CreateBackBuffer();
            Source = _d3D11Image;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (_d3D11Image.IsFrontBufferAvailable)
            {
                StartRendering();
                _resetBackBuffer = true;
            }
            else
                StopRendering();
        }

        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            if (IsInDesignMode || _loaded) return;
            _loaded = true;
            Window parent1 = VisualHelper.FindParent<Window>(this);
            if (parent1 == null)
            {
                throw new NotSupportedException("The game control does not have a parent window, this is currently not supported");
            }
            parent1.Activated += new EventHandler(OnWindowActivated);
            parent1.Deactivated += new EventHandler(OnWindowDeactivated);
            parent1.Closed += new EventHandler(OnWindowClosed);
            TabControl parent2 = VisualHelper.FindParent<TabControl>(this);
            if (parent2 != null) parent2.SelectionChanged += new SelectionChangedEventHandler(TabChanged);
            IEnumerable<Window> source = Application.Current.Windows.OfType<Window>();
            Func<Window, bool> func = (x => x.IsActive);
            if (source.SingleOrDefault(func) == parent1 && IsVisible) IsActive = true;
            if (!GraphicsDeviceInitialized)
            {
                InitializeGraphicsDevice();
                GraphicsDeviceInitialized = true;
            }
            InitializeImageSource();
            try
            {
                Initialize();
                StartRendering();
            }
            catch (Exception ex)
            {
                if (!Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) return;
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += ((e, arg) => arg.Result = arg.Argument);
                backgroundWorker.RunWorkerCompleted += ((e, arg) =>
                {
                    throw new Exception("Initialize failed, see inner exception for details.", (Exception)arg.Result);
                });
                backgroundWorker.RunWorkerAsync(ex);
            }
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl)) return;
            TabItem parent = VisualHelper.FindParent<TabItem>(this);
            if (e.AddedItems.Contains(parent))
            {
                IsActive = true;
            }
            else
            {
                if (!e.RemovedItems.Contains(parent)) return;
                IsActive = false;
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            TabControl parent1 = VisualHelper.FindParent<TabControl>(this);
            if (parent1 != null)
            {
                TabItem parent2 = VisualHelper.FindParent<TabItem>(this);
                IsActive = parent1.SelectedItem == parent2;
            }
            else IsActive = true;
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            IsActive = false;
        }

        private void OnRendering(object sender, EventArgs eventArgs)
        {
            if (!_isRendering) return;
            if (_toBeDisposedNextFrame.Count > 0) DisposeRenderTargetsFromPreviousFrames();
            if (_resetBackBuffer || _dpiChanged)
            {
                CreateBackBuffer();
                _dpiChanged = false;
            }
            RenderingEventArgs renderingEventArgs = (RenderingEventArgs)eventArgs;
            bool presentNeeded = false;
            _d3D11Image.Lock();
            if (_lastRenderingTime != renderingEventArgs.RenderingTime)
            {
                long dt = renderingEventArgs.RenderingTime.Ticks - _lastRenderingTime.Ticks;
                if (dt >= TargetElapsedTime.Ticks)
                {
                    GraphicsDevice.SetRenderTarget(_sharedRenderTarget);
                    Render(new GameTime(renderingEventArgs.RenderingTime, TimeSpan.FromTicks(dt)));
                    _lastRenderingTime = renderingEventArgs.RenderingTime;
                    presentNeeded = true;
                }
            }
            else if (_resetBackBuffer)
            {
                GraphicsDevice.SetRenderTarget(_sharedRenderTarget);
                Render(new GameTime(renderingEventArgs.RenderingTime, TimeSpan.Zero));
                presentNeeded = true;
            }
            if (!presentNeeded && !_resetBackBuffer) return;
            GraphicsDevice.Flush();
            //GraphicsDevice.SetRenderTarget(_sharedRenderTarget);
            //_spriteBatch.Begin(SpriteSortMode.Deferred, 
            //    BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, 
            //    null, new Microsoft.Xna.Framework.Matrix?());
            //_spriteBatch.Draw(_cachedRenderTarget, GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.White);
            //_spriteBatch.End();
            _d3D11Image.AddDirtyRect(new Int32Rect(0, 0, _d3D11Image.PixelWidth, _d3D11Image.PixelHeight));
            _d3D11Image.Unlock();
            _resetBackBuffer = false;
        }

        private void DisposeRenderTargetsFromPreviousFrames()
        {
            int num;
            for (int index1 = 0; index1 < _toBeDisposedNextFrame.Count; index1 = num + 1)
            {
                IDisposable disposable = _toBeDisposedNextFrame[index1];
                if (disposable != null)
                    disposable.Dispose();
                List<IDisposable> disposedNextFrame = _toBeDisposedNextFrame;
                int index2 = index1;
                num = index2 - 1;
                disposedNextFrame.RemoveAt(index2);
            }
        }

        private void StartRendering()
        {
            if (_isRendering) return;
            CompositionTarget.Rendering += new EventHandler(OnRendering);
            _isRendering = true;
        }

        private void StopRendering()
        {
            if (!_isRendering) return;
            CompositionTarget.Rendering -= new EventHandler(OnRendering);
            _isRendering = false;
        }

        private void UnitializeImageSource()
        {
            Source = null;
            if (_d3D11Image != null)
            {
                _d3D11Image.IsFrontBufferAvailableChanged -= new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);
                _d3D11Image.Dispose();
                _d3D11Image = null;
            }
            if (_sharedRenderTarget != null)
            {
                _sharedRenderTarget.Dispose();
                _sharedRenderTarget = null;
            }
            if (_cachedRenderTarget == null) return;
            if (_cachedRenderTarget.MultiSampleCount <= 0 || !UseASingleSharedGraphicsDevice)
            {
                _cachedRenderTarget.Dispose();
            }
            _cachedRenderTarget = null;
        }
    }
}
