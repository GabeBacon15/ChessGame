using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Mirror;
using System.Linq;

public class NetworkManagerChess : NetworkManager
{
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [Scene] [SerializeField] private string gameScene = string.Empty;

    [SerializeField] Transform[] spawns = null;
    [SerializeField] Vector3[] spawnPos = null;
    [SerializeField] Quaternion[] spawnRot = null;
    [SerializeField] public PieceDictionary[] pieces = null;

    [SerializeField] private int minPlayers = 3;

    [SerializeField] private RoomPlayer roomPlayerPrefab = null;
    [SerializeField] private Player gamePlayerPrefab = null;
    [SerializeField] private GameObject boardManagerPref = null;

    [SerializeField] private GameObject mainMenu;

    public static event Action onClienConnected;
    public static event Action onClientDisconnected;

    public int myNum;

    public List<RoomPlayer> RoomPlayers { get; } = new List<RoomPlayer>();
    public List<GameObject> GamePlayers { get; } = new List<GameObject>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        spawnPos = new Vector3[3];
        spawnRot = new Quaternion[3];
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
        for(int i = 0; i < spawns.Length; i++)
        {
            spawnPos[i] = spawns[i].position;
            spawnRot[i] = spawns[i].rotation;
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        if (onClienConnected != null)
            onClienConnected.Invoke();
    }
    public override void OnStopClient()
    {
        if (onClientDisconnected != null)
        {
            onClientDisconnected.Invoke();
        }
        base.OnStopClient();

    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        if(numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if(conn.identity != null)
        {
            var player = conn.identity.GetComponent<RoomPlayer>();
            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();

        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach(var player in RoomPlayers)
        {
            player.HandelReadyToStart(isReadyToStart());
        }
    }

    public bool isReadyToStart()
    {
        if(numPlayers < minPlayers) { return false; }

        foreach(var player in RoomPlayers)
        {
            if(!player.isReady) { return false; }
        }

        return true;
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if(SceneManager.GetActiveScene().path == menuScene)
        {
            RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
            if(RoomPlayers.Count == 0)
                roomPlayerInstance.isLeader = true;
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public void StartGame()
    {
        Debug.Log(gameScene);
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            //if(!isReadyToStart()) { return; }//HEEEEEERRRRREEEE

            ServerChangeScene("GameScene");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        if(SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith("GameScene"))
        {
            GameObject boardManagerObj = Instantiate(boardManagerPref);
            BoardManager boardManager = boardManagerObj.GetComponent<BoardManager>();
            boardManagerObj.name = "BoardManager";
            DontDestroyOnLoad(boardManagerObj);
            NetworkServer.Spawn(boardManagerObj);

            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                //int index = Mathf.Abs(i - 2);
                //Transform start = spawns[index];
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab, spawnPos[i], spawnRot[i]);
                gameplayerInstance.setDisplayName(RoomPlayers[i].DisplayName);
                gameplayerInstance.playerNum = i + 1;
                //DontDestroyOnLoad(gameplayerInstance);
                //NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
                boardManager.CmdAddPlayerReady();
            }
            //boardManager.isFull = true;
        }
        base.ServerChangeScene(newSceneName);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        conn.identity.GetComponent<Player>().piecesList = pieces;
        conn.identity.GetComponent<Player>().setupCam();
        conn.identity.GetComponent<Player>().CmdSpawnPieces(conn.identity.connectionToClient);
       

        //RoomPlayer me = conn.identity.GetComponent<RoomPlayer>();
        //int index1 = 0;
        //Debug.Log("YEESTUYHG: " + me.DisplayName);
        //for (int i = 0; i < RoomPlayers.Count; i++)
        //{
        //    if(RoomPlayers[i].Equals(me))
        //    {
        //        index1 = i;
        //    }
        //}
        //if (me != null)
        //{
        //    var gameplayerInstance = Instantiate(gamePlayerPrefab, spawnPos[index1], spawnRot[index1]);
        //    gameplayerInstance.setDisplayName(me.DisplayName);
        //    DontDestroyOnLoad(gameplayerInstance);
        //    gameplayerInstance.playerNum = index1 + 1;
        //    gameplayerInstance.setupCam();


        //    var conny = me.connectionToClient;

        //    NetworkServer.ReplacePlayerForConnection(conny, gameplayerInstance.gameObject);

        //    GameObject[] myPieces = new GameObject[16];

        //    int index = conn.identity.GetComponent<Player>().playerNum - 1;

        //    myPieces[0] = Instantiate(pieces[index].getPrefab("rook"), Piece.getBoard2World(new Vector3(0, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    myPieces[1] = Instantiate(pieces[index].getPrefab("knight"), Piece.getBoard2World(new Vector3(1, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    myPieces[2] = Instantiate(pieces[index].getPrefab("bishop"), Piece.getBoard2World(new Vector3(2, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    if (index == 2)
        //    {
        //        myPieces[3] = Instantiate(pieces[index].getPrefab("king"), Piece.getBoard2World(new Vector3(3, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //        myPieces[4] = Instantiate(pieces[index].getPrefab("queen"), Piece.getBoard2World(new Vector3(4, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    }
        //    else
        //    {
        //        myPieces[4] = Instantiate(pieces[index].getPrefab("king"), Piece.getBoard2World(new Vector3(4, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //        myPieces[3] = Instantiate(pieces[index].getPrefab("queen"), Piece.getBoard2World(new Vector3(3, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    }
        //    myPieces[5] = Instantiate(pieces[index].getPrefab("bishop"), Piece.getBoard2World(new Vector3(5, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    myPieces[6] = Instantiate(pieces[index].getPrefab("knight"), Piece.getBoard2World(new Vector3(6, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    myPieces[7] = Instantiate(pieces[index].getPrefab("rook"), Piece.getBoard2World(new Vector3(7, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        //    for (int j = 0; j < 8; j++)
        //    {
        //        myPieces[8 + j] = Instantiate(pieces[index].getPrefab("pawn"), Piece.getBoard2World(new Vector3(j, 1, index)), Quaternion.Euler(0, 120 * index, 0));
        //    }
        //    for (int j = 0; j < myPieces.Length; j++)
        //    {
        //        DontDestroyOnLoad(myPieces[j]);
        //        NetworkServer.Spawn(myPieces[j], conny);
        //    }
        //    GameObject.Find("BoardManager").GetComponent<BoardManager>().CmdAddPlayerReady();
        //}
        //else
        //{
        //    Debug.Log("Could not find Room Player");
        //}
        base.OnClientSceneChanged(conn);
    }
}
