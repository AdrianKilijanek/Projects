using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BluetoothManager : MonoBehaviour
{
    public static BluetoothManager Instance {get; private set; }
    public bool readData = false;
    private float speed;
    private float angle;

    private int forceMultiplier = 1;

    public string macAddress = "DD:12:25:F6:6E:94";
    public Text deviceAdd;
    public Text dataToSend;
    public Text receivedData;
    public TextMeshProUGUI receivedDataSpeed;
    public TextMeshProUGUI receivedDataAngle;
    public GameObject devicesListContainer;
    public GameObject deviceMACText;
    public bool isConnected;
    public bool connectedAndReady = false;

    private static AndroidJavaClass AnemoUnityBtPlugin;
    private static AndroidJavaObject BluetoothConnection;
    // Start is called before the first frame update
    void Start()
    {
        InitBluetooth();
        isConnected = false;
    }

        void Awake()
    {
        // Ensures only one instance of BLEManager exists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
    }
    void Update(){

    }

    // creating an instance of the bluetooth class from the plugin 
    public void InitBluetooth()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // Check BT and location permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
            || !Permission.HasUserAuthorizedPermission(Permission.FineLocation)
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {

            Permission.RequestUserPermissions(new string[] {
                        Permission.CoarseLocation,
                            Permission.FineLocation,
                            "android.permission.BLUETOOTH_ADMIN",
                            "android.permission.BLUETOOTH",
                            "android.permission.BLUETOOTH_SCAN",
                            "android.permission.BLUETOOTH_ADVERTISE",
                             "android.permission.BLUETOOTH_CONNECT"
                    });

        }

        AnemoUnityBtPlugin = new AndroidJavaClass("com.example.anemobtconnection.BluetoothConnection");
        BluetoothConnection = AnemoUnityBtPlugin.CallStatic<AndroidJavaObject>("getInstance");
    }

    // Start device scan
    public void StartScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // Destroy devicesListContainer child objects for new scan display
        foreach (Transform child in devicesListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        BluetoothConnection.CallStatic("StartScanDevices");
    }

    // Stop device scan
    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnection.CallStatic("StopScanDevices");
    }

    // This function will be called by Java class to update the scan status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ScanStatus(string status)
    {
        Toast("Scan Status: " + status);
    }

    // This function will be called by Java class whenever a new device is found,
    // and delivers the new devices as a string data="MAC+NAME"
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void NewDeviceFound(string data)
    {
        GameObject newDevice = deviceMACText;
        newDevice.GetComponent<Text>().text = data;
        Instantiate(newDevice, devicesListContainer.transform);  
    }

    // Get paired devices from BT settings
    public void GetPairedDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // This function when called returns an array of PairedDevices as "MAC+Name" for each device found
        string[] data = BluetoothConnection.CallStatic<string[]>("GetPairedDevices"); ;

        // Destroy devicesListContainer child objects for new Paired Devices display
        foreach (Transform child in devicesListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Display the paired devices
        foreach (var d in data)
        {
            GameObject newDevice = deviceMACText;
            newDevice.GetComponent<Text>().text = d;
            Instantiate(newDevice, devicesListContainer.transform);
        }
    }

    public float GetSpeed(){
        return speed;
    }
    public float GetAngle(){
        return angle;
    }
    public float GetForce(){
        return forceMultiplier;
    }

    public void AutoConnectionButton(){
        Toast("YAutoConnection!");
    StartCoroutine(AutoConnection());
        //AutoConnection();
    }
    public IEnumerator AutoConnection(){
        StartConnectionAuto();
        yield return new WaitForSeconds(15);
        ReadDataButton();
        if(isConnected && connectedAndReady){
            
        Toast("You are ready to play!");
  

        }
    }
    // Start BT connect using device MAC address "deviceAdd"
    public void StartConnection()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
        BluetoothConnection.CallStatic("StartConnection", deviceAdd.text.ToString().ToUpper());
    }
        public void StartConnectionAuto()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
        BluetoothConnection.CallStatic("StartConnection", macAddress);
    }

    // Stop BT connetion
    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (isConnected)
            BluetoothConnection.CallStatic("StopConnection");
    }

    // This function will be called by Java class to update BT connection status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ConnectionStatus(string status)
    {
        Toast("Connection Status: " + status);
        isConnected = status == "connected";
    }
    public void ReadDataButton()
{
    if (Application.platform != RuntimePlatform.Android)
        return;

    if (!readData) {
        readData = true;
        BluetoothConnection.CallStatic("ReadSpeed"); // Start the loop by reading Speed
    } else {
        readData = false; // Stop reading
    }
}

        public void DiscoverButton()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnection.CallStatic("DiscoverMethod");
    }
    // This function will be called by Java class whenever BT data is received,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadData(string data )
    {
        Debug.Log("BT Stream: " + data);
        receivedData.text = data;
    }

        public void ReadDataSpeed(string speedCharacteristic)
    {
        //Debug.Log("BT Stream Speed: " + data);
        byte[] byteArray = System.Convert.FromBase64String(speedCharacteristic);
        float dataReceived = SwapBytesAndConvertToDecimal(byteArray, 100);
        speed = dataReceived;
        string speedToString = "Speed: "+dataReceived.ToString();
        receivedDataSpeed.text = speedToString;
        
    }
        public void ReadDataAngle(string angleCharacteristic)
    {
        //Debug.Log("BT Stream Angle: " + data);
        byte[] byteArray = System.Convert.FromBase64String(angleCharacteristic);
        float dataReceived = SwapBytesAndConvertToDecimal(byteArray, 100);
        angle = dataReceived;
        string angleToString = "Angle: "+dataReceived.ToString();
        receivedDataAngle.text = angleToString;
    }


    public void ReadLog(string data)
    {
        Debug.Log(data);
    }


    // Function to display an Android Toast message
    public void Toast(string data)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnection.CallStatic("Toast", data);
    }

        public float SwapBytesAndConvertToDecimal(byte[] byteArray, int divider)
    {
        if (byteArray.Length != 2)
        {
            Debug.LogError("Input byte array must have exactly two elements.");
            return -1; // Return an error value if array doesn't contain exactly two elements
        }

        // Swap the bytes
        byte temp = byteArray[0];
        byteArray[0] = byteArray[1];
        byteArray[1] = temp;

        // Convert the swapped byte array to a decimal value
        // We interpret the first byte as the higher byte (multiplied by 256)
        // and the second byte as the lower byte.
        float decimalValue = (byteArray[0] * 256 + byteArray[1])/divider;

        return decimalValue;
    }
    public void Calibrate()
{
    if (Application.platform != RuntimePlatform.Android)
        return;

    Toast("Calibration mode: Blow steadily for an extended period of 10 seconds");

    StartCoroutine(CalibrationRoutine());
}

private IEnumerator CalibrationRoutine()
{
    float maxSpeed = 0f;
    float startTime = Time.time;

    while (Time.time - startTime < 10f)
    {
        if (speed > maxSpeed)
        {
            maxSpeed = speed;
        }
        yield return null; // Wait for the next frame
    }

    int force = 1;
    if(maxSpeed <= 5f && maxSpeed >=3f){
        force = 2;
    }
    else if(maxSpeed < 3f){
      force = 3;  
    }
    Toast($"Calibration complete. Your force multiplier was set to {force}");
    
}

}
