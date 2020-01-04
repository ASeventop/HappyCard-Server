using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.Game
{
    class Card
    {
        protected Suits suit;
        public int cardValue;
        public byte cardID;
        public Card(Suits _suit, int cardvalue2,byte id)
        {
            suit = _suit;
            cardValue = cardvalue2;
            cardID = id;
        }
        public override string ToString()
        {
            return string.Format("{0} of {1} id {2}", cardValue, suit,cardID);
        }
        public Suits GetSuit()
        {
            return suit;
        }
    }
}
