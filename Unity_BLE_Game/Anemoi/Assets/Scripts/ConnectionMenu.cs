using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{


public Canvas canvasToEnable;
public Button calibrateButton;
    void Start()
    {
        calibrateButton = GameObject.Find("CalibrationButton")?.GetComponent<Button>();
    }
    // Update is called once per frame
     void Update()
    {
        UpdateCalibrateButton();
    }

    void UpdateCalibrateButton()
    {
       if (calibrateButton == null)
    {
        Debug.LogError("CalibrateButton is null!"); // Debugging
        return;
    }

    if (BluetoothManager.Instance != null)
    {
        calibrateButton.interactable = BluetoothManager.Instance.isConnected;
    }
    else
    {
        calibrateButton.interactable = false;
    }
    }

    public void OnResetButton(){

PlayerPrefs.DeleteKey("HighScore"); // Deletes only the high score
PlayerPrefs.Save(); // Ensures changes are saved

    }
    public void OnBackButton(){
        SceneManager.LoadScene(0);
    }
    public void SwitchCanvas()
    {
        // Disable the current canvas (the one this script is attached to)
        Canvas currentCanvas = GetComponent<Canvas>();
        if (currentCanvas != null)
        {
            currentCanvas.enabled = false;
        }

        // Enable the referenced canvas
        if (canvasToEnable != null)
        {
            canvasToEnable.enabled = true;
        }
    }

    // Start is called before the first frame update
  

    // creating an instance of the bluetooth class from the plugin 

}

