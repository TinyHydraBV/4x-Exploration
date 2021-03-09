using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Update_CurrentFunc = Update_DetectModeStart;

        hexMap = GameObject.FindObjectOfType<HexMap>(); // TO CHECK: this may break on WipeMap() and NewMap()

        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
    }

    //Generic bookkeeping variables
    HexMap hexMap;
    Hex hexUnderMouse;
    Hex lastHexUnderMouse;
    Vector3 lastMousePosition; //From Input.mousePosition

    //Camera dragging bookkeeping variables
    [Tooltip("pixel delta threshold for mouse movement to trigger a camera drag")]
    public int mouseDragThreshold = 1; // threshold of mouse movement to start a drag
    Vector3 lastMouseGroundPlanePosition;

    [Tooltip("Does the angle change with zooming in/out using the scroll wheel?")]
    public bool angleChangeEnabled = true;
    [HeaderAttribute("Set zoom levels")]
    public float maxZoomIn = 2;
    public float maxZoomOut = 20;
    [Tooltip("Defines bookends for zoom amount where angle from camera to ground will either decress or increase")]
    public float angleAdjustLimit = 10;

    //Unit movement
    Unit selectedUnit = null;
    Hex[] hexPath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc; //Update_CurrentFunc variable is just a pointer to a function that will run on each update frame

    public LayerMask LayerIDForHexTiles;
    
    void Update()
    {
        hexUnderMouse = MouseToHex();


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
        lastHexUnderMouse = hexUnderMouse;
    }

    void CancelUpdateFunc()
    {
        //reset detect mode start
        Update_CurrentFunc = Update_DetectModeStart;

        //Also cleanup any UI stuff associated with modes.

        selectedUnit = null;
    }

    //Given the current context what is the correct mouse behavior?
    void Update_DetectModeStart()
    {

        if (Input.GetMouseButtonDown(0))
        {
            //left mouse button down (this frame)
            //      This doesn't do much (maybe nothing beyond it's camera drag functionality)
            Debug.Log("Mouse down");
        }
        else if (Input.GetMouseButtonUp(0) )
        {
            //left click & release (no dragging, need a drag threshold to position check to avoid human error)
            Debug.Log("Mouse up - clicked yo!");

            //Detect if clicking on a hex with a unit
            //      If true, select it
            Unit[] us = hexUnderMouse.Units();

            //TODO: Implement cycling through multiple units in the same tile
            if (us.Length > 0)
            {
                selectedUnit = us[0];
                Update_CurrentFunc = Update_UnitMovement;
            }

            

        }
        else if (Input.GetMouseButton(0) && (Vector3.Distance( Input.mousePosition, lastMousePosition) > mouseDragThreshold))
        {
            //left mouse button is being held down && the mouse moved (camera drag)
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();
        }
        else if (selectedUnit != null && Input.GetMouseButton(1))
        {
            //Unit selected and player is holding down the right mouse button: Unit movement mode
            //      Show a path from unit to mouse position via the pathfinding system

        }
    }

    Hex MouseToHex()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        int layerMask = LayerIDForHexTiles.value;

        if ( Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, layerMask) )//take the mouse ray and raycast against things that are on the HexTile layer
        {
            //something got hit
            //Debug.Log(hitInfo.collider.name);

            //The collider is a child of the game object that we want
            GameObject hexGO = hitInfo.rigidbody.gameObject; //the rigid body is on the parent and we should get the parent object

            return hexMap.GetHexFromGameObject(hexGO);
        }
        Debug.Log("Found nothing, this is worrisome.");
        return null;
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

    void Update_UnitMovement()
    {
        if (Input.GetMouseButtonUp(1) || selectedUnit == null)
        {
            Debug.Log("Complete unit movement.");

            //copy pathfinding path to unit's movement queue (if that unit has movement left it will execute move immediately)
            if(selectedUnit != null)
            {
                selectedUnit.SetHexPath(hexPath);
            }
                
            CancelUpdateFunc();
            return;
        }

        //Unit selected 
        //  Look at the hex under our mouse
        //  Is this a different hex than before? Do we have a path?
            //  Do pathfinding search to that hex
            //  draw the path to that hex

        if(hexUnderMouse != lastHexUnderMouse || hexPath == null)
        {
            //pathfinding
            hexPath = QPath.QPath.FindPath<Hex>(
                hexMap,
                selectedUnit,
                selectedUnit.Hex,
                hexUnderMouse,
                Hex.CostEstimate
            );

            //draw path
            DrawPath(hexPath);
        }

    }

    void DrawPath(Hex[] hexPath)
    {
        if(hexPath.Length == 0)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;

        Vector3[] positions = new Vector3[hexPath.Length];

        for ( int i = 0; i < hexPath.Length; i++)
        {
            GameObject hexGO = hexMap.GetGameObjectFromHex(hexPath[i]);
            positions[i] = hexGO.transform.position + (Vector3.up*0.1f);
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
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
            }// angleChangeEnabled
        }
    }// Update_ScrollZoom()
}
