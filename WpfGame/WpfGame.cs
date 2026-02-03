using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoGame.Framework.WpfInterop
{
    /// <summary>
    /// The replacement for <see cref="T:Microsoft.Xna.Framework.Game" />. Unlike <see cref="T:Microsoft.Xna.Framework.Game" /> the <see cref="T:MonoGame.Framework.WpfInterop.WpfGame" /> is a WPF control and can be hosted inside WPF windows.
    /// </summary>
    public abstract class WpfGame : D3D11Host
    {
        private readonly string _contentDir;
        private ContentManager _content;

        /// <summary>Creates a new instance of a game host panel.</summary>
        protected WpfGame(string contentDir = "Content")
        {
            if (string.IsNullOrEmpty(contentDir)) throw new ArgumentNullException(nameof(contentDir));
            _contentDir = contentDir;
            Focusable = true;
        }

        /// <summary>
        /// Gets or sets whether this instance takes focus instantly on mouse over.
        /// If set to false, the user must click into the game panel to gain focus.
        /// This applies to both <see cref="T:MonoGame.Framework.WpfInterop.Input.WpfMouse" /> and <see cref="T:MonoGame.Framework.WpfInterop.Input.WpfKeyboard" /> behaviour.
        /// Defaults to true.
        /// </summary>
        public bool FocusOnMouseOver { get; set; } = true;

        /// <summary>The content manager for this game.</summary>
        public ContentManager Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _content = value;
            }
        }

        /// <summary>Dispose is called to dispose of resources.</summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            UnloadContent();
            ContentManager content = Content;
            if (content == null) return;
            content.Dispose();
        }

        /// <summary>The draw method that is called to render your scene.</summary>
        /// <param name="gameTime"></param>
        protected virtual void Draw(GameTime gameTime)
        {
        }

        /// <summary>
        /// Initialize is called once when the control is created.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Content = new ContentManager(Services, _contentDir);
            LoadContent();
        }

        /// <summary>
        /// Load content is called once by <see cref="M:MonoGame.Framework.WpfInterop.WpfGame.Initialize" />.
        /// </summary>
        protected virtual void LoadContent()
        {
        }

        /// <summary>
        /// Internal method used to integrate <see cref="M:MonoGame.Framework.WpfInterop.WpfGame.Update(Microsoft.Xna.Framework.GameTime)" /> and <see cref="M:MonoGame.Framework.WpfInterop.WpfGame.Draw(Microsoft.Xna.Framework.GameTime)" /> with the WPF control.
        /// </summary>
        /// <param name="time"></param>
        protected override sealed void Render(GameTime time)
        {
            Update(time);
            Draw(time);
        }

        /// <summary>
        /// Unload content is called once when the control is destroyed.
        /// </summary>
        protected virtual void UnloadContent()
        {
        }

        /// <summary>
        /// The update method that is called to update your game logic.
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void Update(GameTime gameTime)
        {
        }
    }
}
