using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class King : Piece
{
    public static int numki = 0;
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
        Vector2[][] directions = new Vector2[4][];
        directions[0] = new Vector2[] { new Vector2(0, 1), new Vector2(1, 0) };
        directions[1] = new Vector2[] { new Vector2(1, 0), new Vector2(0, -1) };
        directions[2] = new Vector2[] { new Vector2(0, -1), new Vector2(-1, 0) };
        directions[3] = new Vector2[] { new Vector2(-1, 0), new Vector2(0, 1) };

        Vector2[][] secondaryDirections = new Vector2[4][];
        secondaryDirections[0] = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1) };
        secondaryDirections[1] = new Vector2[] { new Vector2(0, -1), new Vector2(1, 0) };
        secondaryDirections[2] = new Vector2[] { new Vector2(-1, 0), new Vector2(0, -1) };
        secondaryDirections[3] = new Vector2[] { new Vector2(0, 1), new Vector2(-1, 0) };

        List<Vector3> moves = new List<Vector3>();
        int tempRot = rot;

        Vector3 pos;
        Vector3 clone;
        Vector2 direction = new Vector2(0, 1);

        for (int i = 0; i < 4; i++)
        {
            int bw;
            if (i < 2)
            {
                bw = 3;
            }
            else
            {
                bw = 4;
            }
            pos = getDiagonalMove(position, directions[i], ref rot);
            string tempColor = "";
            if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
            {
                if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                    tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                else
                    tempColor = "";
            }
            if (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && (spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0 || !tempColor.Equals(color)))
            {
                clone = pos;
                moves.Add(clone);
            }
            rot = tempRot;
            pos = getSpaceSimple(position, direction, ref rot);
            if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
            {
                if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                    tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                else
                    tempColor = "";
            }
            vectorRotateCheat(ref direction, 1);
            if (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && (spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0 || !tempColor.Equals(color)))
            {
                clone = pos;
                moves.Add(clone);
            }
            rot = tempRot;
            if (position.y == 3 && position.x == bw && getDiagonalMove(position, directions[i], rot).z != position.z)
            {
                pos = getDiagonalMove(position, secondaryDirections[i], ref rot);
                if ((spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0 || !tempColor.Equals(tempColor)))
                {
                    clone = pos;
                    moves.Add(clone);
                }
            }
            rot = tempRot;
        }
        possibleMoves = moves;
    }


    public void detectCheck(Vector3 pos)
    {

    }


    private void setPID()
    {
        pieceID = 0;
        switch (color)
        {
            case "white":
                pieceID += 140 + numki;
                break;
            case "red":
                pieceID += 240 + numki;
                break;
            case "black":
                pieceID += 340 + numki;
                break;
            default:
                pieceID += 140 + numki;
                break;
        }
        numki++;
    }

}
