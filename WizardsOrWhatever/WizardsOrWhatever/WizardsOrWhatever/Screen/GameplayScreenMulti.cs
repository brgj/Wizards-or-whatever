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
using System.IO;
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

namespace WizardsOrWhatever
{
    /// <summary>
    /// This screen implements the actual game logic.
    /// </summary>
    class GameplayScreenMulti : GameScreen
    {
        ContentManager Content;
        SpriteFont gameFont;

        // Backgrounds
        GameBackground skyLayer;

        //The world object that encapsulates all physics objects
        World world;
        Terrain terrain;

        //Camera object that follows the character
        Camera2D camera;
        //Game character controlled by user. TODO: Use a list for offline multiplayer and HUD
        CompositeCharacter player;
        Dictionary<byte, CompositeCharacter> playerMap = new Dictionary<byte,CompositeCharacter>();
        HUD playerHUD;

        //Walls. Placeholders for terrain.
        PhysicsObject leftWall;
        PhysicsObject rightWall;
        PhysicsObject ground;

        //Projectiles
        List<Projectile> projectiles = new List<Projectile>();

        //Explosions
        List<Explosion> explosions = new List<Explosion>();

        //List of paddles with different properties to be drawn to screen. Placeholder for actual interesting content.
        //List<PhysicsObject> paddles;

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

        Texture2D projectileTexYellow;
        Texture2D projectileTexRed;
        Texture2D projectileTexBlue;
        Texture2D explosionTex;

        byte id = 27;

        bool terrainMaster = false;

        int index;

        byte[] array;

        

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreenMulti()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            world = new World(new Vector2(0, 9.8f));

            terrain = new Terrain(world, new AABB(new Vector2(0, 25), 200, 50))
            {
                PointsPerUnit = 10,
                CellSize = 50,
                SubCellSize = 5
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

            terrain.LoadContent(ScreenManager.GraphicsDevice);

            //Create camera using current viewport. Track a body without rotation.
            camera = new Camera2D(ScreenManager.GraphicsDevice);

            gameFont = Content.Load<SpriteFont>("gamefont");


            // Back ground stuff --------------------------------
            List<Texture2D> list = new List<Texture2D>();
            list.Add(ScreenManager.Game.Content.Load<Texture2D>("GBackground"));
            skyLayer = new GameBackground(list, camera.Position)
            {
                Height = ScreenManager.GraphicsDevice.Viewport.Height,
                Width = ScreenManager.GraphicsDevice.Viewport.Width,
                SpeedX = 0.3f
            };
            //---------------------------------------------------


            Texture2D terrainTex = Content.Load<Texture2D>("ground");

            font = Content.Load<SpriteFont>("font");


            //Create player
            player = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f), ScreenManager.CharacterColor);

            //Create HUD
            playerHUD = new HUD(ScreenManager.Game, player, ScreenManager.Game.Content, ScreenManager.SpriteBatch);
            ScreenManager.Game.Components.Add(playerHUD);

            client = new TcpClient();
            client.NoDelay = true;
            IPAddress ipadd = System.Net.IPAddress.Parse("192.168.1.64");
            client.Connect(ipadd, PORT);
            readBuffer = new byte[BUFFER_SIZE];
            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);

            Thread.Sleep(2000);

            if (id == 0)
            {
                terrain.CreateRandomTerrain(new Vector2(0, 0));
            }
            // Set camera to track player
            camera.TrackingBody = player.body;

            //Create walls
            leftWall = new StaticPhysicsObject(world, new Vector2(ConvertUnits.ToDisplayUnits(-100.0f), 0), Content.Load<Texture2D>("platformTex"),
                new Vector2(100, ConvertUnits.ToDisplayUnits(100.0f)));
            rightWall = new StaticPhysicsObject(world, new Vector2(ConvertUnits.ToDisplayUnits(100.0f), 0), Content.Load<Texture2D>("platformTex"),
                new Vector2(100, ConvertUnits.ToDisplayUnits(100.0f)));

            ground = new StaticPhysicsObject(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 4.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f + 100), Content.Load<Texture2D>("platformTex"), new Vector2(2000f, 20f));
            // Load projectile and explosion textures
            projectileTexRed = Content.Load<Texture2D>("projectile_fire_red");
            projectileTexBlue = Content.Load<Texture2D>("projectile_fire_blue");
            projectileTexYellow = Content.Load<Texture2D>("projectile_fire_yellow");
            explosionTex = Content.Load<Texture2D>("explosion");

            // ----------------------------------------------------------

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
            KeyboardState current = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            base.Update(gameTime, false, false);

            //Check if the player is dead
            //TODO: We need to send a message if the player is dead and somehow make either character invisible (maybe stop drawing it?)
            if (player.Health == 0)
            {
                player.Dead = true;
            }

            // updating the position of the background.
            skyLayer.Move(player.Position, ScreenManager.GraphicsDevice.Viewport.Height, ScreenManager.GraphicsDevice.Viewport.Width);

            //UPDATES EACH PROJECTILE IN THE GAME
            foreach (Projectile projectile in projectiles)
            {
                projectile.UpdateProjectile(gameTime);
            }

            for (int i = 0; i < explosions.Count; i++)
            {
                if (!explosions[i].UpdateParticles(gameTime))
                {
                    explosions.RemoveAt(i);
                }
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

                #region Debug Keys
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
                #endregion

                if (currentState.IsConnected)
                {
                    player.move(currentState);
                }
                else
                {
                    player.move();
                }

                camera.Update(gameTime);

                //Notifies the world that time has progressed.
                //Collision detection, integration, and constraint solution are performed
                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

                //---------------------------------

                //player.move(movement) = ;


                writeStream.Position = 0;
                writer.Write((byte)Protocol.Movement);
                writer.Write(player.Position.X);
                writer.Write(player.Position.Y);
                writer.Write(player.Dead);
                writer.Write((byte)player.Direction);
                writer.Write((byte)player.State);
                lock (client.GetStream())
                {
                    SendData(GetDataFromMemoryStream(writeStream));
                }

                player.Update(gameTime);
                foreach (CompositeCharacter p in playerMap.Values)
                {
                    p.Update(gameTime);
                }

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

            //if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            //{
            //    ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
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
            camera.Zoom = 1f;
            Matrix proj = camera.SimProjection;
            Matrix view = camera.SimView;

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //Begins spriteBatch with the default sort mode, alpha blending on sprites, and a camera.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.View);

            // Draws the game background. 
            skyLayer.Draw(spriteBatch);

            leftWall.Draw(spriteBatch);
            rightWall.Draw(spriteBatch);
            ground.Draw(spriteBatch);

            if (player != null)
            {
                player.Draw(spriteBatch);
            }
            foreach (CompositeCharacter p in playerMap.Values)
            {
                p.Draw(spriteBatch);
            }

            //checks to see if a player is firing through their own input, creates a projectile associated with that player.
            if (player.canFire)
            {
                //Yellow weapon selected
                if (player.Weapon == Character.WeaponSelect.Yellow)
                {
                    if (player.Mana >= YellowProjectile.manaCost)
                    {
                        Projectile projectile = new YellowProjectile(world, player.Position, projectileTexYellow, new Vector2(10.0f, 10.0f), ConvertUnits.ToDisplayUnits(camera.ConvertScreenToWorld(playerHUD.cursorPos)), player, SendModifyTerrainMessage);
                        player.Mana -= YellowProjectile.manaCost;
                        player.fireDelay = projectile.delay;
                        projectiles.Add(projectile);
                    }
                }
                //Red weapon selected
                else if (player.Weapon == Character.WeaponSelect.Red)
                {
                    if (player.Mana >= RedProjectile.manaCost)
                    {
                        Projectile projectile = new RedProjectile(world, player.Position, projectileTexRed, new Vector2(10.0f, 10.0f), ConvertUnits.ToDisplayUnits(camera.ConvertScreenToWorld(playerHUD.cursorPos)), player, CheckCollision);
                        player.Mana -= RedProjectile.manaCost;
                        player.fireDelay = projectile.delay;
                        projectiles.Add(projectile);
                    }
                }
                //Blue weapon selected
                else if (player.Weapon == Character.WeaponSelect.Blue)
                {
                    if (player.Mana >= BlueProjectile.manaCost)
                    {
                        Projectile projectile = new BlueProjectile(world, player.Position, projectileTexBlue, new Vector2(10.0f, 10.0f), ConvertUnits.ToDisplayUnits(camera.ConvertScreenToWorld(playerHUD.cursorPos)), player, CheckCollision);
                        player.Mana -= BlueProjectile.manaCost;
                        player.fireDelay = projectile.delay;
                        projectiles.Add(projectile);
                    }
                }
                
                player.canFire = false;
            }

            foreach (PhysicsObject projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            //-----------------------

            spriteBatch.End();
            terrain.RenderTerrain(ref proj, ref view);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, camera.View);

            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }

            spriteBatch.End();

            DebugView.RenderDebugData(ref proj, ref view);

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

        private void SendModifyTerrainMessage(Projectile p)
        {
            writeStream.Position = 0;
            writer.Write((byte)Protocol.ModifyTerrain);
            writer.Write(p.Position.X);
            writer.Write(p.Position.Y);
            lock (client.GetStream())
            {
                SendData(GetDataFromMemoryStream(writeStream));
            }
            CheckCollision(p);
        }

        private void CheckCollision(Projectile p)
        {
             DrawCircleOnMap(p.body.Position, p.level, 1);
            LaunchPlayer(player, p.Position, ConvertUnits.ToDisplayUnits(p.level), p.damage);
            terrain.RegenerateTerrain();
            explosions.Add(new Explosion(explosionTex, p.level, p.Position, p.color));
            projectiles.Remove(p);
        }

        private void fartbutt(Projectile p)
        {

        }

        private void LaunchPlayer(CompositeCharacter player, Vector2 origin, float radius, int damage)
        {
            if (player.Position.X > origin.X - radius && player.Position.Y > origin.Y - radius && player.Position.X < origin.X + radius && player.Position.Y < origin.Y + radius)
            {
                Vector2 force = player.Position - origin;
                force = Vector2.Normalize(force) * 10;
                player.body.ApplyLinearImpulse(force);
                //Damage the player being hit
                player.Health -= (int)(Math.Abs(force.X) * damage + Math.Abs(force.Y) * damage);
            }
        }

        private void DrawCircleOnMap(Vector2 center, float radius, sbyte value)
        {
            for (float by = -radius; by < radius; by += 0.1f)
            {
                for (float bx = -radius; bx < radius; bx += 0.1f)
                {
                    if ((bx * bx) + (by * by) < radius * radius)
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
            Protocol p;
            while (readStream.Position != data.Length)
            {
                try
                {
                    p = (Protocol)reader.ReadByte();
                    /*if (GameScreen.isExiting)
                    {
                        System.Diagnostics.Debug.WriteLine
                    }*/
                    if (p == Protocol.Initialize)
                    {
                        id = reader.ReadByte();
                        //if (id == 0)
                        //    terrain.CreateRandomTerrain(new Vector2(0, 0));
                    }
                    else if (p == Protocol.Connected)
                    {
                        lock (client.GetStream())
                        {
                            terrainMaster = true;
                            foreach (byte otherID in playerMap.Keys)
                            {
                                if (otherID < this.id)
                                {
                                    terrainMaster = false;
                                    break;
                                }
                            }
                            //pindex++;
                            System.Diagnostics.Debug.WriteLine(pindex);
                            byte id = reader.ReadByte();
                            string ip = reader.ReadString();
                            ScreenManager.CharacterColor = Color.White;



                            CompositeCharacter newPlayer = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                                    Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f), ScreenManager.CharacterColor);
                            newPlayer.body.IgnoreGravity = true;
                            newPlayer.wheel.IgnoreGravity = true;
                            newPlayer.body.CollidesWith = Category.Cat24;
                            newPlayer.wheel.CollidesWith = Category.Cat24;
                            playerMap.Add(id, newPlayer);

                            writeStream.Position = 0;
                            writer.Write((byte)Protocol.AddCharacter);
                            writer.Write((byte)playerMap.Count);
                            SendData(GetDataFromMemoryStream(writeStream));
                            try
                            {
                                if (terrainMaster)
                                {
                                    writeStream.Position = 0;
                                    writer.Write((byte)Protocol.CreateTerrain);
                                    writer.Write(terrain.seed1);
                                    writer.Write(terrain.seed2);
                                    writer.Write(terrain.seed3);
                                    SendData(GetDataFromMemoryStream(writeStream));
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }

                    else if (p == Protocol.Disconnected)
                    {
                        //pindex--;
                        //System.Diagnostics.Debug.WriteLine(pindex);
                        byte id = reader.ReadByte();
                        string ip = reader.ReadString();
                    }
                    else if (p == Protocol.Movement)
                    {
                        float px = reader.ReadSingle();
                        float py = reader.ReadSingle();
                        bool dead = reader.ReadBoolean();
                        Character.CharDirection dir = (Character.CharDirection)reader.ReadByte();
                        Character.CharState state = (Character.CharState)reader.ReadByte();
                        byte id = reader.ReadByte();
                        string ip = reader.ReadString();

                        playerMap[id].Dead = dead;
                        playerMap[id].Direction = dir;
                        playerMap[id].State = state;
                        playerMap[id].Position = new Vector2(px, py);
                    }
                    else if (p == Protocol.AddCharacter)
                    {
                        byte numPlayers = reader.ReadByte();
                        byte id = reader.ReadByte();
                        string ip = reader.ReadString();
                        CompositeCharacter newPlayer;
                        if (numPlayers != playerMap.Count)
                        {
                            newPlayer = new CompositeCharacter(world, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2.0f, ScreenManager.GraphicsDevice.Viewport.Height / 2.0f),
                                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f), ScreenManager.CharacterColor);
                            newPlayer.body.IgnoreGravity = true;
                            newPlayer.wheel.IgnoreGravity = true;
                            newPlayer.body.CollidesWith = Category.Cat24;
                            newPlayer.wheel.CollidesWith = Category.Cat24;
                            playerMap.Add(id, newPlayer);
                        }
                    }
                    else if (p == Protocol.CreateTerrain)
                    {
                        if (!terrain.Created)
                        {
                            double s1 = reader.ReadDouble();
                            double s2 = reader.ReadDouble();
                            double s3 = reader.ReadDouble();
                            byte id = reader.ReadByte();
                            string ip = reader.ReadString();

                            terrain.CreateTerrain(new Vector2(0, 0), s1, s2, s3);
                        }
                    }

                    else if (p == Protocol.ModifyTerrain)
                    {
                        float px = reader.ReadSingle();
                        float py = reader.ReadSingle();

                        byte id = reader.ReadByte();
                        string ip = reader.ReadString();

                        Projectile projectile = new YellowProjectile(world, new Vector2(px, py), projectileTexYellow, new Vector2(10.0f, 10.0f), new Vector2(0, 0), player, fartbutt);
                        CheckCollision(projectile);
                    }
                    else
                    {
                        Console.WriteLine("Fuck you");
                    }

                    oldP = p;
                }
                catch (Exception e)
                {

                }
            }
        }
        private static Protocol oldP;
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
                client.GetStream().BeginWrite(b, 0, b.Length, null, null);
            }
            catch (Exception e)
            {

            }
        }
        #endregion
    }

}
