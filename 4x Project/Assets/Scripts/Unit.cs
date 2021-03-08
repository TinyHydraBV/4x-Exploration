using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;

//TODO: Make this a Strut
public class Unit : IQPathUnit
{

    public string Name = "Unnamed";
    public int HitPoints = 100;
    public int Strength = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

    //the hex a unit is standing in
    public Hex Hex { get; protected set; } //don't want something outside the unit putting us in a different Hex

    //setup a listener for triggering the unit movement
    public delegate void UnitMovedDelegate(Hex oldHex, Hex newHex);
    public event UnitMovedDelegate OnUnitMoved;

    //queue for passing a path to a unit
    Queue<Hex> hexPath;

    //TODO: This should be moved to a centeral option/config file
    public bool Civ6MovementRules = false; //Are the movement rules like Civ 6 (remaining movement is chopped off)

    public void SetHex(Hex newHex)
    {
        Hex oldHex = Hex;
        if(Hex != null)
        {
            Hex.RemoveUnit(this);
        }

        Hex = newHex;

        Hex.AddUnit(this);

        if(OnUnitMoved != null)
        {
            OnUnitMoved(oldHex, newHex);
        }
    }

    //TEMP - TODO: FIX ME!! (in Hex.cs too)
    public void DUMMY_PATHING_FUNCTION()
    {
        Hex[] pathHexes = QPath.QPath.FindPath<Hex>(
            Hex.HexMap,
            this,
            Hex,
            Hex.HexMap.GetHexAt( Hex.Q + 5, Hex.R ), // TEST: try to pathfind 5 tiles to the right
            Hex.CostEstimate
        );

        //accomodate FindPath() IQPathTile implementation requirements
        //Hex[] pathTiles = System.Array.ConvertAll( pathHexes, a => (Hex)a ); //convert IQPathTile array to Hex array

        Debug.Log("Got pathfinding path of length: " + pathHexes.Length);

        SetHexPath(pathHexes);

    }

    public void SetHexPath( Hex[] hexPath )
    {
        this.hexPath = new Queue<Hex>(hexPath);
        this.hexPath.Dequeue(); //First hex is the one unit is standing in, so throw it out
    }

    public void DoTurn()
    {
        Debug.Log("Turn executed");
        //do queued move?

        if(hexPath == null || hexPath.Count == 0)
        {
            return;
        }

        //Grab first hex from our queue
        Hex newHex = hexPath.Dequeue();

        //TEST: Move unit one tile to the right
        //Hex oldHex = Hex;
        //Hex newHex = oldHex.HexMap.GetHexAt(oldHex.Q + 1, oldHex.R);

        //move to the new Hex
        SetHex( newHex );
    }

    //Two helper functions for pathfinding
    public float MovementCostToEnterHex(Hex hex)
    {
        //TEST: Assume it costs one movement to enter a HEX
        //TODO: Override base movement cost based on movement mode & tile type
        return hex.BaseMovementCost();
    }

    public float AggregateTurnsToEnterHex( Hex hex, float turnsToDate)
    {
        //The issue is that if a unis is trying to enter a tile with a movement cost > current remaining movement 
        //      points this will either result in a cheaper than expected turn cost (free move), or a more expensive
        //      than expected turn cost (can't spend all your movement points).

        float baseTurnsToEnterHex = MovementCostToEnterHex(hex) / Movement;
        if(baseTurnsToEnterHex < 0)
        {
            //no entry to this hex, Impassable terrain
            return -999999;
        }
        
        if(baseTurnsToEnterHex > 1)
        {
            //Even if something costs 3 to enter, and a unit has a max move of 2, units can enter it using a full turn of movement
            baseTurnsToEnterHex = 1;
        }

        float turnsRemaining = MovementRemaining / Movement;

        float turnsToDateWhole = Mathf.Floor(turnsToDate); // Example: 4.33 becomes 4
        float turnsToDateFraction = turnsToDate - turnsToDateWhole; // Example: 4.33 becomes .33

        //check
        if((turnsToDateFraction < 0.01f && turnsToDateFraction > 0) || turnsToDateFraction > 0.99f)
        {
            Debug.Log("Indication of floating point drift:" + turnsToDate);
            if (turnsToDateFraction < 0.01f)
            {
                turnsToDateFraction = 0;
            }
            if (turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }
        }

        float turnsUsedAfterThisMove = turnsToDateFraction + baseTurnsToEnterHex; // Example: 0.33 + 1 

        if (turnsUsedAfterThisMove > 1)
        {
            //Not enough movement to complete this move
            //      What to do?
            if( Civ6MovementRules == true)
            {
                //Unit not allowed to enter tile this move
                if(turnsToDateFraction == 0)
                {
                    //Unit has full movement, but this is not enough to enter tile
                    //  Example: Max move of 2 but tile costs 3 to enter
                }
                else
                {
                    //Unit is not on a fresh turn
                    //Sit idle for the remainder of this turn.
                    turnsToDateWhole += 1;
                    turnsToDateFraction = 0;
                }

                //Unit is starting the move into difficult terrain on a fresh turn
                turnsUsedAfterThisMove = baseTurnsToEnterHex;
            }
            else
            {
                //Civ5 style movement rule: where units can always enter a tile,
                //      even if it doesn't have enough movement left.
                turnsUsedAfterThisMove = 1;
            }
        }

        //turnsUsedAfterThisMove is now btwn 0 and 1 (this includes the fractional part of moves from previous turns)

        //Return the number of turns THIS move is going to take? Or, return total turn cost of turnsToDate + turns
        //      for this move. So 4.33 turns => 5 turns to complete as an aggregate.

        return turnsToDateWhole + turnsUsedAfterThisMove;

    }

    /// <summary>
    /// Turn cost to enter hex (i.e. 0.5 turns with a movement cost of 1 and a max Unit movement of 2)
    /// </summary>
    public float CostToEnterHex( IQPathTile sourceTile, IQPathTile destinationTile)
    {
        return 1; //TEMP
    }

}
