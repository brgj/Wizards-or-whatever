using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace WizardsOrWhatever
{
    class Character : PhysicsObject
    {
        public enum CharState
        {
            Idle,
            Jumping,
            Running,
            Wallslide
        }

        public enum CharDirection
        {
            Left,
            Right
        }

        public Vector2 jumpImpulse = new Vector2(0, -5);
        public float launchSpeed;
        protected CharState state = CharState.Idle;
        protected CharState prevState;
        protected CharDirection direction = CharDirection.Right;
        protected CharDirection prevDirection;
        public int runSpeed = 10;

        public CharState State
        {
            get
            {
                return state;
            }
            set
            {
                prevState = state;
                state = value;
            }
        }

        public CharDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                prevDirection = direction;
                direction = value;
            }
        }



        public Character(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size, 10f)
        {
        }

        
    }
}
