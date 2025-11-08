using ChatService.Application.Interfaces;
using ChatService.Application.Services;
using ChatService.Settings.JWT;

namespace ChatService.Settings
{
    public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<JwtUtils>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<WrapperService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IQuizService, QuizService>();
        }
    }
}
