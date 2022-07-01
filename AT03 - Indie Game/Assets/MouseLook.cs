using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 2.5f; // sensitivity of mouse input
    public float drag = 1.5f; // continued mouse movement after input stops
    private Transform character; //this refrensces character transform
    private Vector2 mouseDirection; // this stores curser coordinates
    private Vector2 smoothing; // smoothed curser movement value
    private Vector2 result; //resulting curser position

    public static bool mouseLookEnabled = true;

    void Start()
    {
        mouseLookEnabled = true;
        character = transform.root;
    }
    private void Awake()
    {
        character = transform.parent; // get preference toparents transform's
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

        // Update is called once per frame
        void Update()
        {
            mouseDirection = new Vector2(Input.GetAxisRaw("Mouse X") * sensitivity,
                Input.GetAxisRaw("Mouse Y") * sensitivity); // calculate mouse direction
            smoothing = Vector2.Lerp(smoothing, mouseDirection, 1 / drag); //calculate smoothing
            result += smoothing; //add smoothing to result
            result.y = Mathf.Clamp(result.y, -80, 80); // clamps y angle
            transform.localRotation = Quaternion.AngleAxis(-result.y, Vector3.right); //aply x axis rotation
            character.rotation = Quaternion.AngleAxis(result.x, character.up); //apply y rotation to character
        }
    }
