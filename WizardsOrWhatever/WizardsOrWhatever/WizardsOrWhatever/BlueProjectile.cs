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
    class BlueProjectile : Projectile
    {
        public static int manaCost = 100;

        public BlueProjectile(World world, Vector2 position, Texture2D texture, Vector2 size, Vector2 cursPosition, CompositeCharacter player, CheckCollision collisionChecker)
        : base(world, position, texture, size, cursPosition, player, collisionChecker)
        {
            color = Color.Blue;
            speed = 200;
            damage = 100;
            delay = 5000;
        }
    }
}
