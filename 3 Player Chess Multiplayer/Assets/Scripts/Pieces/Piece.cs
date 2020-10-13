using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class Piece : NetworkBehaviour
{
    /*
     * PIECE ID = ### (color, type, id)
     * 
     * color:
     *     1: white
     *     2: red
     *     3: black
     *     
     * type:
     *     0: pawn
     *     1: rook
     *     2: knight
     *     3: bishop
     *     4: king
     *     5: queen
     *     
     * id:
     *     0: The 1st
     *     1: The 2nd
     *     2: The 3nd
     *     ...
     *     7: The 8th
     *     (2-7 only for pawns)
     *     
     * Examples:
     *     Piece ID = 131: 2nd white bishop
     *     Piece ID = 350: 1st(only) black queen
     */
    public bool updatedNameOnFull;
    public Vector3 position;
    public List<Vector3> possibleMoves;
    public string pieceName = "";
    public string color;
    public int rot;
    public static Vector3[,,] board2World;
    public static Dictionary<Vector3, Vector3> world2Board;
    [SyncVar]
    public int pieceID;
    public BoardManager boardManager;

    public override void OnStartAuthority()
    {
        boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();
        position = getWorld2Board(transform.position);
        updatedNameOnFull = false;
    }

    private void Update()
    {
        if (!hasAuthority) { return; }
        if (!updatedNameOnFull && boardManager.isFull)
        {
            CmdSetNameObj(pieceID + "");
            boardManager.CmdPiecesReadyPlusPlus();
            updatedNameOnFull = true;
        }
    }


    public virtual void move(Vector3 position)
    {
        boardManager.CmdAddToSpaces((int)this.position.x, (int)this.position.y, (int)this.position.z, 0);
        this.position = position;
        boardManager.CmdAddToSpaces((int)position.x, (int)position.y, (int)position.z, pieceID);
        transform.position = getBoard2World(position);
    }

    public virtual void getPossibleMoves()
    {
        possibleMoves = new List<Vector3>() { };
    }

    public Vector3 getDiagonalMove(Vector3 pos, Vector2[] dir, ref int pRot)
    {
        Vector3 currentPos = pos;
        currentPos = getSpaceSimple(currentPos, dir[0], ref pRot);
        currentPos = getSpaceSimple(currentPos, dir[1], ref pRot);

        return currentPos;
    }
    public Vector3 getDiagonalMove(Vector3 pos, Vector2[] dir, int pRot)
    {
        Vector3 currentPos = pos;
        currentPos = getSpaceSimple(currentPos, dir[0], pRot);
        currentPos = getSpaceSimple(currentPos, dir[1], pRot);

        return currentPos;
    }

    public Vector3 getSpaceSimple(Vector3 position, Vector2 dir, ref int pRot)
    {

        vectorRotateCheat(ref dir, pRot);//Orients the piece (in a way)

        Vector3 space = position;

        space.x += dir.x;//only one of these should have a non zero value
        space.y += dir.y;

        //if(space.x < 0 || space.y < 0 || space.x > 7)//So the player cannot move out of the board lol
        //{
        //    return position;
        //}

        if (space.y > 3)// If the player crosses into a new region of the board
        {
            pRot = (pRot + 2) % 4;//Do a 180
            space.y = 3;//All y=3 coords lead to the y=3 of another section
            if (space.x <= 3)//determines which section to go to
            {
                space.z += 1;
                if (space.z > 2)
                    space.z = 0;
                switch (space.x)// the x values are inverse or somthing, idk if i could use that lerp thing to flip the order
                {
                    case 0:
                        space.x = 7;
                        break;
                    case 1:
                        space.x = 6;
                        break;
                    case 2:
                        space.x = 5;
                        break;
                    case 3:
                        space.x = 4;
                        break;
                }
            }
            else
            {
                space.z--;
                if (space.z < 0)
                    space.z = 2;
                switch (space.x)
                {
                    case 4:
                        space.x = 3;
                        break;
                    case 5:
                        space.x = 2;
                        break;
                    case 6:
                        space.x = 1;
                        break;
                    case 7:
                        space.x = 0;
                        break;
                }
            }
        }
        return space;
    }
    public Vector3 getSpaceSimple(Vector3 position, Vector2 dir, int pRot)
    {

        vectorRotateCheat(ref dir, pRot);//Orients the piece (in a way)

        Vector3 space = position;

        space.x += dir.x;//only one of these should have a non zero value
        space.y += dir.y;

        //if(space.x < 0 || space.y < 0 || space.x > 7)//So the player cannot move out of the board lol
        //{
        //    return position;
        //}

        if (space.y > 3)// If the player crosses into a new region of the board
        {
            pRot = (pRot + 2) % 4;//Do a 180
            space.y = 3;//All y=3 coords lead to the y=3 of another section
            if (space.x <= 3)//determines which section to go to
            {
                space.z += 1;
                if (space.z > 2)
                    space.z = 0;
                switch (space.x)// the x values are inverse or somthing, idk if i could use that lerp thing to flip the order
                {
                    case 0:
                        space.x = 7;
                        break;
                    case 1:
                        space.x = 6;
                        break;
                    case 2:
                        space.x = 5;
                        break;
                    case 3:
                        space.x = 4;
                        break;
                }
            }
            else
            {
                space.z--;
                if (space.z < 0)
                    space.z = 2;
                switch (space.x)
                {
                    case 4:
                        space.x = 3;
                        break;
                    case 5:
                        space.x = 2;
                        break;
                    case 6:
                        space.x = 1;
                        break;
                    case 7:
                        space.x = 0;
                        break;
                }
            }
        }
        return space;
    }

    public Vector2 vectorRotateCheat(ref Vector2 v2, int angle)
    {
        float y;
        for (int i = 0; i < angle; i++)
        {
            v2.x *= -1;
            y = v2.y;
            v2.y = v2.x;
            v2.x = y;
        }

        return v2;

    }
    public Vector2 vectorRotateCheat(Vector2 v2, int angle)
    {
        float y;
        Vector2 vector = v2;
        for (int i = 0; i < angle; i++)
        {
            vector.x *= -1;
            y = vector.y;
            vector.y = vector.x;
            vector.x = y;
        }

        return v2;

    }

    public static GameObject[] packageGrid(GameObject[,,] grid)
    {
        GameObject[] tempGrid = new GameObject[8 * 4 * 3];
        for (int z = 0; z < 3; z++)
        {
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 8; x++)
                {
                    tempGrid[x + 8 * y + 32 * z] = grid[x, y, z];
                }
        }
        return tempGrid;
    }
    public static GameObject[,,] unpackageGrid(GameObject[] grid)
    {
        GameObject[,,] tempGrid = new GameObject[8, 4, 3];
        for (int z = 0; z < 3; z++)
        {
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 8; x++)
                {
                    tempGrid[x, y, z] = grid[x + 8 * y + 32 * z];
                }
        }
        return tempGrid;
    }

    public static Vector3 getCoordInWorldSpace(Vector3 coord)
    {

        Vector3 world = new Vector3(0, 0, 0);

        float slope;
        float x = coord.x - 3.5f;
        float y = coord.y + 0.5f;
        float z = coord.z;
        float tan60 = Mathf.Tan(Mathf.Deg2Rad * 60);

        slope = x > 0 ? Mathf.Tan(Mathf.Deg2Rad * (90 - 7.5f * x)) : slope = Mathf.Tan(Mathf.Deg2Rad * (-90 + 7.5f * -x));

        if (x < 0)
        {
            world.x = (x * slope + 4 * tan60 - (4 - y) * Mathf.Cos(Mathf.Deg2Rad * 60) * Mathf.Tan(Mathf.Deg2Rad * (30 - 7.5f * (4 - y))) + 4 * tan60 * Mathf.Cos(Mathf.Deg2Rad * 30) * Mathf.Tan(Mathf.Deg2Rad * (30 - 7.5f * (4 - y))) - 4 * tan60 * Mathf.Sin(Mathf.Deg2Rad * 30) - (4 - y) * Mathf.Sin(Mathf.Deg2Rad * 60)) / (slope - Mathf.Tan(Mathf.Deg2Rad * (30 - 7.5f * (4 - y))));
        }
        else
        {
            world.x = (x * slope + 4 * tan60 + (4 - y) * Mathf.Cos(Mathf.Deg2Rad * 60) * Mathf.Tan(Mathf.Deg2Rad * (150 + 7.5f * (4 - y))) - 4 * tan60 * Mathf.Cos(Mathf.Deg2Rad * 30) * Mathf.Tan(Mathf.Deg2Rad * (150 + 7.5f * (4 - y))) - 4 * tan60 * Mathf.Sin(Mathf.Deg2Rad * 30) - (4 - y) * Mathf.Sin(Mathf.Deg2Rad * 60)) / (slope - Mathf.Tan(Mathf.Deg2Rad * (150 + 7.5f * (4 - y))));
        }

        world.z = (world.x - x) * slope - 4 * tan60;

        float xt = world.x * Mathf.Cos(Mathf.Deg2Rad * (-120 * z)) - world.z * Mathf.Sin(Mathf.Deg2Rad * (-120 * z));
        world.z = world.z * Mathf.Cos(Mathf.Deg2Rad * (-120 * z)) + world.x * Mathf.Sin(Mathf.Deg2Rad * (-120 * z));
        world.x = xt;

        return world;
    }
    public static Vector3 getBoard2World(Vector3 position)
    {
        if (board2World == null)
        {
            board2World = new Vector3[8, 4, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                    for (int n = 0; n < 8; n++)
                    {
                        board2World[n, j, i] = getCoordInWorldSpace(new Vector3(n, j, i));
                    }
        }
        return board2World[(int)position.x, (int)position.y, (int)position.z];
    }
    public static Vector3 getWorld2Board(Vector3 position)
    {
        if (world2Board == null)
        {
            world2Board = new Dictionary<Vector3, Vector3>();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                    for (int n = 0; n < 8; n++)
                    {
                        world2Board.Add(getBoard2World(new Vector3(n, j, i)), new Vector3(n, j, i));
                    }
        }
        return world2Board[position];
    }


    public static string interpretColor(int pieceID)
    {
        int c = pieceID / 100;
        switch (c)
        {
            case 1:
                return "white";
            case 2:
                return "red";
            case 3:
                return "black";
            default:
                return "white";
        }
    }

    public static int interpretType(int pieceID)
    {
        int H = (pieceID / 100) * 10;
        int T = (pieceID / 10) * 10;
        return T - H;
    }

    public static int interpretID(int pieceID)
    {
        int subOv10 = pieceID / 10;
        return pieceID - (subOv10 * 10);
    }


    [Command]
    public void CmdSetNameObj(string n)
    {
        RpcSetNameObj(n);
    }
    [ClientRpc]
    public void RpcSetNameObj(string n)
    {
        this.gameObject.name = n;
    }
}