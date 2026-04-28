using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TourGuide.Domain.Models;

[BsonIgnoreExtraElements]
public class POI
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? OwnerId { get; set; }

    public string? Name { get; set; }
    public string? Address { get; set; }
    
    public string? SourceLanguage { get; set; }
    public string? SourceDescription { get; set; }

    public string? Description_VI { get; set; }
    public string? Description_EN { get; set; }
    public string? Description_KO { get; set; }
    public string? Description_JA { get; set; }
    public string? Description_ZH { get; set; }

    public string? AudioUrl_VI { get; set; }
    public string? AudioUrl_EN { get; set; }
    public string? AudioUrl_KO { get; set; }
    public string? AudioUrl_JA { get; set; }
    public string? AudioUrl_ZH { get; set; }

    public int? Radius { get; set; }

    public GeoLocation? Location { get; set; }

    public int? PriorityLevel { get; set; }
    public int? BoostPriority { get; set; }
    public DateTime? BoostExpiresAt { get; set; }

    public string? Status { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? ModerationStatus { get; set; }
    public string? TranslationStatus { get; set; }
    
    public int? ContentVersion { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CreatedAt { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? UpdatedAt { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReviewedBy { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }
    public string? ImageUrl { get; set; }

    public int? CountedQrScanCount { get; set; }
    public int? ReplayQrScanCount { get; set; }
    public int? CountedTtsPlayCount { get; set; }
    public int? ReplayTtsPlayCount { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? Revenue { get; set; }

    public string? SubscriptionPackage { get; set; }
    public bool? IsPaid { get; set; }
    public string? LastTransactionId { get; set; }
    public DateTime? SubscriptionExpiry { get; set; }

    public string? MerchantNote { get; set; }

    [BsonElement("distance")]
    [BsonIgnoreIfNull]
    public double? Distance { get; set; }
}

[BsonIgnoreExtraElements]
public class GeoLocation
{
    [BsonElement("type")]
    public string? Type { get; set; }
    
    [BsonElement("coordinates")]
    public double[]? Coordinates { get; set; }
}