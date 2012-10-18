using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardsOrWhatever.Screen
{
    class LobbyScreen : MenuScreen
    {

        public LobbyScreen()
            : base("Lobby")
        {
            MenuEntry joingame = new MenuEntry("Join a game");
            MenuEntry back = new MenuEntry("Back");
            MenuEntries.Add(joingame);
            MenuEntries.Add(back);
            back.Selected += OnCancel;
        }
    }
}
