using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin
{
    public enum EventCode : byte
    {
        RequestSeatData,
        ReceiveSeatData,
        PlayerReady,
        SendCard,
        SitRequest,
        SitAccept,
        ReadyRequest,
        ReadyAccept,
        GameReady,
        DistributeCard,
        UpdatePlayerDeck,
        UpdateTimer,
        PlayerUpdateDeckEnd,
        UpdateDeckEnd,
        RestartGameTimer,
        GameResult

    }
    public enum Suits
    {
        Clubs, Diamonds, Hearts, Spades,NONE
    }
    public enum CardRank:byte
    {
        Point, Gost, Straight, StraightFlush, ThreeofKind
    }
    public enum DuelCardResult:byte
    {
        Draw,Win,Lose
    }
    public enum Currency : byte
    {
        Tik,Coin
    }
}
