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
    /// This screen implements the actual game logic.
    /// </summary>
    class GameplayScreen : GameScreen
    {

        ContentManager Content;
        SpriteFont gameFont;

        //The world object that encapsulates all physics objects
        World world;
        Terrain terrain;

        //Camera object that follows the character
        Camera2D camera;
        //Game character controlled by user. TODO: Use a list for offline multiplayer and HUD
        CompositeCharacter player;
        HUD playerHUD;

        //Walls. Placeholders for terrain.
        PhysicsObject ground;
        PhysicsObject leftWall;
        PhysicsObject rightWall;
        PhysicsObject ceiling;

        //Projectiles
        List<Projectile> projectiles = new List<Projectile>();

        //List of paddles with different properties to be drawn to screen. Placeholder for actual interesting content.
        List<PhysicsObject> paddles;

        //Debug View. For viewing all underlying physics components of game.
        DebugViewXNA DebugView;

        //Stores the last keyboard state
        private KeyboardState lastKeyboardState;

        //Used for measuring frames per second
        private int fps = 0;
        private int frames = 0;
        private float frameTimer = 0f;

        //Font for displaying fps
        SpriteFont font;

        //Variable for the alpha transparency on pause
        float pauseAlpha;



        //TODO: Look into camera controls more
        //TODO: get MSTerrain working properly
        //The y pos is losing accuracy at some point during run time.
        //Debug more and figure out the location
        //Figure out debug mode and implement new terrain.
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            world = new World(new Vector2(0, 9.8f));

            terrain = new Terrain(world, new AABB(new Vector2(0, 0), 80, 80))
            {
                PointsPerUnit = 10,
                CellSize = 50,
                SubCellSize = 5,
                Iterations = 2
            };
            terrain.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public override void LoadContent()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            //DebugView Stuff
            Settings.EnableDiagnostics = true;
            DebugView = new DebugViewXNA(world);
            DebugView.LoadContent(ScreenManager.GraphicsDevice, Content);

            //Create camera using current viewport. Track a body without rotation.
            camera = new Camera2D(ScreenManager.GraphicsDevice);
            camera.Zoom = .2f;

            gameFont = Content.Load<SpriteFont>("gamefont");

            // ----------------------------------------------------------
            Texture2D terrainTex = Content.Load<Texture2D>("ground");
            terrain.CreateRandomTerrain(terrainTex, new Vector2(0, 0));
            //terrain.ApplyTexture(terrainTex, new Vector2(terrainTex.Width-10, -170), InsideTerrainTest);

            font = Content.Load<SpriteFont>("font");

            
            player = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f), ScreenManager.CharacterColor);

            //Create HUD
            playerHUD = new HUD(ScreenManager.Game, player, ScreenManager.Game.Content, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(playerHUD);

            // Set camera to track player
            camera.TrackingBody = player.body;

            //Create walls
            ground = new StaticPhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height - 12.5f),
                Content.Load<Texture2D>("platformTex"), new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, 25.0f));
            //leftWall = new StaticPhysicsObject(world, new Vector2(12.5f, GraphicsDevice.Viewport.Height / 2.0f),
            //Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));
            //rightWall = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width - 12.5f, GraphicsDevice.Viewport.Height / 2.0f),
            //Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));
            ceiling = new StaticPhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, 12.5f),
                Content.Load<Texture2D>("platformTex"), new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, 25.0f));

            //Instantiate a list of paddles to be used
            paddles = new List<PhysicsObject>();

            // Creates a simple paddle which center is anchored
            // in the background. It can rotate freely
            PhysicsObject simplePaddle = new PhysicsObject(world, new Vector2(),
                Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

            JointFactory.CreateFixedRevoluteJoint(world, simplePaddle.body, ConvertUnits.ToSimUnits(new Vector2(0, 0)),
                ConvertUnits.ToSimUnits(new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f - 150, ScreenManager.GraphicsDevice.Viewport.Height - 300)));

            paddles.Add(simplePaddle);

            // Creates a motorized paddle which left side is anchored in the background
            // it will rotate slowly but the motor is not set too strong that
            // it can push everything away
            PhysicsObject motorPaddle = new PhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height - 280),
                Content.Load<Texture2D>("Paddle"), new Vector2(128, 16), 10);

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

            PhysicsObject staticPaddle = new StaticPhysicsObject(world, new Vector2(250, ground.Position.Y - 72), Content.Load<Texture2D>("Paddle"), new Vector2(128, 16));

            paddles.Add(staticPaddle);

            // ----------------------------------------------------------

            // Sleep for the loading screen
            Thread.Sleep(500);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        public override void UnloadContent()
        {
            ScreenManager.Game.Components.Remove(playerHUD);
            Content.Unload();
        }

        #region Update and Draw


        //TODO: Move the Update and HandleInput methods together
        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            //UPDATES EACH PROJECTILE IN THE GAME
            foreach(Projectile projectile in projectiles)
            {
                projectile.UpdateProjectile(gameTime);
            }

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                //Stores state of an xbox controller
                GamePadState currentState = GamePad.GetState(PlayerIndex.One);
                KeyboardState keyboardState = Keyboard.GetState();

                //TODO: Remove this
                if (keyboardState.IsKeyDown(Keys.X) && !lastKeyboardState.IsKeyDown(Keys.X) || currentState.Buttons.X == ButtonState.Pressed)
                {
                    DrawCircleOnMap(ConvertUnits.ToSimUnits(player.Position), 1);
                    terrain.RegenerateTerrain();
                }
                if (keyboardState.IsKeyDown(Keys.F1) && !lastKeyboardState.IsKeyDown(Keys.F1))
                {
                    EnableOrDisableFlags(DebugViewFlags.Shape);
                }
                else if (keyboardState.IsKeyDown(Keys.F2) && !lastKeyboardState.IsKeyDown(Keys.F2))
                {
                    EnableOrDisableFlags(DebugViewFlags.DebugPanel);
                }
                else if (keyboardState.IsKeyDown(Keys.F3) && !lastKeyboardState.IsKeyDown(Keys.F3))
                {
                    EnableOrDisableFlags(DebugViewFlags.PerformanceGraph);
                }
                else if (keyboardState.IsKeyDown(Keys.F4) && !lastKeyboardState.IsKeyDown(Keys.F4))
                {
                    EnableOrDisableFlags(DebugViewFlags.AABB);
                }
                else if (keyboardState.IsKeyDown(Keys.F5) && !lastKeyboardState.IsKeyDown(Keys.F5))
                {
                    EnableOrDisableFlags(DebugViewFlags.CenterOfMass);
                }
                else if (keyboardState.IsKeyDown(Keys.F6) && !lastKeyboardState.IsKeyDown(Keys.F6))
                {
                    EnableOrDisableFlags(DebugViewFlags.Joint);
                }
                else if (keyboardState.IsKeyDown(Keys.F7) && !lastKeyboardState.IsKeyDown(Keys.F7))
                {
                    EnableOrDisableFlags(DebugViewFlags.ContactPoints);
                }
                else if (keyboardState.IsKeyDown(Keys.F8) && !lastKeyboardState.IsKeyDown(Keys.F8))
                {
                    EnableOrDisableFlags(DebugViewFlags.ContactNormals);
                }
                else if (keyboardState.IsKeyDown(Keys.F9) && !lastKeyboardState.IsKeyDown(Keys.F9))
                {
                    EnableOrDisableFlags(DebugViewFlags.PolygonPoints);
                }
                else if (keyboardState.IsKeyDown(Keys.F10) && !lastKeyboardState.IsKeyDown(Keys.F10))
                {
                    EnableOrDisableFlags(DebugViewFlags.Pair);
                }
                else if (keyboardState.IsKeyDown(Keys.F11) && !lastKeyboardState.IsKeyDown(Keys.F11))
                {
                    EnableOrDisableFlags(DebugViewFlags.Controllers);
                }
                else if (keyboardState.IsKeyDown(Keys.F12) && !lastKeyboardState.IsKeyDown(Keys.F12))
                {
                    EnableOrDisableFlags(DebugViewFlags.Shape | DebugViewFlags.Joint
                       | DebugViewFlags.AABB | DebugViewFlags.Pair | DebugViewFlags.CenterOfMass
                       | DebugViewFlags.DebugPanel | DebugViewFlags.ContactPoints
                       | DebugViewFlags.ContactNormals | DebugViewFlags.PolygonPoints
                       | DebugViewFlags.PerformanceGraph | DebugViewFlags.Controllers);
                }

                if (currentState.IsConnected)
                {
                    player.move(currentState);
                }
                else
                {
                    player.move();
                }

                player.Update(gameTime);
                camera.Update(gameTime);

                //Notifies the world that time has progressed.
                //Collision detection, integration, and constraint solution are performed
                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

                lastKeyboardState = keyboardState;
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
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            frames++;
            frameTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameTimer > 1000)
            {
                fps = frames;
                frameTimer = 0f;
                frames = 0;
            }

            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            Matrix proj = camera.SimProjection;
            Matrix view = camera.SimView;
            DebugView.RenderDebugData(ref proj, ref view);
            
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //Begins spriteBatch with the default sort mode, alpha blending on sprites, and a camera.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.View);

            terrain.Draw(spriteBatch);
            // Draw the fps to screen
            spriteBatch.DrawString(font, "fps: " + fps, camera.Position - new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 10, ScreenManager.GraphicsDevice.Viewport.Height / 2 - 10), Color.White);
            ground.Draw(spriteBatch);
            //leftWall.Draw(spriteBatch);
            //rightWall.Draw(spriteBatch);
            ceiling.Draw(spriteBatch);

            player.Draw(spriteBatch);

            foreach (PhysicsObject paddle in paddles)
            {
                paddle.Draw(spriteBatch);
            }

            //--------NEED TO REWORK--------
            //checks to see if a player is firing through their own input, creates a projectile associated with that player.
            /* TODO:
             * Need to add projectile types, this means that it will select a players selected weapon and pass it into projectile.
             * Based on this we can subtract the nessesary mana / damage / rate of fire
             * Change if statement to include players mana compared to selected spells requirement
             */
            if(player.canFire)
            {
                if (player.Mana > 20)
                {
                    Console.Write("POSITION: " + player.Position + " CURSOR: " + camera.ConvertScreenToWorld(playerHUD.cursorPos) + "\n");
                    Projectile projectile = new Projectile(world, player.Position, Content.Load<Texture2D>("projectile_fire"), new Vector2(10.0f, 10.0f), ConvertUnits.ToDisplayUnits(camera.ConvertScreenToWorld(playerHUD.cursorPos)), player);
                    player.Mana -= 20;
                    projectiles.Add(projectile);
                }
                player.canFire = false;
            }

            foreach (PhysicsObject projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            //-----------------------

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        private void EnableOrDisableFlags(DebugViewFlags flags)
        {
            if ((DebugView.Flags & flags) != 0)
                DebugView.RemoveFlags(flags);
            else
                DebugView.AppendFlags(flags);
        }

        private void DrawCircleOnMap(Vector2 center, sbyte value)
        {
            for (float by = -2.5f; by < 2.5f; by += 0.1f)
            {
                for (float bx = -2.5f; bx < 2.5f; bx += 0.1f)
                {
                    if ((bx * bx) + (by * by) < 2.5f * 2.5f)
                    {
                        float ax = bx + center.X;
                        float ay = by + center.Y;
                        terrain.ModifyTerrain(new Vector2(ax, ay), value);
                    }
                }
            }
        }


        #endregion
    }
}
