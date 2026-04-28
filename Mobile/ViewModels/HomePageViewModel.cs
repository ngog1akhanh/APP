using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using Mobile.Views;
using System.Collections.ObjectModel;

namespace Mobile.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    private readonly PoiService _poiService;
    private List<PoiModel> _allPois = new();

    // 1. FIX CÚ PHÁP MVVM: Đổi private field thành public partial property
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    public ObservableCollection<PoiModel> Pois { get; } = new();
    public ObservableCollection<PoiModel> NearbyPois { get; } = new();

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = "Tất cả";

    public ObservableCollection<string> Categories { get; } = new() 
    { 
        "Tất cả", "Ốc Đêm", "Ăn Vặt", "Trà Sữa", "Nhậu", "Cơm & Bún" 
    };

    public HomePageViewModel(PoiService poiService)
    {
        _poiService = poiService;
        Title = "Vinh Khanh Food Tour";
    }

    // Tự động gọi hàm Search khi người dùng đang gõ chữ
    partial void OnSearchTextChanged(string? value)
    {
        Search(value);
    }

    [RelayCommand]
    public async Task GetPoisAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Pois.Clear();
            _allPois.Clear();

            var items = await _poiService.GetPoisAsync();

            // Kiểm tra an toàn xem items có bị null không
            if (items != null)
            {
                foreach (var item in items)
                {
                    Pois.Add(item);
                    _allPois.Add(item);
                }
            }

            if (Pois.Count == 0)
            {
                await Shell.Current.DisplayAlert("Thông báo", "Không tìm thấy dữ liệu quán ăn nào.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi kết nối", $"Không thể lấy dữ liệu: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LoadNearbyPoisAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            NearbyPois.Clear();

            // 1. Cấu hình yêu cầu lấy vị trí
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            
            // 2. Lấy tọa độ GPS
            var location = await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                // 3. Gọi API lấy quán gần nhất
                var items = await _poiService.GetNearbyPoisAsync(location.Longitude, location.Latitude, 5000);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        NearbyPois.Add(item);
                    }
                }

                if (NearbyPois.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không tìm thấy quán ăn nào gần bạn trong bán kính 5km.", "OK");
                }
            }
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Lỗi GPS", "Thiết bị của bạn không hỗ trợ định vị (GPS).", "OK");
        }
        catch (FeatureNotEnabledException)
        {
            await Shell.Current.DisplayAlert("Lỗi GPS", "Vui lòng bật định vị (Location/GPS) trên điện thoại.", "OK");
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Quyền truy cập", "Ứng dụng cần quyền Vị trí để tìm quán ăn gần bạn. Vui lòng cấp quyền trong Cài đặt.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi kết nối", $"Đã có lỗi xảy ra: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Search(string query)
    {
        // 2. FIX LỖI VĂNG APP: Thêm điều kiện p.Name != null trước khi dùng Contains
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allPois
            : _allPois.Where(p => p.Name != null && p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        Pois.Clear();
        foreach (var item in filtered) Pois.Add(item);
    }

    [RelayCommand]
    async Task GoToDetails(PoiModel poi)
    {
        if (poi == null) return;

        await Shell.Current.GoToAsync(nameof(PoiDetailPage), true, new Dictionary<string, object>
        {
            { "Poi", poi }
        });
    }
}