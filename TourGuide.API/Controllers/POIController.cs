using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TourGuide.Domain.Models;

namespace TourGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly IMongoCollection<POI> _poiCollection;

        public POIController(IMongoDatabase database)
        {
            _poiCollection = database.GetCollection<POI>("POIs");

            // Tự động tạo Index 2dsphere cho trường Location (Bọc try-catch để tránh crash API nếu DB lỗi kết nối lúc khởi động)
            try
            {
                var indexKeysDefinition = Builders<POI>.IndexKeys.Geo2DSphere(p => p.Location);
                var indexModel = new CreateIndexModel<POI>(indexKeysDefinition);
                _poiCollection.Indexes.CreateOne(indexModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo Index: {ex.Message}");
            }
        }

        // --- NHÓM API TẠO MỚI (POST) ---
        [HttpPost]
        public async Task<IActionResult> CreatePOI([FromBody] POI newPOI)
        {
            if (newPOI.Location == null || newPOI.Location.Coordinates.Length != 2)
            {
                return BadRequest("Tọa độ Location không hợp lệ.");
            }

            newPOI.Status = "Pending";
            newPOI.CreatedAt = DateTime.UtcNow;

            await _poiCollection.InsertOneAsync(newPOI);

            return CreatedAtAction(nameof(GetById), new { id = newPOI.Id }, newPOI);
        }

        // --- NHÓM API LẤY DANH SÁCH (GET) ---
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedPOIs()
        {
            var filter = Builders<POI>.Filter.Eq(p => p.Status, "Approved");
            return Ok(await _poiCollection.Find(filter).ToListAsync());
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPOIs()
        {
            var filter = Builders<POI>.Filter.Eq(p => p.Status, "Pending");
            return Ok(await _poiCollection.Find(filter).ToListAsync());
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyPOIs([FromQuery] double lon, [FromQuery] double lat, [FromQuery] double radius = 5000)
        {
            // Tạm khởi tạo PoiDbService bằng Database từ collection hiện tại (do không chắc đã register DI cho PoiDbService)
            var poiService = new TourGuide.API.Services.PoiDbService(_poiCollection.Database);
            
            // Gọi hàm service chứa Aggregation Pipeline ($geoNear)
            var nearbyPOIs = await poiService.GetNearbyPoisAsync(lon, lat, radius);

            // Sắp xếp lại theo mức độ ưu tiên và khoảng cách
            var sortedPOIs = nearbyPOIs
                .OrderByDescending(p => p.PriorityLevel)
                .ThenBy(p => p.Distance)
                .ToList();

            return Ok(sortedPOIs);
        }

        // --- NHÓM API LẤY CHI TIẾT (GET) ---
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var poi = await _poiCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (poi == null) return NotFound("Không tìm thấy dữ liệu quán ăn này.");
            return Ok(poi);
        }

        // --- NHÓM API THỐNG KÊ (GET) ---
        [HttpGet("dashboard-stats")]
        public IActionResult GetDashboardStats()
        {
            var stats = new
            {
                TotalRevenue = 15000000,
                TotalQRScans = 1250,
                TotalTTSPlays = 3400,
                ChartLabels = new string[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" },
                ChartData = new double[] { 120, 150, 100, 200, 250, 300, 130 }
            };
            return Ok(stats);
        }

        // --- NHÓM API CẬP NHẬT TRẠNG THÁI (PUT) ---
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApprovePOI(string id)
        {
            var update = Builders<POI>.Update.Set(p => p.Status, "Approved");
            var result = await _poiCollection.UpdateOneAsync(p => p.Id == id, update);
            return result.ModifiedCount == 0 ? NotFound() : Ok("Đã duyệt.");
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectPOI(string id)
        {
            var update = Builders<POI>.Update.Set(p => p.Status, "Rejected");
            var result = await _poiCollection.UpdateOneAsync(p => p.Id == id, update);
            return result.ModifiedCount == 0 ? NotFound() : Ok("Đã từ chối.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePOI(string id, [FromBody] POI updatedPOI)
        {
            var existingPoi = await _poiCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existingPoi == null) return NotFound("Không tìm thấy quán ăn.");

            var update = Builders<POI>.Update
                .Set(p => p.Name, updatedPOI.Name)
                .Set(p => p.Status, updatedPOI.Status)
                .Set(p => p.PriorityLevel, updatedPOI.PriorityLevel)
                .Set(p => p.ImageUrl, updatedPOI.ImageUrl)
                .Set(p => p.Description_VI, updatedPOI.Description_VI)
                .Set(p => p.Description_EN, updatedPOI.Description_EN)
                .Set(p => p.Description_KO, updatedPOI.Description_KO)
                .Set(p => p.Description_JA, updatedPOI.Description_JA)
                .Set(p => p.Description_ZH, updatedPOI.Description_ZH);

            var result = await _poiCollection.UpdateOneAsync(p => p.Id == id, update);

            return Ok(updatedPOI);
        }

        // --- NHÓM API TẢI LÊN ẢNH (POST) ---
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Không tìm thấy file ảnh.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { imageUrl = $"/images/{fileName}" });
        }

        // --- HELPER METHOD ---
        private double CalculateDistance(double lon1, double lat1, double lon2, double lat2)
        {
            var r = 6371e3; // Đơn vị: mét
            var rad = Math.PI / 180;
            var dLat = (lat2 - lat1) * rad;
            var dLon = (lon2 - lon1) * rad;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * rad) * Math.Cos(lat2 * rad) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}