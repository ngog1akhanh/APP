using MongoDB.Driver;
using TourGuide.API.Services; // Đảm bảo đúng namespace nơi bạn chứa PoiDbService

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CẤU HÌNH MONGODB (SỬA LỖI SCOPED -> SINGLETON)
// ==========================================
var connectionString = builder.Configuration["MongoDbSettings:ConnectionString"];
var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"];

var mongoClient = new MongoClient(connectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseName);

// Đăng ký toàn bộ là Singleton để tối ưu connection pool của MongoDB
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
builder.Services.AddSingleton<PoiDbService>();

// ==========================================
// 2. CẤU HÌNH CORS (Mở cửa cho WebAdmin, WebQR, Mobile)
// ==========================================
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==========================================
// 3. ĐĂNG KÝ CONTROLLER & SWAGGER
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// --- THỨ TỰ PIPELINE QUAN TRỌNG ---
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// CORS bắt buộc phải gọi trước StaticFiles và Controllers
app.UseCors("AllowAll");

app.UseStaticFiles(); // Mở khóa thư mục chứa ảnh (wwwroot)

app.UseAuthorization();

app.MapControllers();

app.Run();