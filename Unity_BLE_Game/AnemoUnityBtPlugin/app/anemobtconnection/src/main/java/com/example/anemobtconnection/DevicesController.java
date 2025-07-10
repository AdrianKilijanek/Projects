package com.example.anemobtconnection;

import android.app.Application;
import android.bluetooth.BluetoothDevice;

import java.util.HashMap;
import java.util.Map;

/**
 * Manages discovered Bluetooth devices.
 */
public class DevicesController extends Application {

    // Stores Bluetooth devices with their MAC addresses as keys
    private final Map<String, BluetoothDevice> mDevicesMap = new HashMap<>();

    /**
     * Adds a new Bluetooth device to the map if it's not already present.
     *
     * @param device The Bluetooth device to add.
     * @return true if the device was added, false if it was already in the map.
     */
    public boolean AddDevice(BluetoothDevice device) {
        if (mDevicesMap.containsKey(device.getAddress())) {
            return false;
        }
        mDevicesMap.put(device.getAddress(), device);
        return true;
    }

    /**
     * Returns the number of stored Bluetooth devices.
     *
     * @return The count of devices.
     */
    public int getCount() {
        return mDevicesMap.size();
    }

    /**
     * Retrieves a Bluetooth device by its MAC address.
     *
     * @param address The MAC address of the Bluetooth device.
     * @return The BluetoothDevice object, or null if not found.
     */
    public BluetoothDevice getItem(String address) {
        return mDevicesMap.get(address);
    }

    /**
     * Clears all stored Bluetooth devices.
     */
    public void clearAll() {
        mDevicesMap.clear();
    }
}
