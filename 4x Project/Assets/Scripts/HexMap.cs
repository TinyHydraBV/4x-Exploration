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
    public void GenerateMap() 
    {
        //generate our hexes for the map
        //start by making 10x10 map (will make this a variable later)
        //10 columns
        for (int column = 0; column < 10; column++) 
        {
            //10 rows
            for (int row = 0; row < 10; row++)
            {
                //Instantiate a Hex
                    //This will use a helper class (Hex)that is responsible for defining "A Hex" and applies a cube coordinate approach to hex placement.
                    // Hex.cs is a pure c-sharp data class
                    // For more on Cube Coordinate approach checkout www.redblobgames.com/grids/hexagons/
                    //because of the offset in the Hex.cs class we will get a rombus shape as all columns will be drawn on a diagonal
                        //keep in mind this will result in the max row number being offset from the verticle on the map, ie hex coordinates (0,10) will be +5 units (in world coordinates) along the x axis from hex (0,0) 
                Hex h = new Hex(column, row);

                // rotation required by this instantiation, using Quaternion.identity means no rotation
                // fourth parameter (the parent) is optional
                Instantiate(
                    HexPrefab,
                    //new Vector3(column, 0, row), replaced by hex position, h.Position(), from Hex.cs
                    h.Position(),
                    Quaternion.identity,
                    this.transform // pass the HexMap's transform as the parent of the newly created Hexes (everything will be a child of hex map)
                );
            }
            
        }
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
