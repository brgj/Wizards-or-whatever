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
    /// <summary>
    /// Data for the particle effects on explosion
    /// </summary>
    public struct ParticleData
    {
        public float BirthTime;
        public float MaxAge;
        public Vector2 OriginalPosition;
        public Vector2 Acceleration;
        public Vector2 Direction;
        public Vector2 Position;
        public float Scaling;
        public Color ModColor;
    }

    class Explosion : IDisposable
    {
        private Texture2D explosionTex;
        private float elapsedTime = 0f;
        private Random randomizer = new Random();
        private float maxAge = 2000.0f;
        private List<ParticleData> particleList = new List<ParticleData>();
        private float level;
        private Vector2 position;
        private Color color;

        public Explosion(Texture2D explosionTex, float level, Vector2 position, Color color)
        {
            this.explosionTex = explosionTex;
            this.level = level;
            this.position = position;
            this.color = color;
            AddExplosion();
        }


        private void AddExplosion()
        {
            for (int i = 0; i < 10 * level; i++)
            {
                AddExplosionParticle();
            }
        }

        private void AddExplosionParticle()
        {
            ParticleData particle = new ParticleData();

            particle.OriginalPosition = position;
            particle.Position = particle.OriginalPosition;

            particle.BirthTime = elapsedTime;
            particle.MaxAge = maxAge * (float)randomizer.NextDouble();
            particle.Scaling = 0.25f;
            particle.ModColor = Color.White;

            float particleDistance = ConvertUnits.ToDisplayUnits(level) * (float)randomizer.NextDouble();
            Vector2 displacement = new Vector2(particleDistance, 0);
            float angle = MathHelper.ToRadians(randomizer.Next(360));
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            particle.Direction = displacement * 2.0f;
            particle.Acceleration = -particle.Direction;

            particleList.Add(particle);
        }

        public bool UpdateParticles(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            for (int i = particleList.Count - 1; i >= 0; i--)
            {
                ParticleData particle = particleList[i];
                float timeAlive = elapsedTime - particle.BirthTime;

                if (timeAlive > particle.MaxAge)
                {
                    particleList.RemoveAt(i);
                }
                else
                {
                    float relAge = timeAlive / particle.MaxAge;
                    particle.Position = 0.5f * particle.Acceleration * relAge * relAge + particle.Direction * relAge + particle.OriginalPosition;
                    float invAge = 1.0f - relAge;
                    particle.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));
                    Vector2 positionFromCenter = particle.Position - particle.OriginalPosition;
                    float distance = positionFromCenter.Length();
                    particle.Scaling = (50.0f + distance) / 200.0f;
                    particleList[i] = particle;
                }
            }
            if (particleList.Count == 0)
            {
                Dispose();
                return false;
            }
            return true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < particleList.Count; i++)
            {
                ParticleData particle = particleList[i];
                spriteBatch.Draw(explosionTex, particle.Position, null, color, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
            }
        }

        #region IDisposable Members

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                //explosionTex.Dispose();
                IsDisposed = true;
            }
        }

        #endregion
    }
}
