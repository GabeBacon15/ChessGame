using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Knight : Piece
{
    public static int numkn = 0;
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        CmdSetPID();
        BoardMan.CmdAddToSpaces((int)position.x, (int)position.y, (int)position.z, pieceID);
    }

    public override void getPossibleMoves()
    {
        int[,,] spaces = BoardMan.getSpaces();
        List<Vector3> moves = new List<Vector3>();
        Vector3 pos = position;
        Vector3 clone;

        Vector2[][] dirs = new Vector2[4][];
        dirs[0] = new Vector2[] { new Vector2(0, 1), new Vector2(0, 1), new Vector2(1, 0) };
        dirs[1] = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 1) };
        dirs[2] = new Vector2[] { new Vector2(0, 1), new Vector2(0, 1), new Vector2(-1, 0) };
        dirs[3] = new Vector2[] { new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, 1) };

        for (int i = 0; i < 4; i++)
        {
            foreach (Vector2[] dir in dirs)
            {
                Vector3 current = getKnightMove(pos, dir, rot, color, spaces);
                if (current.x != -1)
                {
                    bool skip = false;
                    foreach (Vector3 possible in moves)
                    {
                        if (current == possible)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        clone = current;
                        moves.Add(current);
                    }
                }
            }
            for (int k = 0; k < 4; k++)
            {
                for (int j = 0; j < 3; j++)
                {
                    vectorRotateCheat(ref dirs[k][j], 1);
                    Debug.Log(dirs[k][j]);
                }
            }
        }


        possibleMoves = moves;
    }
    private Vector3 getKnightMove(Vector3 pos, Vector2[] dir, int roty, string pColor, int[,,] spaces)
    {
        pos = getSpaceSimple(pos, dir[0], ref roty);
        if (!(pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && pos.y <= 3))
        {
            Debug.Log("Escape 1");
            return new Vector3(-1, -1, -1);
        }
        pos = getSpaceSimple(pos, dir[1], ref roty);
        if (!(pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && pos.y <= 3))
        {
            Debug.Log("Escape 3");
            return new Vector3(-1, -1, -1);
        }
        pos = getSpaceSimple(pos, dir[2], ref roty);
        string tempColor = "";
        if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
        {
            if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
            else
                tempColor = "";
        }
        if (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && pos.y <= 3 && !tempColor.Equals(pColor))
        {
            return pos;
        }
        else
        {
            Debug.Log("Escape 4");
            return new Vector3(-1, -1, -1);
        }


    }

    [Command]
    private void CmdSetPID()
    {
        int tempId = 0;
        switch (color)
        {
            case "white":
                tempId += 120 + numkn;
                break;
            case "red":
                tempId += 220 + numkn;
                break;
            case "black":
                tempId += 320 + numkn;
                break;
            default:
                tempId += 120 + numkn;
                break;
        }
        numkn++;
        pieceID = tempId;
    }

}
