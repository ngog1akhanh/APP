using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using Mobile.Models;

namespace Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private string nickname = "Khách (Guest)";
    [ObservableProperty] private string avatarUrl = "dotnet_bot.png";
    [ObservableProperty] private string currentLanguage;
    [ObservableProperty] private bool isDarkMode;

    [ObservableProperty] private bool isDistanceExpanded;
    [ObservableProperty] private bool isQrExpanded;
    [ObservableProperty] private bool isFavoritesExpanded;

    [ObservableProperty] private ObservableCollection<QrHistoryModel> qrHistories = new();
    [ObservableProperty] private ObservableCollection<PoiModel> favoriteList = new();

    private bool _isReady = false;

    public ProfileViewModel()
    {
        CurrentLanguage = Preferences.Default.Get("AppLang", "Tiếng Việt");
        isDarkMode = Preferences.Default.Get("AppDarkMode", false);

        Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

        _isReady = true;
    }

    partial void OnCurrentLanguageChanged(string value)
    {
        if (!_isReady) return;
        Preferences.Default.Set("AppLang", value);
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        if (!_isReady) return;

        Preferences.Default.Set("AppDarkMode", value);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var mainPage = Application.Current.MainPage;
            if (mainPage != null)
            {
                // Hiệu ứng mờ dần (Fade Out)
                await mainPage.FadeTo(0.3, 150, Easing.CubicOut);
                
                // Đổi Theme
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
                
                // Hiệu ứng rõ dần (Fade In)
                await mainPage.FadeTo(1, 250, Easing.CubicIn);
            }
            else
            {
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            }
        });
    }

    partial void OnIsQrExpandedChanged(bool value)
    {
        if (value)
        {
            LoadQrHistory();
        }
    }

    partial void OnIsFavoritesExpandedChanged(bool value)
    {
        if (value)
        {
            LoadFavorites();
        }
    }

    private void LoadQrHistory()
    {
        string json = Preferences.Default.Get("QrHistory", "[]");
        var list = JsonSerializer.Deserialize<List<QrHistoryModel>>(json) ?? new List<QrHistoryModel>();

        if (list.Count == 0)
        {
            // Mock data
            list.Add(new QrHistoryModel { PoiId = "1", PoiName = "Quán Ốc Vũ", ScanTime = DateTime.Now.AddHours(-2) });
            list.Add(new QrHistoryModel { PoiId = "2", PoiName = "Lẩu Bò Khu Ba", ScanTime = DateTime.Now.AddDays(-1) });
        }

        QrHistories = new ObservableCollection<QrHistoryModel>(list);
    }

    private void LoadFavorites()
    {
        string json = Preferences.Default.Get("MyFavorites", "[]");
        var list = JsonSerializer.Deserialize<List<PoiModel>>(json) ?? new List<PoiModel>();

        if (list.Count == 0)
        {
            // Mock data if empty for UI testing
            list.Add(new PoiModel { Id = "1", Name = "Quán Ốc Oanh", Address = "Đường Vĩnh Khánh", ImageUrl = "dotnet_bot.png" });
            list.Add(new PoiModel { Id = "2", Name = "Bánh Mì Ngon", Address = "Quận 4", ImageUrl = "dotnet_bot.png" });
        }

        FavoriteList = new ObservableCollection<PoiModel>(list);
    }

    [RelayCommand]
    private void RemoveFavorite(string poiId)
    {
        if (string.IsNullOrEmpty(poiId)) return;

        var item = FavoriteList.FirstOrDefault(x => x.Id == poiId);
        if (item != null)
        {
            FavoriteList.Remove(item);
            
            // Save back to Preferences
            string json = JsonSerializer.Serialize(FavoriteList.ToList());
            Preferences.Default.Set("MyFavorites", json);
        }
    }

    [RelayCommand]
    private void ToggleDistance()
    {
        IsDistanceExpanded = !IsDistanceExpanded;
    }

    [RelayCommand]
    private void ToggleQr()
    {
        IsQrExpanded = !IsQrExpanded;
    }

    [RelayCommand]
    private void ToggleFavorites()
    {
        IsFavoritesExpanded = !IsFavoritesExpanded;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc chắn muốn đăng xuất không?", "OK", "Hủy");
        if (confirm)
        {
            Preferences.Default.Clear();
            
            // Ngăn chặn trigger đổi theme/ngôn ngữ
            _isReady = false;
            IsDarkMode = false;
            CurrentLanguage = "Tiếng Việt";
            _isReady = true;

            await Shell.Current.DisplayAlert("Thành công", "Bạn đã đăng xuất khỏi hệ thống.", "OK");
        }
    }
}