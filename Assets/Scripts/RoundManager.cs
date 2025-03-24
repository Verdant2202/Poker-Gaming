using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
public class RoundManager : NetworkBehaviour
{
    public GameObject[] tablecards;
    public GameObject[] player1cards;
    public GameObject[] player2cards;
    public Sprite backCard;
    public GameObject textt;
    TextMeshProUGUI wintext;
    public Button gobackbutton;
    public TextMeshProUGUI BetButtonText;
    public enum State
    {
        first,
        second,
        third,
        over
    }
    public State gameState = State.first;
    public static RoundManager Instance { get; set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

     
    }
    // Start is called before the first frame update
    void Start()
    {
        textt.SetActive(false);
        gameState = State.first;
        gobackbutton.onClick.AddListener(() => {SceneManager.LoadScene(sceneName: "StartScene", LoadSceneMode.Single); Destroy(NetworkManager.Singleton.gameObject); Destroy(NetworkButtonsStarter.Instance.gameObject); });
        wintext = textt.GetComponent<TextMeshProUGUI>();
        for (int i = 0; i < 2; i++)
        {
            player1cards[i].GetComponent<Image>().sprite = backCard;
        }
        for (int i = 0; i < 2; i++)
        {
            player2cards[i].GetComponent<Image>().sprite = backCard;
        }
        for (int i = 0; i < 5; i++)
        {
            tablecards[i].GetComponent<Image>().sprite = backCard;
        }
        //BettingManager.Instance.bettingState = BettingManager.State.gameStart;
    }
    [ClientRpc]
    public void ContinueGameClientRpc()
    {
        BettingManager.Instance.oppPlayerCheckSign.SetActive(false);
        BettingManager.Instance.checkbutton.interactable = true;
        BettingManager.Instance.betbutton.interactable = true;
        switch (gameState)
        {
            case State.first:
                BetButtonText.text = "Raise";
                UncoverPlayerCards();  
            break;
            case State.second:
                UncoverTableCards();
                break;
            case State.third:
                if (IsOwner)
                {
                    WinCallServerRpc();
                }
                UncoverEverything();
                break;
        }
        gameState++;
    }

    [ServerRpc]
    public void WinCallServerRpc()
    {
        int winner = GameManager.Instance.whichplayerwins;
        SayWhoWinsClientRpc(winner);

    }

    [ClientRpc]
    public void SayWhoWinsClientRpc(int winner)
    {
        NetworkButtonsStarter.Instance.Money = BettingManager.Instance.playerMoney;
        if (winner == BettingManager.Instance.currentPlayer)
        {
            NetworkButtonsStarter.Instance.Money += BettingManager.Instance.pot.Value;
        }
        NetworkButtonsStarter.Instance.SaveData();
        textt.SetActive(true);
        wintext.text = "Player " + winner + " wins!" + ((winner == BettingManager.Instance.currentPlayer) ? " (You)" : " (Them)");
    }

    public void UncoverPlayerCards()
    {
        for (int i = 0; i < 2; i++)
        {
            if(BettingManager.Instance.currentPlayer == 1)
            {
                player1cards[i].GetComponent<Image>().sprite = GameManager.Instance.Player1Cards[i].sprite;
            }
            else
            {
                player1cards[i].GetComponent<Image>().sprite = GameManager.Instance.Player2Cards[i].sprite;
            }
          
        }
    }
    public void UncoverTableCards()
    {
        for (int i = 0; i < 3; i++)
        {
            tablecards[i].GetComponent<Image>().sprite = GameManager.Instance.TableCards[i].sprite;
        }
    }
    public void UncoverEverything()
    {
        for (int i = 0; i < 2; i++)
        {
            if (BettingManager.Instance.currentPlayer == 1)
            {
                player1cards[i].GetComponent<Image>().sprite = GameManager.Instance.Player1Cards[i].sprite;
            }
            else
            {
                player1cards[i].GetComponent<Image>().sprite = GameManager.Instance.Player2Cards[i].sprite;
            }
        }
        for (int i = 0; i < 2; i++)
        {
            if (BettingManager.Instance.currentPlayer == 1)
            {
                player2cards[i].GetComponent<Image>().sprite = GameManager.Instance.Player2Cards[i].sprite;
            }
            else
            {
                player2cards[i].GetComponent<Image>().sprite = GameManager.Instance.Player1Cards[i].sprite;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            tablecards[i].GetComponent<Image>().sprite = GameManager.Instance.TableCards[i].sprite;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
