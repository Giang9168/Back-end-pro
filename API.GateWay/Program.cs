var builder = WebApplication.CreateBuilder(args);

// YARP reverse proxy — nạp routes/clusters từ cấu hình section "ReverseProxy"
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Endpoint kiểm tra gateway sống
app.MapGet("/", () => "API Gateway is running. Routes: /qtht/*");

// Chuyển tiếp mọi request khớp route tới service tương ứng
app.MapReverseProxy();

app.Run();
