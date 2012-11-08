#region Using Statements
using System.IO;
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
using System.Net;
using System.Net.Sockets;
#endregion

namespace WizardsOrWhatever.Screen
{
    class GameplayScreenMulti : GameScreen
    {

        ContentManager Content;
        SpriteFont gameFont;

        //The world object that encapsulates all physics objects
        World world;
        MSTerrain terrain;

        //Camera object that follows the character
        Camera2D camera;
        //Game character controlled by user. TODO: Use a list for offline multiplayer and HUD
        //List<CompositeCharacter> Players;
        CompositeCharacter player;
        CompositeCharacter player2;
        CompositeCharacter player3;
        CompositeCharacter player4;
        HUD playerHUD;
        HUD playerHUD2;
        HUD playerHUD3;
        HUD playerHUD4;

        //Walls. Placeholders for terrain.
        PhysicsObject ground;
        PhysicsObject leftWall;
        PhysicsObject rightWall;
        PhysicsObject ceiling;
        bool isPlayer2 = false;
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
        public GameplayScreenMulti()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);




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

        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public override void LoadContent()
        {
            //read write stream to the server
            readStream = new MemoryStream();
            writeStream = new MemoryStream();

            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);



            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            //DebugView Stuff
            Settings.EnableDiagnostics = true;
            DebugView = new DebugViewXNA(world);
            DebugView.LoadContent(ScreenManager.GraphicsDevice, Content);

            //Create camera using current viewport. Track a body without rotation.
            camera = new Camera2D(ScreenManager.GraphicsDevice);
            camera.EnablePositionTracking = true;
            camera.EnableRotationTracking = false;

            gameFont = Content.Load<SpriteFont>("gamefont");

            // ----------------------------------------------------------
            Texture2D terrainTex = Content.Load<Texture2D>("Terrain");
            terrain.ApplyTexture(terrainTex, new Vector2(terrainTex.Width - 10, -170), InsideTerrainTest);

            font = Content.Load<SpriteFont>("font");

            //Create player
            player = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f), ScreenManager.CharacterColor);

            //Create HUD
            playerHUD = new HUD(ScreenManager.Game, player, ScreenManager.Game.Content, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(playerHUD);
            if (isPlayer2)
            {
                playerHUD2 = new HUD(ScreenManager.Game, player2, ScreenManager.Game.Content, ScreenManager.SpriteBatch);

                ScreenManager.Game.Components.Add(playerHUD2);
            }

            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(IP, PORT);
            readBuffer = new byte[BUFFER_SIZE];
            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);


            /*if (player3 != null)
            {
                playerHUD3 = new HUD(ScreenManager.Game, player3, ScreenManager.Game.Content, ScreenManager.SpriteBatch);

                ScreenManager.Game.Components.Add(playerHUD3);
            }
            if (player4 != null)
            {
                playerHUD4 = new HUD(ScreenManager.Game, player4, ScreenManager.Game.Content, ScreenManager.SpriteBatch);

                ScreenManager.Game.Components.Add(playerHUD4);
            }*/
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
            client.Close();
            //player = null;
            //player2 = null;
            ScreenManager.Game.Components.Remove(playerHUD);
            //ScreenManager.Game.Components.Remove(playerHUD2);
            Content.Unload();
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

            //-------------------------------------


            Vector2 iPosition = new Vector2(player.Position.X, player.Position.Y);

            KeyboardState current = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;


            //-----------------------------------
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

                //---------------------------------

                //player.move(movement) = ;


                Vector2 nPosition = new Vector2(player.Position.X, player.Position.Y);

                Vector2 deltap = Vector2.Subtract(nPosition, iPosition);
                if (deltap != Vector2.Zero)
                {
                    writeStream.Position = 0;
                    writer.Write((byte)2);
                    writer.Write(deltap.X);
                    writer.Write(-deltap.Y);
                    SendData(GetDataFromMemoryStream(writeStream));
                }

                player.Update(gameTime);

                //----------------------------------
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

            // Draw the fps to screen
            spriteBatch.DrawString(font, "fps: " + fps, camera.Position - new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 10, ScreenManager.GraphicsDevice.Viewport.Height / 2 - 10), Color.White);
            ground.Draw(spriteBatch);
            //leftWall.Draw(spriteBatch);
            //rightWall.Draw(spriteBatch);
            ceiling.Draw(spriteBatch);
            if (player != null)
            {
                player.Draw(spriteBatch);
            }
            if (isPlayer2)
            {
                player2.Draw(spriteBatch);
            }/*
            if (player3 != null)
            {
                player3.Draw(spriteBatch);
            }
            if (player4 != null)
            {
                player4.Draw(spriteBatch);
            }*/
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private bool InsideTerrainTest(Color color)
        {
            return color == Color.Black;
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
        #region Networking
        int pindex = 1;
        TcpClient client;
        string IP = "127.0.0.1";
        int PORT = 1490;
        int BUFFER_SIZE = 2048;
        byte[] readBuffer;
        MemoryStream readStream, writeStream;

        BinaryReader reader;
        BinaryWriter writer;
        private void StreamReceived(IAsyncResult ar)
        {
            //MessageBox.Show("Message Received....Thank you for the message, server!");

            int bytesRead = 0;

            try
            {
                lock (client.GetStream())
                {
                    bytesRead = client.GetStream().EndRead(ar);
                }
            }
            catch (Exception ex)
            {

            }

            if (bytesRead == 0)
            {
                client.Close();
                return;
            }

            byte[] data = new byte[bytesRead];

            for (int i = 0; i < bytesRead; i++)
                data[i] = readBuffer[i];

            ProcessData(data);

            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);
        }
        private void ProcessData(byte[] data)
        {
            readStream.SetLength(0);
            readStream.Position = 0;//reset data

            readStream.Write(data, 0, data.Length);//read data
            readStream.Position = 0;

            Protocols p = new Protocols();
            try
            {


                //p = (Protocol)reader.ReadByte();
                p.setData(reader.ReadByte());
                /*if (GameScreen.isExiting)
                {
                    System.Diagnostics.Debug.WriteLine
                }*/
                if (p.getData() == 1)//p == Protocol.Connected)
                {
                    //pindex++;
                    System.Diagnostics.Debug.WriteLine(pindex);
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();
                    if (!isPlayer2) //player2 == null)
                    {
                        isPlayer2 = true;
                        player2 = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f), ScreenManager.CharacterColor);
                        writeStream.Position = 0;
                        writer.Write(p.getData());
                        SendData(GetDataFromMemoryStream(writeStream));
                        //pindex = pindex - 1;
                    }/*
                    if (pindex == 4 && player3 == null)
                    {
                        player3 = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                              Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f));
                        writeStream.Position = 0;
                        writer.Write(p.getData());
                        SendData(GetDataFromMemoryStream(writeStream));
                    }
                    if (pindex == 7 && player4 == null)
                    {
                        player4 = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                              Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f));
                        writeStream.Position = 0;
                        writer.Write(p.getData());
                        SendData(GetDataFromMemoryStream(writeStream));
                    }*/
                }

                else if (p.getData() == 0)//p == Protocol.Disconnected)
                {
                    //pindex--;
                    //System.Diagnostics.Debug.WriteLine(pindex);
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();
                    isPlayer2 = false;
                    //player2 = null;
                }
                else if (p.getData() == 2)
                {
                    float px = reader.ReadSingle();
                    float py = reader.ReadSingle();
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();
                    player2.Position = new Vector2(player2.Position.X + px, player2.Position.Y - py);
                }
            }
            catch (Exception e)
            {

            }
        }
        /// <summary>
        /// Converts a MemoryStream to a byte array
        /// </summary>
        /// <param name="ms">MemoryStream to convert</param>
        /// <returns>Byte array representation of the data</returns>
        private byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;

            //Async method called this, so lets lock the object to make sure other threads/async calls need to wait to use it.
            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
            }

            return result;
        }
        /// <summary>
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendData(byte[] b)
        {
            //Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (client.GetStream())
                {
                    client.GetStream().BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch (Exception e)
            {

            }
        }
        #endregion
    }
}