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
    class CompositeCharacter : Character
    {
        public Body wheel;
        public FixedAngleJoint fixedAngleJoint;
        public RevoluteJoint motor;
        public float centerOffset;
        int texWidth;
        int texHeight;
        private int spriteX = 0;
        private int spriteY = 0;
        private int prevSpriteX = 0;
        private int prevSpriteY = 0;
        protected float spriteTimer = 0f;
        protected float spriteInterval = 100f;

        int SpriteX
        {
            get
            {
                return spriteX;
            }
            set
            {
                prevSpriteX = spriteX;
                spriteX = value;
            }
        }
        int SpriteY
        {
            get
            {
                return spriteY;
            }
            set
            {
                prevSpriteY = spriteY;
                spriteY = value;
            }
        }

        /// <summary>
        /// Class for creating a character with a wheel used to move and a body
        /// </summary>
        /// <param name="world">The world that the character is being added to</param>
        /// <param name="position">The position in the world that the character is being added to</param>
        /// <param name="texture">The texture that is being used for the character</param>
        /// <param name="size">The size of the texture</param>
        public CompositeCharacter(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size)
        {
            if (size.X > size.Y)
            {
                throw new Exception("Cannot make character with width > height");
            }

            texWidth = (int)size.X;
            texHeight = (int)size.Y;

            State = CharState.Idle;
            Direction = CharDirection.Right;

            body.OnCollision += new OnCollisionEventHandler(OnBodyCollision);
            wheel.OnCollision += new OnCollisionEventHandler(OnWheelCollision);
            body.OnSeparation += new OnSeparationEventHandler(body_OnSeparation);
        }

        /// <summary>
        /// Attached body and wheel together to make a character
        /// </summary>
        /// <param name="world">The world that the character is being added to</param>
        /// <param name="position">The position in the world that the character is being added to</param>
        /// <param name="mass">The mass of the character</param>
        protected override void SetUpPhysics(World world, Vector2 position, float mass)
        {
            float upperBodyHeight = size.Y - (size.X / 2);

            // Create upper body
            body = BodyFactory.CreateRectangle(world, (float)size.X, (float)upperBodyHeight, mass / 2);
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.1f;
            body.Friction = 0.5f;
            body.Position = ConvertUnits.ToSimUnits(position) - (Vector2.UnitY * (size.X / 4));

            centerOffset = position.Y - (float)ConvertUnits.ToDisplayUnits(body.Position.Y);

            fixedAngleJoint = JointFactory.CreateFixedAngleJoint(world, body);

            // Create lower body
            wheel = BodyFactory.CreateCircle(world, (float)size.X / 2, mass / 2);
            wheel.Position = body.Position + Vector2.UnitY * (upperBodyHeight / 2);
            wheel.BodyType = BodyType.Dynamic;
            wheel.Restitution = 0.1f;

            // Connecting bodies
            motor = JointFactory.CreateRevoluteJoint(world, body, wheel, Vector2.Zero);

            motor.MotorEnabled = true;
            motor.MaxMotorTorque = 1000f;
            motor.MotorSpeed = 0;

            wheel.IgnoreCollisionWith(body);
            body.IgnoreCollisionWith(wheel);

            wheel.Friction = float.MaxValue;
        }

        /// <summary>
        /// Called when the character's wheel collides with something and switches character state to idle from jumping
        /// </summary>
        /// <param name="fix1">The first Fixture obj</param>
        /// <param name="fix2">The second Fixture obj</param>
        /// <param name="contact">The contact point</param>
        /// <returns></returns>
        public bool OnWheelCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (State == CharState.Jumping)
            {
                State = CharState.Idle;
            }
            return true;
        }

        /// <summary>
        /// Called when the character's body collides with something and changes state to Wallslide from Jumping
        /// </summary>
        /// <param name="fix1">The first Fixture obj</param>
        /// <param name="fix2">The second Fixture obj</param>
        /// <param name="contact">The contact point</param>
        /// <returns></returns>
        public bool OnBodyCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (State == CharState.Jumping)
            {
                State = CharState.Wallslide;
            }
            return true;
        }

        /// <summary>
        /// Called when the character separates from the wall and changes state back to Jumping from Wallslide
        /// </summary>
        /// <param name="fixtureA">The first fixture obj</param>
        /// <param name="fixtureB">The second fixture obj</param>
        void body_OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            if (State == CharState.Wallslide)
            {
                State = CharState.Jumping;
            }
        }

        public void Update(GameTime gameTime)
        {
            //HandleInput();
            spriteTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (spriteTimer > spriteInterval)
            {
                UpdateSprite();
                spriteTimer = 0f;
            }
        }

        private void UpdateSprite()
        {
            if(State == CharState.Running)
            {
                Direction = body.LinearVelocity.X < 0 ? CharDirection.Left : CharDirection.Right;
                SpriteY = State == CharState.Running ? 1 : 2;
                SpriteX = Direction == CharDirection.Left ? 3 : 0;

                if (prevState == state && prevDirection == direction)
                {
                    SpriteX += (prevSpriteX + 1) % 3;
                }
            }
            else
            {
                if (State != CharState.Idle)
                {
                    Direction = body.LinearVelocity.X < 0 ? CharDirection.Left : CharDirection.Right;
                    SpriteY = 2;
                }
                else
                {
                    SpriteY = 0;
                }
                SpriteX = Direction == CharDirection.Left ? 1 : 0;
            }
        }

        private Rectangle GetSpriteRect()
        {
            return new Rectangle(SpriteX * texWidth, SpriteY * texHeight, texWidth, texHeight);
        }

        /// <summary>
        /// Draw method used for drawing the character
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch obj</param>
        new public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(Size.X / (float)texWidth, Size.Y / (float)texHeight);
            //spriteBatch.Draw(texture, new Rectangle((int)ConvertUnits.ToDisplayUnits(wheel.Position.X), (int)ConvertUnits.ToDisplayUnits(wheel.Position.Y), (int)ConvertUnits.ToDisplayUnits(size.X), (int)ConvertUnits.ToDisplayUnits(size.Y)), null, Color.White, wheel.Rotation, new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, Position, GetSpriteRect(), Color.White, 0f, new Vector2(texWidth / 2.0f, texHeight / 2.0f), scale, SpriteEffects.None, 0);
        }
    }
}
