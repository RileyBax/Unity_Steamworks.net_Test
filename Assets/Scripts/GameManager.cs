using System.Data.SqlTypes;
using PurrNet;
using PurrNet.Modules;
using Steamworks;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    private TextMeshProUGUI playerText;
    private LobbyManager lobbyManager;
    private int sq = 10;
    public NetworkTransform cubePrefab;

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

        if (isServer)
        {
            
            for (int x = 0; x < sq; x++)
            {

                for(int y = 0; y < sq; y++)
                {
                    
                    Instantiate(cubePrefab, new Vector3(x * 2 - sq/2 - 1, 0, y * 2 - sq/2 - 1), Quaternion.identity);

                }
                
            }

        }

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
