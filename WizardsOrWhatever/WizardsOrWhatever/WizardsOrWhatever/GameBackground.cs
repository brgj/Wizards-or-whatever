using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WizardsOrWhatever
{
    class GameBackground
    {
        #region Variables/Properties

        private float _speedX = 1;
        private float _speedY = 1;
        private Vector2 _startPosition;
        private Vector2 _currentPosition;
        private Vector2 _previousPosition;
        


        public int Height { get; set; }
        public int Width { get; set; }
        public List<Texture2D> ScrollingTextures { get; set; }
        // List of TextureDetail Structs
        private List<TextureDetail> _textures { get; set; }

        public float SpeedX
        {
            get { return _speedX; }
            set { _speedX = value; }
        }

        public float SpeedY
        {
            get { return _speedY; }
            set { _speedY = value; }
        }

        public Vector2 Position
        {
            get { return _currentPosition; }
            set { _currentPosition = value; }
        }

        #endregion

        #region Constructor/Initializaiton

        public GameBackground(List<Texture2D> ScrollingTextures, Vector2 StartPoint)
        {
            this.ScrollingTextures = ScrollingTextures;
            this._startPosition = StartPoint;
            this._currentPosition = _startPosition;
            Initialize();
        }

        private void Initialize()
        {
            SpeedX = 1;
            SpeedY = 1;

            int LastWidth = 0, LastHeight = 0;
            _textures = new List<TextureDetail>();

            int i = 1;

            foreach (Texture2D txtr in ScrollingTextures )
            {
                _textures.Add(new TextureDetail(txtr, txtr.Width, txtr.Height, LastWidth, LastHeight, i.ToString()));
                LastWidth += txtr.Width;
                LastHeight += txtr.Height;
                i++;
            }
        }
        #endregion

        #region Methods/Draw

        public void Move(Vector2 NewPosition)
        {
            Vector2 mid, distance;
            float X = (_textures[_textures.Count-1].Width)/2;
            float Y = (_textures[_textures.Count-1].Height)/2;

            mid = new Vector2(NewPosition.X - X, NewPosition.Y - Y);
            distance = new Vector2((NewPosition.X - _startPosition.X) * 0.04f, (NewPosition.Y - _startPosition.Y) * 0.01f);

            _currentPosition.X = mid.X - distance.X;
            _currentPosition.Y = mid.Y - distance.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float positionX = _currentPosition.X;
            float positionY = _currentPosition.Y;
            for (int i = 0; i < _textures.Count; i++)
            {
                spriteBatch.Draw(_textures[i].Texture,_currentPosition, Color.White);
            }
        }

        #endregion

    }

    #region Texture Detail

    class TextureDetail
    {
        public string Name;
        public Texture2D Texture;
        public int Width;
        public int Height;
        public int BeltPositionX;
        public int BeltPositionY;
        public float Speed;

        public TextureDetail(Texture2D texture, int width, int height, int posX, int posY, string Name)
        {
            this.Texture = texture;
            this.Width = width;
            this.Height = height;
            this.BeltPositionX = posX;
            this.BeltPositionY = posY;
            this.Name = Name;
        }
    }


    #endregion

    public enum ParallaxDirection
    {
        Horizontal, Vertical
    }
}
