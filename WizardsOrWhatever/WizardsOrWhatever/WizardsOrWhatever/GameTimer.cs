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
using System.Timers;

namespace WizardsOrWhatever
{
    public class GameTimer
    {
       private int start;
        private int endSec;
        private int endMin;
        public Boolean isComplete { get; set; }
        public Boolean timerActive { get; set; }
        public String display { get; set; }
        public static Timer timeElapsed;

        public GameTimer()
        {
            this.start = 0;
            this.endSec = 0;
            this.endMin = 0;
            this.timerActive = false;
            this.isComplete = false;
        }

        public void set(GameTime gameTime, int minutes, int seconds)
        {
            start = gameTime.TotalGameTime.Seconds;
            endSec = seconds;
            endMin = minutes;
            display = endMin.ToString() + ":" + endSec.ToString();
            timeElapsed = new Timer((seconds + (60 * minutes)) * 1000);
            timeElapsed.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timeElapsed.Enabled = true;
            timeElapsed.Interval = 1000;
            timeElapsed.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            endSec--;
            if (endSec < 0)
            {
                endSec = 59;
                endMin--;
                if (endMin < 0)
                {
                    isComplete = true;
                    timeElapsed.Stop();
                }
            }
            display = endMin.ToString() + ":" + endSec.ToString();
        }

        public void reset()
        {
            isComplete = false;
            start = 0;
            endSec = 0;
            endMin = 0;
            display = "";
        }
    }
}