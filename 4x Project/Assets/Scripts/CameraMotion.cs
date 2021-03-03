using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        oldPosition = this.transform.position;
    }

    Vector3 oldPosition;

    // Update is called once per frame
    void Update()
    {
        //TODO: Code to click-and-drag camera
        //      WASD
        //      Zoom in and out

        CheckIfCameraMoved();
    }

    //function for paning to hex
    public void PanToHex ( Hex hex)
    {
        //TODO: Move camera to hex
    }

    //cache the hexes
    HexComponent[] hexes;

    void CheckIfCameraMoved()
    {
        if(oldPosition != this.transform.position)
        {
            //SOMETHING moved the camera
            //note the updated position
            oldPosition = this.transform.position;

            //TODO: Create HexMap dictionary of all these
            
            //cuts out the lookup
            if (hexes == null) {
                //only asign hexes here if they are not already asigned (expectation is number of hexes won't change)
                hexes = GameObject.FindObjectsOfType<HexComponent>();
            }

            //TODO: Better way to cull what hexes get updated

            foreach(HexComponent hex in hexes)
            {
                hex.UpdatePosition();
            }
        }
    }
}
