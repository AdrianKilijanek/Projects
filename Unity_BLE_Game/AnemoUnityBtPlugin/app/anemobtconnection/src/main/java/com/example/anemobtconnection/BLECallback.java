package com.example.anemobtconnection;

import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCharacteristic;

/**
 * BLECallback class serves as a callback interface for Bluetooth Low Energy (BLE) events.
 * It provides methods that can be overridden to handle different BLE operations.
 */
public class BLECallback {

    /**
     * Called when the connection state of the BLE device changes.
     *
     * @param gatt    The Bluetooth GATT client.
     * @param status  The status of the connection attempt (e.g., success or failure).
     * @param newState The new connection state (connected or disconnected).
     */
    public void onBleConnectionStateChange(BluetoothGatt gatt, int status, int newState) {}

    /**
     * Called when the GATT services of a BLE device are discovered.
     *
     * @param gatt   The Bluetooth GATT client.
     * @param status The status of the service discovery process.
     */
    public void onBleServiceDiscovered(BluetoothGatt gatt, int status) {}

    /**
     * Called when a characteristic's value changes (e.g., from a notification or indication).
     *
     * @param gatt           The Bluetooth GATT client.
     * @param characteristic The characteristic that has changed.
     */
    public void onBleCharacteristicChange(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {}

    /**
     * Called when data is written to a characteristic.
     *
     * @param gatt           The Bluetooth GATT client.
     * @param characteristic The characteristic that was written to.
     * @param status         The result of the write operation (success or failure).
     */
    public void onBleWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, int status) {}

    /**
     * Called when data is read from a characteristic.
     *
     * @param gatt           The Bluetooth GATT client.
     * @param characteristic The characteristic that was read.
     * @param value          The data read from the characteristic.
     * @param status         The status of the read operation (success or failure).
     */
    public void onBleRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value, int status) {}
}