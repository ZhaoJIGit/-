using Consul;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace MiscoSoftware_ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ָ������������ļ�·��
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

            // ��� Ocelot ����
            builder.Services.AddOcelot(builder.Configuration).AddConsul();
            // ��� Swagger ����
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "API documentation for My Web API"
                });
            });
            builder.Services.AddControllers();
         
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();
           

            // Configure the HTTP request pipeline.
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                c.RoutePrefix = string.Empty; // ʹ Swagger UI �ڸ�·����ʾ
            });
            app.UseOcelot();
            // �� app.UseRouting() ֮������ CORS
            app.UseCors("AllowAll");
            // �����м��
            app.UseRouting();
        
            app.MapControllers();
            app.Run();
        }

    }
}
