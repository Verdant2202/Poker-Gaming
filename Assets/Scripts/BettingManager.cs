using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
public class BettingManager : NetworkBehaviour
{
    public static BettingManager Instance { get; set; }
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

    public NetworkVariable<int> playersChecked = new NetworkVariable<int>();
    public GameObject textbar;
    public Button checkbutton;
    public Button betbutton;
    public Button equalbutton;
    public Button passbutton;
    public int currentPlayer = 1;
    public NetworkVariable<int> playersNotPaid = new NetworkVariable<int>(0);
    public NetworkVariable<int> reqMoney = new NetworkVariable<int>(0);
    public int playerMoney = 50;
    public int playerPlaced = 0;
    public TextMeshProUGUI Player2Placed;
    public TextMeshProUGUI PlayerMoney;
    public TextMeshProUGUI PlayerPlaced;
    public NetworkVariable<int> playersInGame = new NetworkVariable<int>(2);

    public NetworkVariable<int> bigBlind = new NetworkVariable<int>();
    public NetworkVariable<int> smallBlind = new NetworkVariable<int>();

    public NetworkVariable<int> bigBlindMoney = new NetworkVariable<int>(0);
    public NetworkVariable<int> smallBlindMoney = new NetworkVariable<int>(0);
    public NetworkVariable<int> pot = new NetworkVariable<int>(0);

    public GameObject oppPlayerCheckSign;
    public enum State 
    {
        playerEqualing,
        waitForDecision,
        gameStart
    };


    public NetworkVariable<State> bettingState = new NetworkVariable<State>(State.gameStart);

    [ServerRpc]
    public void StartGameServerRpc()
    {
        int temp = Random.Range(1, 3);
        if (temp == 1)
        {
            bigBlind.Value = 1;
            smallBlind.Value = 2;
        }
        else
        {
            smallBlind.Value = 1;
            bigBlind.Value = 2;
        }
        pot.Value = 0;
    }
    
  
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        oppPlayerCheckSign.SetActive(false);
        playerMoney = NetworkButtonsStarter.Instance.Money;
        UpdatePlayerMoney();
        checkbutton.interactable = false;
        equalbutton.interactable = false;
        passbutton.interactable = false;
        betbutton.interactable = false;
        currentPlayer = NetworkButtonsStarter.Instance.playerID;
        bigBlind.OnValueChanged = (int prevValue, int newValue) =>
        {
            if (currentPlayer == newValue)
            {
                betbutton.GetComponentInChildren<TextMeshProUGUI>().text = "Big Blind";
                betbutton.interactable = true;
            }
            else
            {
                betbutton.GetComponentInChildren<TextMeshProUGUI>().text = "Small Blind";
                betbutton.interactable = false;
            }
        };

        bigBlindMoney.OnValueChanged = (int prevValue, int newValue) =>
        {
            if (currentPlayer == smallBlind.Value)
            {
                betbutton.interactable = true;
            }
        };
    }
    [ServerRpc(RequireOwnership = false)]
    public void CheckServerRpc(int player)
    {
        playersChecked.Value++;
        UpdateButtonsClientRpc(2, 2, 0, 2, player);
        UpdateCheckedOppServerRpc(player, true);
        if (playersChecked.Value == playersInGame.Value)
        {
            playersChecked.Value = 0;
            reqMoney.Value = 0;
            RoundManager.Instance.ContinueGameClientRpc();
            if(RoundManager.Instance.gameState == RoundManager.State.over)
            {
                UpdateButtonsClientRpc(0, 0, 0, 0, player);
                UpdateButtonsClientRpc(0, 0, 0, 0, player % 2 + 1);
            }
            else
            {
                UpdateButtonsClientRpc(1, 0, 1, 0, player);
                UpdateButtonsClientRpc(1, 0, 1, 0, player % 2 + 1);
            }
            UpdatePlayerPlacedClientRpc(0, 1);
            UpdatePlayerPlacedClientRpc(0, 2);
            SetCheckButtonColorClientRpc();
            
        }
    }
    
    [ClientRpc]
    void SetCheckButtonColorClientRpc()
    {
        checkbutton.GetComponent<Image>().color = Color.red;
    }
    [ServerRpc(RequireOwnership = false)]
    public void EqualServerRpc(int player, int playerplaced, int playermoney)
    {
        UpdatePlayerMoneyClientRpc( playermoney - (reqMoney.Value - playerplaced), player);
        pot.Value += reqMoney.Value - playerplaced;
        UpdatePlayerPlacedClientRpc(reqMoney.Value, player);
        playersNotPaid.Value -= 1;
        UpdateButtonsClientRpc(0, 0, 0, 0, player);
        if (playersNotPaid.Value == 0)
        {   
            bettingState.Value = State.waitForDecision;
            UpdateButtonsClientRpc(1, 0, 1, 0, player);
            UpdateButtonsClientRpc(1, 0, 1, 0, player % 2 + 1);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void PassServerRpc(int player)
    {
        playersInGame.Value -= 1;
        UpdateButtonsClientRpc(0, 0, 0, 0, player);
        RoundManager.Instance.gameState = RoundManager.State.third;
        ChangeWinValueServerRpc(player % 2 + 1);
        CheckServerRpc(player % 2 + 1);
        RoundManager.Instance.ContinueGameClientRpc();
        RoundManager.Instance.gameState = RoundManager.State.over;
        UpdateButtonsClientRpc(0, 0, 0, 0, 1);
        UpdateButtonsClientRpc(0, 0, 0, 0, 2);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeWinValueServerRpc(int player)
    {
        GameManager.Instance.whichplayerwins = player;   
    }
    
    void UpdatePlayerMoney()
    {
         PlayerMoney.text = playerMoney.ToString();
    }

    void UpdateTableMoney()
    {
        PlayerPlaced.text = playerPlaced.ToString();
        UpdatePlacedOppServerRpc(playerPlaced, currentPlayer);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdatePlacedOppServerRpc(int playerplaced, int player)
    {
        UpdateTableMoneyClientRpc(playerplaced, player);
    }

    [ClientRpc]
    void UpdateTableMoneyClientRpc(int oppplayerplaced, int player)
    {
        
        if(currentPlayer == player % 2 + 1)
        {
            PlayerPlaced.text = playerPlaced.ToString();
            Player2Placed.text = oppplayerplaced.ToString();
        }
    
    }


    [ServerRpc(RequireOwnership = false)]
    void UpdateCheckedOppServerRpc(int player, bool enable)
    {
        UpdateCheckedClientRpc(player, enable);
    }

    [ClientRpc]
    void UpdateCheckedClientRpc(int player, bool enable)
    {

        if (currentPlayer == player % 2 + 1)
        {
            oppPlayerCheckSign.SetActive(enable);
        }

    }

    [ClientRpc]
    void UpdatePlayerPlacedClientRpc(int val, int playernum)
    {
        if (playernum == currentPlayer)
        {
            playerPlaced = val;
        }
        UpdateTableMoney();
    }

    [ClientRpc]
    void UpdatePlayerMoneyClientRpc(int val, int playernum)
    {
        if (playernum == currentPlayer)
        {
            playerMoney = val;
        }
        UpdatePlayerMoney();
    }

    [ClientRpc]
    public void UpdateButtonsClientRpc(int check, int pass, int bet, int call, int playernum)
    {
        if (playernum != currentPlayer)
        {
            return;
        }

        if (check != 2)
        {
            if (check == 1)
            {
                checkbutton.interactable = true;
            }

            if (check == 0)
            {
                checkbutton.interactable = false;
            }
        }
        if (pass != 2)
        {
            if (pass == 1)
            {
                passbutton.interactable = true;
            }

            if (pass == 0)
            {
                passbutton.interactable = false;
            }
        }
        if (bet != 2)
        {
            if (bet == 1)
            {
                betbutton.interactable = true;
            }

            if (bet == 0)
            {
                betbutton.interactable = false;
            }
        }
        if (call != 2)
        {
            if (call == 1)
            {
                equalbutton.interactable = true;
            }

            if (call == 0)
            {
                equalbutton.interactable = false;
            }
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void BetPlacedServerRpc(int player, int money, int playerplaced, int playermoney)
    {
        if(reqMoney.Value == 0)
        {
            UpdatePlayerPlacedClientRpc(0, player);
        }
        checkbutton.GetComponent<Image>().color = Color.red;
        if(!(bettingState.Value == State.gameStart))
        {
            UpdateButtonsClientRpc(0, 1, 1, 1, player % 2 + 1);
            reqMoney.Value = money;
            UpdatePlayerMoneyClientRpc(playermoney - (money - playerplaced), player);
            pot.Value += money - playerplaced;
            UpdatePlayerPlacedClientRpc(money, player);
        }
        else
        {
            UpdatePlayerMoneyClientRpc(playermoney - money, player);
        }

        playersNotPaid.Value = 2;
        playersChecked.Value = 0;
        UpdateCheckedOppServerRpc(player, false);
        SetCheckButtonColorClientRpc();
        if (bettingState.Value != State.gameStart)
        { 
            playersNotPaid.Value = 1;
            bettingState.Value = State.playerEqualing;
        }
        UpdateButtonsClientRpc(0,0,0,0, player);
        UpdateButtonsClientRpc(0, 2, 2, 2, player % 2 + 1);

        if (bettingState.Value == State.gameStart)
        {
            if (player == smallBlind.Value)
            {
                smallBlindMoney.Value = money;
                pot.Value += money;
                UpdatePlayerPlacedClientRpc(0, 1);
                UpdatePlayerPlacedClientRpc(0, 2);
                bettingState.Value = State.waitForDecision;
                RoundManager.Instance.ContinueGameClientRpc();
                bettingState.Value = State.waitForDecision;
                playersChecked.Value = 0;
            }
            else
            {
                bigBlindMoney.Value = money;
                UpdatePlayerPlacedClientRpc(money, player);
                pot.Value += money;
      
            }

    
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
