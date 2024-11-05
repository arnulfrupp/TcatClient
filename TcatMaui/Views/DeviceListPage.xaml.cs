using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;

using TcatMaui.Models;

namespace TcatMaui.Views;


// [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
public partial class DeviceListPage : ContentPage
{
    internal IBluetoothLE ble = CrossBluetoothLE.Current;
    internal IAdapter adapter = CrossBluetoothLE.Current.Adapter;

    internal BleThreadDevice selectedItem = null;
    internal int minRssi = 0;

    public const int maxItemsInList = 15;

    ObservableCollection<BleThreadDevice> VisibleDevices = new ObservableCollection<BleThreadDevice>();
    internal ObservableCollection<BleThreadDevice> GetVisibleDevices { get { return VisibleDevices; } }

    public DeviceListPage()
    {
        InitializeComponent();
        cviCollection.ItemsSource = VisibleDevices;
        minRssi = (int)(-sldRange.Value);
    }

    override async protected void OnAppearing()
    {
        // Request user permission on startup

        adapter.DeviceDiscovered += Bluetooth_AdvertisementReceived;
 
        VisibleDevices.Clear();
        cviCollection.SelectedItem = null;      // No item selected when the page is re-appearing

        await adapter.StartScanningForDevicesAsync();
    }

    override async protected void OnDisappearing()
    {
        adapter.DeviceDiscovered -= Bluetooth_AdvertisementReceived;
        await adapter.StopScanningForDevicesAsync();
    }


    private void Bluetooth_AdvertisementReceived(object sender, DeviceEventArgs e)
    {
        int iFound = -1;
        int iPositionBelowStrogerRssi = 0;

        if (e.Device == null) return;    // List only Bluetooth devices
        if (e.Device.Rssi < minRssi) return;
        //if (String.IsNullOrEmpty(e.Name)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            for (int i = 0; i < VisibleDevices.Count; i++)          // Run the search loop in the main thread to avoid race condition  
            {
                if (VisibleDevices[i].BluetoothDevice == null) continue;

                if (VisibleDevices[i].BluetoothDevice.Id == e.Device.Id) iFound = i;

                if (VisibleDevices[i].Rssi > e.Device.Rssi) iPositionBelowStrogerRssi = i + 1;
            }

            if (iFound != -1)
            {
                VisibleDevices[iFound].Rssi = e.Device.Rssi;
            }
            else if(iPositionBelowStrogerRssi < maxItemsInList)
            {
                string device_name = String.IsNullOrEmpty(e.Device.Name) ? "<no name: " + e.Device.Id.ToString() + ">": e.Device.Name + " (" + e.Device.Id.ToString() + ")";
                var theNewDev = new BleThreadDevice() { Name = device_name, BluetoothDevice = e.Device, Rssi = e.Device.Rssi };

                if(VisibleDevices.Count >= maxItemsInList)
                {
                    VisibleDevices.RemoveAt(VisibleDevices.Count - 1);
                }

                VisibleDevices.Insert(iPositionBelowStrogerRssi, theNewDev);     // Insert item in main (UI) thread
            }
        });
    }

    private async void sldRange_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        //BluetoothLEScanOptions BleScanOptions = new BluetoothLEScanOptions();

        if (sldRange == null) return;

        adapter.DeviceDiscovered -= Bluetooth_AdvertisementReceived;

        // iOS needs to restart scan for to show all device
        /* ---->  to be verified with Plugin.Ble
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            await adapter.StartScanningForDevicesAsync();
        }
        <---- */

        minRssi = (int)(-sldRange.Value);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            for (int i = VisibleDevices.Count - 1; i >= 0; i--)
            {
                if (VisibleDevices[i].Rssi < minRssi) VisibleDevices.RemoveAt(i);
            }
        });

        adapter.DeviceDiscovered += Bluetooth_AdvertisementReceived;

        // iOS needs to restart scan for to show all device
        /* ---->  to be verified with Plugin.Ble
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            await adapter.StartScanningForDevicesAsync();
        }
        */
    }

    private async void cviCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //int iPreviousSelectedItem = e.PreviousSelection.Count;   
        //int iCurrentSelectedItem = e.CurrentSelection.Count;      //--> not working with iOS (always same index) 
        selectedItem = e.CurrentSelection.FirstOrDefault() as BleThreadDevice;

        if (selectedItem == null) return;

        /*  -----> Not working with Plugin.Ble because IDevice ist not implementing IConvertable
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedDevice", selectedItem.BluetoothDevice }
        };
        <----- */

        // Dirty qick fix ...
        MauiProgram.SelectedDevice = selectedItem.BluetoothDevice;

        //await Shell.Current.GoToAsync("terminal", navigationParameter);
        await Shell.Current.GoToAsync("terminal");
    }
}