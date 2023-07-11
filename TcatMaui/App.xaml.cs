using TcatMaui.Views;

namespace TcatMaui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

        Routing.RegisterRoute("devicelist", typeof(DeviceListPage));
        Routing.RegisterRoute("devicelist/terminal", typeof(TerminalPage));
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        const int newWidth = 400;
        const int newHeight = 600;

        window.Width = newWidth;
        window.Height = newHeight;

        window.Title = "Thread Commissioning";

        return window;
    }
}
