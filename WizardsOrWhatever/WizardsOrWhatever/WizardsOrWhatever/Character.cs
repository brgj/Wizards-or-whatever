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
            None,
            Idle,
            Jumping,
            Running
        }

        public Vector2 jumpImpulse = new Vector2(0, ConvertUnits.ToSimUnits(-10f));
        public Vector2 runImpulse = new Vector2(ConvertUnits.ToSimUnits(10f), 0);
        public float launchSpeed;
        public CharState state;
        public CharState prevState;

        public Character(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size, 50f)
        {
        }

        
    }
}
