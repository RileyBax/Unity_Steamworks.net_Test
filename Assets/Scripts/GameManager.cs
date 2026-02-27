using PurrNet;
using PurrNet.Modules;
using Steamworks;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    public TextMeshProUGUI playerText;
    private LobbyManager lobbyManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();

        UpdatePlayerCount();
    }

    [ObserversRpc(runLocally:true)]
    private void UpdatePlayerCount()
    {

        playerText.text = lobbyManager.GetPlayerCount().ToString();

    }

}
