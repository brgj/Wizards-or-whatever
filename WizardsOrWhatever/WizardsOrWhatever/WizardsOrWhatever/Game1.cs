using System;
using System.Collections.Generic;
using System.Linq;
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

        Player player;

        DrawablePhysicsObject ground;
        DrawablePhysicsObject leftWall;
        DrawablePhysicsObject rightWall;
        DrawablePhysicsObject ceiling;

        List<DrawablePhysicsObject> paddles;

        float launchSpeed;

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

            player = new Player(world, Content.Load<Texture2D>("Player"), new Vector2(35.0f, 50.0f));
            player.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height / 2.0f);

            ground = new DrawablePhysicsObject(world, Content.Load<Texture2D>("platformTex"), new Vector2(GraphicsDevice.Viewport.Width, 25.0f), 1000.0f);
            ground.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 12.5f);
            ground.body.BodyType = BodyType.Static;

            //leftWall = new DrawablePhysicsObject(world, Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height), 1000.0f);
            //leftWall.Position = new Vector2(12.5f, GraphicsDevice.Viewport.Height / 2.0f);
            //leftWall.body.BodyType = BodyType.Static;

            //rightWall = new DrawablePhysicsObject(world, Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height), 1000.0f);
            //rightWall.Position = new Vector2(GraphicsDevice.Viewport.Width - 12.5f, GraphicsDevice.Viewport.Height / 2.0f);
            //rightWall.body.BodyType = BodyType.Static;

            //ceiling = new DrawablePhysicsObject(world, Content.Load<Texture2D>("platformTex"), new Vector2(GraphicsDevice.Viewport.Width, 25.0f), 1000.0f);
            //ceiling.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, 12.5f);
            //ceiling.body.BodyType = BodyType.Static;

            // *** Stolen from demo ***

            paddles = new List<DrawablePhysicsObject>();

            // Creates a simple paddle which center is anchored
            // in the background. It can rotate freely
            DrawablePhysicsObject simplePaddle = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            JointFactory.CreateFixedRevoluteJoint(world, simplePaddle.body, CoordinateHelper.ToWorld(new Vector2(0, 0)),
                CoordinateHelper.ToWorld(new Vector2(GraphicsDevice.Viewport.Width / 2.0f - 150, GraphicsDevice.Viewport.Height - 300)));

            paddles.Add(simplePaddle);

            // Creates a motorized paddle which left side is anchored in the background
            // it will rotate slowly but the motoro is not set too strong that
            // it can push everything away
            DrawablePhysicsObject motorPaddle = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            var j = JointFactory.CreateFixedRevoluteJoint(world, motorPaddle.body, CoordinateHelper.ToWorld(new Vector2(-48, 0)),
                CoordinateHelper.ToWorld(new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 280)));

            // rotate 1/4 of a circle per second
            j.MotorSpeed = MathHelper.PiOver2;
            // have little torque (power) so it can push away a few blocks
            j.MotorTorque = 3;
            j.MotorEnabled = true;
            j.MaxMotorTorque = 10;

            paddles.Add(motorPaddle);

            // Use two line joints (a sort of springs) to create a trampoline
            DrawablePhysicsObject trampolinePaddle = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            trampolinePaddle.Position = new Vector2(600, ground.Position.Y - 175);

            var l = JointFactory.CreateLineJoint(ground.body, trampolinePaddle.body, CoordinateHelper.ToWorld(trampolinePaddle.Position - new Vector2(64, 0)), Vector2.UnitY);

            l.CollideConnected = true;
            l.Frequency = 2.0f;
            l.DampingRatio = 0.05f;

            var r = JointFactory.CreateLineJoint(ground.body, trampolinePaddle.body, CoordinateHelper.ToWorld(trampolinePaddle.Position + new Vector2(64, 0)), Vector2.UnitY);

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

            KeyboardState keyboardState = Keyboard.GetState();
            if (player.state != Player.CharState.Jumping)
            {
                //if (keyboardState.IsKeyDown(Keys.Space))
                //{
                //    Jump();
                //}
                if (keyboardState.IsKeyDown(Keys.Left))
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
            player.body.LinearVelocity = new Vector2(0, player.body.LinearVelocity.Y);
            player.state = Player.CharState.Idle;
        }

        //private void Jump()
        //{
        //    launchSpeed = player.body.LinearVelocity.X;
        //    player.body.ApplyLinearImpulse(player.jumpImpulse, player.body.Position);
        //    player.state = Player.CharState.Jumping;
        //}

        private void RunRight()
        {
            player.body.LinearVelocity = new Vector2(player.runSpeed, player.body.LinearVelocity.Y);
            player.state = Player.CharState.Running;
        }

        private void RunLeft()
        {
            player.body.LinearVelocity = new Vector2(-player.runSpeed, player.body.LinearVelocity.Y);
            player.state = Player.CharState.Running;
        }

        private void AirMove(KeyboardState keyboardState)
        {
            if ((player.launchSpeed > 0 && keyboardState.IsKeyDown(Keys.Left)) || (player.launchSpeed < 0 && keyboardState.IsKeyDown(Keys.Right)))
            {
                player.body.LinearVelocity = new Vector2(0, player.body.LinearVelocity.Y);
            }
            else
            {
                player.body.LinearVelocity = new Vector2(launchSpeed, player.body.LinearVelocity.Y);
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
            //leftWall.Draw(spriteBatch);
            //rightWall.Draw(spriteBatch);
            //ceiling.Draw(spriteBatch);

            player.Draw(spriteBatch);

            foreach (DrawablePhysicsObject paddle in paddles)
            {
                paddle.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
