using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace WizardsOrWhatever
{
    class Character : PhysicsObject
    {
        public enum CharState
        {
            None,
            Idle,
            Jumping,
            Running
        }

        public float runSpeed = 10f;
        public Vector2 jumpImpulse = new Vector2(0, ConvertUnits.ToSimUnits(-.05f));
        public float launchSpeed;
        public CharState state;
        public CharState prevState;

        public Character(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size, 50f)
        {
        }

        new public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(Size.X / (float)texture.Width, Size.Y / (float)texture.Height);
            spriteBatch.Draw(texture, Position, null, Color.White, 0f, new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), scale, SpriteEffects.None, 0);
        }
    }
}
