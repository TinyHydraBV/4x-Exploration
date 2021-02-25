using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap
{
    [Header("Pick a Number of Splats and Range/Radius of each continent Splat")]
    //public int elevateColumn;
    //public int elevateRow;
    public int minSplats;
    public int maxSplats;
    public int minRange;
    public int maxRange;
    //override base generate map function to make a specific type of map
    override public void GenerateMap()
    {
        //First call the base version to make our hexes
        base.GenerateMap();

        //Make a raised area
        //      elevate some hexes, how do we access the hex we want and how do we set the height? (two dimensional array)
        //      randomize placement based on ranges for number of splats and range (radius) of the splats.
        int numSplats = Random.Range(minSplats, maxSplats);
        for(int i= 0; i < numSplats; i++)
        {
            int range = Random.Range(minRange, maxRange);
            int y = Random.Range(range, numRows - range);
            int x = Random.Range(0, 10) - y/2 +20;
            ElevateArea(x, y, range);
        }

        // add elevations (perlin noise?)

        //set mesh to mountain/hill/flat/water based on height

        //simulate rainfall / moisture and set plains/grasslands + forest

        //update all hex visuals to match data
        UpdateHexVisuals();
    }

    void ElevateArea(int q, int r, int range, float centerHeight = 0.5f)
    {
        //grab center hex
        Hex centerHex = GetHexAt(q, r);

        //get hexes within range & set elevation so they will be set as grasslands
        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

        foreach(Hex h in areaHexes)
        {
            //make sure all hexes in the area are at sea level
            if(h.Elevation < 0)
            {
                h.Elevation = 0;
            }

            // lerp elevation from 1 to 0.25 (tunable) depending on the distance from the center hex
            h.Elevation += centerHeight * Mathf.Lerp(1f, 0.25f, Hex.Distance(centerHex, h) / range );
        }
    }
}
