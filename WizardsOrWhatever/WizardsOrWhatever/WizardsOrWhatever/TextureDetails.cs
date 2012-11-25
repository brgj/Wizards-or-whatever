using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace WizardsOrWhatever
{
    class TextureDetails
    {
        public string Name;
        public Texture2D Texture;
        public int Width;
        public int Height;
        public int BeltPositionX;
        public int BeltPositionY;

        public TextureDetails(Texture2D texture, int width, int height, int posX, int posY, string Name)
        {
            this.Texture = texture;
            this.Width = width;
            this.Height = height;
            this.BeltPositionX = posX;
            this.BeltPositionY = posY;
            this.Name = Name;
        }
    }
}

