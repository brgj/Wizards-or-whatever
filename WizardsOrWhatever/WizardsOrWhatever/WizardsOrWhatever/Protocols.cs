using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardsOrWhatever
{

    class Protocols
    {
        /*public enum Protocol:int
        {
            Disconnected = 0,
            Connected = 1,
            index,
        }*/
        public int index;
        private byte data;
        public Protocols()
        {
        }
        public void setData(byte data)
        {
            this.data = data;
        }
        public byte getData()
        {
            return data;
        }
        public void setIndex(int index)
        {
            this.index = index;
        }
        public int getIndex()
        {
            return index;
        }
    }

}
