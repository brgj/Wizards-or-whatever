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
    class CompositeCharacter : Character
    {
        public Body wheel;
        public FixedAngleJoint fixedAngleJoint;
        public RevoluteJoint motor;
        public float centerOffset;

        public CompositeCharacter(World world, Vector2 position, Texture2D texture, Vector2 size) : base(world, position, texture, size)
        {
            if (size.X > size.Y)
            {
                throw new Exception("Cannot make character with width > height");
            }

            state = CharState.None;

            wheel.OnCollision += new OnCollisionEventHandler(OnCollision);
        }

        protected override void SetUpPhysics(World world, Vector2 position, float mass)
        {
            float upperBodyHeight = size.Y - (size.X / 2);

            // Create upper body
            body = BodyFactory.CreateRectangle(world, size.X, upperBodyHeight, mass / 2);
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.3f;
            body.Friction = 0.5f;
            body.Position = ConvertUnits.ToSimUnits(position - (Vector2.UnitY * (size.X / 4)));

            centerOffset = position.Y - (float)ConvertUnits.ToDisplayUnits(body.Position.Y);

            fixedAngleJoint = JointFactory.CreateFixedAngleJoint(world, body);

            // Create lower body
            wheel = BodyFactory.CreateCircle(world, (float)ConvertUnits.ToSimUnits(size.X / 2), mass / 2);
            wheel.BodyType = BodyType.Dynamic;
            wheel.Restitution = 0.3f;
            wheel.Friction = 0.5f;
            wheel.Position = ConvertUnits.ToSimUnits(position + (Vector2.UnitY * (upperBodyHeight / 2)));

            // Connecting bodies
            motor = JointFactory.CreateRevoluteJoint(world, body, wheel, Vector2.Zero);

            motor.MotorEnabled = true;
            motor.MaxMotorTorque = 1000f;
            motor.MotorSpeed = 0;

            wheel.IgnoreCollisionWith(body);
            body.IgnoreCollisionWith(wheel);

            wheel.Friction = float.MaxValue;
        }

        public bool OnCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (state == CharState.Jumping && prevState == CharState.Jumping)
            {
                state = CharState.None;
            }
            return true;
        }
    }
}
