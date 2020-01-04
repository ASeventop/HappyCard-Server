using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.Game
{
    class Deck
    {
        Card[] cards = new Card[52];
        Random rng = new Random();
        int[] numbers = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 1 };

        public Deck()
        {
            byte i = 0;
            foreach (int s in numbers)
            {
                cards[i] = new Card(Suits.Clubs, s, i);
                i++;
            }
            foreach (int s in numbers)
            {
                cards[i] = new Card(Suits.Diamonds, s, i);
                i++;
            }
            foreach (int s in numbers)
            {
                cards[i] = new Card(Suits.Hearts, s, i);
                i++;
            }
            foreach (int s in numbers)
            {
                cards[i] = new Card(Suits.Spades, s, i);
                i++;
            }
        }
        public Card[] Cards
        {
            get
            {
                return cards;
            }
        }
        public Card GetCardByID(byte id)
        {
            return cards[id];
        }
        public void Shuffle()
        {
            rng = new Random();
            rng.Shuffle(Cards);
        }

    }
}
