using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Pass()
    {
         BettingManager.Instance.PassServerRpc(BettingManager.Instance.currentPlayer);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
