using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //add support for ToArray()
using QPath;

/// <summary>
///  The Hex class defines the grid prosition, world space position, size, neighbors, etc... of a Hex tile.
///  The Hex class does NOT interact with Unity directly in any way (only returns a vector 3 for the correct world space position of a hex)
/// </summary>
public class Hex : IQPathTile
{
    // when using cubic coordinates to define your hex you will need 3 coordinates q,r,s
    // hexes should never change its position on the hex grid, so use readonly (meaning value can only be set once during the constructor)
    

    //we would expect 3 values for Hex, but we only need two (given the math above the public fields) we never have to ask for the value of S
    public Hex(HexMap hexMap, int q, int r)
    {
        this.HexMap = hexMap;

        this.Q = q;
        this.R = r;
        this.S = -(q + r);
    }

    //Q + R + S = 0
    // S = -(Q+R)
    public readonly int Q; // Column
    public readonly int R; // Row
    public readonly int S; // Some Sum

    //Data for map generation and in-game effects
    public float Elevation;
    public float Moisture;

    //TODO: Need property to track hex type (ocean, flatland, hills, mountain)
    //TODO: Need property to track hex subtype (plains, grasslands, desert, tundra, snow)
    //TODO: Need property to track hex details (forest, jungle, mine, farm, etc)

    public readonly HexMap HexMap;

    //setup WIDTH MULTIPLIER as a constant rather than doing that square root operation with each instantiation
    readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    //hard code radius of a hex (change later)
    float radius = 1f;

    HashSet<Unit> units;

    //TODO: Link this up to HexMap.cs version
    public bool allowWrapEastWest = true;
    public bool allowWrapNorthSouth = false;
        // this would be hard to implement as you would need to start going down after crossing the North pole, and vice versa.

    /// <summary>
    /// Returns the worldspace position of this hex
    /// </summary>
    /// <returns></returns>
    public Vector3 Position()
    {
        return new Vector3(
            HexHorizontalSpacing() * (this.Q + this.R/2f), //horizontal spacing multiplied by a combination of what Column we're in plus half the Row (drawing rows on a diagonal  between z * x)
            0,
            HexVerticalSpacing() * this.R //vertical spacing multilied by the Row we are in
        );
    }

    public float HexHeight()
    {
        return radius * 2;
    }

    public float HexWidth()
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    //define spacing between tiles
    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }
    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }

    public Vector3 PositionRelativeToCamera()
    {
        return HexMap.GetHexPosition(this);
    }

    public Vector3 PositionRelativeToCamera( Vector3 cameraPosition, float numRows, float numColumns) 
    {
        //float mapHeight = numRows * HexVerticalSpacing();
            // no north south wrapping
            // if we wanted this, use same code as follows up test height rather than width

        float mapWidth = numColumns * HexHorizontalSpacing();
        // how far off from the camera's position are we relative to map width total
            // we want howManyWidthsFromCamera to be btwn -0.5 and 0.5
        Vector3 position = Position();
        if(HexMap.allowWrapEastWest == true)
        {
            float howManyWidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;
            if (Mathf.Abs(howManyWidthsFromCamera) <= 0.5f)
            {
                //we're good
                return position;
            }
            //offset things 
            // if we are at 0.6, we want to be at -0.4
            // if we are at 0.8, we want to be at -0.2
            // if we are at 2.2, we want to be at 0.2
            // if we are at 2.8, we want to be at -0.2
            // we never want to be more than 0.5 away in either direction.

            if (howManyWidthsFromCamera > 0)
            {
                howManyWidthsFromCamera += 0.5f;
            }
            else
            {
                howManyWidthsFromCamera -= 0.5f;
            }

            // how much of a correction do we have to make (how many widths cast to int)
            int howManyWidthsToFix = (int)howManyWidthsFromCamera;

            position.x -= howManyWidthsToFix * mapWidth;
        }
        
        return position;
    }
    //TEMP - TODO: FIX ME! (in Unit.cs too)
    public static float CostEstimate(IQPathTile aa, IQPathTile bb)
    {
        return Distance((Hex)aa, (Hex)bb); //assume these parameters are hexes
    }
    public static float Distance(Hex a, Hex b)
    {
        int dQ = Mathf.Abs(a.Q - b.Q);

        //FIXME: This is probably wrong for wrapping
        if (a.HexMap.allowWrapEastWest)
        {
            if(dQ > a.HexMap.numColumns / 2)
            {
                dQ = a.HexMap.numColumns - dQ;
            }
        }

        //FIXME: This is only works (and is probably wrong) for a North South wrapping implementation that is not enabled atm: double cylindar or donut world wrapping)
        int dR = Mathf.Abs(a.R - b.R);
        if (a.HexMap.allowWrapNorthSouth)
        {
            if (dR > a.HexMap.numRows / 2)
            {
                dR = a.HexMap.numRows - dR;
            }
        }

        return
            // the largest difference will be the distance between two hexes
            Mathf.Max(
                dQ,
                dR,
                Mathf.Abs(a.S - b.S)
            );
    }

    public void AddUnit( Unit unit)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
        }

        units.Add(unit);
    }

    public void RemoveUnit( Unit unit)
    {
        if (units != null)
        {
            units.Remove(unit);
        }
    }

    public Unit[] Units()
    {
        return units.ToArray();
    }

    public int BaseMovementCost()
    {
        //TODO: Factor in terrain type and features
        return 1; //temporary for testing
    }

    //neighbors don't change
    Hex[] neighbours;

    #region IQPathTile implementation
    public IQPathTile[] GetNeighbours()
    {
        if(this.neighbours != null)
        {
            return this.neighbours;
        }

        List<Hex> neighbours = new List<Hex>();

        neighbours.Add(HexMap.GetHexAt(Q +  1, R +  0));
        neighbours.Add(HexMap.GetHexAt(Q + -1, R +  0));
        neighbours.Add(HexMap.GetHexAt(Q +  0, R + +1));
        neighbours.Add(HexMap.GetHexAt(Q +  0, R + -1));
        neighbours.Add(HexMap.GetHexAt(Q + +1, R + -1));
        neighbours.Add(HexMap.GetHexAt(Q + -1, R + +1));

        List<Hex> neighbours2 = new List<Hex>();

        foreach (Hex h in neighbours)
        {
            if (h != null)
            {
                neighbours2.Add(h);
            }
        }

        this.neighbours = neighbours2.ToArray();

        return this.neighbours;
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit theUnit)
    {
        //TODO: We are ignoring source tile atm, this will need to change with rivers
        return ((Unit)theUnit).AggregateTurnsToEnterHex(this, costSoFar);
    }
    #endregion

}
