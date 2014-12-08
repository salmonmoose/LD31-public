using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System;

public class Level : MonoBehaviour {

    private GameObject[,] mapObjects;
    private int [,] map;
    public GameObject[] prefabs;
    public GameObject[] buttons;
    private GameObject brush;
    private bool painting = false;
    private bool playerPaint = false;
    private bool erasing = false;
    private bool paintable = false;
    private bool playing = false;
    private int paintValue = -1;
    private Vector3 worldPos;

    private int crittersOut;
    private int crittersIn;
    private int crittersDead;
    private int crittersTotal;

    public GameObject crittersOutText;
    public GameObject crittersInText;
    public GameObject crittersDeadText;
    public GameObject levelScoreText;

    int mapSizeX = 36;
    int mapSizeY = 16;

    private static Level _singleton;

    public static Level instance {
        get {
            if (_singleton == null) {
                _singleton = GameObject.FindObjectOfType<Level>();
            }

            return _singleton;
        }
    }

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

    void ToggleButtons() {
        for (int i = 0; i < buttons.Length; i++) {
            if (buttons[i].GetComponent<Button>().interactable) {
                buttons[i].GetComponent<Button>().interactable = false;
            } else {
                buttons[i].GetComponent<Button>().interactable = true;        
            }
        }
    }

    void UpdateInterface() {
        crittersOutText.GetComponent<Text>().text = String.Format("{0:0}", crittersOut);
        crittersInText.GetComponent<Text>().text = String.Format("{0:0}", crittersIn);
        crittersDeadText.GetComponent<Text>().text = String.Format("{0:0}", crittersDead);
    }

    public void AddCritterTotal(int count) {
        crittersTotal = crittersTotal + count;
    }

    public void SpawnCritter() {
        crittersOut = crittersOut + 1;
    }

    public void SaveCritter() {
        crittersOut = crittersOut - 1;
        crittersIn = crittersIn + 1;
    }

    public void KillCritter() {
        crittersOut = crittersOut - 1;
        crittersDead = crittersDead + 1;
    }

    public void SaveLevel() {
        LevelData ld = ScriptableObject.CreateInstance<LevelData>();

        ld.mapSizeX = mapSizeX;
        ld.mapSizeY = mapSizeY;

        ld.map = new int[mapSizeX * mapSizeY];

        for (int y = 0; y < mapSizeY; y++) {
            for (int x = 0; x < mapSizeX; x++) {
                ld.map[(y * mapSizeX) + x] = map[x,y];
            }
        }

        AssetDatabase.CreateAsset(ld, "Assets/NewLevelData.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadLevel(LevelData level) {
        int children = transform.childCount;
        for (int i = 0; i < children; i++) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
        
        mapSizeX = level.mapSizeX;
        mapSizeY = level.mapSizeY;

        map = new int [mapSizeX, mapSizeY];

        for (int y = 0; y < mapSizeY; y++) {
            for (int x = 0; x < mapSizeX; x++) {
                map[x,y] = level.map[(y * mapSizeX) + x];
            }
        }
        
        PopulateLevel();
        Reset();
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

    public bool OutOfBounds(Vector3 position) {
        float x = Mathf.Round(position.x + (mapSizeX / 2) - 0.5f);
        float y = Mathf.Round(position.y + (mapSizeY / 2) - 0.5f);

        if (x < 0) return true;
        if (y < 0) return true;
        if (x > mapSizeX) return true;
        if (y > mapSizeY) return true;

        return false;
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

        UpdateInterface();

        if (crittersIn + crittersDead == crittersTotal && playing) {
            EndLevel();
        }
    }

    void EndLevel() {
        playing = false;
        ToggleButtons();
        Debug.Log("End Level");

        float score = ((float)crittersIn / (float)crittersTotal) * 100;

        levelScoreText.GetComponent<Text>().text = String.Format("{0:0}% Saved!", score);

        levelScoreText.transform.parent.gameObject.GetComponent<MenuPanel>().ShowPanel();
    }

    void MoveBrush() {
        Vector3 mousePos=new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        worldPos=Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 coords = GetCoords(worldPos);
        int coordX = (int) coords.x;
        int coordY = (int) coords.y;
        
        if(painting || playerPaint) {
            if(map[coordX, coordY] < 0) {
                brush.transform.position = GetPosition(coords);    
            }
        }
    }

    void PaintBrush() {
        Vector2 coords = GetCoords(worldPos);
        int coordX = (int) coords.x;
        int coordY = (int) coords.y;

        if(painting || playerPaint) {
            if (Input.GetMouseButtonDown(0) && paintable) {
                map[coordX, coordY] = paintValue;
                brush.transform.parent = transform;                
                mapObjects[coordX, coordY] = brush;
                brush = null;
                if (painting) {
                    AddBlock();
                } else {
                    playerPaint = false;
                }
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

    public void PlayerPaint(int blockType) {
        playerPaint = true;
        paintValue = blockType;

        AddBlock();
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
        crittersOut = 0;
        crittersIn = 0;
        crittersDead = 0;
        crittersTotal = 0;

        for (int x = 0; x < mapSizeX; x ++) {
            for (int y = 0; y < mapSizeY; y ++) {
                if(map[x,y] >= 0) {
                    mapObjects[x,y].SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public void Activate() {
        playing = true;
        ToggleButtons();
        for (int x = 0; x < mapSizeX; x ++) {
            for (int y = 0; y < mapSizeY; y ++) {
                if(map[x,y] >= 0) {
                    mapObjects[x,y].SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}