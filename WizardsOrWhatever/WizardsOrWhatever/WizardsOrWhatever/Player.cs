using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace WizardsOrWhatever
{
    class Player : DrawablePhysicsObject
    {
        public enum CharState
        {
            Idle,
            Jumping,
            Running
        }

        public float runSpeed = 10f;
        public Vector2 jumpImpulse = new Vector2(0, ConvertUnits.ToSimUnits(-50));
        public float launchSpeed;
        public CharState state;

        public Player(World world, Texture2D texture, Vector2 size)
            : base(world, texture, size, 50f)
        {
        }

        new public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(Size.X / (float)texture.Width, Size.Y / (float)texture.Height);
            spriteBatch.Draw(texture, Position, null, Color.White, 0f, new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), scale, SpriteEffects.None, 0);
        }
    }
}
