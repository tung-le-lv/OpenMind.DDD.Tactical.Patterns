using Order.Search.Features.GetOrderById;
using Order.Search.Features.GetOrdersByCustomer;
using Order.Search.Features.SearchOrders;
using Order.Search.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Search Service", Version = "v1" });
});

builder.Services.AddOrderSearch(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeOrderSearchAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapSearchOrders();
app.MapGetOrderById();
app.MapGetOrdersByCustomer();

app.Run();
