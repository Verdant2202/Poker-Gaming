using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class FGBackButton : MonoBehaviour
{
    public Button gobackbutton;
    // Start is called before the first frame update
    void Start()
    {
        gobackbutton.onClick.AddListener(() => { SceneManager.LoadScene(sceneName: "StartScene", LoadSceneMode.Single); Destroy(NetworkManager.Singleton.gameObject); Destroy(NetworkButtonsStarter.Instance.gameObject); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
