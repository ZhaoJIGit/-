using Consul;
using Microsoft.OpenApi.Models;

namespace MicroSoftware_Demo1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // ��ȡ Consul ����
            var consulConfig = builder.Configuration.GetSection("Consul");

            // ע�� Consul �ͻ���
            builder.Services.AddSingleton<IConsulClient>(sp => new ConsulClient(config =>
            {
                config.Address = new Uri(consulConfig["Address"] ?? "http://127.0.0.1:8500");
            }));

            // ע������� Consul �ļ���
            builder.Services.AddSingleton<IHostedService, ConsulServiceRegistration>();
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

          
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                c.RoutePrefix = string.Empty; // ʹ Swagger UI �ڸ�·����ʾ
            });
            app.MapGet("/health", () => 
            Results.Ok("Healthy")
            );
            // �����м��
            app.UseRouting();
            // �� app.UseRouting() ֮������ CORS
            app.UseCors("AllowAll");
            app.MapControllers();

            app.Run();
        }
    }

    // ����һ�� Consul ����ע����
    public class ConsulServiceRegistration : IHostedService
    {
        private readonly IConsulClient _consulClient;
        private readonly IWebHostEnvironment _env;

        public ConsulServiceRegistration(IConsulClient consulClient, IWebHostEnvironment env)
        {
            _consulClient = consulClient;
            _env = env;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var registration = new AgentServiceRegistration()
            {
                ID = $"demo1",
                Name = "demo1",
                Address = "127.0.0.1",  // ����ʵ���ĵ�ַ
                Port = 5001,            // ����ʵ���Ķ˿�
                Tags = new[] { "api" }, // ��ǩ
                Check = new AgentServiceCheck
                {
                    HTTP = "http://127.0.0.1:5001/health", // �������� URL
                    Interval = TimeSpan.FromSeconds(3)    // ���Ƶ��
                }
            };

            await _consulClient.Agent.ServiceRegister(registration);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var serviceId = $"demo1";
            await _consulClient.Agent.ServiceDeregister(serviceId);
        }
    }
   
}
