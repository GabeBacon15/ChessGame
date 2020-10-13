using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PieceDictionary
{
    public PieceIndex[] pieces;
    public GameObject getPrefab(string name)
    {
        foreach(PieceIndex pi in pieces)
        {
            if (pi.name.Equals(name))
                return pi.prefab;
        }
        return null;
    }
}
[System.Serializable]
public struct PieceIndex
{
    public string name;
    public GameObject prefab;
}
