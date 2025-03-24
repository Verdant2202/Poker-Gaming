using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using UnityEngine.UI;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
       
    }


    public List<CardSO> Cards;
    public int PlayerCount = 2;
    public List<CardSO> Player1Cards;
    public List<CardSO> Player2Cards;
    public List<CardSO> TableCards;
    List<int> ChosenNumbers = new List<int>();
    List<Card> player1cards = new List<Card>();
    List<Card> player2cards = new List<Card>();
    List<Card> tablecards = new List<Card>();

    public GameObject p1obj;
    public GameObject p2obj;
    public GameObject tobj;
    int iterator = 0;
    public int whichplayerwins;



    [ServerRpc]
    void GameManagerStartServerRpc()
    {
        SelectCardsServerRpc();
        whichplayerwins = WhichPlayerWins();
        BettingManager.Instance.StartGameServerRpc();
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
    
        if (IsServer)
        {
            GameManagerStartServerRpc();
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            WhichPlayerWins();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
           // SelectCards();
            WhichPlayerWins();
        }
    }

    struct Card
    {
        public int Value;
        public int Color;
        public int from; //player = 1/2, table = 3
    };

    int ChooseCard()
    {
        int ran = Random.Range(0, 52);
        while(ChosenNumbers.Contains(ran))
        {
            ran = Random.Range(0, 52);
        }
        ChosenNumbers.Add(ran);
        return ran;
    }
    [ServerRpc]
    void SelectCardsServerRpc()
    {
        ChosenNumbers.Clear();
        for (int i = 0; i < 9; i++)
        {
            int num = ChooseCard();
            TakeCardsClientRpc(num);
        }

    }

    [ClientRpc]
    void TakeCardsClientRpc(int cnum)
    {
        if(iterator == 0)
        {
            Player1Cards.Clear();
            Player2Cards.Clear();
            TableCards.Clear();
        }
        if(iterator < 2)
        {
            CardSO newCard = Cards[cnum];
            Player1Cards.Add(newCard);
        }
        else if(iterator < 4)
        {
            CardSO newCard = Cards[cnum];
            Player2Cards.Add(newCard);
        }
        else if(iterator < 9)
        {
            CardSO newCard = Cards[cnum];
            TableCards.Add(newCard);
        }
        iterator++;
    }

    int WhichPlayerWins()
    {
        player1cards.Clear();
        player2cards.Clear();
        tablecards.Clear();

        for(int i = 0; i < Player1Cards.Count; i++)
        {
            Card newCard = new Card();
            newCard.Value = Player1Cards[i].Value;
            newCard.Color = Player1Cards[i].Color;
            newCard.from = 1;
            player1cards.Add(newCard);
        }

        for (int i = 0; i < Player2Cards.Count; i++)
        {
            Card newCard = new Card();
            newCard.Value = Player2Cards[i].Value;
            newCard.Color = Player2Cards[i].Color;
            newCard.from = 2;
            player2cards.Add(newCard);
        }

        for (int i = 0; i < TableCards.Count; i++)
        {
            Card newCard = new Card();
            newCard.Value = TableCards[i].Value;
            newCard.Color = TableCards[i].Color;
            newCard.from = 3;
            tablecards.Add(newCard);
        }


        int pl1 = RunWinFuncs(1);
        int pl2 = RunWinFuncs(2);
        if(pl1 > pl2)
        {
            return 1;
        }
        if (pl2 > pl1)
        {
            return 2;
        }

        int tiebreak = 0;
        if (pl2 == pl1)
        {
            switch (pl1)
            {
                case 0:
                    tiebreak = 3;
                    break;
                case 1:
                    if (Pair(1).Item2.Max(x => x.Value) > Pair(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (Pair(2).Item2.Max(x => x.Value) > Pair(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (Pair(1).Item2.Max(x => x.Value) == Pair(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 3;
                    }
                    break;
                case 2:
                    if (TwoPairs(1).Item2.Max(x => x.Value) > TwoPairs(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (TwoPairs(2).Item2.Max(x => x.Value) > TwoPairs(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (TwoPairs(1).Item2.Max(x => x.Value) == TwoPairs(2).Item2.Max(x => x.Value))
                    {
                        List<Card> p1 = TwoPairs(1).Item2;
                        List<Card> p2 = TwoPairs(2).Item2;
                        p1.RemoveAll(x => x.Value == p1.Max(x => x.Value));
                        p2.RemoveAll(x => x.Value == p2.Max(x => x.Value));
                        if (TwoPairs(1).Item2.Max(x => x.Value) > TwoPairs(2).Item2.Max(x => x.Value))
                        {
                            tiebreak = 1;
                        }
                        if (TwoPairs(2).Item2.Max(x => x.Value) > TwoPairs(1).Item2.Max(x => x.Value))
                        {
                            tiebreak = 2;
                        }
                        if (TwoPairs(1).Item2.Max(x => x.Value) == TwoPairs(2).Item2.Max(x => x.Value))
                        {
                            tiebreak = 3;
                        }
                       
                    }
                    break;
                case 3:
                    if (Triple(1).Item2.Max(x => x.Value) > Triple(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (Triple(2).Item2.Max(x => x.Value) > Triple(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (Triple(1).Item2.Max(x => x.Value) == Triple(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 3;
                    }
                    break;
                case 4:
                    if (Strit(1).Item2.Max(x => x.Value) > Strit(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (Strit(2).Item2.Max(x => x.Value) > Strit(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (Strit(1).Item2.Max(x => x.Value) == Strit(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 3;
                    }
                    break;
                case 5:
                    if (Color(1).Item2.Max(x => x.Value) > Color(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (Color(2).Item2.Max(x => x.Value) > Color(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (Color(1).Item2.Max(x => x.Value) == Color(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 3;
                    }
                    break;
                case 6:
                    tiebreak = 1;
                    break;
                case 7:
                    if (Kareta(1).Item2.Max(x => x.Value) > Kareta(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (Kareta(2).Item2.Max(x => x.Value) > Kareta(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (Kareta(1).Item2.Max(x => x.Value) == Kareta(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 3;
                    }
                    break;
                case 8:
                    if (FlushStrit(1).Item2.Max(x => x.Value) > FlushStrit(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 1;
                    }
                    if (FlushStrit(2).Item2.Max(x => x.Value) > FlushStrit(1).Item2.Max(x => x.Value))
                    {
                        tiebreak = 2;
                    }
                    if (FlushStrit(1).Item2.Max(x => x.Value) == FlushStrit(2).Item2.Max(x => x.Value))
                    {
                        tiebreak = 3;
                    }
                    break;

            }

            if (tiebreak == 1)
            {
                return 1;
            }
            if(tiebreak == 2)
            {
                return 2;
            }
            if(tiebreak == 3)
            {
                tiebreak = HighCard();
                if (tiebreak == 1)
                {
                    return 1;
                }
                if (tiebreak == 2)
                {
                    return 2;
                }
                if(tiebreak == 3)
                {
                    return 3;
                }
            }
        }
        return 0;
    }


    int RunWinFuncs(int player)
    {
        int num = 0;
        if (Pair(player).Item1)
        {
            num = 1;
        }
        if (TwoPairs(player).Item1)
        {
            num = 2;
        }
        if (Triple(player).Item1)
        {
            num = 3;
        }
        if (Strit(player).Item1)
        {
            num = 4;
        }
        if (Color(player).Item1)
        {
            num = 5;
        }
        if (Full(player).Item1)
        {
            num = 6;
        }
        if (Kareta(player).Item1)
        {
            num = 7;
        }
        if (FlushStrit(player).Item1)
        {
            num = 8;
        }
        if (RoyalFlush(player))
        {
            num = 9;
        }
        return num;
    }
    int HighCard()
    {
            int player1highcard = player1cards.Max(x => x.Value);
            int player2highcard = player2cards.Max(x => x.Value);
            if (player1highcard > player2highcard)
            {
                return 1;
            }
            if(player2highcard > player1highcard)
            {
                return 2;
            }
            if(player2highcard == player1highcard)
            {
                player1cards.Remove(player1cards.Find(x => x.Value == player1highcard));
                player2cards.Remove(player2cards.Find(x => x.Value == player2highcard));
            }

            player1highcard = player1cards.Max(x => x.Value);
            player2highcard = player2cards.Max(x => x.Value);
            if (player1highcard > player2highcard)
            {
                return 1;
            }
            if (player2highcard > player1highcard)
            {
                return 2;
            }
        return 3;
        
    }
    (bool, List<Card>) TwoPairs(int player)
    {
        List<Card> endes = new List<Card>();
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        int realcounter = 0;
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 0;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Value == curcard.Value && counter < 2 && !endes.Any(x => x.Value == curcard.Value && x.Color == curcard.Color))
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                }
            }

            if (counter == 2)
            {
            
                bool can = false;
                for (int o = 0; o < addes.Count; o++)
                {
                    if (addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if (can == true)
                {
                    for (int o = 0; o < addes.Count; o++)
                    {
                        endes.Add(addes[o]);
                    }
                    realcounter++;
                    if(realcounter >= 2)
                    {
                        return (true, endes);
                    }
                }

            }

        }

        return (false, endes);
    }
    (bool, List<Card>) Triple(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 0;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Value == curcard.Value && counter < 3)
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                }
            }

            if (counter == 3)
            {
                bool can = false;
                for (int o = 0; o < addes.Count; o++)
                {
                    if (addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if (can == true)
                {
                    return (true, addes);
                }

            }

        }

        return (false, addes);
    }
    (bool, List<Card>) Pair(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 0;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Value == curcard.Value && counter < 2)
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                }
            }

            if (counter == 2)
            {
                bool can = false;
                for (int o = 0; o < addes.Count; o++)
                {
                    if (addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if (can == true)
                {
                    return (true, addes);
                }

            }

        }

        return (false, addes);
    }
    (bool, List<Card>) Kareta(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 0;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Value == curcard.Value && counter < 4)
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                }
            }

            if (counter == 4)
            {
                bool can = false;
                for (int o = 0; o < addes.Count; o++)
                {
                    if (addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if (can == true)
                {
                    return (true, addes);
                }

            }

        }

        return (false, addes);
    }
    (bool, List<Card>) Full(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 1;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Value == curcard.Value && counter < 3 && !addes.Any(x => x.Value == mainlist[o].Value && x.Color == mainlist[o].Color))
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                }
            }

            if (counter == 3)
            {
             
                int snter = 1;
                List<Card> endes = new List<Card>();
                for (int j = 0; j < 7; j++)
                {
                    snter = 1;
                    endes.Clear();
                    curcard = mainlist[j];
                    endes.Add(curcard);
                    for (int o = 0; o < 7; o++)
                    {
                        if (mainlist[o].Value == curcard.Value && snter < 2 && !addes.Any(x => x.Value == mainlist[o].Value && x.Color == mainlist[o].Color) && !endes.Any(x => x.Value == mainlist[o].Value && x.Color == mainlist[o].Color))
                        {
                            snter++;
                            curcard = mainlist[o];
                            endes.Add(curcard);
                        }
                    }
                }


                if (snter == 2)
                {
                   
                    addes = addes.Concat(endes).ToList();
                    bool can = false;
                    for (int o = 0; o < addes.Count; o++)
                    {
                        if (addes[o].from != 3)
                        {
                            can = true;
                        }
                    }

                    if(can == true)
                    {
                        return (true, addes);
                    }
                }

            }

        }

        return (false, addes);
    }
    (bool, List<Card>) Color(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 1;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Color == curcard.Color && !addes.Contains(mainlist[o]) && counter < 5)
                {
                    counter++;
                    curcard = mainlist[o]; 
                    addes.Add(curcard);
                }
            }

            if (counter == 5)
            {
                bool can = false;
                for (int o = 0; o < addes.Count; o++)
                {
                    if (addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if (can == true)
                {
                    return (true, addes);
                }

            }

        }

        return (false, addes);
    }
    (bool, List<Card>) Strit(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }

        for (int i = 0; i < 7; i++)
        {
            int counter = 1;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if (mainlist[o].Value == curcard.Value - 1 && !addes.Contains(mainlist[o]) &&counter < 5)
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                    o = -1;
                }
            }

            if (counter == 5)
            {
                bool can = false;
                for (int o = 0; o < addes.Count; o++)
                {
                    if (addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if (can == true)
                {
                    return (true, addes);
                }

            }

        }

        return (false, addes);
    }

    (bool, List<Card>) FlushStrit(int player)
    {
        List<Card> addes = new List<Card>();
        List<Card> mainlist = new List<Card>();
        if (player == 1)
        {
            mainlist = player1cards.Concat(tablecards).ToList();
        }
        if (player == 2)
        {
            mainlist = player2cards.Concat(tablecards).ToList();
        }
       
        for (int i = 0; i < 7; i++)
        {
            int counter = 1;
            addes.Clear();
            Card curcard = mainlist[i];
            addes.Add(curcard);
            for (int o = 0; o < 7; o++)
            {
                if(mainlist[o].Value == curcard.Value - 1 && mainlist[o].Color == curcard.Color && counter < 5)
                {
                    counter++;
                    curcard = mainlist[o];
                    addes.Add(curcard);
                    o = -1;
                }     
            }

            if (counter == 5)
            {
                bool can = false;
                for(int o = 0; o < addes.Count; o++)
                {
                    if(addes[o].from != 3)
                    {
                        can = true;
                    }
                }

                if(can == true)
                {
                    return (true, addes);
                }
            
            }
            
        }

        return (false, addes);
    }

    bool RoyalFlush(int player)
    {
        int counter = 0;
        int cc = TableCards[0].Color;
        for(int i = 0; i < 5; i++)
        {
            if(TableCards[i].Color == cc && TableCards[i].Value >= 10 && TableCards[i].Value <= 14)
            {
                counter++;
            }
        }
        if(counter == 5)
        {
            return false;
        }
        List<CardSO> mainlist = new List<CardSO> ();
        if (player == 1)
        {
            mainlist = Player1Cards.Concat(TableCards).ToList();
        }
        if(player == 2)
        {
            mainlist = Player2Cards.Concat(TableCards).ToList();
        }

        List<CardSO> addes = new List<CardSO>();
        for (int i = 0; i < 7; i++)
        {
            if(mainlist[i].Value >= 10 && mainlist[i].Value <= 14)
            {
                CardSO temp = ScriptableObject.CreateInstance<CardSO>();
                temp.Value = mainlist[i].Value;
                temp.Color = mainlist[i].Color;
                addes.Add(temp);
            }
        }
        if(addes.Count >= 5)
        {

            int count = 0;
            int cval = addes[0].Color;
            for (int i = 0; i < addes.Count; i++)
            {
                if (addes[i].Color == cval)
                {
                    count++;
                }
            }
            if(count == 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
