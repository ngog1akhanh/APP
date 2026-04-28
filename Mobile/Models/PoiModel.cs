using System.Text.Json.Serialization;

namespace Mobile.Models;

public class PoiModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("sourceLanguage")]
    public string? SourceLanguage { get; set; }

    [JsonPropertyName("sourceDescription")]
    public string? SourceDescription { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("location")]
    public GeoLocation? Location { get; set; }

    // Tự động móc tọa độ ra ngoài cho App dễ xài
    [JsonIgnore]
    public double Longitude => (Location?.Coordinates != null && Location.Coordinates.Length >= 2) ? Location.Coordinates[0] : 0;

    [JsonIgnore]
    public double Latitude => (Location?.Coordinates != null && Location.Coordinates.Length >= 2) ? Location.Coordinates[1] : 0;

    // Các trường ngôn ngữ
    [JsonPropertyName("description_VI")] public string? Description_VI { get; set; }
    [JsonPropertyName("description_EN")] public string? Description_EN { get; set; }
    [JsonPropertyName("description_KO")] public string? Description_KO { get; set; }
    [JsonPropertyName("description_JA")] public string? Description_JA { get; set; }
    [JsonPropertyName("description_ZH")] public string? Description_ZH { get; set; }

    [JsonPropertyName("audioUrl_VI")] public string? AudioUrl_VI { get; set; }
    [JsonPropertyName("audioUrl_EN")] public string? AudioUrl_EN { get; set; }
    [JsonPropertyName("audioUrl_KO")] public string? AudioUrl_KO { get; set; }
    [JsonPropertyName("audioUrl_JA")] public string? AudioUrl_JA { get; set; }
    [JsonPropertyName("audioUrl_ZH")] public string? AudioUrl_ZH { get; set; }

    [JsonPropertyName("radius")] public int? Radius { get; set; }
    [JsonPropertyName("priorityLevel")] public int? PriorityLevel { get; set; }
    [JsonPropertyName("boostPriority")] public int? BoostPriority { get; set; }
    [JsonPropertyName("boostExpiresAt")] public DateTime? BoostExpiresAt { get; set; }

    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("approvalStatus")] public string? ApprovalStatus { get; set; }
    [JsonPropertyName("moderationStatus")] public string? ModerationStatus { get; set; }
    [JsonPropertyName("translationStatus")] public string? TranslationStatus { get; set; }

    [JsonPropertyName("contentVersion")] public int? ContentVersion { get; set; }
    [JsonPropertyName("createdAt")] public DateTime? CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("reviewedBy")] public string? ReviewedBy { get; set; }
    [JsonPropertyName("reviewedAt")] public DateTime? ReviewedAt { get; set; }
    [JsonPropertyName("rejectionReason")] public string? RejectionReason { get; set; }

    [JsonPropertyName("countedQrScanCount")] public int? CountedQrScanCount { get; set; }
    [JsonPropertyName("replayQrScanCount")] public int? ReplayQrScanCount { get; set; }
    [JsonPropertyName("countedTtsPlayCount")] public int? CountedTtsPlayCount { get; set; }
    [JsonPropertyName("replayTtsPlayCount")] public int? ReplayTtsPlayCount { get; set; }

    [JsonPropertyName("revenue")] public decimal? Revenue { get; set; }

    [JsonPropertyName("subscriptionPackage")] public string? SubscriptionPackage { get; set; }
    [JsonPropertyName("isPaid")] public bool? IsPaid { get; set; }
    [JsonPropertyName("lastTransactionId")] public string? LastTransactionId { get; set; }
    [JsonPropertyName("subscriptionExpiry")] public DateTime? SubscriptionExpiry { get; set; }
    [JsonPropertyName("merchantNote")] public string? MerchantNote { get; set; }
    
    [JsonPropertyName("distance")] public double? Distance { get; set; }
}

public class GeoLocation
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("coordinates")]
    public double[]? Coordinates { get; set; }
}