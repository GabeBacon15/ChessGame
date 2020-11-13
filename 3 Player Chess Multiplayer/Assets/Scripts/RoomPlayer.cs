using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class RoomPlayer : NetworkBehaviour
{

    [SerializeField] private GameObject myUI = null;
    [SerializeField] private Text[] playerNameTexts = new Text[3];
    [SerializeField] private Image[] playerPics = new Image[3];
    [SerializeField] private GameObject readyUp = null, start = null;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Waiting...";
    public SyncListInt imageArray = new SyncListInt();
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    public bool hasName = false, hasImage = false, isAdded = false;
    public int imageSize = 16384;

    private NetworkManagerChess manager;
    private NetworkManagerChess Manager
    {
        get
        {
            if (manager != null) { return manager; }
            return manager = NetworkManager.singleton as NetworkManagerChess;
        }
    }

    public bool isLeader = false;

    public override void OnStartAuthority()
    {
        Debug.Log("WJat: " + Manager.RoomPlayers.Count);
        int[] tempAvatarArray = Manager.GetComponent<SteamLobby>().getMyAvatarArray();
        CmdSetAvatarArray(tempAvatarArray);
        CmdSetDisplayName(MainMenu.DisplayName);
        if (isLeader)
        {
            isReady = true;
            start.gameObject.SetActive(true);
            start.GetComponent<Button>().interactable = false;//HEEEEEERRRRREEEE
        }
        else
        {
            readyUp.gameObject.SetActive(true);
            isReady = false;
        }
        myUI.SetActive(true);
    }

    private void Awake()
    {
        Debug.Log("Hey");
        imageArray.Callback += HandleImageArrayUpdate;
    }

    public void HandleImageArrayUpdate(SyncListInt.Operation op, int index, int oldItem, int newItem)
    {
        switch (op)
        {
            case SyncListInt.Operation.OP_ADD:
                if (!isAdded)
                {
                    if (!hasImage && index == imageSize-1)
                    {
                        Debug.Log("Image Recieved");
                        hasImage = true;
                        if (hasName)
                        {
                            OnClientStart();
                        }
                    }
                }
                else
                {
                    UpdateDisplay();
                }
                break;
            case SyncListInt.Operation.OP_CLEAR:
                Debug.Log("CLEAR");
                hasImage = false;
                break;
            case SyncListInt.Operation.OP_INSERT:
                Debug.Log("INSERT AT " + index + " | " + newItem);
                break;
            case SyncListInt.Operation.OP_REMOVEAT:
                Debug.Log("REMOVED AT " + index + " | " + oldItem);
                break;
            case SyncListInt.Operation.OP_SET:
                Debug.Log("SET AT " + index + " | " + oldItem + " : " + newItem);
                break;
        }
    }
    public void HandleReadyStatusChanged(bool oldValue, bool newValue)
    {
        Debug.Log("Received Ready");
        if (isAdded)
        {
            UpdateDisplay();
        }
    }
    public void HandleDisplayNameChanged(string oldValue, string newValue)
    {
        Debug.Log("Received Name");
        if (!isAdded)
        {
            hasName = true;
            if (hasImage)
            {
                OnClientStart();
            }
        }
        else
        {
            UpdateDisplay();
        }
    }

    public void OnClientStart()
    {
        Debug.Log("CLIENT START THING");
        Debug.Log("Image count: " + imageArray.Count);
        Debug.Log("Room players count 1: " + Manager.RoomPlayers.Count);
        Manager.RoomPlayers.Add(this);
        isAdded = true;
        Debug.Log("Room players count 2: " + Manager.RoomPlayers.Count);
        UpdateDisplay();
    }

    public override void OnStartClient()
    {
        bool skip = false;
        foreach(RoomPlayer rp in Manager.RoomPlayers)
        {
            if(this.Equals(rp))
            {
                skip = true;
                break;
            }
        }
        if(!skip && imageArray.Count == imageSize)
        {
            Manager.RoomPlayers.Add(this);
        }
    }

    public override void OnStopClient()
    {
        Debug.Log("Bye");
        Manager.RoomPlayers.Remove(this);

        UpdateDisplay();

        base.OnStopClient();
    }

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Manager.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
        }

        Debug.Log("Count " + Manager.RoomPlayers.Count);
        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting...";
            playerPics[i].sprite = null;
        }
        for (int i = 0; i < Manager.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Manager.RoomPlayers[i].isReady ?
                "<color=green>" + Manager.RoomPlayers[i].DisplayName + "</color>" :
                "<color=red>" + Manager.RoomPlayers[i].DisplayName + "</color>";
            playerPics[i].sprite = applyTexture(Manager.RoomPlayers[i].imageArray);
        }
    }

    public void HandelReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        start.GetComponent<Button>().interactable = readyToStart;
    }

    [Command]
    public void CmdSetDisplayName(string name)
    {
        DisplayName = name;
    }

    [Command]
    public void CmdFlipReady()
    {
        isReady = !isReady;

        Manager.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Manager.RoomPlayers[0].connectionToClient != connectionToClient) { return; }//HEEEEEERRRRREEEE

        Manager.StartGame();
    }
    [Command]
    public void CmdSetAvatarArray(int[] image)
    {
        Debug.Log("Setting Array");
        imageArray.Clear();
        foreach(int i in image)
        {
            imageArray.Add(i);
        }
    }

    private Sprite applyTexture(SyncListInt image)
    {
        byte[] Image = new byte[image.Count];
        for (int i = 0; i < Image.Length; i++)
        {
            Image[i] = Convert.ToByte(image[i]);
        }
        int side = (int)Mathf.Sqrt(Image.Length/4);
        Debug.Log("Side: " + side);
        Texture2D returnTexture = new Texture2D(side, side, TextureFormat.RGBA32, false, true);
        returnTexture.LoadRawTextureData(Image);
        returnTexture.Apply();
        return Sprite.Create(returnTexture, new Rect(0.0f, 0.0f, returnTexture.width, returnTexture.height), new Vector2(0.5f, 0.5f));
    }

    public void Disconnect()
    {
        //GameObject.FindWithTag("Main Menu").GetComponent<MainMenu>().disconnect();
        Manager.RoomPlayers.Clear();
        if (isLeader)
        {
            Manager.StopServer();
            Manager.StopHost();
        }
        Manager.StopClient();
    }

}
