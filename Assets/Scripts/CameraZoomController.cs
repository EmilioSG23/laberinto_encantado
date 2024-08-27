using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraZoomController : MonoBehaviour{
    public Slider slider;
    public Camera mapCamera;
    public Camera playerCamera;
    private float mapCameraMinValue = 250f;
    private float mapCameraMaxValue = 450f;

    private float playerCameraMinValue = 50f;
    private float playerCameraMaxValue = 350f;
    private Camera actualCamera;
    private bool canUse = false;

    void Start (){
        Navigation noNavigation = new Navigation{
            mode = Navigation.Mode.None
        };
        slider.navigation = noNavigation;

        SwitchToMapCamera();

        slider.onValueChanged.AddListener(OnZoomValueChanged);
    }

    void Update(){
        if (!canUse)
            return;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (actualCamera != null && actualCamera.orthographic){
            if (scrollInput != 0f)
                actualCamera.orthographicSize -= scrollInput * 100f;
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U))
                actualCamera.orthographicSize-= 50f;
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.I))
                actualCamera.orthographicSize+= 50f;

            actualCamera.orthographicSize = Mathf.Clamp(actualCamera.orthographicSize, slider.minValue, slider.maxValue);
            slider.value = actualCamera.orthographicSize;
        }
    }

    public void CanUse (bool canUse){
        this.canUse = canUse;
    }

    public void setCamera(int usingCamera){
        if (usingCamera == 0)
            SwitchToMapCamera();
        else
            SwitchToPlayerCamera();
    }

    private void SwitchToMapCamera(){
        actualCamera = mapCamera;
        slider.minValue = mapCameraMinValue;
        slider.maxValue = mapCameraMaxValue;
        slider.value = 350f;
    }

    private void SwitchToPlayerCamera(){
        actualCamera = playerCamera;
        slider.minValue = playerCameraMinValue;
        slider.maxValue = playerCameraMaxValue;
        slider.value = 200f;
    }

    private void OnZoomValueChanged(float value){
        if (actualCamera == mapCamera)
            mapCamera.orthographicSize = value;
        if (actualCamera == playerCamera)
            playerCamera.orthographicSize = value;
    }
}
