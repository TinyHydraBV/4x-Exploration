using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : HexMap_Continents
{
    public void ReviseMap()
    {
        WipeMap();
        UpdateHexVisuals();

        NewMap();
        UpdateHexVisuals();
    }
}
