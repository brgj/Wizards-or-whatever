#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;
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
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using FarseerPhysics;
#endregion

namespace WizardsOrWhatever
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {

        ContentManager content;
        SpriteFont gameFont;

        // ------------------------------------
        // ----------- Game1 stuffs -----------
        // ------------------------------------


        //int viewportHeight, viewportWidth;

        World world;
        MSTerrain terrain;
        Camera camera;

        CompositeCharacter player;

        //Walls. Placeholders for terrain.
        PhysicsObject ground;
        PhysicsObject leftWall;
        PhysicsObject rightWall;
        PhysicsObject ceiling;

        List<PhysicsObject> paddles;

        private Matrix projection, view;


        // ------------------------------------9--
        // ------------------------------------

        //Vector2 playerPosition = new Vector2(100, 100);
        //Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();

        float pauseAlpha;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            //viewportHeight = vpHeight;
            //viewportWidth = vpWidth;

            world = new World(new Vector2(0, 9.8f));

            terrain = new MSTerrain(world, new AABB(new Vector2(0, 0), 80, 80))
            {
                PointsPerUnit = 10,
                CellSize = 50,
                SubCellSize = 5,
                Decomposer = Decomposer.Earclip,
                Iterations = 2
            };
            terrain.Initialize();


            // DebugView stuff
            /*
            Settings.EnableDiagnostics = true;
            DebugView = new DebugViewXNA(world);
            DebugView.LoadContent(GraphicsDevice, Content);
            projection = Matrix.CreateOrthographic(
                graphics.PreferredBackBufferWidth / 100.0f,
                -graphics.PreferredBackBufferHeight / 100.0f, 0, 1000000);
            Vector3 campos = new Vector3();
            campos.X = (-graphics.PreferredBackBufferWidth / 2) / 100.0f;
            campos.Y = (graphics.PreferredBackBufferHeight / 2) / -100.0f;
            campos.Z = 0;
            Matrix tran = Matrix.Identity;
            tran.Translation = campos;
            view = tran;
             */
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            // ----------------------------------------------------------


            // Create camera using current viewport
            camera = new Camera(ScreenManager.GraphicsDevice.Viewport);

            //terrain.ApplyTexture(Content.Load<Texture2D>("Terrain"), new Vector2(200, 0), InsideTerrainTest);

            //Create player
            player = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f));

            //Create walls
            ground = new StaticPhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height - 12.5f),
                content.Load<Texture2D>("platformTex"), new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, 25.0f));
            //leftWall = new StaticPhysicsObject(world, new Vector2(12.5f, GraphicsDevice.Viewport.Height / 2.0f),
            //Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));
            //rightWall = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width - 12.5f, GraphicsDevice.Viewport.Height / 2.0f),
            //Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));
            ceiling = new StaticPhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, 12.5f),
                content.Load<Texture2D>("platformTex"), new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, 25.0f));

            //Instantiate a list of paddles to be used
            paddles = new List<PhysicsObject>();

            // Creates a simple paddle which center is anchored
            // in the background. It can rotate freely
            PhysicsObject simplePaddle = new PhysicsObject(world, new Vector2(),
                content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            JointFactory.CreateFixedRevoluteJoint(world, simplePaddle.body, ConvertUnits.ToSimUnits(new Vector2(0, 0)),
                ConvertUnits.ToSimUnits(new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f - 150, ScreenManager.GraphicsDevice.Viewport.Height - 300)));

            paddles.Add(simplePaddle);

            // Creates a motorized paddle which left side is anchored in the background
            // it will rotate slowly but the motor is not set too strong that
            // it can push everything away
            PhysicsObject motorPaddle = new PhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height - 280),
                content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            var j = JointFactory.CreateFixedRevoluteJoint(world, motorPaddle.body, ConvertUnits.ToSimUnits(new Vector2(-48, 0)),
                ConvertUnits.ToSimUnits(new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height - 280)));

            // rotate 1/4 of a circle per second
            j.MotorSpeed = MathHelper.PiOver2;
            // have little torque (power) so it can push away a few blocks
            j.MotorTorque = 50;
            j.MotorEnabled = true;
            j.MaxMotorTorque = 100;

            paddles.Add(motorPaddle);

            // Use two line joints (a sort of springs) to create a trampoline
            PhysicsObject trampolinePaddle = new PhysicsObject(world, new Vector2(600, ground.Position.Y - 175), content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

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






            // ----------------------------------------------------------




            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                //Stores state of an xbox controller
                GamePadState currentState = GamePad.GetState(PlayerIndex.One);

                if (currentState.IsConnected)
                {
                    player.move(currentState);
                }
                else
                {
                    player.move();
                }

                player.Update(gameTime);
                camera.Update(player);

                //Notifies the world that time has progressed.
                //Collision detection, integration, and constraint solution are performed
                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            //else
            //{
            //    // Otherwise move the player position.
            //    Vector2 movement = Vector2.Zero;

            //    if (keyboardState.IsKeyDown(Keys.Left))
            //        movement.X--;

            //    if (keyboardState.IsKeyDown(Keys.Right))
            //        movement.X++;

            //    if (keyboardState.IsKeyDown(Keys.Up))
            //        movement.Y--;

            //    if (keyboardState.IsKeyDown(Keys.Down))
            //        movement.Y++;

            //    Vector2 thumbstick = gamePadState.ThumbSticks.Left;

            //    movement.X += thumbstick.X;
            //    movement.Y -= thumbstick.Y;

            //    if (movement.Length() > 1)
            //        movement.Normalize();

            //    playerPosition += movement * 2;
            //}
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //Begins spriteBatch with the default sort mode, alpha blending on sprites, and a camera.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.transform);

            ground.Draw(spriteBatch);
            //leftWall.Draw(spriteBatch);
            //rightWall.Draw(spriteBatch);
            ceiling.Draw(spriteBatch);

            player.Draw(spriteBatch);

            foreach (PhysicsObject paddle in paddles)
            {
                paddle.Draw(spriteBatch);
            }

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
