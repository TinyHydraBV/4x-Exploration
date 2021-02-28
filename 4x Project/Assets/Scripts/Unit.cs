using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Make this a Strut
public class Unit
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

    public void DoTurn()
    {
        //do queued move?

        //TEST: Move unit one tile to the right
        Hex oldHex = Hex;
        Hex newHex = oldHex.HexMap.GetHexAt(oldHex.Q + 1, oldHex.R);

        SetHex( newHex );
    }
}
