#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace WizardsOrWhatever
{
    /// <summary>
    /// The HUD will deal with everything related to the current player on the screen
    /// including mana, health, spells, and other things
    /// </summary>
    public class HUD : DrawableGameComponent
    {
        #region Fields

        Texture2D health;
        Texture2D mana;
        Character player;
        SpriteBatch spriteBatch;
        Game game;
        Color healthStatus;

        //Crosshair texture
        Texture2D crosshair;
        public Vector2 cursorPos;
        MouseState mousestate;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public HUD(Game game, Character player, ContentManager content, SpriteBatch spriteBatch) : base(game)
        {
            this.game = game;
            this.player = player;
            this.spriteBatch = spriteBatch;
            health = content.Load<Texture2D>("HealthBar");
            mana = content.Load<Texture2D>("HealthBar");
            crosshair = content.Load<Texture2D>("CrossHair");
            //TransitionOnTime = TimeSpan.FromSeconds(1.5);
            //TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            mousestate = Mouse.GetState();
            cursorPos = new Vector2(mousestate.X, mousestate.Y);
            //Determines a players health bar color, if the player is less than 
            //half it will turn yellow, and at less than a quarter it will turn red
            if (player.Health <= player.maxHealth / 4)
            {
                healthStatus = Color.Red;
            }
            else if (player.Health <= player.maxHealth / 2)
            {
                healthStatus = Color.Yellow;
            }
            else
            {
                healthStatus = Color.YellowGreen;
            }

            //Draw Cursor
            spriteBatch.Draw(crosshair, cursorPos, Color.White);

            //Draw the health bar relative to players HP
            spriteBatch.Draw(health, new Rectangle(100 - health.Width / 2,
                        30, health.Width, 14), new Rectangle(0, 45, health.Width, 14), Color.Gray);

            spriteBatch.Draw(health, new Rectangle(100 - health.Width / 2,
                 30, (int)(health.Width * ((double)player.Health / player.maxHealth)), 14),
                 new Rectangle(0, 45, health.Width, 14), healthStatus);

            spriteBatch.Draw(health, new Rectangle(100 - health.Width / 2,
                        30, health.Width, 14), new Rectangle(0, 0, health.Width, 14), Color.White); 


            //Draw the mana bar relative to players mana
            spriteBatch.Draw(mana, new Rectangle(100 - mana.Width / 2,
                        46, mana.Width, 14), new Rectangle(0, 45, mana.Width, 14), Color.Gray);

            spriteBatch.Draw(mana, new Rectangle(100 - mana.Width / 2,
                 46, (int)(mana.Width * ((double)player.Mana / player.maxMana)), 14),
                 new Rectangle(0, 45, mana.Width, 14), Color.BlueViolet);

            spriteBatch.Draw(mana, new Rectangle(100 - mana.Width / 2,
                        46, mana.Width, 14), new Rectangle(0, 0, mana.Width, 14), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion
    }
}
