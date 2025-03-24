using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.IO;
using Unity.Services.Authentication;
using TMPro;
public class NetworkButtonsStarter : NetworkBehaviour
{
    public Button ServerButton;
    public Button HostButton;
    public Button ClientButton;
    public int playerID;
    public NetworkVariable<int> curPlayerID = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public static NetworkButtonsStarter Instance { get; private set; }
    public int Money = 50;
    public PlayerData Data;
    public TextMeshProUGUI moneycount;
    public RelayObject relay;
    public string code;
    public TMP_InputField input;
    public Button FGButton;
    public Button PlusMButton;
    public int ClickCounter = 0;
    public override void OnNetworkSpawn()
    {
        playerID = curPlayerID.Value;
        AddPlayerIDValServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void AddPlayerIDValServerRpc()
    {
        curPlayerID.Value++;
        if (curPlayerID.Value == 3)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName: "Main Scene", LoadSceneMode.Single);
        }
    }

    private void Awake()
    {

        if (File.Exists(Application.persistentDataPath + "/Save.dat"))
        {
            LoadData();
        }
        else
        {
            Money = 50;
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SaveData();

    }
    void LoadData()
    {
        Data = PlayerData.Load();
        Money = Data.Money;
    }

    public void SaveData()
    {
        Data.Money = Money;
        PlayerData.Save(Data);
    }

    // Start is called before the first frame update
    void Start()
    {
        moneycount.text = Money.ToString();
        DontDestroyOnLoad(this.gameObject);
        ServerButton.onClick.AddListener(() => { NetworkManager.Singleton.StartServer();});

        HostButton.onClick.AddListener(() => 
        { 
            if (AuthenticationService.Instance.IsSignedIn == true) 
            { 
                relay.CreateRelay(); 
                HostButton.interactable = false; 
                ClientButton.interactable = false; 
            } 
        } );

        ClientButton.onClick.AddListener(() => 
        { 
            code = input.text; 
            relay.JoinRelay(code.ToUpper());
        }) ;
        //FGButton.onClick.AddListener(() => { SceneManager.LoadScene(sceneName: "Fumy", LoadSceneMode.Single); });
        PlusMButton.onClick.AddListener(() => { 

            if(input.text == "H4X0R")
            {
                ClickCounter++;
                Money += 1;
                moneycount.text = Money.ToString();
            }
        } );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
