using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMProEffects : MonoBehaviour
{
    private Transform cameraTransform;

    public bool disableLookDirection;
    public bool changeColorByLevel;
    public bool useParentForCameraAlign;
    public int alignThreshold = 20;
    public string Text_Level1;
    public string Text_Level2;
    public string Text_Level3;
    public string Text_Level4;

    public Color32 Color_Level1 = new Color32(0, 0, 0, 255);
    public Color32 Color_Level2 = new Color32(0, 0, 0, 255);
    public Color32 Color_Level3 = new Color32(33, 148, 166, 255);
    public Color32 Color_Level4 = new Color32(231, 0, 97, 255); 


    private TextMeshProUGUI myTextMesh;
    private enum TextLevels { Hidden, Level1, Level2, Level3, Level4 }
    private TextLevels textLevel = TextLevels.Hidden;

    private bool? wasVisible;
    private Quaternion defaultRotation;
    private bool doFaceToCamera = false;
    private bool doFaceToCameraActive = false;
    private float oldFaceAngle = -1000;
    private Color32 defaultColor;
    private Transform refTransform;
    private bool isFadedOut = false;
    private float original_font_size;

    void Start()
    {
        myTextMesh = GetComponent<TextMeshProUGUI>();
        refTransform = useParentForCameraAlign ? myTextMesh.transform.parent : myTextMesh.transform;
        defaultColor = myTextMesh.color;
        cameraTransform = Camera.main.transform;
        wasVisible = null;
        textLevel = TextLevels.Hidden;
        myTextMesh.text = "";
        defaultRotation = refTransform.rotation;
        original_font_size = myTextMesh.fontSize;
    }

    void FadeIn()
    {
        isFadedOut = false;
        StopCoroutine("FadeTo");
        StartCoroutine("FadeTo", 1);
    }

    void FadeOut()
    {
        isFadedOut = true;
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


    IEnumerator FadeTo(float targetOpacity)
    {
        var duration = 0.3f;
        
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
            //myTextMesh.fontSize = (int)Mathf.Lerp(startFontSize, targetFontSize, blend);
            // Apply the resulting color to the material.
            myTextMesh.color = color;

            // Wait one frame, and repeat.
            yield return null;
        }

        doFaceToCamera = targetOpacity > 0.9;
    }

    private bool isCameraAndTextAligned()
    {
        var camToText = refTransform.position - cameraTransform.position;
        
        camToText.y = 0;
        var angle = Vector3.Angle(camToText, cameraTransform.forward);
        var isAligned = Mathf.Abs(angle) < alignThreshold;

        if (!disableLookDirection && !isFadedOut)
        {

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
                            var ld = myTextMesh.transform.position - cameraTransform.position;

                            ld.y = 0;

                            Quaternion targetRotation = Quaternion.LookRotation(ld);
                            
                            StartCoroutine("faceToCameraQ", targetRotation);
                        }
                        else
                        {
                            doFaceToCameraActive = false;
                        }
                    }
                }
            }
            else if (oldFaceAngle > -999)
            {
                oldFaceAngle = -1000;
                StopCoroutine("faceToCameraQ");
                StartCoroutine("faceToCameraQ", defaultRotation);
            }
        }

        return isAligned;
    }

    
    void updateLevel(TextLevels level)
    {
        if (level == textLevel) return;
        if (isFadedOut && level != TextLevels.Hidden) return;

        textLevel = level;

        var txt = "";


        if (level == TextLevels.Level1)
        {
            txt = Text_Level1;
            if(txt.Trim().Length == 1 || txt.Equals("...") || txt.Equals("[!]") || txt.Equals("(!)") || txt.Equals("[...]"))
            {
                original_font_size = myTextMesh.fontSize;
                myTextMesh.fontSize = original_font_size * 5;
            }
        }
        else
        {

            myTextMesh.fontSize = original_font_size;
            if (level == TextLevels.Level2) txt = Text_Level2;
            else if (level == TextLevels.Level3) txt = Text_Level3;
            else if (level == TextLevels.Level4) txt = Text_Level4;
        }

        txt = txt.Replace("\\r", "").Replace("\\n", "\r\n");

        if(changeColorByLevel)
        {
            var c = defaultColor;

            if (level == TextLevels.Level1) c = Color_Level1;
            else if (level == TextLevels.Level2) c = Color_Level2;
            else if (level == TextLevels.Level3) c = Color_Level3;
            else if (level == TextLevels.Level4) c = Color_Level4;

            myTextMesh.color = c;
        }

        myTextMesh.text = /*
            level.ToString() + "\r\n" + // */
            txt;
    }

    void Update()
    {
        var diff = cameraTransform.position - refTransform.position;
        diff.y = 0;

        var scale = 2.5f;
        var distance = diff.magnitude * scale;
        
        var lvl = TextLevels.Hidden;

        if (distance < 200) lvl = TextLevels.Level4;
        else if (distance < 250) lvl = TextLevels.Level3;
        else if (distance < 400) lvl = TextLevels.Level2;
        else if (distance < 650) lvl = TextLevels.Level1;
        
        updateLevel(lvl);

        if (isCameraAndTextAligned())
        {
            if (wasVisible.HasValue && !wasVisible.Value)
            {
                FadeIn();
                wasVisible = true;
            }
        }
        else
        {
            if (!wasVisible.HasValue || wasVisible.Value)
            {
                wasVisible = false;
                FadeOut();
            }
        }

    }
}
