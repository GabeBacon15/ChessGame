using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BoardManager : NetworkBehaviour
{

    public SyncListInt spaces = new SyncListInt();
    [SyncVar]
    public bool isFull = false;
    [SyncVar]
    public int piecesReady = 0;

    public override void OnStartClient()
    {
        this.gameObject.name = "BoardManager";
        base.OnStartClient();
    }

    [Command(ignoreAuthority = true)]
    public void CmdAddToSpaces(int x, int y, int z, int pieceID)
    {
        if(spaces.Count == 0)
        {
            for(int i = 0; i < 8 * 4 * 3; i++)
            {
                spaces.Add(0);
            }
        }
        spaces[x + 8 * y + 32 * z] = pieceID;
    }

    public int[,,] getSpaces()
    {
        int[,,] tempSpaces = new int[8, 4, 3];
        for (int z = 0; z < 3; z++)
        {
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 8; x++)
                {
                    tempSpaces[x, y, z] = spaces[x + 8 * y + 32 * z];
                }
        }
        return tempSpaces;
    }

    public int getSpacesSize()
    {
        int count = 0;
        foreach(int i in spaces)
        {
            if (i != 0)
                count++;
        }
        return count;
    }

    [Command(ignoreAuthority = true)]
    public void CmdRemovePiece(int id)
    {
        for(int i = 0; i < spaces.Count; i++)
        {
            if(spaces[i] == id)
            {
                spaces[i] = 0;
            }
        }
    }
    [Command(ignoreAuthority = true)]
    public void CmdPiecesReadyPlusPlus()
    {
        piecesReady++;
    }

}