﻿using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WizardsOrWhatever
{
    public class Camera2D
    {
        // Zoom min = 1/20 * default
        private const float _minZoom = 0.02f;
        // Zoom max = 20 * default
        private const float _maxZoom = 20f;
        
        private static GraphicsDevice _graphics;

        // The display view
        private Matrix _batchView;
        // The sim view
        private Matrix _view;

        private Matrix _projection;

        private Vector2 _translateCenter;

        // The current position and zoom of the camera
        private Vector2 _currentPosition;
        private float _currentZoom;

        // Restricts the camera bounds to be within max and min
        private Vector2 _maxPosition;
        private Vector2 _minPosition;
        
        // The position and body of the target being followed
        private Vector2 _targetPosition;
        private Body _trackingBody;

        /// <summary>
        /// The constructor for the Camera2D class.
        /// </summary>
        /// <param name="graphics"></param>
        public Camera2D(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(_graphics.Viewport.Width),
                                                             ConvertUnits.ToSimUnits(_graphics.Viewport.Height), 0f, 0f,
                                                             1f);
            _view = Matrix.Identity;
            _batchView = Matrix.Identity;

            _translateCenter = new Vector2(ConvertUnits.ToSimUnits(_graphics.Viewport.Width / 2f),
                                           ConvertUnits.ToSimUnits(_graphics.Viewport.Height / 2f));

            ResetCamera();
        }

        public Matrix View
        {
            get { return _batchView; }
        }

        public Matrix SimView
        {
            get { return _view; }
        }

        public Matrix SimProjection
        {
            get { return _projection; }
        }

        /// <summary>
        /// The current position of the camera.
        /// </summary>
        public Vector2 Position
        {
            get { return ConvertUnits.ToDisplayUnits(_currentPosition); }
            set
            {
                _targetPosition = ConvertUnits.ToSimUnits(value);
                if (_minPosition != _maxPosition)
                {
                    Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out _targetPosition);
                }
            }
        }

        /// <summary>
        /// The furthest up, and the furthest left the camera can go.
        /// if this value equals maxPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MinPosition
        {
            get { return ConvertUnits.ToDisplayUnits(_minPosition); }
            set { _minPosition = ConvertUnits.ToSimUnits(value); }
        }

        /// <summary>
        /// the furthest down, and the furthest right the camera will go.
        /// if this value equals minPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MaxPosition
        {
            get { return ConvertUnits.ToDisplayUnits(_maxPosition); }
            set { _maxPosition = ConvertUnits.ToSimUnits(value); }
        }

        /// <summary>
        /// The current zoom of the camera.
        /// </summary>
        public float Zoom
        {
            get { return _currentZoom; }
            set
            {
                _currentZoom = value;
                _currentZoom = MathHelper.Clamp(_currentZoom, _minZoom, _maxZoom);
            }
        }

        /// <summary>
        /// the body that this camera is currently tracking. 
        /// Null if not tracking any.
        /// </summary>
        public Body TrackingBody
        {
            get { return _trackingBody; }
            set
            {
                _trackingBody = value;
            }
        }

        /// <summary>
        /// Moves the camera by the specified amount. 
        /// Will snap back if following body.
        /// </summary>
        public void MoveCamera(Vector2 amount)
        {
            _currentPosition += amount;
            if (_minPosition != _maxPosition)
            {
                Vector2.Clamp(ref _currentPosition, ref _minPosition, ref _maxPosition, out _currentPosition);
            }
            _targetPosition = _currentPosition;
        }

        /// <summary>
        /// Resets the camera to default values.
        /// </summary>
        public void ResetCamera()
        {
            _currentPosition = Vector2.Zero;
            _targetPosition = Vector2.Zero;
            _minPosition = Vector2.Zero;
            _maxPosition = Vector2.Zero;

            _currentZoom = 1f;

            SetView();
        }

        private void SetView()
        {
            Matrix matZoom = Matrix.CreateScale(_currentZoom);
            Vector3 translateCenter = new Vector3(_translateCenter, 0f);
            Vector3 translateBody = new Vector3(-_currentPosition, 0f);

            _view = Matrix.CreateTranslation(translateBody) *
                    matZoom *
                    Matrix.CreateTranslation(translateCenter);

            translateCenter = ConvertUnits.ToDisplayUnits(translateCenter);
            translateBody = ConvertUnits.ToDisplayUnits(translateBody);

            _batchView = Matrix.CreateTranslation(translateBody) *
                         matZoom *
                         Matrix.CreateTranslation(translateCenter);
        }

        /// <summary>
        /// Moves the camera forward one timestep.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (_trackingBody != null)
            {
                _targetPosition = _trackingBody.Position;
                if (_minPosition != _maxPosition)
                {
                    Vector2.Clamp(ref _targetPosition, ref _minPosition, ref _maxPosition, out _targetPosition);
                }
            }
            _currentPosition = _targetPosition;

            SetView();
        }

        public Vector2 ConvertScreenToWorld(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Unproject(t, _projection, _view, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }

        public Vector2 ConvertWorldToScreen(Vector2 location)
        {
            Vector3 t = new Vector3(location, 0);

            t = _graphics.Viewport.Project(t, _projection, _view, Matrix.Identity);

            return new Vector2(t.X, t.Y);
        }
    }
}