using System;

namespace Mobile.Models;

public class QrHistoryModel
{
    public string PoiId { get; set; }
    public string PoiName { get; set; }
    public DateTime ScanTime { get; set; }

    public string FormattedScanTime => ScanTime.ToString("dd/MM/yyyy HH:mm");
}
