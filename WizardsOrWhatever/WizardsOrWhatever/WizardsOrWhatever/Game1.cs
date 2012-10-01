using System;
using System.Collections.Generic;
//using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;

namespace WizardsOrWhatever
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        World world;

        CompositeCharacter player;

        PhysicsObject ground;
        PhysicsObject leftWall;
        PhysicsObject rightWall;
        PhysicsObject ceiling;

        List<PhysicsObject> paddles;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            world = new World(new Vector2(0, 9.8f));

            Vector2 size = new Vector2(50, 50);

            player = new CompositeCharacter(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f));

            ground = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 12.5f),
                Content.Load<Texture2D>("platformTex"), new Vector2(GraphicsDevice.Viewport.Width, 25.0f));

            leftWall = new StaticPhysicsObject(world, new Vector2(12.5f, GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));

            rightWall = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width - 12.5f, GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));

            ceiling = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, 12.5f),
                Content.Load<Texture2D>("platformTex"), new Vector2(GraphicsDevice.Viewport.Width, 25.0f));

            // *** Stolen from demo ***

            paddles = new List<PhysicsObject>();

            // Creates a simple paddle which center is anchored
            // in the background. It can rotate freely
            PhysicsObject simplePaddle = new PhysicsObject(world, new Vector2(),
                Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            JointFactory.CreateFixedRevoluteJoint(world, simplePaddle.body, ConvertUnits.ToSimUnits(new Vector2(0, 0)),
                ConvertUnits.ToSimUnits(new Vector2(GraphicsDevice.Viewport.Width / 2.0f - 150, GraphicsDevice.Viewport.Height - 300)));

            paddles.Add(simplePaddle);

            // Creates a motorized paddle which left side is anchored in the background
            // it will rotate slowly but the motoro is not set too strong that
            // it can push everything away
            PhysicsObject motorPaddle = new PhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 280),
                Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            var j = JointFactory.CreateFixedRevoluteJoint(world, motorPaddle.body, ConvertUnits.ToSimUnits(new Vector2(-48, 0)),
                ConvertUnits.ToSimUnits(new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 280)));

            // rotate 1/4 of a circle per second
            j.MotorSpeed = MathHelper.PiOver2;
            // have little torque (power) so it can push away a few blocks
            j.MotorTorque = 50;
            j.MotorEnabled = true;
            j.MaxMotorTorque = 100;

            paddles.Add(motorPaddle);

            // Use two line joints (a sort of springs) to create a trampoline
            PhysicsObject trampolinePaddle = new PhysicsObject(world, new Vector2(600, ground.Position.Y - 175), Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            var l = JointFactory.CreateLineJoint(ground.body, trampolinePaddle.body, ConvertUnits.ToSimUnits(trampolinePaddle.Position - new Vector2(64, 0)), Vector2.UnitY);

            l.CollideConnected = true;
            l.Frequency = 2.0f;
            l.DampingRatio = 0.05f;

            var r = JointFactory.CreateLineJoint(ground.body, trampolinePaddle.body, ConvertUnits.ToSimUnits(trampolinePaddle.Position + new Vector2(64, 0)), Vector2.UnitY);

            r.CollideConnected = true;
            r.Frequency = 2.0f;
            r.DampingRatio = 0.05f;

            world.AddJoint(l);
            world.AddJoint(r);

            paddles.Add(trampolinePaddle);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            player.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();
            if (player.State == Character.CharState.Wallslide)
            {
                if (keyboardState.IsKeyDown(Keys.Left) && keyboardState.IsKeyDown(Keys.Space))
                {
                    WallJumpLeft();
                }
                else if (keyboardState.IsKeyDown(Keys.Right) && keyboardState.IsKeyDown(Keys.Space))
                {
                    WallJumpRight();
                }
            }
            else if (player.State != Character.CharState.Jumping)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    Jump();
                }
                else if (keyboardState.IsKeyDown(Keys.Left))
                {
                    RunLeft();
                }
                else if (keyboardState.IsKeyDown(Keys.Right))
                {
                    RunRight();
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                AirMove(keyboardState);
            }
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        private void Stop()
        {
            player.motor.MotorSpeed = 0;
            player.body.LinearVelocity = new Vector2(0, player.body.LinearVelocity.Y);
            player.State = Character.CharState.Idle;
        }

        private void Jump()
        {
            player.launchSpeed = player.body.LinearVelocity.X;
            player.body.ApplyLinearImpulse(player.jumpImpulse, player.body.Position);
            player.State = Character.CharState.Jumping;
        }

        private void RunRight()
        {
            player.motor.MotorSpeed = player.runSpeed;
            player.State = Character.CharState.Running;
        }

        private void RunLeft()
        {
            player.motor.MotorSpeed = -player.runSpeed;
            player.State = Character.CharState.Running;
        }

        private void AirMove(KeyboardState keyboardState)
        {
            if ((keyboardState.IsKeyDown(Keys.Left) && player.launchSpeed < 0) || (keyboardState.IsKeyDown(Keys.Right) && player.launchSpeed > 0))
            {
                player.body.LinearVelocity = new Vector2(player.launchSpeed, player.body.LinearVelocity.Y);
            }
            else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.Left))
            {
                player.body.LinearVelocity = new Vector2(-player.launchSpeed, player.body.LinearVelocity.Y);
            }
        }

        private void WallJumpLeft()
        {
            if (player.launchSpeed > 0)
            {
                player.body.LinearVelocity = new Vector2(-player.launchSpeed, player.body.LinearVelocity.Y);
                Jump();
            }
        }

        private void WallJumpRight()
        {
            if (player.launchSpeed < 0)
            {
                player.body.LinearVelocity = new Vector2(player.launchSpeed, player.body.LinearVelocity.Y);
                Jump();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector2 size = new Vector2(50, 50);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            ground.Draw(spriteBatch);
            leftWall.Draw(spriteBatch);
            rightWall.Draw(spriteBatch);
            ceiling.Draw(spriteBatch);

            player.Draw(spriteBatch);

            foreach (PhysicsObject paddle in paddles)
            {
                paddle.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
