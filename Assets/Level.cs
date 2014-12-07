using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Level : MonoBehaviour {

    private GameObject[,] mapObjects;
    private int [,] map;
    public GameObject[] prefabs;
    private GameObject brush;
    private bool painting = false;
    private bool erasing = false;
    private bool paintable = false;
    private int paintValue = -1;
    private Vector3 worldPos;

    int mapSizeX = 36;
    int mapSizeY = 16;

    // Use this for initialization
    void Start () {
        mapObjects = new GameObject[mapSizeX, mapSizeY];
        map = new int[mapSizeX, mapSizeY];

        InitLevel();
    }
    
    void InitLevel() {
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                map[x,y] = -1;
            }
        }

        PopulateLevel();
    }

    void SaveLevel(string name) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + name + "_map.dat");

        LevelData ld = new LevelData();
        ld.map = map;
        ld.mapSizeX = mapSizeX;
        ld.mapSizeY = mapSizeY;

        bf.Serialize(file, ld);
        file.Close();
    }

    void LoadLevel(string name) {
        if(File.Exists(Application.persistentDataPath + "/map.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + name + "map.dat", FileMode.Open);

            LevelData ld = bf.Deserialize(file) as LevelData;
            file.Close();

            map = ld.map;
            mapSizeX = ld.mapSizeX;
            mapSizeY = ld.mapSizeY;
        }
    }

    void PopulateLevel() {
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                if(map[x,y] >= 0) {
                    GameObject go = Instantiate (
                        prefabs[map[x,y]],
                        GetPosition(x,y),
                        Quaternion.identity
                    ) as GameObject;

                    go.transform.parent = transform;

                    mapObjects[x,y] = go;
                }
            }
        }
    }

    private Vector3 GetPosition(Vector2 position) {
        return GetPosition((int) position.x, (int) position.y);
    }

    private Vector3 GetPosition(int x, int y) {
        return new Vector3(
            x - (mapSizeX / 2) + 0.5f,
            y - (mapSizeY / 2) + 0.5f,
            0f
        );
    }

    private Vector2 GetCoords(Vector3 position) {
        paintable = true;
        float x = Mathf.Round(position.x + (mapSizeX / 2) - 0.5f);
        float y = Mathf.Round(position.y + (mapSizeY / 2) - 0.5f);

        if (x < 0) {
            x = 0;
            paintable = false;
        } else if (x > mapSizeX -1) {
            x = mapSizeX -1;
            paintable = false;
        }

        if (y < 0) {
            y = 0;
            paintable = false;
        } else if (y > mapSizeY -1) {
            y = mapSizeY -1;
            paintable = false;
        }

        return new Vector2(x,y);
    }

    // Update is called once per frame
    void Update () {
        Vector3 mousePos=new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        worldPos=Camera.main.ScreenToWorldPoint(mousePos);

        Debug.DrawLine(Vector3.zero, worldPos, Color.white);

        MoveBrush();
        PaintBrush();
    }

    void MoveBrush() {
        Vector3 mousePos=new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        worldPos=Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 coords = GetCoords(worldPos);
        int coordX = (int) coords.x;
        int coordY = (int) coords.y;
        
        if(painting) {
            if(map[coordX, coordY] < 0) {
                brush.transform.position = GetPosition(coords);    
            }
        }
    }

    void PaintBrush() {
        Vector2 coords = GetCoords(worldPos);
        int coordX = (int) coords.x;
        int coordY = (int) coords.y;

        if(painting) {
            if (Input.GetMouseButtonDown(0) && paintable) {
                map[coordX, coordY] = paintValue;
                brush.transform.parent = transform;                
                mapObjects[coordX, coordY] = brush;
                brush = null;
                AddBlock();
            }
        } else if (erasing) {
            if (Input.GetMouseButtonDown(0)) {
                map[coordX, coordY] = -1;
                Destroy(mapObjects[coordX, coordY]);
            }
        }
    }

    void AddBlock() {
        Destroy(brush);

        brush = Instantiate(
            prefabs[paintValue],
            Vector3.zero,
            Quaternion.identity
        ) as GameObject;

        MoveBrush();
    }

    public void Paint(int blockType) {
        if (painting && blockType == paintValue) {
            Destroy(brush);
            painting = false;
        } else {
            if (erasing) Eraser();
            painting = true;
            paintValue = blockType;

            AddBlock();
        }
    }

    public void Eraser() {
        if (erasing) {
            erasing = false;
        } else {
            if (painting) Paint(paintValue);
            erasing = true;
        }
    }

    public void Reset() {
        for (int x = 0; x < mapSizeX; x ++) {
            for (int y = 0; y < mapSizeY; y ++) {
                if(map[x,y] >= 0) {
                    mapObjects[x,y].SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public void Activate() {
        for (int x = 0; x < mapSizeX; x ++) {
            for (int y = 0; y < mapSizeY; y ++) {
                if(map[x,y] >= 0) {
                    mapObjects[x,y].SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}

[Serializable]
class LevelData
{
    public int [,] map;
    public int mapSizeX;
    public int mapSizeY;
}