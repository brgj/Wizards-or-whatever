using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WizardsOrWhatever
{
    class StaticPhysicsObject : PhysicsObject
    {
        public StaticPhysicsObject(World world, Vector2 position, Texture2D texture, Vector2 size)
            : base(world, position, texture, size, 1000f)
        {
        }

        protected override void SetUpPhysics(World world, Vector2 position, float mass)
        {
            base.SetUpPhysics(world, position, mass);
            body.BodyType = BodyType.Static;
        }
    }
}
