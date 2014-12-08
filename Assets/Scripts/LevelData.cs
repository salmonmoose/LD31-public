using UnityEngine;
using System.Collections;

[System.Serializable]
public class LevelData : ScriptableObject
{
    public int [] map;
    public int mapSizeX;
    public int mapSizeY;
}