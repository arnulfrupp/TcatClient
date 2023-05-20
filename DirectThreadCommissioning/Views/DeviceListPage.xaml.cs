using DirectThreadCommissioning.Models;
using System.Collections.ObjectModel;

using InTheHand.Bluetooth;

namespace DirectThreadCommissioning.Views;


// [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
public partial class DeviceListPage : ContentPage
{
    internal BluetoothLEScan bleScan = null;
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
        BluetoothLEScanOptions BleScanOptions = new BluetoothLEScanOptions();

        // Request user permission on startup
        bool bleAvailabe = await BleThreadDevice.GetBleAvailabilityAsync();

        if (!bleAvailabe)
        {
            await DisplayAlert("Bluetooth", "Bluetooth not available", "Ok");
            return;
        }

        Bluetooth.AdvertisementReceived += Bluetooth_AdvertisementReceived;
        BleScanOptions.AcceptAllAdvertisements = true;

        bleScan = await Bluetooth.RequestLEScanAsync(BleScanOptions);

        VisibleDevices.Clear();
        cviCollection.SelectedItem = null;      // No item selected when the page is re-appearing
    }

    override protected void OnDisappearing()
    {
        Bluetooth.AdvertisementReceived -= Bluetooth_AdvertisementReceived;
        bleScan.Stop();
    }


    private void Bluetooth_AdvertisementReceived(object sender, BluetoothAdvertisingEvent e)
    {
        int iFound = -1;
        int iPositionBelowStrogerRssi = 0;

        if (e.Device == null) return;    // List only Bluetooth devices
        if (e.Rssi < minRssi) return;
        if (String.IsNullOrEmpty(e.Name)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            for (int i = 0; i < VisibleDevices.Count; i++)          // Run the search loop in the main thread to avoid race condition  
            {
                if (VisibleDevices[i].BluetoothDevice == null) continue;

                if (VisibleDevices[i].BluetoothDevice.Id == e.Device.Id) iFound = i;

                if (VisibleDevices[i].Rssi > e.Rssi) iPositionBelowStrogerRssi = i + 1;
            }

            if (iFound != -1)
            {
                VisibleDevices[iFound].Rssi = e.Rssi;
            }
            else if(iPositionBelowStrogerRssi < maxItemsInList)
            {
                string device_name = String.IsNullOrEmpty(e.Name) ? "<no name>" : e.Name;
                var theNewDev = new BleThreadDevice() { Name = device_name, BluetoothDevice = e.Device, Rssi = e.Rssi };

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

        Bluetooth.AdvertisementReceived -= Bluetooth_AdvertisementReceived;
        
        // iOS needs to restart scan for to show all device
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            bleScan.Stop();
        }

        minRssi = (int)(-sldRange.Value);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            for (int i = VisibleDevices.Count - 1; i >= 0; i--)
            {
                if (VisibleDevices[i].Rssi < minRssi) VisibleDevices.RemoveAt(i);
            }
        });

        Bluetooth.AdvertisementReceived += Bluetooth_AdvertisementReceived;

        // iOS needs to restart scan for to show all device
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            BluetoothLEScanOptions BleScanOptions = new BluetoothLEScanOptions();
            
            BleScanOptions.AcceptAllAdvertisements = true;
            bleScan = await Bluetooth.RequestLEScanAsync(BleScanOptions);
        }
    }

    private async void cviCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //int iPreviousSelectedItem = e.PreviousSelection.Count;   
        //int iCurrentSelectedItem = e.CurrentSelection.Count;      //--> not working with iOS (always same index) 
        selectedItem = e.CurrentSelection.FirstOrDefault() as BleThreadDevice;

        if (selectedItem == null) return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedDevice", selectedItem.BluetoothDevice }
        };

        await Shell.Current.GoToAsync("terminal", navigationParameter);
    }
}