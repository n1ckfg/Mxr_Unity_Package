using UnityEngine;
using System.Collections;

/* s3dCameraSBS.js - revised 2/12/13
 * Please direct any bugs/comments/suggestions to hoberman@usc.edu.
 * FOV2GO for Unity Copyright (c) 2011-13 Perry Hoberman & MxR Lab. All rights reserved.
 * Usage: Attach to camera. Creates, manages and renders side-by-side stereoscopic view.
 * NOTE: interaxial is measured in millimeters; zeroPrlxDist is measured in meters 
 * Has companion Editor script (s3dCameraSBSEditor.js) for custom inspector 
 */
// Cameras
 // left view camera
 // right view camera
 // black mask for mobile formats
// Stereo Parameters
 // Distance (in millimeters) between cameras
 // Distance (in meters) at which left and right images overlap exactly
// View Parameters
public enum cams_3D
{
    LeftRight = 0,
    LeftOnly = 1,
    RightOnly = 2,
    RightLeft = 3
}

[System.Serializable]
 // View order - swap cameras for cross-eyed free-viewing
 // Horizontal Image Transform - shift left and right image horizontally
 // Esoteric parameter
// Side by Side Parameters
 // 50% horizontal scale for 3DTVs
 // Mask for mobile formats
 // left, bottom, width, height
// Initialization
[UnityEngine.AddComponentMenu("Stereoskopix/s3d Camera S3D")]
[UnityEngine.ExecuteInEditMode]
public partial class s3dCameraSBS : MonoBehaviour
{
    public GameObject leftCam;
    public GameObject rightCam;
    public GameObject maskCam;
    public float interaxial;
    public float zeroPrlxDist;
    public cams_3D cameraSelect;
    public float H_I_T;
    public float offAxisFrustum;
    public bool sideBySideSqueezed;
    public bool usePhoneMask;
    public Vector4 leftViewRect;
    public Vector4 rightViewRect;
    private bool initialized;
    public virtual void Awake()
    {
        initStereoCamera();
    }

    public virtual void initStereoCamera()
    {
        SetupCameras();
        SetStereoFormat();
    }

    public virtual void SetupCameras()
    {
        Transform lcam = transform.Find("leftCam"); // check if we've already created a leftCam
        if (lcam)
        {
            leftCam = lcam.gameObject;
            leftCam.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
        }
        else
        {
            leftCam = new GameObject("leftCam", new System.Type[] {typeof(Camera)});
            leftCam.AddComponent(typeof(GUILayer));
            leftCam.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
            leftCam.transform.parent = transform;
        }
        Transform rcam = transform.Find("rightCam"); // check if we've already created a rightCam
        if (rcam)
        {
            rightCam = rcam.gameObject;
            rightCam.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
        }
        else
        {
            rightCam = new GameObject("rightCam", new System.Type[] {typeof(Camera)});
            rightCam.AddComponent(typeof(GUILayer));
            rightCam.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
            rightCam.transform.parent = transform;
        }
        Transform mcam = transform.Find("maskCam"); // check if we've already created a maskCam
        if (mcam)
        {
            maskCam = mcam.gameObject;
        }
        else
        {
            maskCam = new GameObject("maskCam", new System.Type[] {typeof(Camera)});
            maskCam.AddComponent(typeof(GUILayer));
            maskCam.GetComponent<Camera>().CopyFrom(GetComponent<Camera>());
            maskCam.transform.parent = transform;
        }
        GetComponent<Camera>().depth = -2; // rendering order (back to front): Main Camera/maskCam/leftCam/rightCam
        maskCam.GetComponent<Camera>().depth = GetComponent<Camera>().depth + 1;
        leftCam.GetComponent<Camera>().depth = GetComponent<Camera>().depth + 2;
        rightCam.GetComponent<Camera>().depth = GetComponent<Camera>().depth + 3;
        maskCam.GetComponent<Camera>().cullingMask = 0; // nothing shows in mask layer
        maskCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        maskCam.GetComponent<Camera>().backgroundColor = Color.black;
    }

    public virtual void SetStereoFormat()
    {
        if (!usePhoneMask)
        {
            leftCam.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
            rightCam.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
        }
        else
        {
            leftCam.GetComponent<Camera>().rect = Vector4toRect(leftViewRect);
            rightCam.GetComponent<Camera>().rect = Vector4toRect(rightViewRect);
        }
        leftViewRect = RectToVector4(leftCam.GetComponent<Camera>().rect);
        rightViewRect = RectToVector4(rightCam.GetComponent<Camera>().rect);
        fixCameraAspect();
        maskCam.GetComponent<Camera>().enabled = usePhoneMask;
    }

    public virtual void fixCameraAspect()
    {
        GetComponent<Camera>().ResetAspect();
        GetComponent<Camera>().aspect = GetComponent<Camera>().aspect * ((leftCam.GetComponent<Camera>().rect.width * 2) / leftCam.GetComponent<Camera>().rect.height);
        leftCam.GetComponent<Camera>().aspect = GetComponent<Camera>().aspect;
        rightCam.GetComponent<Camera>().aspect = GetComponent<Camera>().aspect;
    }

    public virtual Rect Vector4toRect(Vector4 v)
    {
        Rect r = new Rect(v.x, v.y, v.z, v.w);
        return r;
    }

    public virtual Vector4 RectToVector4(Rect r)
    {
        Vector4 v = new Vector4(r.x, r.y, r.width, r.height);
        return v;
    }

    public virtual void Update()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            GetComponent<Camera>().enabled = false;
        }
        else
        {
            GetComponent<Camera>().enabled = true; // need camera enabled when in edit mode
        }
        if (Application.isPlaying)
        {
            if (!initialized)
            {
                initialized = true;
            }
        }
        else
        {
            initialized = false;
            SetStereoFormat();
        }
        UpdateView();
    }

    public virtual void UpdateView()
    {
        switch (cameraSelect)
        {
            case cams_3D.LeftRight:
                leftCam.transform.position = transform.position + transform.TransformDirection(-interaxial / 2000f, 0, 0);
                rightCam.transform.position = transform.position + transform.TransformDirection(interaxial / 2000f, 0, 0);
                break;
            case cams_3D.LeftOnly:
                leftCam.transform.position = transform.position + transform.TransformDirection(-interaxial / 2000f, 0, 0);
                rightCam.transform.position = transform.position + transform.TransformDirection(-interaxial / 2000f, 0, 0);
                break;
            case cams_3D.RightOnly:
                leftCam.transform.position = transform.position + transform.TransformDirection(interaxial / 2000f, 0, 0);
                rightCam.transform.position = transform.position + transform.TransformDirection(interaxial / 2000f, 0, 0);
                break;
            case cams_3D.RightLeft:
                leftCam.transform.position = transform.position + transform.TransformDirection(interaxial / 2000f, 0, 0);
                rightCam.transform.position = transform.position + transform.TransformDirection(-interaxial / 2000f, 0, 0);
                break;
        }
        leftCam.transform.rotation = transform.rotation;
        rightCam.transform.rotation = transform.rotation;
        switch (cameraSelect)
        {
            case cams_3D.LeftRight:
                leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
                rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
                break;
            case cams_3D.LeftOnly:
                leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
                rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
                break;
            case cams_3D.RightOnly:
                leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
                rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
                break;
            case cams_3D.RightLeft:
                leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
                rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
                break;
        }
    }

    public virtual Matrix4x4 setProjectionMatrix(bool isLeftCam)
    {
        float left = 0.0f;
        float right = 0.0f;
        float a = 0.0f;
        float b = 0.0f;
        float FOVrad = 0.0f;
        float tempAspect = GetComponent<Camera>().aspect;
        FOVrad = (GetComponent<Camera>().fieldOfView / 180f) * Mathf.PI;
        if (!sideBySideSqueezed)
        {
            tempAspect = tempAspect / 2; // if side by side unsqueezed, double width
        }
        a = GetComponent<Camera>().nearClipPlane * Mathf.Tan(FOVrad * 0.5f);
        b = GetComponent<Camera>().nearClipPlane / zeroPrlxDist;
        if (isLeftCam)
        {
            left = (((-tempAspect * a) + ((interaxial / 2000f) * b)) + (H_I_T / 100)) + (offAxisFrustum / 100);
            right = (((tempAspect * a) + ((interaxial / 2000f) * b)) + (H_I_T / 100)) + (offAxisFrustum / 100);
        }
        else
        {
            left = (((-tempAspect * a) - ((interaxial / 2000f) * b)) - (H_I_T / 100)) + (offAxisFrustum / 100);
            right = (((tempAspect * a) - ((interaxial / 2000f) * b)) - (H_I_T / 100)) + (offAxisFrustum / 100);
        }
        return PerspectiveOffCenter(left, right, -a, a, GetComponent<Camera>().nearClipPlane, GetComponent<Camera>().farClipPlane);
    }

    public virtual Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4x4 m = default(Matrix4x4);
        float x = (2f * near) / (right - left);
        float y = (2f * near) / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -((2f * far) * near) / (far - near);
        float e = -1f;
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }

    public virtual void OnDrawGizmos()
    {
        Vector3 gizmoLeft = transform.position + transform.TransformDirection(-interaxial / 2000f, 0, 0);
        Vector3 gizmoRight = transform.position + transform.TransformDirection(interaxial / 2000f, 0, 0);
        Vector3 gizmoTarget = transform.position + (transform.TransformDirection(Vector3.forward) * zeroPrlxDist);
        Gizmos.color = new Color(1, 1, 1, 1);
        Gizmos.DrawLine(gizmoLeft, gizmoTarget);
        Gizmos.DrawLine(gizmoRight, gizmoTarget);
        Gizmos.DrawLine(gizmoLeft, gizmoRight);
        Gizmos.DrawSphere(gizmoLeft, 0.02f);
        Gizmos.DrawSphere(gizmoRight, 0.02f);
        Gizmos.DrawSphere(gizmoTarget, 0.02f);
    }

    public s3dCameraSBS()// end s3dCameraSBS.js
    {
        interaxial = 65;
        zeroPrlxDist = 3f;
        cameraSelect = cams_3D.LeftRight;
        usePhoneMask = true;
        leftViewRect = new Vector4(0, 0, 0.5f, 1);
        rightViewRect = new Vector4(0.5f, 0, 1, 1);
    }

}