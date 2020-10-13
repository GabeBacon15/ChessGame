﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
public class Player : NetworkBehaviour
{
    [SyncVar]
    public int playerNum = 0;

    public string nickname;

    [SerializeField] int turn;
    int pawnPromotionId;
    Vector3 pawnPromotionPos;
    public int myTurn;
    public Dictionary<int, GameObject> pieces;
    public Camera cam;

    private bool hasSetPieces, isChat, newPieceB, waitingForInput;

    [SerializeField] GameObject displayUI = null, promotionUI = null;
    [SerializeField] Text chatText = null;
    [SerializeField] InputField chatInput = null;
    private string chatHistory;

    public Material hoverMat, killMat;
    public Material[] possibleMat;

    public LayerMask clickMask;
    public Vector3 point, mouseCoord, mouseClosest;
    GameObject hoverMesh;
    List<GameObject> possibleMesh;
    GameObject currentPiece;
    private BoardManager boardManager;

    public PieceDictionary[] promotion;

    GameObject newPiece;
    GameObject camObj;

    string lookingForPromotionID;

    private static event Action<int> OnTurnUpdate, OnDestroyPiece;
    private static event Action<string> OnChatMessage;

    public override void OnStartAuthority()
    {
        waitingForInput = false;
        newPieceB = false;
        chatHistory = string.Empty;
        isChat = false;
        hasSetPieces = false;
        boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();
        pieces = null;
        this.gameObject.name = "Player-" + playerNum;
        currentPiece = null;
        camObj = GameObject.Find("Camera");
        cam = camObj.GetComponent<Camera>();
        camObj.GetComponent<CameraMovment>().move = false;
        camObj.transform.position = this.transform.position;
        camObj.transform.rotation = this.transform.rotation;

        OnTurnUpdate += HandelTurnUpdate;
        OnDestroyPiece += HandelDestroyPiece;
        OnChatMessage += HandelChatMessage;

        turn = 0;
        chatText.text = nickname;
        point = new Vector3();

        mouseCoord = new Vector3();
        possibleMesh = new List<GameObject>();
        hoverMesh = new GameObject("blank");
        hoverMesh.AddComponent<MeshRenderer>().material = hoverMat;
        hoverMesh.AddComponent<MeshFilter>();
    }

    private void Update()
    {
        if (!hasAuthority || waitingForInput) { return; }
        if (newPieceB)
        {
            newPiece = GameObject.Find(lookingForPromotionID);
            if (newPiece != null)
            {
                pieces.Add(newPiece.GetComponent<Piece>().pieceID, newPiece);
                newPieceB = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Quote))
        {
            if (!isChat)
            {
                displayUI.SetActive(true);
                chatText.text = chatHistory;
                isChat = true;
            }
            else
            {
                displayUI.SetActive(false);
                isChat = false;
            }
        }
        if (isChat)
        {
            return;
        }
        if (boardManager.piecesReady < 48) { return; }

        if (!hasSetPieces)
        {
            pieces = new Dictionary<int, GameObject>();
            foreach (int i in boardManager.spaces)
            {
                if (i / 100 == playerNum)
                {
                    pieces.Add(i, GameObject.Find(i + ""));
                }
            }
            hasSetPieces = true;
        }

        updatePlayerPoint();

        if (Input.GetMouseButtonDown(0))
        {
            if (currentPiece == null)
            {
                currentPiece = getPiece(mouseCoord);
            }
            else
            {
                if (isValidMove(mouseCoord) && (turn == playerNum - 1 || true))
                {
                    if (boardManager.getSpaces()[(int)mouseCoord.x, (int)mouseCoord.y, (int)mouseCoord.z] != 0)
                    {
                        int id = boardManager.getSpaces()[(int)mouseCoord.x, (int)mouseCoord.y, (int)mouseCoord.z];
                        DestroyPiece(id);
                        boardManager.CmdRemovePiece(id);
                    }
                    currentPiece.GetComponent<Piece>().move(mouseCoord);
                    if (currentPiece.GetComponent<Piece>().pieceName.Equals("pawn") && mouseCoord.y == 0)
                    {
                        pawnPromotionId =  currentPiece.GetComponent<Piece>().pieceID;
                        pawnPromotionPos = mouseCoord;
                        promotionUI.SetActive(true);
                    }
                    foreach (GameObject go in possibleMesh)
                    {
                        Destroy(go);
                    }
                    possibleMesh = new List<GameObject>();
                    currentPiece = null;
                    UpdateTurn();
                }
                else
                {
                    currentPiece = getPiece(mouseCoord);
                }
            }
        }
    }

    private void updatePlayerPoint()
    {
        Ray clickRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(clickRay, out hit, 100f, clickMask))
        {
            point = hit.point;
        }
        mouseClosest = Piece.getWorld2Board(getClosest(point));
        if (mouseCoord != mouseClosest)
        {
            mouseCoord = mouseClosest;
            hoverMesh.GetComponent<MeshFilter>().mesh = createMesh(mouseCoord, .1f);
            hoverMesh.name = "(" + mouseCoord.x + ", " + mouseCoord.y + ", " + mouseCoord.z + ")";
        }
    }
    private GameObject getPiece(Vector3 requested)
    {
        if (currentPiece != null && requested.Equals(currentPiece.GetComponent<Piece>().position))
            return currentPiece;
        foreach (KeyValuePair<int, GameObject> p in pieces)
        {
            if (p.Value.GetComponent<Piece>().position == requested)
            {
                p.Value.GetComponent<Piece>().getPossibleMoves();
                foreach (GameObject go in possibleMesh)
                {
                    Destroy(go);
                }
                possibleMesh = new List<GameObject>();
                foreach (Vector3 pm in p.Value.GetComponent<Piece>().possibleMoves)
                {
                    GameObject temp = new GameObject("Possible Move (" + pm.x + ", " + pm.y + ", " + pm.z + ")");
                    if (boardManager.getSpaces()[(int)pm.x, (int)pm.y, (int)pm.z] == 0)
                        temp.AddComponent<MeshRenderer>().material = possibleMat[((int)pm.x + (int)pm.y) % 2];
                    else
                        temp.AddComponent<MeshRenderer>().material = killMat;
                    temp.AddComponent<MeshFilter>().mesh = createMesh(pm, .1f);
                    possibleMesh.Add(temp);
                }
                return p.Value;
            }
        }
        foreach (GameObject go in possibleMesh)
        {
            Destroy(go);
        }
        possibleMesh = new List<GameObject>();
        return null;
    }
    private bool isValidMove(Vector3 requested)
    {
        foreach (Vector3 move in currentPiece.GetComponent<Piece>().possibleMoves)
        {
            if (move.Equals(requested))
            {
                return true;
            }
        }
        return false;
    }

    private Mesh createMesh(Vector3 coord, float height)
    {

        Vector3[] vertecies = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles;

        Vector3 heightV = new Vector3(0, height, 0);

        vertecies[0] = Piece.getCoordInWorldSpace(new Vector3(coord.x - .5f, coord.y + .5f, coord.z)) + heightV;
        vertecies[1] = Piece.getCoordInWorldSpace(new Vector3(coord.x + .5f, coord.y + .5f, coord.z)) + heightV;
        vertecies[2] = Piece.getCoordInWorldSpace(new Vector3(coord.x - .5f, coord.y - .5f, coord.z)) + heightV;
        vertecies[3] = Piece.getCoordInWorldSpace(new Vector3(coord.x + .5f, coord.y - .5f, coord.z)) + heightV;
        for (int i = 0; i < 4; i++)
            vertecies[i + 4] = vertecies[i] - heightV;

        triangles = new int[]
        {
            0, 1, 2,
            2, 1, 3,
            4, 0, 6,
            6, 0, 2,
            6, 2, 7,
            7, 2, 3,
            7, 3, 5,
            5, 3, 1,
            5, 1, 4,
            4, 1, 0,
            4, 6, 7,
            5, 4, 7

        };

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertecies;
        mesh.triangles = triangles;
        mesh.Optimize();
        mesh.RecalculateNormals();

        return mesh;

    }
    public Vector3 getClosest(Vector3 position)
    {
        Vector3 shortest = Piece.getBoard2World(new Vector3(0, 0, 0));
        float length = Mathf.Sqrt(Mathf.Pow(position.x - Piece.getBoard2World(new Vector3(0, 0, 0)).x, 2) + Mathf.Pow(position.z - Piece.getBoard2World(new Vector3(0, 0, 0)).z, 2));

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int n = 0; n < 8; n++)
                {
                    float current = Mathf.Sqrt(Mathf.Pow(position.x - Piece.getBoard2World(new Vector3(n, j, i)).x, 2) + Mathf.Pow(position.z - Piece.getBoard2World(new Vector3(n, j, i)).z, 2));
                    if (current < length)
                    {
                        shortest = Piece.getBoard2World(new Vector3(n, j, i));
                        length = current;
                    }
                }
            }
        }
        return shortest;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority) { return; }
        OnTurnUpdate -= HandelTurnUpdate;
        OnDestroyPiece -= HandelDestroyPiece;
        OnChatMessage -= HandelChatMessage;
    }

    private void HandelTurnUpdate(int newTurn)
    {
        turn = newTurn;
    }
    [Client]
    public void UpdateTurn()
    {
        turn++;
        if (turn > 2)
            turn = 0;
        CmdSendTurnUpdate(turn);
    }
    [Command]
    private void CmdSendTurnUpdate(int newTurn)
    {
        RpcHandelTurn(newTurn);
    }
    [ClientRpc]
    private void RpcHandelTurn(int newTurn)
    {
        OnTurnUpdate.Invoke(newTurn);
    }


    private void HandelDestroyPiece(int id)
    {
        if (hasAuthority)
        {
            Destroy(GameObject.Find(id + ""));
        }
        if (id / 100 == playerNum)
        {
            pieces.Remove(id);
            currentPiece = null;
        }
    }
    [Client]
    public void DestroyPiece(int id)
    {
        CmdSendDestroyPiece(id);
    }
    [Command]
    private void CmdSendDestroyPiece(int id)
    {
        RpcHandelDestroyPiece(id);
    }
    [ClientRpc]
    private void RpcHandelDestroyPiece(int id)
    {
        OnDestroyPiece.Invoke(id);
    }

    [Command]
    public void CmdAddPiece(string name, int color, Vector3 pos)
    {
        GameObject piece = Instantiate(promotion[color-1].getPrefab(name), Piece.getBoard2World(pos), Quaternion.Euler(0, 120 * color-1, 0));
        NetworkServer.Spawn(piece, this.gameObject);
    }


    private void HandelChatMessage(string message)
    {
        if(chatHistory.Length > 0)
        {
            chatHistory += "\n";
        }
        chatHistory += message;
        Debug.Log(chatHistory);
        if(isChat)
            chatText.text = chatHistory;
    }
    [Client]
    public void SendChatMessage(string message)
    {
        if (!Input.GetKeyDown(KeyCode.Return) || string.IsNullOrWhiteSpace(message)) { Debug.Log("BREAK");  return; }
        Debug.Log("IN" + message);
        CmdSendChatMessage(nickname + ": " + message);
        chatInput.text = string.Empty;
    }
    [Command]
    private void CmdSendChatMessage(string message)
    {
        RpcHandelChatMessage(message);
    }
    [ClientRpc]
    private void RpcHandelChatMessage(string message)
    {
        OnChatMessage.Invoke(message);
    }

    public void promoteToRook()
    {
        lookingForPromotionID = playerNum + "" + 1 + "" + Rook.numr;
        DestroyPiece(pawnPromotionId);
        CmdAddPiece("rook", playerNum, pawnPromotionPos);
        newPieceB = true;
        waitingForInput = false;
        promotionUI.SetActive(false);
    }
    public void promoteToKnight()
    {
        lookingForPromotionID = playerNum + "" + 2 + "" + Knight.numkn;
        DestroyPiece(pawnPromotionId);
        CmdAddPiece("knight", playerNum, pawnPromotionPos);
        newPieceB = true;
        waitingForInput = false;
        promotionUI.SetActive(false);
    }
    public void promoteToBishop()
    {
        lookingForPromotionID = playerNum + "" + 3 + "" + Bishop.numb;
        DestroyPiece(pawnPromotionId);
        CmdAddPiece("bishop", playerNum, pawnPromotionPos);
        newPieceB = true;
        waitingForInput = false;
        promotionUI.SetActive(false);
    }
    public void promoteToQueen()
    {
        lookingForPromotionID = playerNum + "" + 5 + "" + Queen.numq;
        DestroyPiece(pawnPromotionId);
        CmdAddPiece("queen", playerNum, pawnPromotionPos);
        newPieceB = true;
        waitingForInput = false;
        promotionUI.SetActive(false);
    }
    [Command]
    public void CmdDebug()
    {
        Debug.Log("HELLO");
    }
}