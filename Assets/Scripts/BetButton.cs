using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public class BetButton : MonoBehaviour
{
    public TMP_InputField input;
    public GameObject oppPlayerCheckSign;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void Bet()
    {
        

        if (input.text != string.Empty)
        {
            if (BettingManager.Instance.bettingState.Value == BettingManager.State.gameStart)
            {
                if (BettingManager.Instance.playerMoney >= int.Parse(input.text))
                {
                    if (BettingManager.Instance.bigBlind.Value == BettingManager.Instance.currentPlayer && int.Parse(input.text) > 1)
                    {
                        BettingManager.Instance.BetPlacedServerRpc(BettingManager.Instance.currentPlayer, int.Parse(input.text), BettingManager.Instance.playerPlaced, BettingManager.Instance.playerMoney);
                        input.text = null;
                    }

                    if (BettingManager.Instance.smallBlind.Value == BettingManager.Instance.currentPlayer && int.Parse(input.text) < BettingManager.Instance.bigBlindMoney.Value)
                    {
                        BettingManager.Instance.BetPlacedServerRpc(BettingManager.Instance.currentPlayer, int.Parse(input.text), BettingManager.Instance.playerPlaced, BettingManager.Instance.playerMoney);
                        input.text = null;
                    }
                }
            }
            else
            {
                if (int.Parse(input.text) > BettingManager.Instance.reqMoney.Value)
                {
                    if (BettingManager.Instance.playerMoney + BettingManager.Instance.playerPlaced >= int.Parse(input.text))
                    {
                        oppPlayerCheckSign.SetActive(false);
                        BettingManager.Instance.BetPlacedServerRpc(BettingManager.Instance.currentPlayer, int.Parse(input.text), BettingManager.Instance.playerPlaced, BettingManager.Instance.playerMoney);
                        input.text = null;
                    }

                }
                
            }
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
