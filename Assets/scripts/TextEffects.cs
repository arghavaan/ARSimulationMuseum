using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEffects : MonoBehaviour
{
    public Transform cameraTransform;
    public float DistanceToView = 250f;
    private TextMesh myTextMesh;
    private string MyOriginalText;
    private bool wasVisible;
    private Renderer myRenderer;
    private float defaultRotationY;
    private Quaternion defaultRotation;
    private bool doFaceToCamera = false;
    private bool doFaceToCameraActive = false;
    private float oldFaceAngle = -1000;

    void Start()
    {
        myTextMesh = GetComponent<TextMesh>();
        MyOriginalText = myTextMesh.text;
        cameraTransform = Camera.main.transform;
        myRenderer = GetComponent<Renderer>();
        wasVisible = false;
        defaultRotation = myTextMesh.transform.rotation;
        defaultRotationY = myTextMesh.transform.eulerAngles.y;
    }

    void FadeIn()
    {
        StopCoroutine("FadeTo");
        StartCoroutine("FadeTo", 1);
    }

    void FadeOut()
    {
        StopCoroutine("FadeTo");
        StartCoroutine("FadeTo", 0.2);
    }
    IEnumerator faceToCameraQ(Quaternion q)
    {
        var duration = 0.2f;

        var startRotation = myTextMesh.transform.rotation;
        var targetRotation = q;
        float t = 0;
        
        while (t < duration)
        {
            // Step the fade forward one frame.
            t += Time.deltaTime;

            float blend = Mathf.Clamp01(t / duration);

            myTextMesh.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, blend);
            
            // Wait one frame, and repeat.
            yield return null;
        }

        doFaceToCameraActive = false;
    }

    IEnumerator faceToCamera(float yRot)
    {
        var duration = 0.2f;

        float startRotation = myTextMesh.transform.eulerAngles.y;
        float targetRotation = yRot;
        float t = 0;
        var r = myTextMesh.transform.eulerAngles;

        while (t < duration)
        {
            // Step the fade forward one frame.
            t += Time.deltaTime;
            // Turn the time into an interpolation factor between 0 and 1.
            float blend = Mathf.Clamp01(t / duration);

            // Blend to the corresponding opacity between start & target.
            r = myTextMesh.transform.eulerAngles;
            var xr = Mathf.Round(Mathf.Lerp(startRotation, targetRotation, blend) * 10) / 10.0f;
            //while (xr > 180) xr -= 360;
            //while (xr < -180) xr += 360;
            r.y = xr;
            myTextMesh.transform.eulerAngles = r;

            // Wait one frame, and repeat.
            yield return null;
        }

        doFaceToCameraActive = false;
    }

    IEnumerator FadeTo(float targetOpacity)
    {
        var duration = 0.3f;
        //var material = m_Renderer.material;
        // Cache the current color of the material, and its initiql opacity.
        Color color = myTextMesh.color;
        float startOpacity = color.a;

        // Track how many seconds we've been fading.
        float t = 0;

        float startFontSize = myTextMesh.fontSize;
        float targetFontSize = (float)((targetOpacity > 0.9 ? 1.6 : 0.625) * startFontSize);

        while (t < duration)
        {
            // Step the fade forward one frame.
            t += Time.deltaTime;
            // Turn the time into an interpolation factor between 0 and 1.
            float blend = Mathf.Clamp01(t / duration);

            // Blend to the corresponding opacity between start & target.
            color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);
            myTextMesh.fontSize = (int)Mathf.Lerp(startFontSize, targetFontSize, blend);
            // Apply the resulting color to the material.
            myTextMesh.color = color;

            // Wait one frame, and repeat.
            yield return null;
        }

        doFaceToCamera = targetOpacity > 0.9;
    }

    private bool isCameraAndTextAligned()
    {
        var camToText = myTextMesh.transform.position - cameraTransform.position;
        
        camToText.y = 0;
        var angle = Vector3.Angle(camToText, cameraTransform.forward);
        var isAligned = Mathf.Abs(angle) < 20;
        if (isAligned)
        {
            if (doFaceToCamera)
            {
                if (!doFaceToCameraActive)
                {
                    doFaceToCameraActive = true;

                    var angle2 = Vector3.Angle(camToText, myTextMesh.transform.forward);
                    var angErr = Mathf.Abs(angle2);
                    if (angErr > 10)
                    {
                        oldFaceAngle = angle2;
                        StopCoroutine("faceToCameraQ");

                        Quaternion targetRotation = Quaternion.LookRotation(myTextMesh.transform.position - cameraTransform.position);
                        StartCoroutine("faceToCameraQ", targetRotation);
                        //oldFaceAngle = angle2;
                        //StopCoroutine("faceToCamera");
                        //
                        //if (angle2 > 180) angle2 -= 360;
                        //if (angle2 < -180) angle2 += 360;
                        //
                        //var oy = myTextMesh.transform.eulerAngles.y;
                        //
                        //bool changed = false;
                        //if (Mathf.Abs(oy - 180) < 0.1) { changed = true; oy -= 0.2f; }
                        //if (Mathf.Abs(oy + 180) < 0.1) { changed = true; oy += 0.2f; }
                        //
                        //var ang = myTextMesh.transform.eulerAngles.y + angle2;
                        //
                        ////while (ang > 180) ang -= 360;
                        ////while (ang < -180) ang += 360;
                        //
                        //Debug.Log(myTextMesh.transform.eulerAngles.y + " + " + angle2 + " = " + ang);
                        //
                        //if (changed)
                        //{
                        //    var ea = myTextMesh.transform.eulerAngles;
                        //    ea.y = oy;
                        //    myTextMesh.transform.eulerAngles = ea;
                        //}
                        //
                        //StartCoroutine("faceToCamera", ang);
                    }
                    else
                    {
                        doFaceToCameraActive = false;
                    }
                }
            }
        }
        else if (oldFaceAngle  > -999)
        {
            oldFaceAngle = -1000;
            //StopCoroutine("faceToCamera");
            //StartCoroutine("faceToCamera", defaultRotationY);
            StopCoroutine("faceToCameraQ");
            StartCoroutine("faceToCameraQ", defaultRotation);
        }
        return isAligned;
    }


    void Update()
    {
        var diff = cameraTransform.position - myTextMesh.transform.position;
        diff.y = 0;

        if (diff.magnitude <= DistanceToView)
        {
            myTextMesh.text = MyOriginalText;
        }
        else
        {
            myTextMesh.text = "";
        }

        if (isCameraAndTextAligned())
        {
            if (!wasVisible)
            {
                //.Log("Object is visible to camera");
                FadeIn();
                wasVisible = true;
            }
        }
        else
        {
            if (wasVisible)
            {
                //Debug.Log("Object is no longer visible to camera");
                wasVisible = false;
                FadeOut();
            }
        }

    }
}
