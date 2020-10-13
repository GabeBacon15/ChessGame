using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    public static int numr = 0;
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        setPID();
        //CmdSetNameObj(pieceID + "");
        boardManager.CmdAddToSpaces((int)position.x, (int)position.y, (int)position.z, pieceID);
    }

    public override void getPossibleMoves()
    {
        int[,,] spaces = boardManager.getSpaces();
        List<Vector3> moves = new List<Vector3>();
        int tempRot = rot;
        Vector2 dir = new Vector2(0, 1);

        Vector3 pos;
        Vector3 clone;

        for (int i = 0; i < 4; i++)
        {
            pos = getSpaceSimple(position, dir, ref rot);
            string tempColor = "";
            if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
            {
                if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                    tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                else
                    tempColor = "";
            }
            while (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && (spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0 || !tempColor.Equals(color)))
            {
                clone = pos;
                moves.Add(clone);
                if (!tempColor.Equals(color) && !tempColor.Equals(""))
                    break;
                pos = getSpaceSimple(pos, dir, ref rot);
                if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
                {
                    if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                        tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                    else
                        tempColor = "";
                }
            }
            vectorRotateCheat(ref dir, 1);
            rot = tempRot;
        }

        possibleMoves = moves;
    }

    private void setPID()
    {
        pieceID = 0;
        switch (color)
        {
            case "white":
                pieceID += 110 + numr;
                break;
            case "red":
                pieceID += 210 + numr;
                break;
            case "black":
                pieceID += 310 + numr;
                break;
            default:
                pieceID += 110 + numr;
                break;
        }
        numr++;
    }

}