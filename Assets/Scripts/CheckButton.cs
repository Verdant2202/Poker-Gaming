using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CheckButton : MonoBehaviour
{
    public Button but;
    public Image img;
    // Start is called before the first frame update
    void Start()
    {
        img.color = Color.red;
    }

    public void Check()
    {
        but.interactable = false;
        img.color = Color.green;
        BettingManager.Instance.CheckServerRpc(BettingManager.Instance.currentPlayer);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
