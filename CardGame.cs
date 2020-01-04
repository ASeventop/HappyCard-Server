using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFirstPlugin.Game;
using System.Linq;
using System.Collections;

namespace MyFirstPlugin
{
    class CardGame
    {
        public Deck deck;
        Deck mapDeck;
        public float time;
        public float restartTime;
        public Card[] distributeDecks;
        public Dictionary<object, byte[]> playerDecks = new Dictionary<object, byte[]>();
        public Dictionary<object, List<Card[]>> playerCards = new Dictionary<object, List<Card[]>>();
        public Dictionary<object, DeckRank[]> playerDeckRank = new Dictionary<object, DeckRank[]>();
        public Dictionary<object, List<DeckTotal[]>> playerDeckTotal = new Dictionary<object, List<DeckTotal[]>>();
        public Dictionary<object, List<PlayerDuelTotal>> playerDuelTotal = new Dictionary<object, List<PlayerDuelTotal>>();
        public Dictionary<object, List<object>> playerVSActors = new Dictionary<object, List<object>>();
        public Dictionary<object, object> playerReady;
        byte[] rowDeckInfo = new byte[] { 2, 3, 3 }; // number of row form each deck;
        int cardForPlayer = 8;

        //game setting 
        float tax;
        int currencyAmount;
        Currency currency;

        public CardGame(int _currencyAmount,float _tax,Currency _currency)
        {
            currency = _currency;
            currencyAmount = _currencyAmount;
            tax = _tax;
            deck = new Deck();
            mapDeck = new Deck();
        }
        public void Start(Dictionary<object,object> _playerReady)
        {
            playerReady = _playerReady;
            distributeDecks = new Card[playerReady.Count];
            deck.Shuffle();
            time = GameConstant.GAMETIME;
            restartTime = GameConstant.RESTART_TIME;
            Distribute();
        }
        void Distribute()
        {
            int index = 0;
            for (int i = 0; i < distributeDecks.Length; i++)
            {
                byte[] cardIDs = new byte[cardForPlayer];
                for (int j = 0; j < cardForPlayer; j++)
                {
                    if (deck.Cards[index] != null)
                    {
                        cardIDs[j] = deck.Cards[index].cardID;
                        index++;
                    }
                }
                playerDecks.Add(playerReady.ElementAt(i).Key, cardIDs);
                UpdatePlayerDeck(playerReady.ElementAt(i).Key, cardIDs);
               
            }
        }
        public byte[] GetCardIDFormDeck(object playerKey)
        {
            if(playerDecks.ContainsKey(playerKey))
                return playerDecks[playerKey];
            return null;
        }
        public void UpdatePlayerDeck(object actorNr, byte[] data)
        {
            if (playerDecks.ContainsKey(actorNr))
            {
                playerDecks[actorNr] = data;
                UpdatePlayerCards(actorNr, data);
            }
        }
        void UpdatePlayerCards(object actorNr, byte[] data)
        {
            if (playerCards.ContainsKey(actorNr)) playerCards.Remove(actorNr);
            if (playerDeckRank.ContainsKey(actorNr)) playerDeckRank.Remove(actorNr);

            playerCards.Add(actorNr, new List<Card[]>());
            playerDeckRank.Add(actorNr, new DeckRank[3]);
            var index = 0;
            var deckRankInd = 0;
            foreach (var row in rowDeckInfo)
            {
                Card[] subDecks = new Card[row];
                DeckRank deckrank = new DeckRank();
                for (int i = 0; i < row; i++)
                {
                    subDecks[i] = mapDeck.GetCardByID(data[index++]);
                }
                playerCards[actorNr].Add(subDecks);
                playerDeckRank[actorNr][deckRankInd++] = deckrank;
                UpdatePlayerDeckRank(actorNr, subDecks, deckrank);
            }
            
        }
        void UpdatePlayerDeckRank(object actorNr, Card[] _cards,DeckRank deckRank)
        {
            List<Card> cards = new List<Card>();
            cards = _cards.OrderBy(card => card.cardValue).ToList();
            int point = 0;
            bool isStraight = true;
            bool isGhost = cards.All(c => c.cardValue > 10);
            bool isThreeofKind = cards.All(c => c.cardValue == cards[0].cardValue);
            byte higherCard = 0;
            byte multiply = 1;
            CardRank rank = CardRank.Point;
            if (cards.FirstOrDefault(c => c.cardValue == 1) != null)
                higherCard = 14;
            else
                higherCard = (byte)cards[cards.Count - 1].cardValue;

            List<bool> suitFlush = new List<bool>();
            suitFlush.Add(cards.All(card => card.GetSuit() == Suits.Clubs));
            suitFlush.Add(cards.All(card => card.GetSuit() == Suits.Diamonds));
            suitFlush.Add(cards.All(card => card.GetSuit() == Suits.Hearts));
            suitFlush.Add(cards.All(card => card.GetSuit() == Suits.Spades));

            int currentValue = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                point += (cards[i].cardValue > 10) ? 10 : cards[i].cardValue;
                if (i > 0 && isStraight)
                    isStraight = ((cards[i].cardValue - currentValue) == 1 || (cards[i].cardValue - currentValue) == 11);
                currentValue = cards[i].cardValue;
            }

            bool isFlush = suitFlush.Any(x => x == true);
            Suits suit = Suits.NONE;
            if (isFlush)
            {
                suit = (Suits)suitFlush.IndexOf(true);
            }
            //point = point % 10;
            deckRank.ownerActor = actorNr;
            deckRank.amount = (byte)_cards.Length;
            deckRank.point = (byte)(point % 10);
            deckRank.suits = suit;
            deckRank.isGhost = isGhost;
            deckRank.isFlush = isFlush;
            deckRank.isStraight = isStraight;
            deckRank.isStraightFlush = isStraight && isFlush;
            deckRank.isThreeofKinds = isThreeofKind;
            deckRank.cardRank = CardRank.Point;
            deckRank.multiply = (sbyte)multiply;
            deckRank.higherCard = higherCard;

            if (deckRank.isThreeofKinds)
            {
                deckRank.cardRank = CardRank.ThreeofKind;
                deckRank.multiply = 6;
            }
            else if (deckRank.isStraight)
            {
               
                if (deckRank.amount == 2)
                {
                    deckRank.cardRank = CardRank.Point;
                    deckRank.multiply = (sbyte)((deckRank.isFlush) ? 2 : 1);
                }
                else
                {
                    deckRank.cardRank = (deckRank.isFlush) ? CardRank.StraightFlush : CardRank.Straight;
                    deckRank.multiply = (sbyte)((deckRank.isFlush) ? 5 : 4);
                }
            }
            else if (deckRank.isGhost)
            {
                deckRank.cardRank = CardRank.Gost;
                deckRank.multiply = 3;
            }
            if(deckRank.cardRank == CardRank.Point)
            {
                if (deckRank.isFlush)
                    deckRank.multiply = (sbyte)deckRank.amount;
                else
                    deckRank.multiply = 1;
            }
        }
        
        public void PairAllPlayers()
        {
            List<object> actors = playerDeckRank.Keys.ToList();
            for (int i = 0; i < actors.Count; i++)
            {
                var deckTarget = playerDeckRank[actors[i]] as DeckRank[];
                if (!playerDeckTotal.ContainsKey(actors[i]))
                    playerDeckTotal.Add(actors[i], new List<DeckTotal[]>());
                if(!playerVSActors.ContainsKey(actors[i]))
                    playerVSActors.Add(actors[i],new List<object>());

                foreach (var deck in playerDeckRank)
                {
                    if (deck.Key == actors[i]) continue;
                    var vsDeck = deck.Value;
                    playerVSActors[actors[i]].Add(deck.Key);
                    playerDeckTotal[actors[i]].Add(DeckPairDeck(deckTarget, vsDeck));
                }
            }
        }
        public DeckTotal[] DeckPairDeck(DeckRank[] firstDeckRank, DeckRank[] secondDeckRank)
        {
            DeckTotal[] deckTotal = new DeckTotal[firstDeckRank.Length];
            for (int i = 0; i < firstDeckRank.Length; i++)
            {
                var targetDeck = firstDeckRank[i];
                var duelDeck = secondDeckRank[i];
                var total = new DeckTotal();
                deckTotal[i] = total;
                if (targetDeck.cardRank > duelDeck.cardRank)
                {
                    SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Win, targetDeck.multiply);
                }
                else if (targetDeck.cardRank < duelDeck.cardRank)
                {
                    SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Lose, -duelDeck.multiply);
                }
                else if (targetDeck.cardRank == duelDeck.cardRank)
                {
                    if (targetDeck.cardRank == CardRank.Gost)
                    {
                        SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Draw, 0);
                    }
                    else if (targetDeck.cardRank == CardRank.ThreeofKind)
                    {
                        if(targetDeck.higherCard > duelDeck.higherCard)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Win, targetDeck.multiply);
                        if (targetDeck.higherCard < duelDeck.higherCard)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Lose, -duelDeck.multiply);
                        if (targetDeck.higherCard == duelDeck.higherCard)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Draw, 0);
                    }
                    else if (targetDeck.cardRank == CardRank.StraightFlush)
                    {
                        if (targetDeck.suits > duelDeck.suits)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Win, targetDeck.multiply);
                        if (targetDeck.suits < duelDeck.suits)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Lose, -duelDeck.multiply);
                        if(targetDeck.suits == duelDeck.suits)
                        {
                            if (targetDeck.higherCard > duelDeck.higherCard)
                                SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Win, targetDeck.multiply);
                            if (targetDeck.higherCard < duelDeck.higherCard)
                                SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Lose, -duelDeck.multiply);
                            if (targetDeck.higherCard == duelDeck.higherCard)
                                SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Draw, 0);
                        }
                    }
                    else if (targetDeck.cardRank == CardRank.Straight)
                    {
                        if (targetDeck.higherCard > duelDeck.higherCard)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Win, targetDeck.multiply);
                        if (targetDeck.higherCard < duelDeck.higherCard)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Lose, -duelDeck.multiply);
                        if (targetDeck.higherCard == duelDeck.higherCard)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Draw, 0);
                    }
                    else if (targetDeck.cardRank == CardRank.Point)
                    {
                        if (targetDeck.point > duelDeck.point)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Win, targetDeck.multiply);
                        if (targetDeck.point < duelDeck.point)
                            SetDecktotal(duelDeck.ownerActor, total, DuelCardResult.Lose, -duelDeck.multiply);
                        if (targetDeck.point == duelDeck.point)
                            SetDecktotal(duelDeck.ownerActor,total, DuelCardResult.Draw, 0);
                    }
                }
                
            }
            SetDuelTotal(firstDeckRank[0].ownerActor, deckTotal);
            return deckTotal;
        }
        void SetDuelTotal(object actorNr,DeckTotal[] totals)
        {
            if (!playerDuelTotal.ContainsKey(actorNr))
                playerDuelTotal.Add(actorNr, new List<PlayerDuelTotal>());

            PlayerDuelTotal duelTotal = new PlayerDuelTotal();
            duelTotal.actor = actorNr;
            duelTotal.duelActor = totals[0].duelActor;
            duelTotal.results = new sbyte[totals.Length];
            duelTotal.pointResult = 0;
            for (int i = 0; i < totals.Length; i++)
            {
                duelTotal.results[i] = totals[i].point;
                duelTotal.pointResult += totals[i].point;
            }
            duelTotal.amount = (duelTotal.pointResult > 0) ? (duelTotal.pointResult * currencyAmount)-(duelTotal.pointResult * currencyAmount * tax) : -(duelTotal.pointResult * currencyAmount);
            duelTotal.duelAmount = (duelTotal.pointResult < 0) ? (duelTotal.pointResult * currencyAmount) - (duelTotal.pointResult * currencyAmount * tax) : -(duelTotal.pointResult * currencyAmount);
            playerDuelTotal[actorNr].Add(duelTotal);
        }
        void SetDecktotal(object duelActor ,DeckTotal deckTotal,DuelCardResult result,int multiply)
        {
            deckTotal.duelActor = duelActor;
            deckTotal.result = result;
            deckTotal.point = (sbyte)multiply;
        }
        public Dictionary<object, Hashtable> GetDeckRank(object actorNr)
        {
            var deckRank = playerDeckRank[actorNr];
            Dictionary<object, Hashtable> rowList = new Dictionary<object, Hashtable>();
            for (int i = 0; i < deckRank.Count(); i++)
            {
                rowList.Add(i, deckRank[i].Serialize());
            }
            return rowList;
        }
        public List<PlayerDuelTotal> GetDuelTotal(object actorNr)
        {
            return playerDuelTotal[actorNr];
        }
       /* public Dictionary<object, Hashtable> GetDeckTotal(object actorNr)
        {
            var decktotal = playerDeckTotal[actorNr];

        }*/
    }

    public class DeckRank
    {
        public object ownerActor;
        public byte point;
        public byte amount;
        public Suits suits;
        public bool isGhost;
        public bool isFlush;
        public bool isStraight;
        public bool isStraightFlush;
        public bool isThreeofKinds;
        public int higherCard;
        public sbyte multiply;
        public CardRank cardRank;
        public Hashtable Serialize()
        {
            var hash = new Hashtable();
            hash.Add("ownerActor", ownerActor);
            hash.Add("point", point);
            hash.Add("suits", suits);
            hash.Add("isGhost", isGhost);
            hash.Add("isFlush", isFlush);
            hash.Add("isStraight", isStraight);
            hash.Add("isStraightFlush", isStraightFlush);
            hash.Add("isThreeofKinds", isThreeofKinds);
            hash.Add("higherCard", higherCard);
            hash.Add("multiply", multiply);
            hash.Add("cardRank", cardRank);
            return hash;
        }
    }
    public class DeckTotal
    {
        public object duelActor;
        public DuelCardResult result;
        public sbyte point;
        public float currencyTotal;
        public Hashtable Serialize()
        {
            var hash = new Hashtable();
            hash.Add("duelActor", duelActor);
            hash.Add("result", result);
            hash.Add("point", point);
            return hash;
        }
    }
    public class PlayerDuelTotal
    {
        public object actor;
        public object duelActor;
        public sbyte[] results;
        public int pointResult;
        public float amount;
        public float duelAmount;
        public Hashtable Serialize()
        {
            var hash = new Hashtable();
            hash.Add("actor", actor);
            hash.Add("duelActor", duelActor);
            hash.Add("results", results);
            hash.Add("pointResult", pointResult);
            hash.Add("currencyAmount", amount);
            hash.Add("duelCurrencyAmount", duelAmount);
            return hash;
        }
    }
}
public static class GameConstant
{
    public const float GAMETIME = 60;
    public const float REDUCE_TIME = 1;
    public const float RESTART_TIME = 10;

}
