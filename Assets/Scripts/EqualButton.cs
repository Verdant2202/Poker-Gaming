using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EqualButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Equal()
    {
        
        if (BettingManager.Instance.playerMoney >= BettingManager.Instance.reqMoney.Value - BettingManager.Instance.playerPlaced)
        {
            BettingManager.Instance.EqualServerRpc(BettingManager.Instance.currentPlayer, BettingManager.Instance.playerPlaced, BettingManager.Instance.playerMoney);
        }
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
