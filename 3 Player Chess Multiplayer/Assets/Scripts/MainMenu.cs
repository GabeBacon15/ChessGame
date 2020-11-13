using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenu : NetworkBehaviour
{

    [SerializeField] private InputField nameInputField = null;
    [SerializeField] private Button host = null, join = null;
    [SerializeField] private GameObject networkManagerObj = null;

    [SerializeField] private GameObject landingPagePanel = null;

    private SteamLobby steamLobby = null;
    private NetworkManagerChess networkManager = null;

    public static string DisplayName { get; private set; }

    public const string PlayerPrefsNameKey = "PlayerName";

    void Start()
    {
        landingPagePanel = GameObject.FindGameObjectWithTag("Main Menu");
        NetworkManagerChess.onClientDisconnected += disconnect;
        SetUpInputField();
    }

    private void OnDestroy()
    {
        NetworkManagerChess.onClientDisconnected -= disconnect;
        Debug.Log("hey");
    }

    public void SetUpInputField()
    {
        networkManager = networkManagerObj.GetComponent<NetworkManagerChess>();
        steamLobby = networkManagerObj.GetComponent<SteamLobby>();
        string defaultName = "";
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            defaultName = steamLobby.getSteamName();
        }
        else
        {
            defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        }

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void HostLobby()
    {
        SavePlayerName();
        steamLobby.HostLobby();
        landingPagePanel.SetActive(false);
    }

    public void SetPlayerName()
    {
        string name = nameInputField.text;
        host.interactable = !string.IsNullOrEmpty(name);
        join.interactable = !string.IsNullOrEmpty(name);
    }
    public void SetPlayerName(string name)
    {
        host.interactable = !string.IsNullOrEmpty(name);
        join.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;
        Debug.Log("In Main: " + DisplayName);

        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }
    public void disconnect()
    {
        Debug.Log("Disconnect!");
        landingPagePanel.SetActive(true);
    }
}
