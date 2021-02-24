using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The Hex class defines the grid prosition, world space position, size, neighbors, etc... of a Hex tile.
///  The Hex class does NOT interact with Unity directly in any way (only returns a vector 3 for the correct world space position of a hex)
/// </summary>
public class Hex
{
    // when using cubic coordinates to define your hex you will need 3 coordinates q,r,s
    // hexes should never change its position on the hex grid, so use readonly (meaning value can only be set once during the constructor)
    

    //we would expect 3 values for Hex, but we only need two (given the math above the public fields) we never have to ask for the value of S
    public Hex(int q, int r)
    {
        this.Q = q;
        this.R = r;
        this.S = -(q + r);
    }

    //Q + R + S = 0
    // S = -(Q+R)
    public readonly int Q; // Column
    public readonly int R; // Row
    public readonly int S; // Some Sum

    readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    /// <summary>
    /// Returns the worldspace position of this hex
    /// </summary>
    /// <returns></returns>
    public Vector3 Position()
    {
        float radius = 1f;
        float height = radius * 2;
        //setup WIDTH MULTIPLIER as a constant above rather than doing that square root operation with each instantiation
        float width = WIDTH_MULTIPLIER * height;

        //define spacing between tiles
        float vert = height * 0.75f;
        float horiz = width;


        return new Vector3(
            horiz * (this.Q + this.R/2f), //horizontal spacing multiplied by a combination of what Column we're in plus half the Row (drawing rows on a diagonal  between z * x)
            0,
            vert * this.R //vertical spacing multilied by the Row we are in
        );
    }
}
