using Microsoft.Maui.Dispatching;

namespace Mobile;

public partial class App : Application
{
    private IDispatcherTimer? _pingTimer;
    private readonly string _deviceId;

    // 🚨 NHỚ THAY BẰNG LINK RENDER CỦA BẠN
    private readonly string _apiPingUrl = "https://appnnlt.onrender.com/api/Tracking/ping";

    public App()
    {
        InitializeComponent();

        // 1. Khởi tạo ID thiết bị
        _deviceId = Preferences.Get("DeviceId", Guid.NewGuid().ToString());
        Preferences.Set("DeviceId", _deviceId);

        // 2. Cài đặt Timer bắn Ping mỗi 2 phút
        _pingTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_pingTimer != null)
        {
            _pingTimer.Interval = TimeSpan.FromMinutes(2);
            _pingTimer.Tick += async (s, e) => await SendPingAsync();
        }
    }

    // Giữ nguyên hàm tạo Window của MAUI
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await SendPingAsync(); // Bắn ping ngay khi mở App
        _pingTimer?.Start();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        _pingTimer?.Stop(); // Tắt ping khi app thu nhỏ xuống nền
    }

    protected override async void OnResume()
    {
        base.OnResume();
        await SendPingAsync(); // Bắn ping báo cáo trở lại
        _pingTimer?.Start();
    }

    private async Task SendPingAsync()
    {
        try
        {
            using var client = new HttpClient();
            await client.PostAsync($"{_apiPingUrl}/{_deviceId}", null);
            System.Diagnostics.Debug.WriteLine($"[TRACKING] Đã gửi Ping cho thiết bị: {_deviceId}");
        }
        catch (Exception)
        {
            // Bỏ qua lỗi nếu mất mạng để không làm gián đoạn trải nghiệm người dùng
        }
    }
}