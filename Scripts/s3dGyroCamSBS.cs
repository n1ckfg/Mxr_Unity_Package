using UnityEngine;
using System.Collections;

[System.Serializable]
/* s3dGyroCamBS.js - revised 3/11/13
 * Please direct any bugs/comments/suggestions to hoberman@usc.edu.
 * FOV2GO for Unity Copyright (c) 2011-13 Perry Hoberman & MxR Lab. All rights reserved.
 * Gyroscope-controlled camera for iPhone & Android revised 5.12.12
 * Usage: Attach this script to main camera.
 * Note: Unity Remote does not currently support gyroscope. 
 * This script uses three techniques to get the correct orientation out of the gyroscope attitude:
   1. creates a parent transform (camParent) and rotates it with eulerAngles
   2. for Android + Unity 3.5 only: remaps gyro.Attitude quaternion values from xyzw to wxyz (quatMap)
   3. multiplies attitude quaternion by quaternion quatMult
 * Also creates a grandparent (camGrandparent) which can be rotated to change heading
   This allows an arbitrary heading to be added to the gyroscope reading
   so that the virtual camera can be facing any direction in the scene, no matter which way the phone is actually facing
 * Option for direct touch input - horizontal swipe controls heading
 * Has companion Editor script (s3dGyroCamSBSEditor.js) for custom inspector 
 */
// camera grandparent node to rotate heading
// mouse/touch input
[UnityEngine.AddComponentMenu("Stereoskopix/s3d Gyro Cam")]
public partial class s3dGyroCamSBS : MonoBehaviour
{
    public static bool gyroBool;
    private Gyroscope gyro;
    private Compass compass;
    private Quaternion quatMult;
    private Quaternion quatMap;
    private ScreenOrientation prevScreenOrientation;
    private GameObject camParent;
    private GameObject camGrandparent;
    public float heading;
    public float Pitch;
    public bool setZeroToNorth;
    public bool checkForAutoRotation;
    public bool touchRotatesHeading;
    private float headingAtTouchStart;
    private float pitchAtTouchStart;
    private Vector2 mouseStartPoint;
    private Vector2 screenSize;
    public virtual void Awake()
    {
        // find the current parent of the camera's transform
        Transform currentParent = transform.parent;
        // instantiate a new transform
        camParent = new GameObject("camParent");
        // match the transform to the camera position
        camParent.transform.position = transform.position;
        // make the new transform the parent of the camera transform
        transform.parent = camParent.transform;
        // instantiate a new transform
        camGrandparent = new GameObject("camGrandParent");
        // match the transform to the camera position
        camGrandparent.transform.position = transform.position;
        // make the new transform the grandparent of the camera transform
        camParent.transform.parent = camGrandparent.transform;
        // make the original parent the great grandparent of the camera transform
        camGrandparent.transform.parent = currentParent;
        // check whether device supports gyroscope
        s3dGyroCamSBS.gyroBool = SystemInfo.supportsGyroscope;
        if (s3dGyroCamSBS.gyroBool)
        {
            prevScreenOrientation = Screen.orientation;
            gyro = Input.gyro;
            gyro.enabled = true;
            if (setZeroToNorth)
            {
                compass = Input.compass;
                compass.enabled = true;
            }
            fixScreenOrientation();
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public virtual void Start()
    {
        if (s3dGyroCamSBS.gyroBool)
        {
            if (setZeroToNorth)
            {
                StartCoroutine(turnToFaceNorth());
            }
        }
        screenSize.x = Screen.width;
        screenSize.y = Screen.height;
    }

    public virtual IEnumerator turnToFaceNorth()
    {
        yield return new WaitForSeconds(1);
        heading = Input.compass.magneticHeading;
    }

    public virtual void Update()
    {
        if (s3dGyroCamSBS.gyroBool)
        {
            transform.localRotation = quatMap * quatMult;
        }
        if (touchRotatesHeading)
        {
            GetTouchMouseInput();
        }

        {
            float _5 = heading;
            Vector3 _6 = camGrandparent.transform.localEulerAngles;
            _6.y = _5;
            camGrandparent.transform.localEulerAngles = _6;
        }

        {
            float _7 = // only update pitch if in Unity Editor (on device, pitch is handled by gyroscope)
            Pitch;
            Vector3 _8 = transform.localEulerAngles;
            _8.x = _7;
            transform.localEulerAngles = _8;
        }
    }

    public virtual void checkAutoRotation()
    {
        // check if Screen.orientation has changed
        if (prevScreenOrientation != Screen.orientation)
        {
            // fix gyroscope orientation settings
            fixScreenOrientation();
        }
        // also need to fix camera aspect
        prevScreenOrientation = Screen.orientation;
    }

    public virtual void fixScreenOrientation()
    {
    }

    public virtual void GetTouchMouseInput()
    {
        Vector2 delta = default(Vector2);
        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPoint = Input.mousePosition;
            headingAtTouchStart = heading;
            pitchAtTouchStart = Pitch;
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Input.mousePosition;
                delta.x = (mousePos.x - mouseStartPoint.x) / screenSize.x;
                heading = headingAtTouchStart + (delta.x * 100);
                heading = heading % 360;
                delta.y = (mousePos.y - mouseStartPoint.y) / screenSize.y;
                Pitch = pitchAtTouchStart + (delta.y * -100);
                Pitch = Mathf.Clamp(Pitch % 360, -60, 60);
            }
        }
    }

    public s3dGyroCamSBS()// end s3dGyroCamSBS.js
    {
        setZeroToNorth = true;
    }

}