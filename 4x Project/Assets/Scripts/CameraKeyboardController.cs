using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKeyboardController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Tooltip("WASD controlled camera movement speed")]
    public float moveSpeed = 25f;

    // Update is called once per frame
    void Update()
    {
        //override mouse movement with WASD controls
        Vector3 translate = new Vector3
            (
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );
        this.transform.Translate(translate * moveSpeed * Time.deltaTime, Space.World);
    }
}
