using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;

namespace WizardsOrWhatever
{
    public class Character : PhysicsObject
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
        private const int MAXHEALTH = 1000;
        private const int MAXMANA = 100;
        private int health = 1000;
        private int mana = 100;
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

        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }

        public int Mana
        {
            get
            {
                return mana;
            }
            set
            {
                mana = value;
            }
        }

        public int maxHealth
        {
            get
            {
                return MAXHEALTH;
            }
        }

        public int maxMana
        {
            get
            {
                return MAXMANA;
            }
        }

        
    }
}
