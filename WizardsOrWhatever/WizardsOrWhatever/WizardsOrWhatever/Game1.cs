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
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using FarseerPhysics;

namespace WizardsOrWhatever
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //The world object that encapsulates all physics objects
        World world;
        MSTerrain terrain;

        //Variable for accessing the graphics device manager of the current game
        GraphicsDeviceManager graphics;
        //The spritebatch used for drawing the current game
        SpriteBatch spriteBatch;

        //Adds a new camera object, follows the character
        Camera camera;
        //Game character controlled by user. TODO: Use a list for offline multiplayer
        CompositeCharacter player;

        //Walls. Placeholders for terrain.
        PhysicsObject ground;
        PhysicsObject leftWall;
        PhysicsObject rightWall;
        PhysicsObject ceiling;

        //List of paddles with different properties to be drawn to screen. Placeholder for actual interesting content.
        List<PhysicsObject> paddles;

        //Debug View. For viewing all underlying physics components of game.
        DebugViewXNA DebugView;

        //For Debug View. Temporary until fixed.
        private Matrix projection, view;


        //TODO: get DebugView working properly
        //TODO: get MSTerrain working properly
        //The y pos is losing accuracy at some point during run time.
        //Debug more and figure out the location
        //Figure out debug mode and implement new terrain.
        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create new SpriteBatch from GraphicsDevice
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Create world with gravity = 9.8 m/s downwards
            world = new World(new Vector2(0, 9.8f));

            //Create and initialize terrain
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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create camera using current viewport
            camera = new Camera(GraphicsDevice.Viewport);

            //terrain.ApplyTexture(Content.Load<Texture2D>("Terrain"), new Vector2(200, 0), InsideTerrainTest);

            //Create player
            player = new CompositeCharacter(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height / 2.0f),
                Content.Load<Texture2D>("bean_ss1"), new Vector2(35.0f, 50.0f));

            //Create walls
            ground = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 12.5f),
                Content.Load<Texture2D>("platformTex"), new Vector2(GraphicsDevice.Viewport.Width, 25.0f));
            //leftWall = new StaticPhysicsObject(world, new Vector2(12.5f, GraphicsDevice.Viewport.Height / 2.0f),
                //Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));
            //rightWall = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width - 12.5f, GraphicsDevice.Viewport.Height / 2.0f),
                //Content.Load<Texture2D>("platformTex"), new Vector2(25.0f, GraphicsDevice.Viewport.Height));
            ceiling = new StaticPhysicsObject(world, new Vector2(GraphicsDevice.Viewport.Width / 2.0f, 12.5f),
                Content.Load<Texture2D>("platformTex"), new Vector2(GraphicsDevice.Viewport.Width, 25.0f));

            //Instantiate a list of paddles to be used
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

            //Update other components tracked by game
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Background drawn to CornflowerBlue
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //DebugView.RenderDebugData(ref projection, ref view);

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
            base.Draw(gameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private bool InsideTerrainTest(Color color)
        {
            throw new NotImplementedException();
            //return color == Color.Black;
        }
    }
}
