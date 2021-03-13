using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Camera mainCamera;
    public RawImage miniMapRawImage;
    public bool EnableKeypadRotation;
    public float headDownThreshold = -0.65f;
    public float speed = 120.0f;
    public float rotationSpeed = 100.0f;
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;
    //private bool isMiniMapActive = false;
    
    private bool wasHeadUp;
    private enum MapShowModes { Hidden, Mini, Center };
    private MapShowModes mapShowMode = MapShowModes.Hidden;
    private Queue<DateTime> headUpDownTimes = new Queue<DateTime>();
    
    // Start is called before the first frame update
    private void Start()
    {
        miniMapRawImage.gameObject.SetActive(false);
        wasHeadUp = transform.forward.y > headDownThreshold;
    }

    private void setMapShowMode(MapShowModes mode)
    {
        if (mode == mapShowMode) return;
        if(mode == MapShowModes.Hidden)
        {
            miniMapRawImage.gameObject.SetActive(false);
        }
        else
        {
            if(mode == MapShowModes.Mini)
            {
                miniMapRawImage.transform.localScale = new Vector3(1, 1, 1);
                float y = (Screen.height - miniMapRawImage.rectTransform.rect.height) * 0.5f - 10f;
                float x = (Screen.width - miniMapRawImage.rectTransform.rect.width) * 0.5f - 10f;
                miniMapRawImage.rectTransform.anchoredPosition = new Vector3(x, y, 0);
            }
            else
            {
                miniMapRawImage.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
                miniMapRawImage.transform.localScale = new Vector3(1, 1, 1) * 1.5f;
            }
            miniMapRawImage.gameObject.SetActive(true);
        }
        mapShowMode = mode;
    }

    // Update is called once per frame
    private void Update()
    {
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * (EnableKeypadRotation ? rotationSpeed : speed);
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");

        if (transform.forward.y < headDownThreshold)
        {
            if(wasHeadUp)
            {
                wasHeadUp = false;
                if(headUpDownTimes.Count < 2)
                {
                    setMapShowMode(MapShowModes.Center);
                }
                else if (headUpDownTimes.Count > 1)
                {
                    if (DateTime.Now.Subtract(headUpDownTimes.Peek()).TotalSeconds < 2)
                        setMapShowMode(MapShowModes.Hidden);
                }

                headUpDownTimes.Enqueue(DateTime.Now);
                if (headUpDownTimes.Count > 4) 
                    headUpDownTimes.Dequeue();
            }
            //if (!isMiniMapActive)
            //{
            //    isMiniMapActive = true;
            //    miniMapCanvas.SetActive(isMiniMapActive);
            //}
        }
        else
        {
            if (!wasHeadUp)
            {
                wasHeadUp = true;

                if (headUpDownTimes.Count < 3)
                {
                    setMapShowMode(MapShowModes.Hidden);
                }
                else if (headUpDownTimes.Count > 2)
                {
                    if (DateTime.Now.Subtract(headUpDownTimes.Peek()).TotalSeconds < 2.5)
                        setMapShowMode(MapShowModes.Mini);
                }


                headUpDownTimes.Enqueue(DateTime.Now);
                if (headUpDownTimes.Count > 4)
                    headUpDownTimes.Dequeue();
            }
            //if (isMiniMapActive)
            //{
            //    isMiniMapActive = false;
            //    miniMapCanvas.SetActive(isMiniMapActive);
            //}
        }

        if(headUpDownTimes.Count > 0)
        {
            if (DateTime.Now.Subtract(headUpDownTimes.Peek()).TotalSeconds > 3.3)
                headUpDownTimes.Dequeue();
        }

        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        // Move translation along the object's z-axis
        controller.transform.Translate(EnableKeypadRotation ? 0 : rotation, 0, translation);

        // Rotate around our y-axis
        if (EnableKeypadRotation) h += rotation;
        controller.transform.Rotate(0, h, 0);

        transform.Rotate(v, 0, 0);

    }
}
