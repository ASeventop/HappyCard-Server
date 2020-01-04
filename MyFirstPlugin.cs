using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Hive.Plugin;
namespace MyFirstPlugin
{
    public class MyFirstPlugin : PluginBase
    {
        CardGame cardGame;
        SeatManager seatManager;
        object Timer;
        Currency currency;
        int currencyAmount;
        float tax;
        public MyFirstPlugin()
        {
            seatManager = new SeatManager();
            currency = Currency.Tik;
            currencyAmount = 100;
            tax = 0.0025f;
        }
        public override string Name
        {
            get { return "MyFirstPlugin"; }
        }
        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            this.PluginHost.LogInfo(string.Format("OnCreateGame --------------------------- {0} by user {1}", info.Request.GameId, info.UserId));
           info.Continue(); // same as base.OnCreateGame(info);
            
        }
        public override void BeforeJoin(IBeforeJoinGameCallInfo info)
        {
            info.Continue();
        }
        public override void OnJoin(IJoinGameCallInfo info)
        {
            this.PluginHost.LogInfo(string.Format("{0} has joined game is userid {1}", info.Nickname, info.UserId));
            info.Continue();
        }
        //Client Raise To server
        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            EventCode eventCode = (EventCode)info.Request.EvCode;
            var eventString = eventCode;
            //this.PluginHost.LogInfo(string.Format("OnRaiseEvent send {0} status {1} ,{2},{3},{4},{5},{6},{7}", info.Nickname, info.Status, info.Request.Group, info.ActorNr, info.UserId, info.OperationRequest.Parameters.Count, eventCode.ToString(), info.Request.OperationCode));
            base.OnRaiseEvent(info);
            CheckEventCode((EventCode)info.Request.EvCode,info);
        }

        void CheckEventCode(EventCode @event, IRaiseEventCallInfo info)
        {
            switch (@event)
            {
                case EventCode.RequestSeatData:
                    SendSeatData(info);
                    break;
                case EventCode.PlayerReady:
                    CheckPlayerReady(info);
                    break;
                case EventCode.SendCard:
                    break;
                case EventCode.SitRequest:
                    CheckSeat(info);
                    break;
                case EventCode.ReadyRequest:
                    CheckPlayerReady(info);
                    break;
                case EventCode.UpdatePlayerDeck:
                    UpdatePlayerDeck(info);
                    break;
                case EventCode.PlayerUpdateDeckEnd:
                    PlayerUpdateDeckEnd(info);
                    break;
            }
        }
        void PlayerUpdateDeckEnd(IRaiseEventCallInfo info)
        {
            seatManager.PlayerUpdateDeckEnd(info.ActorNr);
            RaiseEvent((byte)EventCode.PlayerUpdateDeckEnd,null, ReciverGroup.All,info.ActorNr);
            CheckPlayerEndUpdate();
        }
        void UpdatePlayerDeck(IRaiseEventCallInfo info)
        {
            byte[] data = info.Request.Parameters[245] as byte[];
            cardGame.UpdatePlayerDeck(info.ActorNr,data);
            CheckCard();
        }
        void SendSeatData(IRaiseEventCallInfo info)
        {
            this.PluginHost.LogInfo("send seat data  is " );
            var seatdata = seatManager.GetData();
            this.PluginHost.LogInfo("seatdata is "+seatdata);
            RaiseEvent((byte)EventCode.ReceiveSeatData, seatdata);
        }
        void CheckSeat(IRaiseEventCallInfo info)
        {
            this.PluginHost.LogInfo("Actor " + info.ActorNr);
            byte[] data = info.Request.Parameters[245] as byte[];
            this.PluginHost.LogInfo("data " + data[0]);
            if (seatManager.Register(info.ActorNr, data[0]))
            {
                object[] sendData = new object[] { info.ActorNr, data[0] };
                RaiseEvent((byte)EventCode.SitAccept, sendData);
            }
        }
        void CheckPlayerReady(IRaiseEventCallInfo info)
        {
            this.PluginHost.LogInfo(" PluginHost.GameActors.Count aaa" + PluginHost.GameActors.Count);
            byte[] data = info.Request.Parameters[245] as byte[];
            if(seatManager.PlayerReady(info.ActorNr, data[0],data[1] == 0 ? false : true))
            {
                object[] sendData = new object[] { info.ActorNr, data[0], data[1] };
                if (seatManager.AllPlayerReady())
                {
                    //RaiseEvent((byte)EventCode.GameReady, null);
                    RaiseEvent((byte)EventCode.ReadyAccept, sendData);
                    StartGame();
                }
                else
                {
                    RaiseEvent((byte)EventCode.ReadyAccept, sendData);
                }
            }
        }

        void StartGame()
        {
            seatManager.RestartUpdateDeck();
            cardGame = new CardGame(currencyAmount,tax,currency);
            this.PluginHost.LogInfo("StartGame GetPlayerInSeat" + seatManager.GetPlayerInSeat().Count);
            for (int i = 0; i < cardGame.deck.Cards.Length; i++)
            {
                if(cardGame.deck.Cards[i] == null)
                {
                    this.PluginHost.LogInfo("cardGame index" + i+" is null");
                }
            }
            cardGame.Start(seatManager.GetPlayerInSeat());
            // send card data for all player;
            foreach (var deck in cardGame.playerDecks)
            {
                var player = this.PluginHost.GameActors.FirstOrDefault(p => p.ActorNr == (int)deck.Key);
                if (player != null)
                {
                    Dictionary<string, object> sendData = new Dictionary<string, object>();
                    sendData.Add("deck", cardGame.GetCardIDFormDeck(player.ActorNr));
                    RaiseEvent((byte)EventCode.DistributeCard,sendData,ReciverGroup.All,player.ActorNr);
                }
            }
            // check card
            CheckCard();
            CheckDeckRank();


            Timer = PluginHost.CreateTimer(ScheduledEvent, 1000,1000);

        }
        private void ScheduledEvent()
        {
            cardGame.time -= GameConstant.REDUCE_TIME;
            var sendData = new object[] { cardGame.time };
            RaiseEvent((byte)EventCode.UpdateTimer, sendData, ReciverGroup.All);
            if (cardGame.time <= 0)
            {
                PluginHost.StopTimer(Timer);
                seatManager.ForceAllPlayerUpdateEnd();
                CheckPlayerEndUpdate();
            }
        }
        void ResetGameEvent()
        {
           
            cardGame.restartTime -= GameConstant.REDUCE_TIME;
            var sendData = new object[] { cardGame.restartTime };
            RaiseEvent((byte)EventCode.RestartGameTimer, sendData, ReciverGroup.All);
            if (cardGame.restartTime <= 0)
            {
                PluginHost.StopTimer(Timer);
                RestartGame();
            }
        }
        void RestartGame()
        {
            if (seatManager.AllPlayerReady())
            {
                StartGame();
            }
        }
        void CheckPlayerEndUpdate()
        {
           // this.PluginHost.LogInfo("CheckPlayerEndUpdate " + seatManager.AllPlayerUpdateEnd());
            if (seatManager.AllPlayerUpdateEnd())
            {
                PluginHost.StopTimer(Timer);
                RaiseEvent((byte)EventCode.UpdateDeckEnd, null, ReciverGroup.All);
                AllPlayerPair();
            }
        }
        void AllPlayerPair()
        {
            this.PluginHost.LogInfo(string.Format("PairAllPlayers"));
            cardGame.PairAllPlayers();
            // var test = cardGame.GetPlayerDeckTotal(cardGame)
            this.PluginHost.LogInfo(string.Format("PairAllPlayers is READY!!!!"));
            // check card

            CheckCard();
            CheckDeckRank();
            CheckOverAll();

            SendResult();

            Timer = PluginHost.CreateTimer(ResetGameEvent, 1000, 1000);
        }
        void SendResult()
        {
            var data = new Dictionary<object,object>();
            foreach (var player in cardGame.playerReady)
            {
                data.Add(player.Key, GetPlayerResult(player.Key));
            }
            this.PluginHost.LogInfo(string.Format("data {0}", data));

            foreach (var item in data)
            {
                this.PluginHost.LogInfo(string.Format("item key {0} value {1}", item.Key, item.Value));
                foreach (var aaa in item.Value as Dictionary<object, object>)
                {
                    this.PluginHost.LogInfo(string.Format("aaa key {0} aaa.value {1}", aaa.Key, aaa.Value));
                }
            }
            RaiseEvent((byte)EventCode.GameResult, data, ReciverGroup.All);
        }
        Dictionary<object, object> GetPlayerResult(object actorNr)
        {
           // Hashtable playerData = new Hashtable();
            Dictionary<object, object> playerData = new Dictionary<object, object>();
            var duelTotals = cardGame.GetDuelTotal(actorNr);
            //List<Hashtable> duelTotalsArray = new List<Hashtable>();
            Hashtable[] duelTotalsArray = new Hashtable[duelTotals.Count];
            for (int j = 0; j < duelTotals.Count; j++)
            {
                var serialize = duelTotals[j].Serialize();
                this.PluginHost.LogInfo(string.Format("duelTotals Serialize {0},{1}", serialize["actor"],serialize["results"]));
                //duelTotalsArray.Add(duelTotals[j].Serialize());
                duelTotalsArray[j] = duelTotals[j].Serialize();
            }
            playerData.Add("cards", cardGame.GetCardIDFormDeck(actorNr));
            playerData.Add("totals", duelTotalsArray);

            return playerData;
        }
        public void CheckCard()
        {
            foreach (var playerCard in cardGame.playerCards.Values)
            {
                foreach (var cards in playerCard)
                {
                    for (int i = 0; i < cards.Length; i++)
                    {
                        this.PluginHost.LogInfo(string.Format("card [{0}] is {1}", i, cards[i].cardValue));
                    }
                }
                this.PluginHost.LogInfo(string.Format("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"));
            }
        }
        public void CheckDeckRank() {

            foreach (var deckrank in cardGame.playerDeckRank.Values)
            {
                for (int i = 0; i < deckrank.Length; i++)
                {
                    this.PluginHost.LogInfo(string.Format("deckrank[{0}] point {1} rankcode {2}", i, deckrank[i].point, deckrank[i].cardRank));
                }
                this.PluginHost.LogInfo(string.Format("----------------------end deckrank-------------------------"));
            }
        }
        public void CheckOverAll()
        {
            foreach (var item in cardGame.playerDeckTotal)
            {
                var key = item.Key;
                List<DeckTotal[]> value = item.Value;
                this.PluginHost.LogInfo(string.Format("key {0}", key));
                foreach (var deckRank in cardGame.playerDeckRank)
                {
                    for (int i = 0; i < deckRank.Value.Length; i++)
                    {
                        var dr = deckRank.Value[i];
                        this.PluginHost.LogInfo(string.Format("name:{0} multiply:{1} point:{2} cardrank:{3} highercard:{4}, ", deckRank.Key, dr.multiply, dr.point, dr.cardRank, dr.higherCard));
                    }
                    this.PluginHost.LogInfo(string.Format("-----------------------------------"));
                }
                for (int i = 0; i < item.Value.Count; i++)
                {
                    this.PluginHost.LogInfo(string.Format("-----------------------------------"));
                    foreach (var decktotal in item.Value[i])
                    {
                        this.PluginHost.LogInfo(string.Format("result {0} , point {1}", decktotal.result, decktotal.point));
                    }
                }
            }
        }
        public void RaiseEvent(byte eventCode, object eventData,
                                byte receiverGroup = ReciverGroup.All,
                                int senderActorNumber = 0,
                                byte cachingOption = CacheOperations.DoNotCache,
                                byte interestGroup = 0,
                                SendParameters sendParams = default(SendParameters))
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters.Add(245, eventData);
            parameters.Add(254, senderActorNumber);
            PluginHost.BroadcastEvent(receiverGroup, senderActorNumber, interestGroup, eventCode, parameters, cachingOption, sendParams);
        }

        public void RaiseEvent(byte eventCode, object eventData, IList<int> targetActorsNumbers,
            int senderActorNumber = 0,
            byte cachingOption = CacheOperations.DoNotCache,
            SendParameters sendParams = default(SendParameters))
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters.Add(245, eventData);
            parameters.Add(254, senderActorNumber);
            PluginHost.BroadcastEvent(targetActorsNumbers, senderActorNumber, eventCode, parameters, cachingOption, sendParams);
        }
    }
}
