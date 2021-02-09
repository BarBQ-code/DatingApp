using API.Data;
using API.Hubs;
using API.Interfaces;
using API.Services;
using API.Util;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Exstensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<PresenceTracker>();
            
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            services.AddScoped<ITokenService, TokenService>();
            
            services.AddScoped<IPhotoUploadService, PhotoUploadService>();
            
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<ILikesReposirtory, LikesRepository>();

            services.AddScoped<IMessageRepository, MessageRepository>();
            
            services.AddScoped<LogUserActivity>();

            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}
