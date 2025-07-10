package com.example.anemobtconnection;

import android.app.Application;
import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.widget.Toast;
import android.util.Base64;

import com.unity3d.player.UnityPlayer;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Set;
import java.util.UUID;

import android.annotation.SuppressLint;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothManager;
import android.bluetooth.le.BluetoothLeScanner;
import android.content.BroadcastReceiver;
import android.content.Intent;
import android.content.IntentFilter;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothProfile;
import android.util.Log;

/**
 * Manages Bluetooth Low Energy (BLE) connection for an anemometer in a Unity Android game.
 */
public class BluetoothConnection extends Application {

    private static BluetoothConnection mInstance = null;
    private static BluetoothManager mBluetoothManager = null;
    private static BluetoothAdapter mBluetoothAdapter = null;
    private static BluetoothLeScanner mBluetoothLeScanner = null;
    private static DevicesController mDeviceListAdapter;
    private static InputStream inputStream;

    public static String DeviceString;
    public static BLECallback bleCallback;
    public static BluetoothGatt mBluetoothGatt;

    private static final int STATE_DISCONNECTED = 0;
    private static final int STATE_CONNECTED = 1;
    private static Thread ReadDatathread = null;
    private static int mConnectionState = STATE_DISCONNECTED;

    /**
     * Displays a toast message on the main thread.
     */
    public void PrintString(Context context, final String message) {
        new Handler(Looper.getMainLooper()).post(() ->
                Toast.makeText(context, message, Toast.LENGTH_SHORT).show()
        );
    }

    /**
     * Returns a singleton instance of BluetoothConnection.
     */
    @SuppressLint("MissingPermission")
    public static BluetoothConnection getInstance() {
        if (mInstance == null)
            mInstance = new BluetoothConnection();
        return mInstance;
    }

    /**
     * Initializes Bluetooth manager, adapter, and scanner.
     * Registers a broadcast receiver to detect Bluetooth scan events.
     */
    @SuppressLint("MissingPermission")
    public BluetoothConnection() {
        mBluetoothManager = UnityPlayer.currentActivity.getApplication()
                .getApplicationContext().getSystemService(BluetoothManager.class);
        mBluetoothAdapter = mBluetoothManager.getAdapter();

        if (!mBluetoothAdapter.isEnabled()) {
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            UnityPlayer.currentActivity.startActivityForResult(enableBtIntent, 1);
            mBluetoothAdapter.enable();
        }

        mBluetoothLeScanner = mBluetoothAdapter.getBluetoothLeScanner();
        mDeviceListAdapter = new DevicesController();

        // Register a broadcast receiver for Bluetooth events
        IntentFilter filter = new IntentFilter();
        filter.addAction(BluetoothDevice.ACTION_FOUND);
        filter.addAction(BluetoothAdapter.ACTION_DISCOVERY_STARTED);
        filter.addAction(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);

        BroadcastReceiver receiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                String action = intent.getAction();
                if (BluetoothDevice.ACTION_FOUND.equals(action)) {
                    BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                    if (mDeviceListAdapter.AddDevice(device)) {
                        String deviceName = device.getName() == null ? "null" : device.getName();
                        String deviceAddress = device.getAddress();
                        UnityPlayer.UnitySendMessage("BluetoothManager", "NewDeviceFound",
                                deviceAddress + "+" + deviceName);
                    }
                } else if (BluetoothAdapter.ACTION_DISCOVERY_STARTED.equals(action)) {
                    UnityPlayer.UnitySendMessage("BluetoothManager", "ScanStatus", "started");
                } else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED.equals(action)) {
                    UnityPlayer.UnitySendMessage("BluetoothManager", "ScanStatus", "stopped");
                }
            }
        };
        UnityPlayer.currentActivity.getApplication().getApplicationContext()
                .registerReceiver(receiver, filter);
    }

    /**
     * Starts scanning for Bluetooth devices.
     */
    @SuppressLint("MissingPermission")
    private static void StartScanDevices() {
        if (mBluetoothAdapter.isDiscovering()) {
            mBluetoothAdapter.cancelDiscovery();
        }
        mDeviceListAdapter.clearAll();
        mBluetoothAdapter.startDiscovery();
    }

    /**
     * Stops Bluetooth device scanning.
     */
    @SuppressLint("MissingPermission")
    private static void StopScanDevices() {
        if (mBluetoothAdapter.isDiscovering()) {
            mBluetoothAdapter.cancelDiscovery();
            UnityPlayer.UnitySendMessage("BluetoothManager", "ScanStatus", "stopped");
        }
    }

    /**
     * Initiates a Bluetooth connection to the specified device.
     */
    @SuppressLint("MissingPermission")
    public static void StartConnection(String DeviceAdd) {
        UnityPlayer.UnitySendMessage("BluetoothManager", "ConnectionStatus", "connecting");

        try {
            if (!BluetoothAdapter.checkBluetoothAddress(DeviceAdd)) {
                UnityPlayer.UnitySendMessage("BluetoothManager", "ConnectionStatus", "Device not found");
                return;
            }

            BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(DeviceAdd);
            if (mBluetoothGatt == null && !isConnected()) {
                bleCallback = bleCallbacks();
                mBluetoothGatt = device.connectGatt(UnityPlayer.currentActivity, false, mGattCallback);
                DeviceString = DeviceAdd;
            }

            UnityPlayer.UnitySendMessage("BluetoothManager", "ConnectionStatus", "connected");
        } catch (Exception ex) {
            UnityPlayer.UnitySendMessage("BluetoothManager", "ConnectionStatus", "unable to connect");
            UnityPlayer.UnitySendMessage("BluetoothManager", "ReadLog", "StartConnection Error: " + ex);
        }
    }

    /**
     * Checks if the Bluetooth device is connected.
     */
    public static boolean isConnected() {
        return mConnectionState == STATE_CONNECTED;
    }

    /**
     * Disconnects from the Bluetooth device.
     */
    public static void StopConnection() {
        if (mBluetoothGatt != null && isConnected()) {
            mBluetoothGatt.close();
            mBluetoothGatt = null;
        }
    }

    /**
     * Reads speed data from the anemometer.
     */
    public static void ReadSpeed() {
        if (isConnected()) {
            mBluetoothGatt.readCharacteristic(
                    mBluetoothGatt.getService(UUID.fromString("0000181a-0000-1000-8000-00805f9b34fb"))
                            .getCharacteristic(UUID.fromString("00002a72-0000-1000-8000-00805f9b34fb"))
            );
        }
    }

    /**
     * Reads angle data from the anemometer.
     */
    public static void ReadAngle() {
        if (isConnected()) {
            mBluetoothGatt.readCharacteristic(
                    mBluetoothGatt.getService(UUID.fromString("0000181a-0000-1000-8000-00805f9b34fb"))
                            .getCharacteristic(UUID.fromString("00002a73-0000-1000-8000-00805f9b34fb"))
            );
        }
    }

    /**
     * Discovers available services on the connected Bluetooth device.
     */
    public static void DiscoverMethod() {
        new Handler(Looper.getMainLooper()).post(() ->
                Toast.makeText(UnityPlayer.currentActivity.getApplicationContext(),
                        "Discovering services.", Toast.LENGTH_SHORT).show()
        );
        mBluetoothGatt.discoverServices();
    }

    /**
     * Displays a toast message with the given information.
     */
    public static void Toast(String info) {
        UnityPlayer.currentActivity.runOnUiThread(() ->
                Toast.makeText(UnityPlayer.currentActivity.getApplicationContext(), info, Toast.LENGTH_SHORT).show()
        );
    }

    public static BLECallback bleCallbacks(){

        return new BLECallback(){

            @Override
            public void onBleConnectionStateChange(BluetoothGatt gatt, int status, int newState) {
                super.onBleConnectionStateChange(gatt, status, newState);

                if (newState == BluetoothProfile.STATE_CONNECTED) {
                    Log.e("Ble ServiceDiscovered","onServicesDiscovered received: " + status);

                    new Handler(Looper.getMainLooper()).post(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(UnityPlayer.currentActivity.getApplicationContext(), "Connected to GATT server.",Toast.LENGTH_SHORT).show();
                            UnityPlayer.UnitySendMessage("BluetoothManager", "DetectConnection", "true");
                        }
                    });
                }

                if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                    new Handler(Looper.getMainLooper()).post(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(UnityPlayer.currentActivity.getApplicationContext(), "Disconnected from GATT server.",Toast.LENGTH_SHORT).show();;

                        }
                    });
                }
            }

            @Override
            public void onBleServiceDiscovered(BluetoothGatt gatt, int status) {
                super.onBleServiceDiscovered(gatt, status);
                if (status != BluetoothGatt.GATT_SUCCESS) {
                    Log.e("Ble ServiceDiscovered","onServicesDiscovered received: " + status);
                }
            }

            @Override
            public void onBleCharacteristicChange(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
                super.onBleCharacteristicChange(gatt, characteristic);
                Log.i("BluetoothLEHelper","onCharacteristicChanged Value: " + Arrays.toString(characteristic.getValue()));

            }

            @Override
            public void onBleRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value, int status) {
                super.onBleRead(gatt, characteristic, value, status);

                if (status == BluetoothGatt.GATT_SUCCESS) {
                    Log.i("TAG", Arrays.toString(characteristic.getValue()));
                    String base64String = Base64.encodeToString(value, Base64.DEFAULT);
                    if (characteristic.getUuid().equals(UUID.fromString("00002a72-0000-1000-8000-00805f9b34fb"))) {
                        // Action A for UUID "00002a72-0000-1000-8000-00805f9b34fb"
                        new Handler(Looper.getMainLooper()).post(new Runnable() {
                            @Override
                            public void run() {
                                // Replace "ReadDataSpeed" with the appropriate Unity method if needed
                                UnityPlayer.UnitySendMessage("BluetoothManager", "ReadDataSpeed", base64String);
                                ReadAngle();
                            }
                        });
                    } else if (characteristic.getUuid().equals(UUID.fromString("00002a73-0000-1000-8000-00805f9b34fb"))) {
                        // Action B for UUID "00002a73-0000-1000-8000-00805f9b34fb"
                        new Handler(Looper.getMainLooper()).post(new Runnable() {
                            @Override
                            public void run() {
                                // Replace "SomeOtherMethod" with the Unity method for action B
                                UnityPlayer.UnitySendMessage("BluetoothManager", "ReadDataAngle", base64String);
                                ReadSpeed();
                            }
                        });
                    }
                }
            }

            @Override
            public void onBleWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, int status) {
                super.onBleWrite(gatt, characteristic, status);

                new Handler(Looper.getMainLooper()).post(new Runnable() {
                    @Override
                    public void run() {
                        Toast.makeText(UnityPlayer.currentActivity.getApplicationContext(), "onCharacteristicWrite Status : " + status,Toast.LENGTH_SHORT).show();
                    }
                });

            }
        };
    }

    /**
     * Bluetooth GATT callback for handling connection events.
     */
    private static BluetoothGattCallback mGattCallback;
    {
        mGattCallback = new BluetoothGattCallback() {
            @Override
            public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {

                if (newState == BluetoothProfile.STATE_CONNECTED) {
                    Log.i("BluetoothLEHelper", "Attempting to start service discovery: " + mBluetoothGatt.discoverServices());

                    mConnectionState = STATE_CONNECTED;
                }

                if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                    mConnectionState = STATE_DISCONNECTED;
                }

                bleCallback.onBleConnectionStateChange(gatt, status, newState);
            }

            @Override
            public void onServicesDiscovered(BluetoothGatt gatt, int status) {
                bleCallback.onBleServiceDiscovered(gatt, status);
            }

            @Override
            public void onCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, int status) {
                super.onCharacteristicWrite(gatt, characteristic, status);
                bleCallback.onBleWrite(gatt, characteristic, status);
            }

            @Override
            public void onCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value, int status) {
                bleCallback.onBleRead(gatt, characteristic, value, status);
            }

            @Override
            public void onCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
                bleCallback.onBleCharacteristicChange(gatt, characteristic);
            }

        };
    }



}
