using System.Collections;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Steam;
using Steamworks;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    public SteamTransport _steamTransport; // WE HAVE TO SET steamTransport.address to the HOST PLAYERS ADDRESS
    public NetworkManager _networkManager; // START HOST AND CLIENT FROM HERE LIKE NORMAL
    public TextMeshProUGUI logText;
    private CSteamID lobbyID;
    private CSteamID ownerID;
    public GameObject uiButton;
    public GameObject uiLeaveButton;
    public GameObject uiJoinButton;
    public GameManager gameManager;

    // WHAT WE NEED:
    // - HOST PRESS OPEN BUTTON 
    //   -> CREATE LOBBY
    //   -> set steamTransport.address to (CSteamID) SteamMatchmaking.GetLobbyOwner(id) .ToString()
    //      - this is the owner id, should only be used on the purrnet steam transport thing.
    // - CLIENT PRESS JOIN BUTTON WITH ID INPUT TEXT
    //   -> set steamTransport.address to (CSteamID) SteamMatchmaking.GetLobbyOwner(id) .ToString()

    protected Callback<LobbyEnter_t> m_LobbyEntered;
    protected Callback<LobbyCreated_t> m_LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> m_JoinRequested;
    protected Callback<LobbyChatUpdate_t> m_ChatUpdated;

    public void StartHost()
    {
        
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);

    }

    // THIS FUNCTION IS FOR LOCAL TESTING ONLY!
    public void JoinHost()
    {
        
        _networkManager.StartClient();

        uiButton.SetActive(false);
        uiJoinButton.SetActive(false);
        uiLeaveButton.SetActive(true);

    }

    void OnEnable()
    {

        if (SteamManager.Initialized)
        {
            
            m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            m_JoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            m_ChatUpdated = Callback<LobbyChatUpdate_t>.Create(OnChatUpdated);
            logText.text = "Initialized";

        }

        // hides the join local button if we are on steam
        if(_networkManager.transport != _steamTransport) uiJoinButton.SetActive(true);

    }

    private void OnLobbyEntered(LobbyEnter_t pCallback)
    {
        
        logText.text = "Lobby Entered, LobbyID: " + pCallback.m_ulSteamIDLobby;

        Debug.Log("OwnerID: " + ownerID);
        Debug.Log("LobbyID: " + lobbyID);

        uiButton.SetActive(false);
        uiJoinButton.SetActive(false);
        uiLeaveButton.SetActive(true);

    }

    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        
        logText.text = "Lobby Created, LobbyID: " + pCallback.m_ulSteamIDLobby;

        ownerID = SteamMatchmaking.GetLobbyOwner(new CSteamID(pCallback.m_ulSteamIDLobby));

        _steamTransport.address = ownerID.ToString();
        
        if(_networkManager.transport) _networkManager.StartHost();

        lobbyID = new CSteamID(pCallback.m_ulSteamIDLobby);

    }

    private void OnJoinRequested(GameLobbyJoinRequested_t pCallback)
    {
        
        SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);

        logText.text = "Joining Friend: " + pCallback.m_steamIDFriend;

        _steamTransport.address = pCallback.m_steamIDFriend.ToString();
        _networkManager.StartClient();
        
        lobbyID = pCallback.m_steamIDLobby;
        ownerID = pCallback.m_steamIDFriend;

    }

    public int GetPlayerCount()
    {
        
        return SteamMatchmaking.GetNumLobbyMembers(lobbyID);

    }

    private void OnChatUpdated(LobbyChatUpdate_t pCallback)
    {
        
        Debug.Log("Lobby Updated: " + pCallback.m_rgfChatMemberStateChange);

        if(new CSteamID(pCallback.m_ulSteamIDUserChanged) == ownerID) LeaveLobby(); // this doesnt work
        else gameManager.UpdatePlayerCount();

    }

    void OnApplicationQuit()
    {
        
        LeaveLobby();

    }

    public void LeaveLobby()
    {

        gameManager.SetPlayerText("0");

        SteamMatchmaking.LeaveLobby(lobbyID);
        _networkManager.StopClient();
        if(_networkManager.isServer) _networkManager.StopServer();

        uiLeaveButton.SetActive(false);
        if(_networkManager.transport != _steamTransport) uiJoinButton.SetActive(true);
        uiButton.SetActive(true); // this should be a function

    }

}
