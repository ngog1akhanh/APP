using Mobile.Models;
using Mobile.Services;
using ZXing.Net.Maui;

namespace Mobile.Views;

public partial class QrScannerPage : ContentPage
{
    private readonly PoiService _poiService;

    // Nhét PoiService vào đây để tí nữa gọi API
    public QrScannerPage(PoiService poiService)
    {
        InitializeComponent();
        _poiService = poiService;

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        barcodeReader.IsDetecting = true;
        AnimateScanLine();
    }

    private void AnimateScanLine()
    {
        ScanLine.TranslateTo(0, 240, 2000, Easing.Linear).ContinueWith(async (t) =>
        {
            if (!barcodeReader.IsDetecting) return;
            await ScanLine.TranslateTo(0, 0, 0); // Reset immediately
            AnimateScanLine(); // Loop
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        barcodeReader.IsDetecting = false;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first is null) return;

        // Tạm dừng camera ngay để không quét đúp
        barcodeReader.IsDetecting = false;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            string qrText = first.Value;
            lblResult.Text = "Đang xử lý dữ liệu...";

            try
            {
                // 1. CHUYỂN TEXT THÀNH LINK (URI)
                Uri uri = new Uri(qrText);

                // 2. BÓC TÁCH ID TỪ ĐUÔI LINK
                // Ví dụ: https://vinhkhanh.com/qr/65b1c2d3e4f5a6b7c8d9e001 -> Lấy khúc cuối
                string poiId = uri.Segments.Last().Trim('/');

                // 3. GỌI API LẤY THÔNG TIN QUÁN THEO ID
                var poi = await _poiService.GetPoiByIdAsync(poiId);

                if (poi != null)
                {
                    // 4. LẤY ĐƯỢC RỒI THÌ NHẢY SANG TRANG CHI TIẾT
                    await Shell.Current.GoToAsync(nameof(PoiDetailPage), true, new Dictionary<string, object>
                    {
                        { "Poi", poi }
                    });
                }
                else
                {
                    await DisplayAlert("Lỗi", "Không tìm thấy thông tin quán ăn này trên hệ thống!", "OK");
                    barcodeReader.IsDetecting = true; // Bật lại camera cho quét cái khác
                    lblResult.Text = "Hãy hướng Camera vào mã QR của quán";
                }
            }
            catch (UriFormatException)
            {
                // Nếu quét nhầm mã QR không phải là Link
                await DisplayAlert("Mã QR không hợp lệ", "Đây không phải là mã QR của ứng dụng Food Tour.", "Thử lại");
                barcodeReader.IsDetecting = true;
                lblResult.Text = "Hãy hướng Camera vào mã QR của quán";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi xử lý", ex.Message, "OK");
                barcodeReader.IsDetecting = true;
            }
        });
    }
}