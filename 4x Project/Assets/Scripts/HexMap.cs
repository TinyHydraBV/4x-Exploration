using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }
    public GameObject HexPrefab;

    //terrain Mesh types
    public Mesh MeshWater;
    public Mesh MeshFlat;
    public Mesh MeshHill;
    public Mesh MeshMountain;

    //Fixed Material Types (replacing array)
    public Material MatOcean;
    public Material MatPlains;
    public Material MatGrasslands;
    public Material MatMountain;

    //tunable values for minimum height to define terrain type
    [Header("Define minimum height for a tile to be a certain terrain type")]
    public float mountainHeight = 0.9f;
    public float hillHeight = 0.5f;
    [Tooltip("This tunes sea level height")]
    public float flatHeight = 0.0f; //sea level

    //define map dimensions
    public enum mapSize { Test, Tiny, Small, Standard, Huge };
    public mapSize myMapSize;

    [HideInInspector]
    public int numRows;

    [HideInInspector]
    public int numColumns; //don't use width because width of base hex != 1

    // TODO: Link up with Hex.cs class of this!
    [Tooltip("Allows map to wrap East to West, this is true by default and should probably not be disabled")]
    public bool allowWrapEastWest = true;
    [HideInInspector]
    public bool allowWrapNorthSouth = false;  //TODO: Set this up to work (don't like doubkle cylinder wrapping which is what copying E to W implementation would do)

    private Hex[,] hexes;
    //track some way to link which game objects are asigned to which hex
    private Dictionary<Hex, GameObject> hexToGameObjectMap;

    public Hex GetHexAt(int x, int y)
    {
        if(hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated!");
            return null;
        }

        if (allowWrapEastWest)
        {
            x = x % numColumns; //module doesn't seem to fix negatives
            if(x < 0)
            {
                x += numColumns;
            }
        }

        //find continent generation going out of bounds (asking for negative numbers)
        try
        {
            return hexes[x, y];
        }
        catch
        {
            Debug.LogError("GetHexAt: "+ x +","+y);
            return null;
        }
    }

    virtual public void GenerateMap()
    {
        //base map generation (all ocean)


        //generate our number of rows and columns for the map based on selected map size
        if(myMapSize == mapSize.Tiny)
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

                //the hex game object should know about itself and the map to pass to the HexComponent to move itself based on this info
                hexGO.GetComponent<HexComponent>().Hex = h;
                hexGO.GetComponent<HexComponent>().HexMap = this;

                //show debug hex coordinates
                hexGO.GetComponentInChildren<TextMesh>().text = string.Format("{0}, {1}", column, row);

                
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
        for(int column = 0; column < numColumns; column++)
        {
            for(int row = 0; row < numRows; row++)
            {
                Hex h = hexes[column, row];
                GameObject hexGO = hexToGameObjectMap[h];

                //Set material and mesh
                //grab GameObject Mesh Renderer materials slot
                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                //grab GameObject Mesh Filter component
                MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();
                if (h.Elevation >= mountainHeight)
                {
                    mr.material = MatMountain;
                    mf.mesh = MeshMountain;
                }
                else if (h.Elevation >= hillHeight)
                {
                    mr.material = MatGrasslands; //temporarily grassland material will be hills until we institute specific models for hills
                    mf.mesh = MeshHill;
                }
                else if (h.Elevation >= flatHeight)
                {
                    mr.material = MatPlains;
                    mf.mesh = MeshFlat;
                }
                else
                {
                    mr.material = MatOcean; //set materials slot to ocean
                    mf.mesh = MeshWater; //set materials slot to ocean
                } 
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
                results.Add( GetHexAt(centerHex.Q + dx, centerHex.R + dy));
            }
        }

        return results.ToArray();
    }

    //Update is called once per frame
    //void Update()
    //{

    //}
}
