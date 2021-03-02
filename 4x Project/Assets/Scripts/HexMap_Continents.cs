using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HexMap_Continents : HexMap
{
    [Header("Pick a Number of Splats and Range/Radius of each continent Splat")]
    //public int elevateColumn;
    //public int elevateRow;
    public int minSplats;
    public int maxSplats;
    public int minRange;
    public int maxRange;
    [HeaderAttribute("Crinkliness of the continent")]
    [Tooltip("The higher this is the more condensed (blobby) the continents will be.")]
    public float noiseResolution = 0.01f;
    [Tooltip("The higher this is the more islands we will have (more fractal it will be).")]
    public float noiseScale = 2f;

    [HeaderAttribute("Map moisture levels")]
    public float moistureResolution = 0.05f;
    public float moistureScale = 2f;

    [Header("Pick number and spacing of continents")]
    [Tooltip("Continent spacing will be determined based on map size.")]
    public int numContinents = 1;

    //override base generate map function to make a specific type of map
    override public void GenerateMap()
    {
        //First call the base version to make our hexes
        base.GenerateMap();

        //spacing between continents should be spread fairly evenly across map based on how many columns we have
        int continentSpacing = numColumns / numContinents;

        //make continents using random seed based on time
        Debug.Log("Time is: " + seed);
        UnityEngine.Random.InitState(seed); //maybe can store the seed for replication?

        for (int c = 0; c < numContinents; c++)
        {
            //Make a raised area
            //      elevate some hexes, how do we access the hex we want and how do we set the height? (two dimensional array)
            //      randomize placement based on ranges for number of splats and range (radius) of the splats.
            int numSplats = UnityEngine.Random.Range(minSplats, maxSplats);
            for (int i = 0; i < numSplats; i++)
            {
                int range = UnityEngine.Random.Range(minRange, maxRange);
                int y = UnityEngine.Random.Range(range, numRows - range);
                int x = UnityEngine.Random.Range(0, 10) - y / 2 + (c * continentSpacing);
                ElevateArea(x, y, range);
            }
        }

        // add elevations (perlin noise?)
        //      loop through whole map again
        //make noise more random (perlin noise is not inherently random)
        Vector2 noiseOffset = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f) );

        for (int column = 0; column < numColumns; column++)
        {
            for(int row = 0; row < numRows; row++)
            {
                Hex h = GetHexAt(column, row);
                //thank you Unity for building this in. this is a system for generating sudo-randomness.
                //      must force these to be a float so we don't end up with integers where the divisions always comes out to 0 or 1
                float n = 
                    Mathf.PerlinNoise(((float)column / Mathf.Max(numColumns, numRows) / noiseResolution) + noiseOffset.x,
                    ((float)row / Mathf.Max(numColumns, numRows) / noiseResolution) + noiseOffset.y)
                    - 0.5f;
                        //Mathf.Max(numColumns, numRows) fixes the alignment of perlin noise map to be rombus like our hex map, so we don't get stretched continents
                        //      if we just use numColumns and numRows our islands end up being long East to West

                //subtract 0.5 because perlin noise generates a value from 0 to 1 (thus we now have negative values)
                h.Elevation += n * noiseScale;
            }
        }


        //set mesh to mountain/hill/flat/water based on height

        //simulate rainfall / moisture and set plains/grasslands + forest
        noiseOffset = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                Hex h = GetHexAt(column, row);
                //thank you Unity for building this in. this is a system for generating sudo-randomness.
                //      must force these to be a float so we don't end up with integers where the divisions always comes out to 0 or 1
                float n =
                    Mathf.PerlinNoise(((float)column / Mathf.Max(numColumns, numRows) / moistureResolution) + noiseOffset.x,
                    ((float)row / Mathf.Max(numColumns, numRows) / moistureResolution) + noiseOffset.y)
                    - 0.5f;
                //Mathf.Max(numColumns, numRows) fixes the alignment of perlin noise map to be rombus like our hex map, so we don't get stretched continents
                //      if we just use numColumns and numRows our islands end up being long East to West

                //subtract 0.5 because perlin noise generates a value from 0 to 1 (thus we now have negative values)
                h.Moisture += n * moistureScale;
            }
        }

        //TODO: Create helper class that:
        //      1. determines equator
        //      2. defines a minimum distance from equator for arid zone based on map size (allow user to tune arid zone size)
        //      3. grabs tiles distance from equator
        //      4. determines if desert tiles are inside the arid zone, if not set's moisture level to plains.

        //TODO: Create helper class that:
        //      1. determines poles
        //      2. defines a minimum distance from poles for tundra zones (allow user to tune tundra zone size)
        //      3. grabs tiles distance from from pole
        //      4. determines if tile is inside tundra zone, if it is make it a tundra or snow tile based on moisture levels


        //update all hex visuals to match data
        UpdateHexVisuals();

        //spawn unit at starting location
        Unit unit = new Unit();
        SpawnUnitAt(unit, UnitTestPrefab, 36, 15);
    }

    void ElevateArea(int q, int r, int range, float centerHeight = 0.8f)
    {
        //grab center hex
        Hex centerHex = GetHexAt(q, r);

        //get hexes within range & set elevation so they will be set as grasslands
        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

        foreach(Hex h in areaHexes)
        {
            //make sure all hexes in the area are at sea level
            //if(h.Elevation < 0)
            //{
            //    h.Elevation = 0;
            //}

            // lerp elevation from 1 to 0.25 (tunable) depending on the distance from the center hex
            h.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(Hex.Distance(centerHex, h) / range, 2f) );
        }
    }
}
