using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    private void Start()
    {
        newPosition = this.transform.position;
    }

    Vector3 newPosition;

    Vector3 currentVelocity;
    public float smoothTime = 0.5f;

    public void OnUnitMoved( Hex oldHex, Hex newHex )
    {
        //This GameObject is supposed to be a child of the hex the unit is standing in
        //      ensuring we the unit is in the right place in the hierarchy
        //      The correct world position when not moving is at 0,0 local position relative to the parent

        this.transform.position = oldHex.PositionRelativeToCamera();
        newPosition = newHex.PositionRelativeToCamera();
        currentVelocity = Vector3.zero;

        if(Vector3.Distance(this.transform.position, newPosition) > 2)
        {
            //This is OnUnitMoved is considerably more than the expected move between two adjacent tiles
            //      its probably a map seem, so just teleport.
            this.transform.position = newPosition;
        }
    }

    private void Update()
    {
        //Try to move to new position over time
        this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
    }

}
