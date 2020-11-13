using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
public class Player : NetworkBehaviour
{
    [SyncVar]
    public int playerNum = 0;

    [SyncVar]
    private string displayName = "Ummm";

    [SerializeField] int turn;
    int pawnPromotionId;
    Vector3 pawnPromotionPos;
    public int myTurn;
    public Dictionary<int, GameObject> pieces;
    public PieceDictionary[] piecesList = null;
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
    private NetworkManagerChess manager;
    private NetworkManagerChess Manager
    {
        get
        {
            if (manager != null) { return manager; }
            return manager = NetworkManager.singleton as NetworkManagerChess;
        }
    }

    [Server]
    public void setDisplayName(string nameToBeSet)
    {
        displayName = nameToBeSet;
    }

    public void setupCam()
    {
        camObj = GameObject.Find("Camera");
        cam = camObj.GetComponent<Camera>();
        camObj.transform.position = this.transform.position;
        camObj.transform.rotation = this.transform.rotation;
        boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();

    }

    public override void OnStartAuthority()
    {

        waitingForInput = false;
        newPieceB = false;
        chatHistory = string.Empty;
        isChat = false;
        hasSetPieces = false;
        pieces = null;
        this.gameObject.name = "Player-" + playerNum;
        currentPiece = null;

        OnTurnUpdate += HandelTurnUpdate;
        OnDestroyPiece += HandelDestroyPiece;
        OnChatMessage += HandelChatMessage;

        turn = 0;
        chatText.text = displayName;
        point = new Vector3();

        mouseCoord = new Vector3();
        possibleMesh = new List<GameObject>();
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(this);
        base.OnStartClient();
    }

    private void Update()
    {
        if (!hasAuthority || waitingForInput || boardManager == null) { return; }
        if (newPieceB)
        {
            newPiece = GameObject.Find(lookingForPromotionID);
            if (newPiece != null)
            {
                pieces.Add(newPiece.GetComponent<Piece>().pieceID, newPiece);
                newPieceB = false;
                hoverMesh.SetActive(true);
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
        //if (boardManager.piecesReady < 48) { return; }
        if(playerNum == 0) { return; }
        if(boardManager.spaces.Count < 96) { return; }
        if((GameObject.Find((playerNum * 100 + 7) + "") == null && !hasSetPieces) || (boardManager.spaces[7 + 8 * 1 + 32 * (playerNum-1)] == 0) && !hasSetPieces) { Debug.Log("Not Found: " + (playerNum * 100 + 7)); return; }
        if (!hasSetPieces)
        {
            Debug.Log(boardManager.spaces.Count + " |Yeet| " + boardManager.spaces[7 + 8 * 1 + 32 * (playerNum - 1)] + ", " + (playerNum-1));
            pieces = new Dictionary<int, GameObject>();
            foreach (int i in boardManager.spaces)
            {
                Debug.Log(i);
                if (i / 100 == playerNum)
                {
                    Debug.Log("Here Yeet: " + i);
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
                if (isValidMove(mouseCoord) && (turn == playerNum - 1 || false))//cheat: false = turns, true = freeforall
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
                        waitingForInput = true;
                        hoverMesh.SetActive(false);
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
        if(hoverMesh == null)
        {
            hoverMesh = new GameObject("blank");
            hoverMesh.AddComponent<MeshRenderer>().material = hoverMat;
            hoverMesh.AddComponent<MeshFilter>();
        }
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
            hoverMesh.GetComponent<MeshFilter>().mesh = createMesh(mouseCoord, .2f, .2f);
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
                    temp.AddComponent<MeshFilter>().mesh = createMesh(pm, .2f, .2f);
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

    private Mesh createMesh(Vector3 coord, float height, float peak)
    {
        float triHeight = 0.125f;
        Vector3[] vertecies = new Vector3[14];
        Vector2[] uv;
        int[] triangles;

        Vector3 heightV = new Vector3(0, height, 0);
        Vector3 peakV = new Vector3(0, peak, 0);

        vertecies[0] = Piece.getCoordInWorldSpace(new Vector3(coord.x - .5f, coord.y - .5f, coord.z)) + heightV;
        vertecies[1] = Piece.getCoordInWorldSpace(new Vector3(coord.x + .5f, coord.y - .5f, coord.z)) + heightV;
        vertecies[2] = Piece.getCoordInWorldSpace(new Vector3(coord.x + .5f, coord.y + .5f, coord.z)) + heightV;
        vertecies[3] = Piece.getCoordInWorldSpace(new Vector3(coord.x - .5f, coord.y + .5f, coord.z)) + heightV;
        vertecies[4] = vertecies[0];
        for (int i = 0; i < 5; i++)
            vertecies[i + 5] = vertecies[i] - heightV;
        for (int i = 10; i < 14; i++)
            vertecies[i] = Piece.getCoordInWorldSpace(new Vector3(coord.x, coord.y, coord.z)) + heightV + peakV;

        triangles = new int[]
        {
            //Front
            0, 1, 5,
            1, 6, 5,
            //Right
            1, 2, 6,
            2, 7, 6,
            //Back
            2, 3, 7,
            3, 8, 7,
            //Left
            3, 4, 8,
            4, 9, 8,
            //Top
            10, 1, 0,
            11, 2, 1,
            12, 3, 2,
            13, 4, 3
        };

        uv = new Vector2[]
        {
            new Vector2(0.00f, 1 - triHeight),
            new Vector2(0.25f, 1 - triHeight),
            new Vector2(0.50f, 1 - triHeight),
            new Vector2(0.75f, 1 - triHeight),
            new Vector2(1.00f, 1 - triHeight),

            new Vector2(0.00f, 1f - (height/3) - triHeight),
            new Vector2(0.25f, 1f - (height/3) - triHeight),
            new Vector2(0.50f, 1f - (height/3) - triHeight),
            new Vector2(0.75f, 1f - (height/3) - triHeight),
            new Vector2(1.00f, 1f - (height/3) - triHeight),

            new Vector2(0.125f, 1f),
            new Vector2(0.375f, 1f),
            new Vector2(0.625f, 1f),
            new Vector2(0.875f, 1f)
        };

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertecies;
        mesh.triangles = triangles;
        mesh.uv = uv;
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
        GameObject piece = Instantiate(promotion[color-1].getPrefab(name), Piece.getBoard2World(pos), Quaternion.Euler(0, 120 * (color-1), 0));
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
        CmdSendChatMessage(displayName + ": " + message);
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

    [Command(ignoreAuthority = true)]
    public void CmdSpawnPieces(NetworkConnectionToClient conn = null)
    {
        Pawn.numpw = 0;
        Rook.numr = 0;
        Knight.numkn = 0;
        Bishop.numb = 0;
        King.numki = 0;
        Queen.numq = 0;

        Debug.Log(displayName + ": " + playerNum);
        GameObject[] myPieces = new GameObject[16];

        int index = playerNum - 1;
        Debug.Log(index + ": " + piecesList.Length);

        myPieces[0] = Instantiate(piecesList[index].getPrefab("rook"), Piece.getBoard2World(new Vector3(0, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        myPieces[1] = Instantiate(piecesList[index].getPrefab("knight"), Piece.getBoard2World(new Vector3(1, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        myPieces[2] = Instantiate(piecesList[index].getPrefab("bishop"), Piece.getBoard2World(new Vector3(2, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        if (index == 2)
        {
            myPieces[3] = Instantiate(piecesList[index].getPrefab("king"), Piece.getBoard2World(new Vector3(3, 0, index)), Quaternion.Euler(0, 120 * index, 0));
            myPieces[4] = Instantiate(piecesList[index].getPrefab("queen"), Piece.getBoard2World(new Vector3(4, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        }
        else
        {
            myPieces[4] = Instantiate(piecesList[index].getPrefab("king"), Piece.getBoard2World(new Vector3(4, 0, index)), Quaternion.Euler(0, 120 * index, 0));
            myPieces[3] = Instantiate(piecesList[index].getPrefab("queen"), Piece.getBoard2World(new Vector3(3, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        }
        myPieces[5] = Instantiate(piecesList[index].getPrefab("bishop"), Piece.getBoard2World(new Vector3(5, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        myPieces[6] = Instantiate(piecesList[index].getPrefab("knight"), Piece.getBoard2World(new Vector3(6, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        myPieces[7] = Instantiate(piecesList[index].getPrefab("rook"), Piece.getBoard2World(new Vector3(7, 0, index)), Quaternion.Euler(0, 120 * index, 0));
        for (int j = 0; j < 8; j++)
        {
            myPieces[8 + j] = Instantiate(piecesList[index].getPrefab("pawn"), Piece.getBoard2World(new Vector3(j, 1, index)), Quaternion.Euler(0, 120 * index, 0));
        }
        for (int j = 0; j < myPieces.Length; j++)
        {
            NetworkServer.Spawn(myPieces[j], conn);
        }
    }
}