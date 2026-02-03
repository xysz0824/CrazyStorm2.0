using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows;
using System.Windows.Media;

namespace MonoGame.Framework.WpfInterop
{
    /// <summary>
    /// The <see cref="T:Microsoft.Xna.Framework.Content.ContentManager" /> needs a <see cref="T:Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService" /> to be in the <see cref="T:System.ComponentModel.Design.IServiceContainer" />. This class fulfills this purpose.
    /// </summary>
    public class WpfGraphicsDeviceService : IGraphicsDeviceService, IGraphicsDeviceManager
    {
        internal const int MsaaSampleLimit = 32;
        private readonly WpfGame _host;

        /// <summary>
        /// Create a new instance of the dummy. The constructor will autom. add the instance itself to the <see cref="P:MonoGame.Framework.WpfInterop.D3D11Host.Services" /> container of <see cref="!:host" />.
        /// </summary>
        /// <param name="host"></param>
        public WpfGraphicsDeviceService(WpfGame host)
        {
            WpfGame wpfGame = host;
            if (wpfGame == null) throw new ArgumentNullException(nameof(host));
            _host = wpfGame;
            if (host.Services.GetService(typeof(IGraphicsDeviceService)) != null)
            {
                throw new NotSupportedException("A graphics device service is already registered.");
            }
            if (host.GraphicsDevice == null)
            {
                throw new ArgumentException("Provided host graphics device is null.");
            }
            GraphicsDevice = host.GraphicsDevice;
            _host.GraphicsDevice.DeviceReset += ((sender, args) =>
            {
                EventHandler<EventArgs> deviceReset = DeviceReset;
                if (deviceReset == null) return;
                deviceReset(this, args);
            });
            _host.GraphicsDevice.DeviceResetting += ((sender, args) =>
            {
                EventHandler<EventArgs> deviceResetting = DeviceResetting;
                if (deviceResetting == null)
                    return;
                deviceResetting(this, args);
            });
            host.Services.AddService(typeof(IGraphicsDeviceService), this);
            host.Services.AddService(typeof(IGraphicsDeviceManager), this);
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceCreated;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceDisposing;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceReset;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice { get; }

        public bool PreferMultiSampling { get; set; }

        /// <summary>
        /// Gets the scaling factor that is applied to the attached gamecontrol.
        /// For legacy compatibility this always defaults to a factor of 1.
        /// If your monitor is scaled at 200%, then this will cause the game to render at only half the size.
        /// In order to render at full native resolution, set this value to the correct <see cref="P:MonoGame.Framework.WpfInterop.WpfGraphicsDeviceService.SystemDpiScalingFactor" />.
        /// </summary>
        public double DpiScalingFactor
        {
            get
            {
                return _host.DpiScalingFactor;
            }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(DpiScalingFactor), "value must be positive");
                }
                _host.DpiScalingFactor = value;
            }
        }

        /// <summary>
        /// When called returns the system Dpi scaling factor.
        /// The scaling factor may be different between different monitors.
        /// This value will always return the value based on the monitor where the attached gamecontrol is positioned.
        /// </summary>
        public double SystemDpiScalingFactor
        {
            get
            {
                return PresentationSource.FromVisual(_host).CompositionTarget.TransformToDevice.M11;
            }
        }

        public int PreferredBackBufferWidth
        {
            get
            {
                return (int)_host.ActualWidth;
            }
        }

        public int PreferredBackBufferHeight
        {
            get
            {
                return (int)_host.ActualHeight;
            }
        }

        public bool BeginDraw()
        {
            return true;
        }

        public void CreateDevice()
        {
            ApplyChanges();
            EventHandler<EventArgs> deviceCreated = DeviceCreated;
            if (deviceCreated == null) return;
            deviceCreated(this, EventArgs.Empty);
        }

        public void EndDraw()
        {
        }

        public void ApplyChanges()
        {
            int num1 = Math.Max((int)_host.ActualWidth, 1);
            int num2 = Math.Max((int)_host.ActualHeight, 1);
            PresentationParameters pp = new PresentationParameters()
            {
                MultiSampleCount = PreferMultiSampling ? 32 : 0,
                BackBufferWidth = num1,
                BackBufferHeight = num2,
                DeviceWindowHandle = IntPtr.Zero
            };
            EventHandler<EventArgs> deviceDisposing = DeviceDisposing;
            if (deviceDisposing != null) deviceDisposing(this, EventArgs.Empty);
            _host.RecreateGraphicsDevice(pp);
        }
    }
}
