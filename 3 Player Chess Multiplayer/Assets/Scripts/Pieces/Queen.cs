using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Queen : Piece
{
    public static int numq = 0;
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        CmdSetPID();
        //CmdSetNameObj(pieceID + "");
        BoardMan.CmdAddToSpaces((int)position.x, (int)position.y, (int)position.z, pieceID);
    }

    public override void getPossibleMoves()
    {
        int[,,] spaces = BoardMan.getSpaces();
        List<Vector3> moves = getStraightMoves(spaces);
        List<Vector3> dmoves = getDiagonalMoves(spaces);
        moves.AddRange(dmoves);
        possibleMoves = moves;
    }

    private List<Vector3> getStraightMoves(int[,,] spaces)
    {
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

        return moves;
    }

    private List<Vector3> getDiagonalMoves(int[,,] spaces)
    {
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
            pos = position;
            bool isStartOutOfMiddle = true;
            if (!(pos.y == 3 && pos.x == bw && getDiagonalMove(pos, directions[i], rot).z != pos.z))
            {
                pos = getDiagonalMove(pos, directions[i], ref rot);
            }
            else
            {
                isStartOutOfMiddle = false;
            }

            string tempColor = "";
            if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
            {
                if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                    tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                else
                    tempColor = "";
            }
            while (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && (spaces[(int)pos.x, (int)pos.y, (int)pos.z] == 0 || !tempColor.Equals(color) || !isStartOutOfMiddle))
            {
                if (pos.y == 3 && pos.x == bw && getDiagonalMove(pos, directions[i], rot).z != pos.z)
                {
                    if (isStartOutOfMiddle)
                    {
                        clone = pos;
                        moves.Add(clone);
                        isStartOutOfMiddle = false;
                    }
                    Vector3 center = pos;
                    int centerRot = rot;
                    pos = getDiagonalMove(pos, secondaryDirections[i], ref rot);
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
                        pos = getDiagonalMove(pos, directions[i], ref rot);
                        if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
                        {
                            if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                                tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                            else
                                tempColor = "";
                        }

                    }
                    rot = centerRot;
                    pos = center;
                    pos = getDiagonalMove(pos, directions[i], ref rot);
                    if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
                    {
                        if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                            tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                        else
                            tempColor = "";
                    }
                }
                clone = pos;
                moves.Add(clone);
                if (!tempColor.Equals(color) && !tempColor.Equals(""))
                    break;
                pos = getDiagonalMove(pos, directions[i], ref rot);

                if ((int)pos.x >= 0 && (int)pos.x <= 7 && (int)pos.y >= 0 && (int)pos.y <= 3)
                {
                    if (spaces[(int)pos.x, (int)pos.y, (int)pos.z] != 0)
                        tempColor = interpretColor(spaces[(int)pos.x, (int)pos.y, (int)pos.z]);
                    else
                        tempColor = "";
                }
            }
            rot = tempRot;
        }

        return moves;
    }

    [Command]
    private void CmdSetPID()
    {
        int tempId = 0;
        switch (color)
        {
            case "white":
                tempId += 150 + numq;
                break;
            case "red":
                tempId += 250 + numq;
                break;
            case "black":
                tempId += 350 + numq;
                break;
            default:
                tempId += 150 + numq;
                break;
        }
        numq++;
        pieceID = tempId;
    }

}
