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
            Debug.LogError("Why is mouse pointing up?");
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

    }
}
