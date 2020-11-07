using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class NetworkManagerChessOld : NetworkManager
{
    [SerializeField] Transform[] spawns = null;
    [SerializeField] public PieceDictionary[] pieces = null;
    public BoardManager boardManager = null;
    public string nickname = "Player-";
    public string steamName;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();
        Transform start = spawns[numPlayers];
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        player.GetComponent<Player>().playerNum = numPlayers+1;

        GameObject[] myPieces = new GameObject[16];

        myPieces[0] = Instantiate(pieces[numPlayers].getPrefab("rook"), Piece.getBoard2World(new Vector3(0, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        myPieces[1] = Instantiate(pieces[numPlayers].getPrefab("knight"), Piece.getBoard2World(new Vector3(1, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        myPieces[2] = Instantiate(pieces[numPlayers].getPrefab("bishop"), Piece.getBoard2World(new Vector3(2, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        if (numPlayers == 2)
        {
            myPieces[3] = Instantiate(pieces[numPlayers].getPrefab("king"), Piece.getBoard2World(new Vector3(3, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
            myPieces[4] = Instantiate(pieces[numPlayers].getPrefab("queen"), Piece.getBoard2World(new Vector3(4, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        }
        else
        {
            myPieces[4] = Instantiate(pieces[numPlayers].getPrefab("king"), Piece.getBoard2World(new Vector3(4, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
            myPieces[3] = Instantiate(pieces[numPlayers].getPrefab("queen"), Piece.getBoard2World(new Vector3(3, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        }
        myPieces[5] = Instantiate(pieces[numPlayers].getPrefab("bishop"), Piece.getBoard2World(new Vector3(5, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        myPieces[6] = Instantiate(pieces[numPlayers].getPrefab("knight"), Piece.getBoard2World(new Vector3(6, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        myPieces[7] = Instantiate(pieces[numPlayers].getPrefab("rook"), Piece.getBoard2World(new Vector3(7, 0, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        for(int i = 0; i < 8; i++)
        {
            myPieces[8+i] = Instantiate(pieces[numPlayers].getPrefab("pawn"), Piece.getBoard2World(new Vector3(i, 1, numPlayers)), Quaternion.Euler(0, 120 * numPlayers, 0));
        }
        for (int i = 0; i < myPieces.Length; i++)
        {
            NetworkServer.Spawn(myPieces[i], conn);
        }
        //player.GetComponent<Player>().nickname = nickname;
        NetworkServer.AddPlayerForConnection(conn, player);
        if (numPlayers == 3)
        {
            boardManager.isFull = true;
        }
    }
}