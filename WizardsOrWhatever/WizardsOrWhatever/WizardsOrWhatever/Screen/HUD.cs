﻿#region File Description
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
        Texture2D box;

        //projectile icons
        Texture2D projectileIcon;
        Texture2D projectileIcon2;
        Texture2D projectileIcon3;

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
        public HUD(Game game, Character player, ContentManager content, SpriteBatch spriteBatch)
            : base(game)
        {
            this.game = game;
            this.player = player;
            this.spriteBatch = spriteBatch;
            health = content.Load<Texture2D>("HealthBar");
            mana = content.Load<Texture2D>("HealthBar");
            box = content.Load<Texture2D>("Box");
            crosshair = content.Load<Texture2D>("CrossHair");
            projectileIcon = content.Load<Texture2D>("projectile_fire");
            projectileIcon2 = content.Load<Texture2D>("projectile_fire2");
            projectileIcon3 = content.Load<Texture2D>("projectile_fire3");
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

            //Draw the health bar relative to players HP, DO NOT CHANGE THE POSITIONS
            spriteBatch.Draw(health, new Rectangle(15,
                        15, health.Width, 14), new Rectangle(0, 45, health.Width, 14), Color.Gray);

            spriteBatch.Draw(health, new Rectangle(15,
                 15, (int)(health.Width * ((double)player.Health / player.maxHealth)), 14),
                 new Rectangle(0, 45, health.Width, 14), healthStatus);

            spriteBatch.Draw(health, new Rectangle(15,
                        15, health.Width, 14), new Rectangle(0, 0, health.Width, 14), Color.White);


            //Draw the mana bar relative to players mana
            spriteBatch.Draw(mana, new Rectangle(15,
                        31, mana.Width, 14), new Rectangle(0, 45, mana.Width, 14), Color.Gray);

            spriteBatch.Draw(mana, new Rectangle(15,
                 31, (int)(mana.Width * ((double)player.Mana / player.maxMana)), 14),
                 new Rectangle(0, 45, mana.Width, 14), Color.BlueViolet);

            spriteBatch.Draw(mana, new Rectangle(15,
                        31, mana.Width, 14), new Rectangle(0, 0, mana.Width, 14), Color.White);

            //Draw the weapon selector, do not midfy position
            spriteBatch.Draw(box, new Rectangle(16 + health.Width, 15, box.Width, box.Height), Color.White);

            //Draw weapon icons
            if (player.Weapon == Character.WeaponSelect.Yellow)
            {
                //Use the rectangle position to get an object in the center of the box
                spriteBatch.Draw(projectileIcon, new Rectangle((16 + health.Width) + 5, 20, 20, 20), Color.White);
            }
            else if (player.Weapon == Character.WeaponSelect.Blue)
            {
                spriteBatch.Draw(projectileIcon2, new Rectangle((16 + health.Width) + 5, 20, 20, 20), Color.White);
            }
            else if (player.Weapon == Character.WeaponSelect.Red)
            {
                spriteBatch.Draw(projectileIcon3, new Rectangle((16 + health.Width) + 5, 20, 20, 20), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion
    }
}
