using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Input;

namespace WizardsOrWhatever
{
    class Input
    {
        protected void ProcessInput(Player player)
        {
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

        }
    }
    public struct ContLayoutA
    {
        public Object Right, Left, Jump;

        public ContLayoutA()
        {
            Right = Keys.Right;
            Left = Keys.Left;
            Jump = Keys.LeftControl;

        }
    }
}

