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
        this.initStereoCamera();
    }

    public virtual void initStereoCamera()
    {
        this.SetupCameras();
        this.SetStereoFormat();
    }

    public virtual void SetupCameras()
    {
        Transform lcam = this.transform.Find("leftCam"); // check if we've already created a leftCam
        if (lcam)
        {
            this.leftCam = lcam.gameObject;
            this.leftCam.GetComponent<Camera>().CopyFrom(this.GetComponent<Camera>());
        }
        else
        {
            this.leftCam = new GameObject("leftCam", new System.Type[] {typeof(Camera)});
            this.leftCam.AddComponent(typeof(GUILayer));
            this.leftCam.GetComponent<Camera>().CopyFrom(this.GetComponent<Camera>());
            this.leftCam.transform.parent = this.transform;
        }
        Transform rcam = this.transform.Find("rightCam"); // check if we've already created a rightCam
        if (rcam)
        {
            this.rightCam = rcam.gameObject;
            this.rightCam.GetComponent<Camera>().CopyFrom(this.GetComponent<Camera>());
        }
        else
        {
            this.rightCam = new GameObject("rightCam", new System.Type[] {typeof(Camera)});
            this.rightCam.AddComponent(typeof(GUILayer));
            this.rightCam.GetComponent<Camera>().CopyFrom(this.GetComponent<Camera>());
            this.rightCam.transform.parent = this.transform;
        }
        Transform mcam = this.transform.Find("maskCam"); // check if we've already created a maskCam
        if (mcam)
        {
            this.maskCam = mcam.gameObject;
        }
        else
        {
            this.maskCam = new GameObject("maskCam", new System.Type[] {typeof(Camera)});
            this.maskCam.AddComponent(typeof(GUILayer));
            this.maskCam.GetComponent<Camera>().CopyFrom(this.GetComponent<Camera>());
            this.maskCam.transform.parent = this.transform;
        }
        this.GetComponent<Camera>().depth = -2; // rendering order (back to front): Main Camera/maskCam/leftCam/rightCam
        this.maskCam.GetComponent<Camera>().depth = this.GetComponent<Camera>().depth + 1;
        this.leftCam.GetComponent<Camera>().depth = this.GetComponent<Camera>().depth + 2;
        this.rightCam.GetComponent<Camera>().depth = this.GetComponent<Camera>().depth + 3;
        this.maskCam.GetComponent<Camera>().cullingMask = 0; // nothing shows in mask layer
        this.maskCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        this.maskCam.GetComponent<Camera>().backgroundColor = Color.black;
    }

    public virtual void SetStereoFormat()
    {
        if (!this.usePhoneMask)
        {
            this.leftCam.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
            this.rightCam.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
        }
        else
        {
            this.leftCam.GetComponent<Camera>().rect = this.Vector4toRect(this.leftViewRect);
            this.rightCam.GetComponent<Camera>().rect = this.Vector4toRect(this.rightViewRect);
        }
        this.leftViewRect = this.RectToVector4(this.leftCam.GetComponent<Camera>().rect);
        this.rightViewRect = this.RectToVector4(this.rightCam.GetComponent<Camera>().rect);
        this.fixCameraAspect();
        this.maskCam.GetComponent<Camera>().enabled = this.usePhoneMask;
    }

    public virtual void fixCameraAspect()
    {
        this.GetComponent<Camera>().ResetAspect();
        this.GetComponent<Camera>().aspect = this.GetComponent<Camera>().aspect * ((this.leftCam.GetComponent<Camera>().rect.width * 2) / this.leftCam.GetComponent<Camera>().rect.height);
        this.leftCam.GetComponent<Camera>().aspect = this.GetComponent<Camera>().aspect;
        this.rightCam.GetComponent<Camera>().aspect = this.GetComponent<Camera>().aspect;
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
            this.GetComponent<Camera>().enabled = false;
        }
        else
        {
            this.GetComponent<Camera>().enabled = true; // need camera enabled when in edit mode
        }
        if (Application.isPlaying)
        {
            if (!this.initialized)
            {
                this.initialized = true;
            }
        }
        else
        {
            this.initialized = false;
            this.SetStereoFormat();
        }
        this.UpdateView();
    }

    public virtual void UpdateView()
    {
        switch (this.cameraSelect)
        {
            case cams_3D.LeftRight:
                this.leftCam.transform.position = this.transform.position + this.transform.TransformDirection(-this.interaxial / 2000f, 0, 0);
                this.rightCam.transform.position = this.transform.position + this.transform.TransformDirection(this.interaxial / 2000f, 0, 0);
                break;
            case cams_3D.LeftOnly:
                this.leftCam.transform.position = this.transform.position + this.transform.TransformDirection(-this.interaxial / 2000f, 0, 0);
                this.rightCam.transform.position = this.transform.position + this.transform.TransformDirection(-this.interaxial / 2000f, 0, 0);
                break;
            case cams_3D.RightOnly:
                this.leftCam.transform.position = this.transform.position + this.transform.TransformDirection(this.interaxial / 2000f, 0, 0);
                this.rightCam.transform.position = this.transform.position + this.transform.TransformDirection(this.interaxial / 2000f, 0, 0);
                break;
            case cams_3D.RightLeft:
                this.leftCam.transform.position = this.transform.position + this.transform.TransformDirection(this.interaxial / 2000f, 0, 0);
                this.rightCam.transform.position = this.transform.position + this.transform.TransformDirection(-this.interaxial / 2000f, 0, 0);
                break;
        }
        this.leftCam.transform.rotation = this.transform.rotation;
        this.rightCam.transform.rotation = this.transform.rotation;
        switch (this.cameraSelect)
        {
            case cams_3D.LeftRight:
                this.leftCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(true);
                this.rightCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(false);
                break;
            case cams_3D.LeftOnly:
                this.leftCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(true);
                this.rightCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(true);
                break;
            case cams_3D.RightOnly:
                this.leftCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(false);
                this.rightCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(false);
                break;
            case cams_3D.RightLeft:
                this.leftCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(false);
                this.rightCam.GetComponent<Camera>().projectionMatrix = this.setProjectionMatrix(true);
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
        float tempAspect = this.GetComponent<Camera>().aspect;
        FOVrad = (this.GetComponent<Camera>().fieldOfView / 180f) * Mathf.PI;
        if (!this.sideBySideSqueezed)
        {
            tempAspect = tempAspect / 2; // if side by side unsqueezed, double width
        }
        a = this.GetComponent<Camera>().nearClipPlane * Mathf.Tan(FOVrad * 0.5f);
        b = this.GetComponent<Camera>().nearClipPlane / this.zeroPrlxDist;
        if (isLeftCam)
        {
            left = (((-tempAspect * a) + ((this.interaxial / 2000f) * b)) + (this.H_I_T / 100)) + (this.offAxisFrustum / 100);
            right = (((tempAspect * a) + ((this.interaxial / 2000f) * b)) + (this.H_I_T / 100)) + (this.offAxisFrustum / 100);
        }
        else
        {
            left = (((-tempAspect * a) - ((this.interaxial / 2000f) * b)) - (this.H_I_T / 100)) + (this.offAxisFrustum / 100);
            right = (((tempAspect * a) - ((this.interaxial / 2000f) * b)) - (this.H_I_T / 100)) + (this.offAxisFrustum / 100);
        }
        return this.PerspectiveOffCenter(left, right, -a, a, this.GetComponent<Camera>().nearClipPlane, this.GetComponent<Camera>().farClipPlane);
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
        Vector3 gizmoLeft = this.transform.position + this.transform.TransformDirection(-this.interaxial / 2000f, 0, 0);
        Vector3 gizmoRight = this.transform.position + this.transform.TransformDirection(this.interaxial / 2000f, 0, 0);
        Vector3 gizmoTarget = this.transform.position + (this.transform.TransformDirection(Vector3.forward) * this.zeroPrlxDist);
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
        this.interaxial = 65;
        this.zeroPrlxDist = 3f;
        this.cameraSelect = cams_3D.LeftRight;
        this.usePhoneMask = true;
        this.leftViewRect = new Vector4(0, 0, 0.5f, 1);
        this.rightViewRect = new Vector4(0.5f, 0, 1, 1);
    }

}