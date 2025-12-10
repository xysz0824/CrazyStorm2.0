using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CrazyStorm_Player
{
    public class Controllable
    {
        int currentFrame;
        bool slow;
        int movableWidth;
        int movableHeight;
        public Vector2 selfPosition;
        public string imagePath = string.Empty;
        public Vector2 selfStart = Vector2.Zero;
        public Vector2 selfSize = Vector2.Zero;
        public Vector2 selfCenter = Vector2.Zero;
        public int selfFrames = 0;
        public int selfDelay = 0;
        public int selfRadius = 0;
        public Controllable(int movableWidth, int movableHeight)
        {
            selfPosition.Y = movableHeight / 3;
            this.movableWidth = movableWidth;
            this.movableHeight = movableHeight;
        }
        public void Update(KeyboardState state)
        {
            Vector2 direction = Vector2.Zero;
            if (state.IsKeyDown(Keys.Left))
            {
                direction.X = -1;
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                direction.X = 1;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                direction.Y = -1;
            }
            else if (state.IsKeyDown(Keys.Down))
            {
                direction.Y = 1;
            }
            if (direction.LengthSquared() != 0) direction.Normalize();
            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
            {
                slow = true;
                selfPosition += direction * 2.0f;
            }
            else
            {
                slow = false;
                selfPosition += direction * 4.0f;
            }
            selfPosition.X = MathHelper.Clamp(selfPosition.X, -movableWidth / 2, movableWidth / 2);
            selfPosition.Y = MathHelper.Clamp(selfPosition.Y, -movableHeight / 2, movableHeight / 2);
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D character, Texture2D point, Texture2D slowMode)
        {
            Vector2 center = new Vector2(movableWidth / 2, movableHeight / 2);
            Vector2 position = selfPosition + center;
            Color color = new Color(1f, 1f, 1f, 1f);
            Rectangle rect;
            if (character != null)
            {
                int frame = currentFrame / (selfDelay + 1) % selfFrames;
                rect = new Rectangle((int)selfStart.X + frame * (int)selfSize.X, (int)selfStart.Y, (int)selfSize.X, (int)selfSize.Y);
                spriteBatch.Draw(character, position, rect, color, 0, this.selfCenter, new Vector2(1, 1), SpriteEffects.None, 0);
            }
            Vector2 selfCenter = new Vector2(7, 7);
            rect = new Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(point, position, rect, color, 0, selfCenter, new Vector2(1, 1), SpriteEffects.None, 0);
            if (slow)
            {
                selfCenter = new Vector2(31, 31);
                float rotation = currentFrame / 30.0f;
                rect = new Rectangle(0, 0, 64, 64);
                spriteBatch.Draw(slowMode, position, rect, color, rotation, selfCenter, new Vector2(1, 1), SpriteEffects.None, 0);
            }
            ++currentFrame;
        }
    }
}
