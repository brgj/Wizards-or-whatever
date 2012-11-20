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
    public class Projectile : PhysicsObject
    {

        public Vector2 position;
        public Vector2 velocity;
        public Vector2 cursPosition;
        public Vector2 direction;
        public Vector2 movements;
        public float speed = 500;

        public bool isVisible;

        public Projectile(World world, Vector2 position, Texture2D texture, Vector2 size, Vector2 cursPosition, CompositeCharacter player)
            : base(world, position, texture, size, 0f)
        {
            this.cursPosition = cursPosition;
            this.position = position;
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

        public void UpdateProjectile(GameTime gametime)
        {
            Position += movements * speed * (float)gametime.ElapsedGameTime.TotalSeconds;
        }

        public bool OnProjectileCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            Console.WriteLine(fix1.Body.Position);
            Console.WriteLine(fix2.Body.Position);
            return true;
        }
    }
}
