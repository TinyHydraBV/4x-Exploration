using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Update_CurrentFunc = Update_DetectModeStart;
    }

    //Generic bookkeeping variables
    Vector3 lastMousePosition; //From Input.mousePosition

    //Camera dragging bookkeeping variables
    Vector3 lastMouseGroundPlanePosition;

    [Tooltip("Does the angle change with zooming in/out using the scroll wheel?")]
    public bool angleChangeEnabled = true;
    [HeaderAttribute("Set zoom levels")]
    public float maxZoomIn = 2;
    public float maxZoomOut = 20;
    [Tooltip("Defines bookends for zoom amount where angle from camera to ground will either decress or increase")]
    public float angleAdjustLimit = 10;
   

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc; //Update_CurrentFunc variable is just a pointer to a function that will run on each update frame

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //cancel any mode you are in
            CancelUpdateFunc();
        }

        //listen for new mode to start
        Update_CurrentFunc();

        //right now this always is allowed
        //TODO: Check for being over a scrolling UI element
        Update_ScrollZoom();

        //where's my mouse at yo?
        lastMousePosition = Input.mousePosition;
    }

    void CancelUpdateFunc()
    {
        //reset detect mode start
        Update_CurrentFunc = Update_DetectModeStart;

        //Also cleanup any UI stuff associated with modes.
    }

    //Given the current context what is the correct mouse behavior?
    void Update_DetectModeStart()
    {
        //left click & release
        //if it's point to a unit
        //if it's pointing to an interactable UI element

        if (Input.GetMouseButtonDown(0))
        {
            //left mouse button down (this frame)
            //      This doesn't do much (maybe nothing beyond it's camera drag functionality)
        }
        else if (Input.GetMouseButton(0) && Input.mousePosition != lastMousePosition)
        {
            //left mouse button is being held down && the mouse moved (camera drag)
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();
        }
    }

    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        //Find ray to from mouse position
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        //What is the point at which the mouse ray intersects y = 0?
        if (mouseRay.direction.y >= 0)
        {
            //Debug.LogError("Why is mouse pointing up?");
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    void Update_CameraDrag()
    {
        //TODO: camera controls
        //      click and drag, camera moves <DONE>
        //      mouse to edge of frame, camera moves
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Cancelling camera drag.");
            CancelUpdateFunc();
            return;
        }

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);

    }

    void Update_ScrollZoom()
    {
        //zoom in via scroll wheel
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        if (Mathf.Abs(scrollAmount) > 0.01f)
        {
            //move camera toward hitPos within a limit
            Vector3 dir = hitPos - Camera.main.transform.position;

            Vector3 p = Camera.main.transform.position;
            // Stop zooming out at a certain distance
            //      TODO: maybe you shouldn't slide around when you are zoomed all the way in or slide around at maximum zoom out.
            if (scrollAmount > 0 || p.y < maxZoomOut)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }

            //limit zoom out and in
            p = Camera.main.transform.position;
            if (p.y < maxZoomIn)
            {
                p.y = maxZoomIn;
            }
            if (p.y > maxZoomOut)
            {
                p.y = maxZoomOut;
            }
            Camera.main.transform.position = p;

            if (angleChangeEnabled == true)
            {
                //change camera angle
                float lowZoomAngle = maxZoomIn + angleAdjustLimit;
                float highZoomAngle = maxZoomOut - angleAdjustLimit;
                if (p.y < lowZoomAngle)
                {
                    Camera.main.transform.rotation = Quaternion.Euler(
                        Mathf.Lerp(35, 55, ((p.y - maxZoomIn) / (lowZoomAngle - maxZoomIn))),
                        Camera.main.transform.rotation.eulerAngles.y,
                        Camera.main.transform.rotation.eulerAngles.z
                        );
                }
                else if (p.y > highZoomAngle)
                {
                    Camera.main.transform.rotation = Quaternion.Euler(
                        Mathf.Lerp(55, 85, ((p.y - highZoomAngle) / (maxZoomOut - highZoomAngle))),
                        Camera.main.transform.rotation.eulerAngles.y,
                        Camera.main.transform.rotation.eulerAngles.z
                        );
                }
                else
                {
                    Camera.main.transform.rotation = Quaternion.Euler(
                        55,
                        Camera.main.transform.rotation.eulerAngles.y,
                        Camera.main.transform.rotation.eulerAngles.z
                        );

                }
            }
        }
    }
}
