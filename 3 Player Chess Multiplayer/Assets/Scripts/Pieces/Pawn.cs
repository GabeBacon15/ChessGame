using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Pawn : Piece
{
    public static int numpw = 0;
    private bool hasMoved;
    public override void OnStartAuthority()
    {
        hasMoved = false;
        base.OnStartAuthority();
        CmdSetPID();
        BoardMan.CmdAddToSpaces((int)position.x, (int)position.y, (int)position.z, pieceID);
    }

    public override void move(Vector3 position)
    {
        if (this.position.z != position.z)
        {
            rot += 2;
        }
        base.move(position);
        hasMoved = true;
    }

    public override void getPossibleMoves()
    {
        int[,,] spaces = BoardMan.getSpaces();
        List<Vector3> moves = new List<Vector3>();
        Vector3 pos = position;
        Vector3 clone;
        int tempRot = rot;

        if (!hasMoved)
        {
            pos = getSpaceSimple(pos, new Vector2(0, 1), rot);
            if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0)
            {
                moves.Add(pos);
                pos = getSpaceSimple(pos, new Vector2(0, 1), rot);
                if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0)
                {
                    clone = pos;
                    moves.Add(clone);
                }
            }
        }
        else
        {
            tempRot = rot;
            pos = getSpaceSimple(position, new Vector2(0, 1), ref rot);
            if (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0)
            {
                clone = pos;
                moves.Add(clone);
            }
            rot = tempRot;
        }
        tempRot = rot;
        pos = getDiagonalMove(position, new Vector2[] { new Vector2(1, 0), new Vector2(0, 1) }, ref rot);
        string tempColor = "";
        if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
        {
            if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
            else
                tempColor = "";
        }
        if (!tempColor.Equals(color) && !tempColor.Equals("") && (int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
            moves.Add(pos);

        rot = tempRot;

        tempRot = rot;
        pos = getDiagonalMove(position, new Vector2[] { new Vector2(-1, 0), new Vector2(0, 1) }, ref rot);
        if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
        {
            if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
            else
                tempColor = "";
        }
        if (!tempColor.Equals(color) && !tempColor.Equals("") && (int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
            moves.Add(pos);

        rot = tempRot;
        pos = position;
        if (pos.y == 3 && (pos.x == 3 || pos.x == 4))
        {
            tempRot = rot;
            pos = getDiagonalMove(position, new Vector2[] { new Vector2(0, 1), new Vector2(pos.x == 3 ? 1 : -1, 0) }, ref rot);
            if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
            else
                tempColor = "";
            if (!tempColor.Equals(color) && !tempColor.Equals(""))
                moves.Add(pos);
            rot = tempRot;
        }
        possibleMoves = moves;
    }

    [Command]
    private void CmdSetPID()
    {
        int tempId = 0;
        switch (color)
        {
            case "white":
                tempId += 100 + numpw;
                break;
            case "red":
                tempId += 200 + numpw;
                break;
            case "black":
                tempId += 300 + numpw;
                break;
            default:
                tempId += 100 + numpw;
                break;
        }
        numpw++;
        pieceID = tempId;
    }

}