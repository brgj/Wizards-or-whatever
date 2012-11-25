using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;

namespace WizardsOrWhatever
{
    public delegate void CheckCollision(Projectile p);

    public class Projectile : PhysicsObject
    {
        public Vector2 cursPosition;
        public Vector2 movements;
        public float speed = 500;
        private CheckCollision collisionChecker;
        public float level = 1.5f;

        public Projectile(World world, Vector2 position, Texture2D texture, Vector2 size, Vector2 cursPosition, CompositeCharacter player, CheckCollision collisionChecker)
            : base(world, position, texture, size, 0f)
        {
            this.collisionChecker = collisionChecker;
            this.cursPosition = cursPosition;
            movements = cursPosition - position;
            if (movements != Vector2.Zero)
            {
                movements.Normalize();
            }
            
            float angle = (float)Math.Atan2( movements.Y, movements.X );
            movements = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            //ignore collision with the player shooting
            body.IgnoreCollisionWith(player.body);
            body.IgnoreCollisionWith(player.wheel);
            body.IgnoreGravity = true;
            body.IsBullet = true;
            body.LinearVelocity = movements + player.body.LinearVelocity;
            body.OnCollision += new OnCollisionEventHandler(OnProjectileCollision);
        }

        public void UpdateProjectile(GameTime gameTime)
        {
            Position += movements * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public bool OnProjectileCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            collisionChecker(this);
            this.Dispose();
            return true;
        }
    }
}
