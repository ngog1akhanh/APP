using Mobile.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mobile.Services;

public class PoiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _ip; // Đưa IP ra ngoài để dùng chung cho các hàm khác

    public PoiService()
    {
        _httpClient = new HttpClient();

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                // Máy ảo (Emulator)
                _ip = "10.0.2.2"; 
            }
            else
            {
                // MÁY THẬT (Điện thoại cắm cáp): Thay 192.168.X.X bằng IPv4 máy tính của bạn
                _ip = "192.168.1.X"; 
            }
        }
        else
        {
            _ip = "localhost"; // Dành cho Windows
        }

        // Đường dẫn chính xác trỏ đến API lấy danh sách đã duyệt
        _baseUrl = $"http://{_ip}:5276/api/POI/approved";
    }

    public async Task<List<PoiModel>> GetPoisAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_baseUrl);
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return await response.Content.ReadFromJsonAsync<List<PoiModel>>(options) ?? new List<PoiModel>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi gọi API danh sách: {ex.Message}");
        }
        return new List<PoiModel>();
    }

    // THÊM HÀM MỚI: Phục vụ cho tính năng Quét QR
    public async Task<PoiModel?> GetPoiByIdAsync(string id)
    {
        try
        {
            // Đường dẫn gọi API lấy chi tiết 1 quán theo ID
            string detailUrl = $"http://{_ip}:5276/api/POI/details/{id}";

            var response = await _httpClient.GetAsync(detailUrl);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Trả về thẳng object PoiModel thay vì List
                return await response.Content.ReadFromJsonAsync<PoiModel>(options);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi gọi API chi tiết POI: {ex.Message}");
        }

        return null; // Trả về null nếu không tìm thấy hoặc lỗi
    }

    // THÊM HÀM MỚI: TÌM QUÁN ĂN GẦN ĐÂY BẰNG GEOJSON
    public async Task<List<PoiModel>> GetNearbyPoisAsync(double longitude, double latitude, double maxDistanceInMeters)
    {
        try
        {
            // Gọi endpoint api/POI/nearby mà ta đã cập nhật trên Backend
            string nearbyUrl = $"http://{_ip}:5276/api/POI/nearby?lon={longitude}&lat={latitude}&radius={maxDistanceInMeters}";

            var response = await _httpClient.GetAsync(nearbyUrl);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return await response.Content.ReadFromJsonAsync<List<PoiModel>>(options) ?? new List<PoiModel>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi gọi API quán gần đây: {ex.Message}");
        }

        return new List<PoiModel>();
    }
}