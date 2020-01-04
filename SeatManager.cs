using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin
{
    class SeatManager
    {
        public Dictionary<object, object> seats;
        public Dictionary<object, object> playerReady;
        public Dictionary<object, bool> playerUpdateDeckEnd;
        public SeatManager()
        {
            Init();
        }
        public void Init()
        {
            seats = new Dictionary<object, object>();
            playerReady = new Dictionary<object, object>();
            playerUpdateDeckEnd = new Dictionary<object, bool>();
        }

        public bool Register(int actorNumber, byte viewID)
        {
            if (seats.ContainsKey(actorNumber)) return false;
            if (seats.ContainsValue(viewID)) return false;
            seats.Add(actorNumber, viewID);
            playerReady.Add(actorNumber, false);
            playerUpdateDeckEnd.Add(actorNumber, false);
            return true;
        }
        public void Unregister(int actorNumber)
        {
            if (seats.ContainsKey(actorNumber))
                seats.Remove(actorNumber);
            if (playerReady.ContainsKey(actorNumber))
                playerReady.Remove(actorNumber);
            if (playerUpdateDeckEnd.ContainsKey(actorNumber))
                playerUpdateDeckEnd.Remove(actorNumber);
            // send event heare
        }
        public bool AllPlayerReady()
        {
            if (playerReady.Count >= 2 && playerReady.All(player => (bool)player.Value == true))
                return true;
            return false;
        }
        public bool PlayerReady(int actorNumber, byte viewID, bool ready)
        {
            if (playerReady.ContainsKey(actorNumber))
            {
                playerReady[actorNumber] = ready;
                return true;
            }
            return false;
        }
        public Hashtable GetData()
        {
           Hashtable data = new Hashtable();
            data.Add("seats",seats);
            data.Add("playerready",playerReady);
           return data;
        }
        public int GetPlayerCount()
        {
            return playerReady.Count;
        }
        public Dictionary<object, object> GetPlayerInSeat()
        {
            seats.Where(s => s.Key == playerReady.Keys && (bool)s.Value == true);
            return seats;
        }
        public void ForceAllPlayerUpdateEnd()
        {
            foreach (var key in seats.Keys)
                PlayerUpdateDeckEnd(key);
        }
        public void PlayerUpdateDeckEnd(object ActorNr)
        {
            if (playerUpdateDeckEnd.ContainsKey(ActorNr))
                playerUpdateDeckEnd[ActorNr] = true;
        }
        public bool AllPlayerUpdateEnd()
        {
            if (playerUpdateDeckEnd.Count >= 2 && playerUpdateDeckEnd.All(player => (bool)player.Value == true))
                return true;
            return false;
        }
        public void RestartUpdateDeck()
        {
           playerUpdateDeckEnd = playerUpdateDeckEnd.ToDictionary(p => p.Key, p => false);
        }
    }
}
