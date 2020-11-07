using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using System.Text;
using System;

public class SteamLobby : MonoBehaviour
{

    //[SerializeField] GameObject buttons = null;

    private NetworkManagerChess networkManager = null;

    public static event Action OnFailedConnect;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    public List<CSteamID> joinableFriends;

    private const string hostAddressKey = "HostAddress";

    private void Start()
    {
        //buttons.SetActive(true);
        //buttons.GetComponentInChildren<InputField>().text = SteamFriends.GetPersonaName();
        networkManager = GetComponent<NetworkManagerChess>();

        getJoinableFriendList();

        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(onLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(onGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(onLobbyEntered);
    }


    public string getSteamName()
    {
        return SteamFriends.GetPersonaName();
    }

    public void JoinTheLobby(CSteamID lobbyId)
    {
        try
        {
            SteamMatchmaking.JoinLobby(lobbyId);
        }
        catch
        {
            Debug.Log("BRRUUHHHUH");
        }
    }

    public int[] getMyAvatarArray()
    {
        int FriendAvatar = SteamFriends.GetMediumFriendAvatar(SteamUser.GetSteamID());
        Debug.Log("MY Name: " + getSteamName());
        uint ImageWidth;
        uint ImageHeight;
        bool success = SteamUtils.GetImageSize(FriendAvatar, out ImageWidth, out ImageHeight);

        if (success && ImageWidth > 0 && ImageHeight > 0)
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];
            success = SteamUtils.GetImageRGBA(FriendAvatar, Image, (int)(ImageWidth * ImageHeight * 4));
            if (success)
            {
                int[] intArray = new int[Image.Length];
                for (int i = 0; i < Image.Length; i++)
                {
                    intArray[i] = Convert.ToInt32(Image[i]);
                }
                return intArray;
            }
            return new int[0];
        }
        else
        {
            Debug.LogError("Couldn't get avatar.");
            return new int[0];
        }
    }

    public CSteamID getLobbyId(CSteamID friendId)
    {
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll);
        for (int i = 0; i < friendCount; i++)
        {
            if(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll).Equals(friendId))
            {
                FriendGameInfo_t friendGameInfo = new FriendGameInfo_t();
                if (SteamFriends.GetFriendGamePlayed(friendId, out friendGameInfo))
                {
                    return friendGameInfo.m_steamIDLobby;
                }
            }
        }
        return CSteamID.Nil;
    }

    public List<string> getJoinableFriendNames()
    {
        List<string> names = new List<string>();
        getJoinableFriendList();
        foreach(CSteamID id in joinableFriends)
        {
            names.Add(SteamFriends.GetFriendPersonaName(id));
        }
        return names;
    }
    private void getJoinableFriendList()
    {
        joinableFriends = new List<CSteamID>();
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll);
        for(int i = 0; i < friendCount; i++)
        {
            FriendGameInfo_t friendGameInfo = new FriendGameInfo_t();
            CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll);
            
            if (SteamFriends.GetFriendGamePlayed(friendId, out friendGameInfo))
            {
                if (friendGameInfo.m_gameID.ToString().Equals("480"))
                {
                    Debug.Log(SteamFriends.GetFriendPersonaName(friendId));
                    joinableFriends.Add(friendId);
                }
            }
        }
    }


    public void HostLobby()
    {
        Debug.Log("HOST LOBBY");
        //buttons.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void onLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("LOBBY CREATED");
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            //buttons.SetActive(true);
            return;
        }

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey, SteamUser.GetSteamID().ToString());


    }

    private void onGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("JOIN REQUEST");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void onLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("LOBBY ENTERED");
        if(NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            hostAddressKey);

        if (hostAddress.Length <= 0) { OnFailedConnect.Invoke(); return; }

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
        //buttons.SetActive(false);
    }
}
