/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace CrazyStorm_Player.DirectX
{
    class DeviceContext : IDisposable
    {
        #region Private Members
        bool disposed;
        Direct3D direct3D;
        #endregion

        #region Public Members
        public Device Device { get; private set; }
        public PresentParameters PresentParameters { get; private set; }
        #endregion

        #region Constructor
        public DeviceContext(IntPtr handle, DeviceSettings settings)
        {

            PresentParameters = new PresentParameters();
            PresentParameters.BackBufferFormat = Format.X8R8G8B8;
            PresentParameters.BackBufferCount = 1;
            PresentParameters.BackBufferWidth = settings.Width;
            PresentParameters.BackBufferHeight = settings.Height;
            PresentParameters.Multisample = MultisampleType.None;
            PresentParameters.SwapEffect = SwapEffect.Discard;
            PresentParameters.EnableAutoDepthStencil = true;
            PresentParameters.AutoDepthStencilFormat = Format.D16;
            PresentParameters.PresentFlags = PresentFlags.DiscardDepthStencil;
            PresentParameters.PresentationInterval = PresentInterval.Default;
            PresentParameters.Windowed = settings.Windowed;
            PresentParameters.DeviceWindowHandle = handle;

            direct3D = new Direct3D();
            Device = new Device(direct3D, settings.Adapter, DeviceType.Hardware, handle, settings.CreateFlags, PresentParameters);
        }
        #endregion

        #region Destructor
        ~DeviceContext()
        {
            Dispose(false);
        }
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Device.Dispose();
                direct3D.Dispose();
            }
            disposed = true;
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
