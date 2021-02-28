using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool isDraggingCamera = false;
    [Tooltip("Does the angle change with zooming in/out using the scroll wheel?")]
    public bool angleChangeEnabled = true;

    [HeaderAttribute("Set zoom levels")]
    public float maxZoomIn = 2;
    public float maxZoomOut = 20;
    [Tooltip("Defines bookends for zoom amount where angle from camera to ground will either decress or increase")]
    public float angleAdjustLimit = 10;
    Vector3 lastMousePosition;


    // Update is called once per frame
    void Update()
    {
        //TODO: camera controls
        //      click and drag, camera moves <DONE>
        //      mouse to edge of frame, camera moves

        //Find ray to from mouse position
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //What is the point at which the mouse ray intersects y = 0?
        if (mouseRay.direction.y >= 0)
        {
            //Debug.LogError("Why is mouse pointing up?");
            return;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        Vector3 hitPos = mouseRay.origin - (mouseRay.direction * rayLength);

        if (Input.GetMouseButtonDown(0))
        {
            //Mouse button down start a drag
            isDraggingCamera = true;

            lastMousePosition = hitPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Mouse button up stop drag
            isDraggingCamera = false;
        }

        if (isDraggingCamera)
        {
            Vector3 diff = lastMousePosition - hitPos;
            Camera.main.transform.Translate(diff, Space.World);

            //redo the hit to eliminate camera jutter
            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //What is the point at which the mouse ray intersects y = 0?
            if (mouseRay.direction.y >= 0)
            {
                Debug.LogError("Why is mouse pointing up?");
                return;
            }
            rayLength = (mouseRay.origin.y / mouseRay.direction.y);
            lastMousePosition = mouseRay.origin - (mouseRay.direction * rayLength);
        }

        //zoom in via scroll wheel
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollAmount) > 0.01f)
        {
            //move camera toward hitPos within a limit
            Vector3 dir = hitPos - Camera.main.transform.position;

            Vector3 p = Camera.main.transform.position;
            // Stop zooming out at a certain distance
            //      TODO: maybe you shouldn't slide around when you are zoomed all the way in or slide around at maximum zoom out.
            if ( scrollAmount > 0 || p.y < maxZoomOut)
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
