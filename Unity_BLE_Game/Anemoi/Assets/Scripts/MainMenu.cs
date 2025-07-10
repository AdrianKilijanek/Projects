using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public Button PlayButton;

    void Start()
    {
        
    }


 void Update()
    {
        UpdatePlayButton();
    }

    void UpdatePlayButton()
    {
        if (BluetoothManager.Instance != null)
        {
            PlayButton.interactable = BluetoothManager.Instance.isConnected;
        }
        else
        {
            PlayButton.interactable = false; 
        }
    }

        public void OnExitButton()
    {

        // Step 2: Exit the game
        ExitApplication();
    }

    public void OnConectionMenuButton(){
        SceneManager.LoadScene(1);
    }
        public void OnPlayButton(){
        SceneManager.LoadScene(2);
    }

        private void ExitApplication()
    {
        #if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("finish"); // Finish the current activity (exiting the game)
            System.Environment.Exit(0);
        #else
            Application.Quit();
        #endif

        Debug.Log("Exiting the game...");
    }


}
