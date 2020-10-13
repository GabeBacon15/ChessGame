using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using UnityEngine.UI;

public class SteamLobby : MonoBehaviour
{

    [SerializeField] GameObject buttons = null;

    private NetworkManagerChess networkManager = null;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string hostAddressKey = "HostAddress";

    private void Start()
    {
        buttons.SetActive(true);
        buttons.GetComponentInChildren<InputField>().text = SteamFriends.GetPersonaName();
        networkManager = GetComponent<NetworkManagerChess>();
        networkManager.steamName = SteamFriends.GetPersonaName();

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(onLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(onGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(onLobbyEntered);
    }

    public void HostLobby()
    {
        Debug.Log("HOST LOBBY");
        buttons.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void onLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("LOBBY CREATED");
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            return;
        }

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey, SteamUser.GetSteamID().ToString());


    }

    private void onGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("JOIN REQUEST");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void onLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("LOBBY ENTERED");
        if(NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            hostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
        buttons.SetActive(false);
    }
}
