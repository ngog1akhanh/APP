using MongoDB.Driver;
// Đổi thành namespace chứa class POI của bạn
using TourGuide.Domain.Models;

namespace TourGuide.API.Services; // Đổi lại namespace cho đúng với API

public class PoiDbService
{
    // Đổi Poi thành POI (viết hoa)
    private readonly IMongoCollection<POI> _poiCollection;

    public PoiDbService(IMongoDatabase database) // Dùng IMongoDatabase giống Controller của bạn cho đồng nhất
    {
        _poiCollection = database.GetCollection<POI>("POIs");

        // Tự động tạo Index 2dsphere cho trường Location để hỗ trợ $geoNear
        var indexKeysDefinition = Builders<POI>.IndexKeys.Geo2DSphere(p => p.Location);
        var indexModel = new CreateIndexModel<POI>(indexKeysDefinition);
        _poiCollection.Indexes.CreateOne(indexModel);
    }

    public async Task<List<POI>> GetAsync() =>
        await _poiCollection.Find(_ => true).ToListAsync();

    public async Task<POI?> GetAsync(string id) =>
        await _poiCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<POI>> GetNearbyPoisAsync(double longitude, double latitude, double maxDistanceInMeters = 5000)
    {
        var geoNearOptions = new MongoDB.Bson.BsonDocument
        {
            { "$geoNear", new MongoDB.Bson.BsonDocument
                {
                    { "near", new MongoDB.Bson.BsonDocument
                        {
                            { "type", "Point" },
                            { "coordinates", new MongoDB.Bson.BsonArray { longitude, latitude } }
                        }
                    },
                    { "distanceField", "distance" },
                    { "maxDistance", maxDistanceInMeters },
                    { "spherical", true },
                    { "query", new MongoDB.Bson.BsonDocument("Status", "Approved") }
                }
            }
        };

        var pipeline = new MongoDB.Bson.BsonDocument[] { geoNearOptions };
        
        return await _poiCollection.Aggregate<POI>(pipeline).ToListAsync();
    }
}