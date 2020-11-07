using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class JoinLobbyMenu : MonoBehaviour
{

    [SerializeField] private NetworkManagerChess networkManager = null;
    [SerializeField] private GameObject joinableFriendsDisplay = null;
    [SerializeField] private GameObject landingPagePanel = null;
    private SteamLobby steamLobby;

    [SerializeField] private GameObject friendButtonPrefab = null;

    private List<Button> friendButtons = null;

    private void OnEnable()
    {
        steamLobby = networkManager.GetComponent<SteamLobby>();

        loadFriendButtons();

        SteamLobby.OnFailedConnect += HandelClientDisconnected;
        NetworkManagerChess.onClienConnected += HandelClientConnected;
        NetworkManagerChess.onClientDisconnected += HandelClientDisconnected;
    }

    public void OnDisable()
    {
        SteamLobby.OnFailedConnect -= HandelClientDisconnected;
        NetworkManagerChess.onClienConnected -= HandelClientConnected;
        NetworkManagerChess.onClientDisconnected -= HandelClientDisconnected;
    }


    public void JoinLobby(int i)
    {
        CSteamID lobbyID = steamLobby.getLobbyId(steamLobby.joinableFriends[i]);
        foreach (Button b in friendButtons)
        {
            b.interactable = false;
        }
        steamLobby.JoinTheLobby(lobbyID);
    }

    public void loadFriendButtons()
    {
        if(friendButtons == null)
            friendButtons = new List<Button>();
        foreach (Button b in friendButtons)
        {
            Destroy(b.gameObject);
        }

        friendButtons.Clear();

        RectTransform listPanel = joinableFriendsDisplay.GetComponent<RectTransform>();
        float buttonHeight = friendButtonPrefab.GetComponent<RectTransform>().sizeDelta.y;
        float offset = 60;

        List<string> names = steamLobby.getJoinableFriendNames();
        float listHeight = (buttonHeight - offset) + names.Count * (2 * buttonHeight - offset);
        listPanel.sizeDelta = new Vector2(550f, listHeight);
        listPanel.position = new Vector3(0f, GetComponentInParent<RectTransform>().sizeDelta.y / 2 - listHeight / 2, 0f);
        int n = 0;
        foreach (string friend in names)
        {
            GameObject button = Instantiate(friendButtonPrefab, joinableFriendsDisplay.transform);
            button.GetComponent<RectTransform>().localPosition = new Vector3(0f, listHeight / 2 - (offset + (n) * (2 * buttonHeight - offset)), 0f);
            button.GetComponentInChildren<Text>().text = friend;
            int t = n;
            button.GetComponent<Button>().onClick.AddListener(delegate { JoinLobby(t); });
            friendButtons.Add(button.GetComponent<Button>());
            n++;
        }
    }

    public void HandelClientConnected()
    {
        foreach (Button b in friendButtons)
        {
            Destroy(b);
        }
        friendButtons.Clear();

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }
    public void HandelClientDisconnected()
    {
        Debug.Log("Yuyp");
        foreach (Button b in friendButtons)
        {
            b.interactable = true;
        }
    }


}
