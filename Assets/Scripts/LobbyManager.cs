using System.Collections;
using PurrNet;
using PurrNet.Steam;
using Steamworks;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    public SteamTransport _steamTransport; // WE HAVE TO SET steamTransport.address to the HOST PLAYERS ADDRESS
    public NetworkManager _networkManager; // START HOST AND CLIENT FROM HERE LIKE NORMAL

    // WHAT WE NEED:
    // - HOST PRESS OPEN BUTTON 
    //   -> CREATE LOBBY
    //   -> set steamTransport.address to (CSteamID) SteamMatchmaking.GetLobbyOwner(id) .ToString()
    //      - this is the lobby code, should be input by join clients.
    // - CLIENT PRESS JOIN BUTTON WITH ID INPUT TEXT
    //   -> set steamTransport.address to (CSteamID) SteamMatchmaking.GetLobbyOwner(id) .ToString()

    protected Callback<LobbyEnter_t> m_LobbyEntered;
    protected Callback<LobbyCreated_t> m_LobbyCreated;
    protected Callback<GameRichPresenceJoinRequested_t> m_JoinRequested;

    public void StartHost()
    {
        
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);

    }

    public void JoinHost(CSteamID lobbyID)
    {
        
        SteamMatchmaking.JoinLobby(lobbyID);

    }

    void OnEnable()
    {

        if (SteamManager.Initialized)
        {
            
            m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            m_JoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnJoinRequested);

        }

    }

    private void OnLobbyEntered(LobbyEnter_t pCallback)
    {
        
        Debug.Log("Lobby Entered, LobbyID: " + pCallback.m_ulSteamIDLobby);

        _steamTransport.address = SteamMatchmaking.GetLobbyOwner(new CSteamID(pCallback.m_ulSteamIDLobby)).ToString();

    }

    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        
        Debug.Log("Lobby Created, LobbyID: " + pCallback.m_ulSteamIDLobby);

        _networkManager.StartHost();

    }

    private void OnJoinRequested(GameRichPresenceJoinRequested_t pCallback)
    {
        
        SteamMatchmaking.JoinLobby(pCallback.m_steamIDFriend);
        Debug.Log("Joining Friend: " + pCallback.m_steamIDFriend);

    }

}
