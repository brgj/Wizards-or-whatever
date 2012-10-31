using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;

namespace WizardsOrWhatever
{
    class CharacterColorScreen : MenuScreen
    {
        #region Initialization

        private MenuEntry red;
        private MenuEntry blue;
        private MenuEntry green;
        private MenuEntry yellow;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterColorScreen()
            : base("Choose Bean Colour")
        {
            // Creating menu entries for different colours.
            red = new MenuEntry("Red");
            blue = new MenuEntry("Blue");
            green = new MenuEntry("Green");
            yellow = new MenuEntry("Yellow");
            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            red.Selected += redSelected;
            blue.Selected += blueSelected;
            green.Selected += greenSelected;
            yellow.Selected += yellowSelected;
            back.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(red);
            MenuEntries.Add(blue);
            MenuEntries.Add(green);
            MenuEntries.Add(yellow);
            MenuEntries.Add(back);
        }

        #endregion
        #region Handling Inputs

        /// <summary>
        /// Event handler for when red entry is selected.
        /// </summary>
        void redSelected(object sender, PlayerIndexEventArgs e)
        {
            red.Text = "[RED]";
            blue.Text = "Blue";
            green.Text = "Green";
            yellow.Text = "Yellow";
            ScreenManager.CharacterColor = Color.Red;
            
        }

        /// <summary>
        /// Event handler for when blue entry is selected.
        /// </summary>
        void blueSelected(object sender, PlayerIndexEventArgs e)
        {
            red.Text = "Red";
            blue.Text = "[BLUE]";
            green.Text = "Green";
            yellow.Text = "Yellow";
            ScreenManager.CharacterColor = Color.Blue;
        }

        /// <summary>
        /// Event handler for when green entry is selected.
        /// </summary>
        void greenSelected(object sender, PlayerIndexEventArgs e)
        {
            red.Text = "Red";
            blue.Text = "Blue";
            green.Text = "[GREEN]";
            yellow.Text = "Yellow";
            ScreenManager.CharacterColor = Color.Green;
        }

        /// <summary>
        /// Event handler for when yellow entry is selected.
        /// </summary>
        void yellowSelected(object sender, PlayerIndexEventArgs e)
        {
            red.Text = "Red";
            blue.Text = "Blue";
            green.Text = "Green";
            yellow.Text = "[YELLOW]";
            ScreenManager.CharacterColor = Color.Yellow;
        }

        #endregion
    }
}
