using PurrNet;
using PurrNet.Modules;
using Steamworks;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    private TextMeshProUGUI playerText;
    private LobbyManager lobbyManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        playerText = GameObject.Find("Players").GetComponent<TextMeshProUGUI>();

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        UpdatePlayerCount();
    }

    [ObserversRpc(runLocally:true)]
    public void UpdatePlayerCount()
    {

        playerText.text = lobbyManager.GetPlayerCount().ToString();

    }

    public void SetPlayerText(string t)
    {
        
        playerText.text= t;

    }

}
