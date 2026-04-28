using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using System.Text.Json; // BẮT BUỘC THÊM ĐỂ ĐỌC/GHI JSON DỮ LIỆU YÊU THÍCH

namespace Mobile.ViewModels;

[QueryProperty(nameof(Poi), "Poi")]
public partial class PoiDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private PoiModel? poi;

    // =======================================================
    // TÍNH NĂNG MỚI: QUẢN LÝ YÊU THÍCH (FAVORITE)
    // =======================================================
    [ObservableProperty]
    private bool isFavorite; // Trạng thái tim: Đỏ (true) hay Trống (false)

    // Hàm này tự động chạy ngay khi Trang Chi tiết nhận được dữ liệu Quán ăn (Poi)
    partial void OnPoiChanged(PoiModel? value)
    {
        if (value == null) return;

        // Mở bộ nhớ ra kiểm tra xem quán này đã từng được thả tim chưa
        string savedJson = Preferences.Default.Get("MyFavorites", "[]");
        var favList = JsonSerializer.Deserialize<List<PoiModel>>(savedJson) ?? new List<PoiModel>();

        // Nếu ID của quán này có trong list -> Bật trạng thái IsFavorite = true
        IsFavorite = favList.Any(p => p.Id == value.Id);

        // FALLBACK LOGIC: Nếu không có khoảng cách (quán Hot) -> Tính thủ công
        if (value.Distance == null || value.Distance == 0)
        {
            _ = CalculateDistanceLocallyAsync();
        }
    }

    private async Task CalculateDistanceLocallyAsync()
    {
        if (Poi == null || Poi.Location == null) return;

        try
        {
            // 1. Lấy vị trí người dùng
            var userLocation = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            
            if (userLocation != null)
            {
                // 2. Tọa độ quán (Index 0: Lon, Index 1: Lat)
                double poiLon = Poi.Longitude;
                double poiLat = Poi.Latitude;

                // 3. Tính toán (Đơn vị Kilometers)
                double distanceKm = Location.CalculateDistance(userLocation.Latitude, userLocation.Longitude, poiLat, poiLon, DistanceUnits.Kilometers);

                // 4. Cập nhật (Đổi ra mét)
                Poi.Distance = distanceKm * 1000;

                // 5. Thông báo UI cập nhật lại toàn bộ object Poi
                OnPropertyChanged(nameof(Poi));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi tính khoảng cách local: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync()
    {
        if (Poi == null)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy dữ liệu quán ăn!", "OK");
            return;
        }

        // ĐOẠN ĐIỆP VIÊN: Kiểm tra xem ID có bị mất không
        if (string.IsNullOrEmpty(Poi.Id))
        {
            await Shell.Current.DisplayAlert("Lỗi Nghiêm Trọng", "ID của quán này đang bị Null! Lỗi do lúc kéo từ API về chưa khớp tên biến ID.", "OK");
            return;
        }

        string savedJson = Preferences.Default.Get("MyFavorites", "[]");
        var favList = JsonSerializer.Deserialize<List<PoiModel>>(savedJson) ?? new List<PoiModel>();

        var existingPoi = favList.FirstOrDefault(p => p.Id == Poi.Id);

        if (existingPoi != null)
        {
            favList.Remove(existingPoi);
            IsFavorite = false;
            await Shell.Current.DisplayAlert("Đã bỏ lưu", $"Đã xóa {Poi.Name}", "OK");
        }
        else
        {
            favList.Add(Poi);
            IsFavorite = true;
            await Shell.Current.DisplayAlert("Thành công", $"Đã lưu {Poi.Name}", "OK");
        }

        Preferences.Default.Set("MyFavorites", JsonSerializer.Serialize(favList));
    }


    // =======================================================
    // CÁC BIẾN CHO TÍNH NĂNG THUYẾT MINH (GIỮ NGUYÊN)
    // =======================================================

    [ObservableProperty]
    private string selectedLanguage = "Tiếng Việt";

    public List<string> AvailableLanguages { get; } = new()
    {
        "Tiếng Việt", "English", "한국어", "日本語", "中文"
    };

    [ObservableProperty]
    private bool isSpeaking;

    private CancellationTokenSource _cts;

    public PoiDetailViewModel() { }

    partial void OnSelectedLanguageChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayDescription));

        if (IsSpeaking)
        {
            _cts?.Cancel();
            IsSpeaking = false;
        }
    }

    public string DisplayDescription => SelectedLanguage switch
    {
        "English" => Poi?.Description_EN,
        "한국어" => Poi?.Description_KO,
        "日本語" => Poi?.Description_JA,
        "中文" => Poi?.Description_ZH,
        _ => Poi?.Description_VI
    };

    [RelayCommand]
    public async Task SpeakDescriptionAsync()
    {
        if (IsSpeaking)
        {
            _cts?.Cancel();
            IsSpeaking = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(DisplayDescription))
        {
            await Shell.Current.DisplayAlert("Thông báo", "Chưa có bài thuyết minh cho ngôn ngữ này.", "OK");
            return;
        }

        try
        {
            IsSpeaking = true;
            _cts = new CancellationTokenSource();

            var locales = await TextToSpeech.Default.GetLocalesAsync();
            Locale locale = SelectedLanguage switch
            {
                "English" => locales.FirstOrDefault(l => l.Language.StartsWith("en")),
                "한국어" => locales.FirstOrDefault(l => l.Language.StartsWith("ko")),
                "日本語" => locales.FirstOrDefault(l => l.Language.StartsWith("ja")),
                "中文" => locales.FirstOrDefault(l => l.Language.StartsWith("zh")),
                _ => locales.FirstOrDefault(l => l.Language.StartsWith("vi"))
            };

            var options = new SpeechOptions { Locale = locale };
            await TextToSpeech.Default.SpeakAsync(DisplayDescription, options, cancelToken: _cts.Token);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Không thể phát giọng đọc trên thiết bị này.", "OK");
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    [RelayCommand]
    public async Task OpenMapAsync()
    {
        // ... (Giữ nguyên code hàm OpenMapAsync như cũ) ...
    }
}