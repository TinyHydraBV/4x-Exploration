using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using QPath;

public class HexMap : MonoBehaviour, IQPathWorld
{
    // Start is called before the first frame update
    void Start()
    {
        // WipeMap();
        GenerateMap();
    }
    private void Awake()
    {
        seed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private void Update()
    {
        //TESTING: Hit spacebar to advance to next turn
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(units != null)
            {
                foreach (Unit u in units)
                {
                    u.DoTurn();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (units != null)
            {
                foreach (Unit u in units)
                {
                    u.DUMMY_PATHING_FUNCTION();
                }
            }
        }
    }

    public GameObject HexPrefab;
    //Get parent for wiping map

    //terrain Mesh types
    public Mesh MeshWater;
    public Mesh MeshFlat;
    public Mesh MeshHill;
    public Mesh MeshMountain;

    //forest/jungle
    //      TODO: make seperate jungle mesh
    public GameObject ForestPrefab;
    public GameObject JunglePrefab;

    //Fixed Material Types (replacing array)
    public Material MatOcean;
    public Material MatPlains;
    public Material MatDesert;
    public Material MatGrasslands;
    public Material MatMountain;

    public GameObject UnitTestPrefab;

    //define map dimensions
    public enum mapSize { Test, Tiny, Small, Standard, Huge };
    [Header("Pick a Map Size")]
    public mapSize myMapSize;

    //tunable values for minimum height to define terrain type
    [Header("Define minimum height for a tile to be a certain terrain type")]
    [Range(0.0f,1.0f)] public float mountainHeight = 0.9f;
    [Range(0.0f, 1.0f)] public float hillHeight = 0.5f;
    [Tooltip("This tunes sea level height")]
    [Range(-1.0f, 1.0f)] public float flatHeight = 0.0f; //sea level

    [Header("Define minimum moisture levels for a tile to be a certain terrain subtype")]
    public float MoistureJungle = 1f;
    public float MoistureForest = 0.5f;
    public float MoistureGrasslands = 0.0f;
    public float MoisturePlains = -0.5f;

    [HideInInspector]
    public int numRows;

    [HideInInspector]
    public int numColumns; //don't use width because width of base hex != 1

    // TODO: Link up with Hex.cs class of this!
    [Tooltip("Allows map to wrap East to West, this is true by default and should probably not be disabled")]
    public bool allowWrapEastWest = true;
    [System.NonSerialized]
    public bool allowWrapNorthSouth = false;  //TODO: Set this up to work (don't like doubkle cylinder wrapping which is what copying E to W implementation would do)

    private Hex[,] hexes;
    //track some way to link which game objects are asigned to which hex
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;

    private HashSet<Unit> units;
    private Dictionary<Unit, GameObject> unitToGameObjectMap;

    [System.NonSerialized]
    public int seed;

    public Hex GetHexAt(int x, int y)
    {
        if (hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated!");
            return null;
        }

        if (allowWrapEastWest)
        {
            x = x % numColumns; //module doesn't seem to fix negatives
            if (x < 0)
            {
                x += numColumns;
            }
        }
        if (allowWrapNorthSouth)
        {
            y = y % numRows;
            if (y < 0)
            {
                y += numRows;
            }
        }

        //find continent generation going out of bounds (asking for negative numbers)
        try
        {
            return hexes[x, y];
        }
        catch
        {
            Debug.LogError("GetHexAt: " + x + "," + y);
            return null;
        }
    }

    public Hex GetHexFromGameObject( GameObject hexGO)
    {
        if( gameObjectToHexMap.ContainsKey(hexGO))
        {
            return gameObjectToHexMap[hexGO];
        }
        return null;
    }
    public GameObject GetGameObjectFromHex(Hex h)
    {
        if (hexToGameObjectMap.ContainsKey(h))
        {
            return hexToGameObjectMap[h];
        }
        return null;
    }

    public Vector3 GetHexPosition(int q, int r)
    {
        Hex hex = GetHexAt(q, r);
        return GetHexPosition(hex);
    }
    public Vector3 GetHexPosition(Hex hex)
    {
        return hex.PositionRelativeToCamera(Camera.main.transform.position, numRows, numColumns);
    }

    virtual public void GenerateMap()
    {
        //base map generation (all ocean)
        //TODO: Call helper function to wipe map between map generations.

        //generate our number of rows and columns for the map based on selected map size
        if (myMapSize == mapSize.Tiny)
        {
            numColumns = 60;
            numRows = 38;
        }
        else if (myMapSize == mapSize.Small)
        {
            numColumns = 74;
            numRows = 46;
        }
        else if (myMapSize == mapSize.Huge)
        {
            numColumns = 96;
            numRows = 60;
        }
        else if (myMapSize == mapSize.Test)
        {
            numColumns = 10;
            numRows = 10;
        }
        else
        {
            //standard map size
            numColumns = 84;
            numRows = 54;
        }

        //store hexes into 2D array
        hexes = new Hex[numColumns, numRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

        //Generate Base ocean map
        //for number of columns
        for (int column = 0; column < numColumns; column++)
        {
            //for number of rows
            for (int row = 0; row < numRows; row++)
            {
                //Instantiate a Hex
                //      This will use a helper class (Hex)that is responsible for defining "A Hex" and applies a cube coordinate approach to hex placement.
                //      Hex.cs is a pure c-sharp data class
                //      For more on Cube Coordinate approach checkout www.redblobgames.com/grids/hexagons/
                //      because of the offset in the Hex.cs class we will get a rombus shape as all columns will be drawn on a diagonal
                //      keep in mind this will result in the max row number being offset from the verticle on the map, ie hex coordinates (0,10) will be +5 units (in world coordinates) along the x axis from hex (0,0) 
                Hex h = new Hex(this, column, row);

                //set starting elevation for all hexes (base ocean level)
                h.Elevation = -0.5f;

                // story at x & y coordinates in the array
                hexes[column, row] = h;

                Vector3 pos = h.PositionRelativeToCamera(
                    Camera.main.transform.position,
                    numRows,
                    numColumns
                );

                // rotation required by this instantiation, using Quaternion.identity means no rotation
                // fourth parameter (the parent) is optional
                GameObject hexGO = (GameObject)Instantiate(
                    HexPrefab,
                    pos,
                    Quaternion.identity,
                    this.transform // pass the HexMap's transform as the parent of the newly created Hexes (everything will be a child of hex map)
                );

                hexToGameObjectMap[h] = hexGO;
                gameObjectToHexMap[hexGO] = h;

                //the hex game object should know about itself and the map to pass to the HexComponent to move itself based on this info
                hexGO.name = string.Format("HEX: {0}, {1}", column, row); // name each hex in the hierarchy with its coordinats 
                hexGO.GetComponent<HexComponent>().Hex = h;
                hexGO.GetComponent<HexComponent>().HexMap = this;
            }
        }

        UpdateHexVisuals();

        //Can't use static batching with wrap around solution because we need to be able to move the hex transform based on camera position
        //get static batching at runtime for this root object (try to combine some stuff for efficiencies sake)
        //for a 10x10 map this takes our batches from hundreds to tens, so this should have a real impact on larger maps
        //StaticBatchingUtility.Combine(this.gameObject);
    }

    public void UpdateHexVisuals()
    {
        //loop through all our hexes
        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                Hex h = hexes[column, row];
                GameObject hexGO = hexToGameObjectMap[h];

                //Set material and mesh
                //grab GameObject Mesh Renderer materials slot
                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                //grab GameObject Mesh Filter component
                MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();

                //set moisture level
                if (h.Elevation >= flatHeight && h.Elevation < mountainHeight)
                {
                    if (h.Moisture >= MoistureJungle)
                    {
                        mr.material = MatGrasslands;
                        //Spawn jungle adjusting for hill height
                        Vector3 p = hexGO.transform.position;
                        if (h.Elevation >= hillHeight)
                        {
                            p.y += 0.25f;
                        }
                        h.MovementCost = 2; //jungles cost two to move through
                        GameObject.Instantiate(JunglePrefab, p, Quaternion.identity, hexGO.transform);
                    }
                    else if (h.Moisture >= MoistureForest)
                    {
                        mr.material = MatGrasslands;
                        //Spawn forests adjusting for hill height
                        Vector3 p = hexGO.transform.position;
                        if (h.Elevation >= hillHeight)
                        {
                            p.y += 0.25f;
                        }
                        h.MovementCost = 2; //forests cost two to move through
                        GameObject.Instantiate(ForestPrefab, p, Quaternion.identity, hexGO.transform);
                    }
                    else if (h.Moisture >= MoistureGrasslands)
                    {
                        mr.material = MatGrasslands;
                    }
                    else if (h.Moisture >= MoisturePlains)
                    {
                        mr.material = MatPlains;
                    }
                    else
                    {
                        mr.material = MatDesert; //set materials slot to ocean
                    }
                }

                //apply models to terrain based on elevation (will override mountain texture)
                if (h.Elevation >= mountainHeight)
                {
                    mr.material = MatMountain;
                    mf.mesh = MeshMountain;
                    h.MovementCost = -999; //mountains are impassable (this is so low the pathfinding will always go around it but need to figure out a better solution)
                }
                else if (h.Elevation >= hillHeight)
                {
                    mf.mesh = MeshHill;
                    h.MovementCost = 2; //hills cost two to move through
                }
                else if (h.Elevation >= flatHeight)
                {
                    mf.mesh = MeshFlat;
                    h.MovementCost = 1.001f; //adding small amount to terrain to reduce sizgzagging in path finding and favor hills over flatland (if paths are equal)

                }
                else
                {
                    mr.material = MatOcean; //set materials slot to ocean
                    mf.mesh = MeshWater; //set materials slot to ocean
                    h.MovementCost = -999; //TEMP: no walking on water either
                }

                //show debug hex coordinates
                hexGO.GetComponentInChildren<TextMesh>().text = string.Format("{0}, {1}\n{2}", column, row, h.BaseMovementCost());

            }
        }
    }

    public Hex[] GetHexesWithinRangeOf(Hex centerHex, int range)
    {
        //use cubic math to get all tiles within a range of a defined central tile.
        List<Hex> results = new List<Hex>();

        for (int dx = -range; dx < range - 1; dx++)
        {
            for (int dy = Mathf.Max(-range + 1, -dx - range); dy < Mathf.Min(range, -dx + range - 1); dy++)
            {
                results.Add(GetHexAt(centerHex.Q + dx, centerHex.R + dy));
            }
        }

        return results.ToArray();
    }

    public void SpawnUnitAt(Unit unit, GameObject prefab, int q, int r)
    {

        if(units == null)
        {
            units = new HashSet<Unit>();
            unitToGameObjectMap = new Dictionary<Unit, GameObject>();
        }

        Hex myHex = GetHexAt(q, r);
        GameObject myHexGO = hexToGameObjectMap[myHex];
        unit.SetHex(myHex);

        GameObject unitGO = (GameObject)Instantiate(prefab, myHexGO.transform.position, Quaternion.identity, myHexGO.transform);
        unit.OnUnitMoved += unitGO.GetComponent<UnitView>().OnUnitMoved;

        units.Add(unit);
        unitToGameObjectMap[unit] = unitGO;
    }

    virtual public void WipeMap()
    {
        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                Hex h = hexes[column, row];
                h.Elevation = -0.5f;
            }
        }

        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (gameObj.name == "ForestPrefab(Clone)")
            {
                Destroy(gameObj);
            }
            if (gameObj.name == "JunglePrefab(Clone)")
            {
                Destroy(gameObj);
            }
        }
    }
}
