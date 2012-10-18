using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;

namespace WizardsOrWhatever
{
    public class Projectile : PhysicsObject
    {

        public Vector2 origin;
        public Vector2 velocity;
        public Vector2 position;

        public bool isVisible;

        public Projectile(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size, 0f)
        {
        }

    }
}
