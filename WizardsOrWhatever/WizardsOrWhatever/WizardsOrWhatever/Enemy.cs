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
    public class Enemy : CompositeCharacter
    {
        public Enemy(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size, Color.White) 
        {
            Health = 100;
            body.CollisionCategories = Category.Cat31;
            wheel.CollisionCategories = Category.Cat31;
            body.CollidesWith = Category.Cat30;
            body.CollidesWith = Category.Cat30;
        }

        public void Update(GameTime gameTime, Vector2 playerPos)
        {
            spriteTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (spriteTimer > spriteInterval)
            {
                UpdateSprite();
                spriteTimer = 0f;
            }

            motor.MotorSpeed = runSpeed * (Position.X < playerPos.X ? 1 : -1);
            if (Health <= 0 || CheckBoundaries())
                Dispose();
        }

        /// <summary>
        /// Called when the character's body collides with something and changes state to Wallslide from Jumping
        /// </summary>
        /// <param name="fix1">The first Fixture obj</param>
        /// <param name="fix2">The second Fixture obj</param>
        /// <param name="contact">The contact point</param>
        /// <returns></returns>
        new public bool OnBodyCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.CollisionCategories != Category.Cat30)
                body.ApplyLinearImpulse(jumpImpulse, body.Position);
            return true;
        }

        new protected void UpdateSprite()
        {
            SpriteY = body.LinearVelocity.X > 0 ? 1 : 0;
            ++SpriteX;
            SpriteX %= 3;
        }
    }

    
}
