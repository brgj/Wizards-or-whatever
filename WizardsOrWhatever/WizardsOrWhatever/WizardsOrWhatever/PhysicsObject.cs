using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;

namespace WizardsOrWhatever
{
    public class PhysicsObject : IDisposable
    {
        public Body body;
        public Vector2 Position
        {
            get { return ConvertUnits.ToDisplayUnits(body.Position); }
            set { body.Position = ConvertUnits.ToSimUnits(value); }
        }

        protected Texture2D texture;

        protected Vector2 size;
        public Vector2 Size
        {
            get { return ConvertUnits.ToDisplayUnits(size); }
            set { size = ConvertUnits.ToSimUnits(value); }
        }

        public PhysicsObject(World world, Vector2 position, Texture2D texture, Vector2 size, float mass)
        {
            this.Size = size;
            this.texture = texture;

            SetUpPhysics(world, position, mass);
        }

        public bool CheckBoundaries()
        {
            return body.Position.X > 100 || body.Position.X < -100 || body.Position.Y > 150 || body.Position.Y < -150;
        }

        protected virtual void SetUpPhysics(World world, Vector2 position, float mass)
        {
            body = BodyFactory.CreateRectangle(world, size.X, size.Y, mass, ConvertUnits.ToSimUnits(position));
            body.BodyType = BodyType.Dynamic;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(Size.X / (float)texture.Width, Size.Y / (float)texture.Height);
            spriteBatch.Draw(texture, Position, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, body.Rotation, new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), scale, SpriteEffects.None, 0);
        }

        #region IDisposable Members

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                body.Dispose();
                IsDisposed = true;
            }
        }

        #endregion
    }
}
