using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WizardsOrWhatever
{
    class Camera
    {
        public Matrix transform;
        Viewport view;
        Vector2 center;

        public Camera(Viewport view)
        {
            this.view = view;
        }

        public void Update(GameTime time, Game1 character)
        {
            //Vector places on top of the character. Must subtract half the game screen size for accuracy.
            center = new Vector2(character.position.X - (view.Width/2), character.position.Y - (view.Height/2));
            //Shifts the camera matrix with the character by a factor of 1.
            transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-center.X, -center.Y, 0));
        }
    }
}
