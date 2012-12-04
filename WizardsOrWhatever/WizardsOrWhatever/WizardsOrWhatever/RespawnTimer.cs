using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;  

namespace WizardsOrWhatever
{
    public class RespawnTimer
    {
        private int start;
        private int end;
        public Boolean isComplete { get; set; }
        public Boolean timerActive { get; set; }
        public String display { get; set; }

        public RespawnTimer()
        {
            this.start = 0;
            this.end = 0;
            this.timerActive = false;
            this.isComplete = false;
        }

        public void set(GameTime gameTime, int seconds)
        {
            start = gameTime.TotalGameTime.Seconds;
            end = seconds;
            display = "Respawn: " + end.ToString();
        }

        public Boolean checkTimer(GameTime gameTime)
        {
            if (!isComplete)
            {
                if (gameTime.TotalGameTime.Seconds > start)
                {
                    start = gameTime.TotalGameTime.Seconds;
                    end = end - 1;
                    display = "Respawn: " + end.ToString();
                    if (end < 0)
                    {
                        end = 0;
                        isComplete = true;
                    }
                }
            }
            return isComplete;
        }
        public void reset()
        {
            isComplete = false;
            start = 0;
            end = 0;
            display = "";
        }
    }
}
