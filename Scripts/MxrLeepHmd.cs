//Open source predistortion code for LEEP optics and aspheric lenses
//University of Southern California Irvine
//Institute for Creative Technologies
//Mixed Reality Lab
//
//Contributors:
//Evan Suma, PhD
//Thai Phan
//Bradley Newman
//Sean Dumas

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*
USER GUIDE
--------------------------------
Follow these steps to enable camera culling so 
the HMD cameras don't draw the renderer meshes:
- Create a layer called "LEEP".
- Select the HMD camera(s) (e.g. LeepStereoPhasespace > LeftEyeCamera)
and in the Camera Component > Culling Mask turn off the LEEP layer.
- Select the LEEP Renderer Camera (e.g. LeepStereoRenderer > Camera)
and assign it to the LEEP layer. Set the Camera Component > Culling Mask
to None and then set it again to LEEP.
- Select the LEEP Renderer Meshes (e.g. LeepStereoRenderer > LeftEyeMesh)
and assign it to the LEEP layer.

*/

public class MxrLeepHmd : MonoBehaviour {
	
    //TP: this value can be changed in the Unity Inspector panel
	public bool isStereo = true;

    //TP: by default, we set the program to run in fullscreen mode, 1280 by 800 pixels
    private bool fullScreen = true;
    private int resolutionHor = 1280;
    private int resolutionVert = 800;

    private Camera cameraLeft = null;
    private Camera cameraRight = null;
	private Camera cameraMono = null;
    private Transform lEyeMesh;
    private Transform rEyeMesh;
    public GameObject leepRenderer;

    // near clipping plane
    public float nearClipPlane = 0.01f;

    // far clipping plane
    public float farClipPlane = 1000.0f;

    //TP: The interaxial distance is defined by the value
    //TP: stored inside hmd-config.cfg
    private float interaxialDistance = 0.064f;

    //TP: The vertical field-of-view angle is defined
    //TP: by the value (in degrees) stored inside hmd-config.cfg
    static float verticalFOV = 84.1f;

    //static float horizontalFOV = 75.12f; //TP: not using horizontalFOV since the aspectRatio will determine it

    //TP: The aspect ratio is defined
    //TP: inside hmd-config.hmd
    private float aspectRatio = 0.76923f;

    //TP: The predistortion constant K is defined
    //TP: inside hmd-config.hmd
    private float g_preDistort; //TP: the presistorion constant used in MxrPredistortionMesh.cs

    private bool displayValues = false;

	// Use this for initialization
	void Start () {
		if (isStereo)
		{
	        Transform camLeftTransform;
	        Transform camRightTransform;
	        camLeftTransform  = transform.Find("LeftEyeCamera");
	        camRightTransform = transform.Find("RightEyeCamera");
	
	        cameraLeft = camLeftTransform.GetComponent<Camera>();
	        cameraRight = camRightTransform.GetComponent<Camera>();
		}
		else
		{
	        Transform camMonoTransform;
	        camMonoTransform  = transform.Find("MonoCamera");	
	        cameraMono = camMonoTransform.GetComponent<Camera>();
		}

        lEyeMesh = leepRenderer.transform.Find("LeftEyeMesh");
        rEyeMesh = leepRenderer.transform.Find("RightEyeMesh");

        //TP: Read the config file
        if (File.Exists("hmd-config.cfg")) {
            List<string> list = new List<string>();
            using (StreamReader reader = new StreamReader("hmd-config.cfg")) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    list.Add(line); // Add to list.
                }
            }

            foreach (string s in list) {
                string[] argument = s.Split('=');
                bool canConvert;
                double argValue;
                if (argument[0] == "interaxial") {
                    canConvert = double.TryParse(argument[1].Trim(), out argValue);
                    if (canConvert)
                        interaxialDistance = (float)argValue;
                }
                else if (argument[0] == "verticalFOV") {
                    canConvert = double.TryParse(argument[1].Trim(), out argValue);
                    if (canConvert)
                        verticalFOV = (float)argValue;
                }
                else if (argument[0] == "aspectRatio") {
                    canConvert = double.TryParse(argument[1].Trim(), out argValue);
                    if (canConvert)
                        aspectRatio = (float)argValue;
                }
                else if (argument[0] == "preDistortK") {
                    canConvert = double.TryParse(argument[1].Trim(), out argValue);
                    if (canConvert) {
                        if (leepRenderer != null) {
                            leepRenderer.transform.Find("RightEyeMesh").GetComponent<MxrPredistortionMesh>().PreDistortK = (float)argValue;
                            leepRenderer.transform.Find("RightEyeMesh").GetComponent<MxrPredistortionMesh>().RebuildMesh();
                            leepRenderer.transform.Find("LeftEyeMesh").GetComponent<MxrPredistortionMesh>().PreDistortK = (float)argValue;
                            leepRenderer.transform.Find("LeftEyeMesh").GetComponent<MxrPredistortionMesh>().RebuildMesh();
                        }
                        else {
                            Debug.LogError("Assign a LEEPrenderer to the LEEPrenderer variable in the Inspector!");
                        }
                    }
                }
            }
        }

        //setup screen
        if (fullScreen) {
            Screen.SetResolution(resolutionHor, resolutionVert, true);
            Screen.fullScreen = true;
        }

        //set up clip planes
		if (isStereo)
		{
            cameraLeft.GetComponent<Camera>().near = nearClipPlane;
            cameraLeft.GetComponent<Camera>().far = farClipPlane;
            cameraRight.GetComponent<Camera>().near = nearClipPlane;
            cameraRight.GetComponent<Camera>().far = farClipPlane;
		}
		else
		{
            cameraMono.GetComponent<Camera>().near = nearClipPlane;
            cameraMono.GetComponent<Camera>().far = farClipPlane;
		}
	}
	
	// Update is called once per frame
    void Update()
    {
        //TP: You can move this line to the Start method
        //TP: if you know the values of verticalFOV, aspectRatio, and interaxial
        //TP: will not change during runtime.
        UpdateCameraSettings();

        //TP: Display the predistortion values to the screen
        if (Input.GetKeyDown(KeyCode.F1)) {
            displayValues = !displayValues;
        }

        //KeyboardAdjust(); //TP: Uncomment this line if you need hotkeys to adjust size and position of view ports
    }

	//This method can be called every update
	//if you need hotkeys to adjust the \
	//size and position of the left eye
	//and right eye view ports.
    private void KeyboardAdjust() {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            verticalFOV+=0.1f;
        }
        else if (Input.GetKeyDown(KeyCode.End))
        {
            verticalFOV -= 0.1f;
        }
        //if (Input.GetKeyDown(KeyCode.PageUp))
        //{
        //    horizontalFOV += 0.1f;
        //}
        //else if (Input.GetKeyDown(KeyCode.PageDown))
        //{
        //    horizontalFOV -= 0.1f;
        //}
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            interaxialDistance += 0.001f;
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            interaxialDistance -= 0.001f;
        }

        Vector3 rEyeMeshPos = rEyeMesh.localPosition;
        Vector3 lEyeMeshPos = lEyeMesh.localPosition;
        float delta = 0.0010f;

        if (Input.GetKeyDown(KeyCode.A))
        {
            lEyeMeshPos.x = delta + lEyeMeshPos.x;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            lEyeMeshPos.x = -delta + lEyeMeshPos.x;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            lEyeMeshPos.y = delta + lEyeMeshPos.y;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            lEyeMeshPos.y = -delta + lEyeMeshPos.y;
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            rEyeMeshPos.x = delta + rEyeMeshPos.x;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            rEyeMeshPos.x = -delta + rEyeMeshPos.x;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            rEyeMeshPos.y = delta + rEyeMeshPos.y;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            rEyeMeshPos.y = -delta + rEyeMeshPos.y;
        }

        if (Input.GetKeyDown(KeyCode.Insert))
        {
            rEyeMesh.GetComponent<MxrPredistortionMesh>().sizeX += 0.025f;
            rEyeMesh.GetComponent<MxrPredistortionMesh>().sizeY += 0.025f;
            lEyeMesh.GetComponent<MxrPredistortionMesh>().sizeX += 0.025f;
            lEyeMesh.GetComponent<MxrPredistortionMesh>().sizeY += 0.025f;
            rEyeMesh.GetComponent<MxrPredistortionMesh>().RebuildMesh();
            lEyeMesh.GetComponent<MxrPredistortionMesh>().RebuildMesh();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            rEyeMesh.GetComponent<MxrPredistortionMesh>().sizeX -= 0.025f;
            rEyeMesh.GetComponent<MxrPredistortionMesh>().sizeY -= 0.025f;
            lEyeMesh.GetComponent<MxrPredistortionMesh>().sizeX -= 0.025f;
            lEyeMesh.GetComponent<MxrPredistortionMesh>().sizeY -= 0.025f;
            rEyeMesh.GetComponent<MxrPredistortionMesh>().RebuildMesh();
            lEyeMesh.GetComponent<MxrPredistortionMesh>().RebuildMesh();
        }

        rEyeMesh.localPosition = rEyeMeshPos;
        lEyeMesh.localPosition = lEyeMeshPos;
    }

    private void UpdateCameraSettings() {
        if (isStereo) {
            //set up FOV
            cameraLeft.GetComponent<Camera>().fieldOfView = verticalFOV;
            cameraRight.GetComponent<Camera>().fieldOfView = verticalFOV;

            //aspectRatio = Mathf.Tan((horizontalFOV * Mathf.Deg2Rad) / 2) / Mathf.Tan((verticalFOV * Mathf.Deg2Rad) / 2);

            //set up aspect ratio
            cameraLeft.aspect = aspectRatio;
            cameraRight.aspect = aspectRatio;

            //set up eye separation
            cameraLeft.transform.localPosition = new Vector3(-interaxialDistance * 0.5f, 0.0f, 0.0f);
            cameraRight.transform.localPosition = new Vector3(interaxialDistance * 0.5f, 0.0f, 0.0f);
        }
        else {
            //set up FOV
            cameraMono.GetComponent<Camera>().fieldOfView = verticalFOV;

            //set up aspect ratio
            cameraMono.aspect = aspectRatio;

            //set up eye separation --> none for mono
            cameraMono.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    void OnGUI()
    {
        if (displayValues) {
            GUI.Label(new Rect(10, 10, 200, 20), "Interaxial distance:" + interaxialDistance.ToString("F4"));
            GUI.Label(new Rect(10, 30, 200, 20), "Vertical FOV:" + verticalFOV.ToString("F3"));
            //GUI.Label(new Rect(10, 30, 200, 20), "horizAngle:" + horizontalFOV.ToString("F3"));
            GUI.Label(new Rect(10, 50, 200, 20), "Aspect ratio:" + aspectRatio.ToString("F3"));
            GUI.Label(new Rect(10, 70, 200, 20), "PreDistortion K:" + rEyeMesh.GetComponent<MxrPredistortionMesh>().PreDistortK.ToString("F5"));
            GUI.Label(new Rect(10, 90, 200, 20), "EyeMeshSize:" + rEyeMesh.GetComponent<MxrPredistortionMesh>().sizeX.ToString("F4"));
            GUI.Label(new Rect(10, 110, 400, 20), "rEyeMeshPos:" + rEyeMesh.localPosition.ToString("F4"));
            GUI.Label(new Rect(10, 130, 400, 20), "lEyeMeshPos:" + lEyeMesh.localPosition.ToString("F4"));
        }
         

        //TP: the pixel coordinates of the lower-left corner of this viewport with respect to the entire screen
        Vector2 vwprtBL = Camera.main.ViewportToScreenPoint(new Vector3(0.0f, 0.0f, 0));
        //TP: the pixel coordinates of the upper-right corner of this viewport with respect to the entire screen
        Vector2 vwprtUR = Camera.main.ViewportToScreenPoint(new Vector3(1.0f, 1.0f, 0));
        //TP: the width and height of this viewport in pixels
        Vector2 vwprtWH = vwprtUR - vwprtBL;
    }
}
