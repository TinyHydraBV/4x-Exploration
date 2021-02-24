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

    //list of materials for defining different areas of the hex map
    public Material[] HexMaterials;

    //define map dimensions
    public enum mapSize { Tiny, Small, Standard, Huge };
    public mapSize myMapSize;

    [HideInInspector]
    public int numRows;

    [HideInInspector]
    public int numColumns; //don't use width because width of base hex != 1

    public void GenerateMap()
    {
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
        else
        {
            //standard map size
            numColumns = 84;
            numRows = 54;
        }

        //for number of columns
        for (int column = 0; column < numColumns; column++) 
        {
            //for number of rows
            for (int row = 0; row < numRows; row++)
            {
                //Instantiate a Hex
                    //This will use a helper class (Hex)that is responsible for defining "A Hex" and applies a cube coordinate approach to hex placement.
                    // Hex.cs is a pure c-sharp data class
                    // For more on Cube Coordinate approach checkout www.redblobgames.com/grids/hexagons/
                    //because of the offset in the Hex.cs class we will get a rombus shape as all columns will be drawn on a diagonal
                        //keep in mind this will result in the max row number being offset from the verticle on the map, ie hex coordinates (0,10) will be +5 units (in world coordinates) along the x axis from hex (0,0) 
                Hex h = new Hex(column, row);
                Vector3 pos = h.PositionRelativeToCamera(
                    Camera.main.transform.position,
                    numRows,
                    numColumns
                );

                // rotation required by this instantiation, using Quaternion.identity means no rotation
                // fourth parameter (the parent) is optional
                GameObject hexGO = (GameObject) Instantiate(
                    HexPrefab,
                    pos,
                    Quaternion.identity,
                    this.transform // pass the HexMap's transform as the parent of the newly created Hexes (everything will be a child of hex map)
                );

                //the hex game object should know about itself and the map to pass to the HexComponent to move itself based on this info
                hexGO.GetComponent<HexComponent>().Hex = h;
                hexGO.GetComponent<HexComponent>().HexMap = this;

                //grab GameObject Mesh Renderer materials slot
                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();

                //set materials slot to one of our materials at random
                mr.material = HexMaterials[ Random.Range(0,HexMaterials.Length) ];
            }  
        }

        //Can't use static batching with wrap around solution because we need to be able to move the hex transform based on camera position
            //get static batching at runtime for this root object (try to combine some stuff for efficiencies sake)
            //for a 10x10 map this takes our batches from hundreds to tens, so this should have a real impact on larger maps
       //StaticBatchingUtility.Combine(this.gameObject);
    }

    //Update is called once per frame
    //void Update()
    //{
        
    //}
}
