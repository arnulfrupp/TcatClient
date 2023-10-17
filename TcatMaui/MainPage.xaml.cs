using System.Threading.Tasks;
using TcatMaui.Models;

#if ANDROID
using Android.Content;
using Android.Locations;
#elif IOS || MACCATALYST
using CoreLocation;
#elif WINDOWS
using Windows.Devices.Geolocation;
#endif

namespace TcatMaui;



public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await CheckPermissions())
        {
            await DisplayAlert("Alert", "Not all permissions were accepted.", "OK");
            Application.Current.Quit();
        }
    }

    // From: https://stackoverflow.com/questions/76275381/an-easy-way-to-check-and-request-maui-permissions-including-bluetooth
    private async Task<bool> CheckPermissions()
    {
        PermissionStatus bluetoothStatus = await CheckBluetoothPermissions();

        /*
        PermissionStatus cameraStatus = await CheckPermissions<Permissions.Camera>();
        PermissionStatus mediaStatus = await CheckPermissions<Permissions.Media>();
        PermissionStatus storageWriteStatus = await CheckPermissions<Permissions.StorageWrite>();
        //PermissionStatus photosStatus = await CheckPermissions<Permissions.Photos>();
        */

        bool locationServices = IsLocationServiceEnabled();

        // return IsGranted(cameraStatus) && IsGranted(mediaStatus) && IsGranted(storageWriteStatus) && IsGranted(bluetoothStatus);
        return IsGranted(bluetoothStatus);
    }

    private async Task<PermissionStatus> CheckBluetoothPermissions()
    {
        PermissionStatus bluetoothStatus = PermissionStatus.Granted;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            if (DeviceInfo.Version.Major >= 12)
            {
                bluetoothStatus = await CheckPermissions<BluetoothPermissions>();
            }
            else
            {
                bluetoothStatus = await CheckPermissions<Permissions.LocationWhenInUse>();
            }
        }

        return bluetoothStatus;
    }

    private async Task<PermissionStatus> CheckPermissions<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<TPermission>();
        }

        return status;
    }

    private static bool IsGranted(PermissionStatus status)
    {
        return status == PermissionStatus.Granted || status == PermissionStatus.Limited;
    }

#if ANDROID
    private bool IsLocationServiceEnabled()
    {
        LocationManager locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
        return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
    }
#elif IOS || MACCATALYST
    public bool IsLocationServiceEnabled()
    {
        return CLLocationManager.Status == CLAuthorizationStatus.Denied;
    }
#elif WINDOWS
    private bool IsLocationServiceEnabled()
    {
        Geolocator locationservice = new Geolocator();
        return locationservice.LocationStatus == PositionStatus.Disabled;
    }
#endif


    private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}


    private async void OnAddDeviceBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("devicelist");
    }
}

