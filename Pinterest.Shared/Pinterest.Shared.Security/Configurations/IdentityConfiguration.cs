﻿using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pinterest.Application.Tokens;
using Pinterest.Domain.Authorization.Settings;
using Pinterest.Domain.Core.MessageBus;
using Pinterest.Shared.Security.Models;
using Pinterest.Shared.Security.Settings;

namespace Pinterest.Shared.Security.Configurations;

public static class IdentityConfiguration
{
    private static AuthorizationPolicyBuilder RequireRestUserChecking(this AuthorizationPolicyBuilder builder)
    {
        return builder.RequireAssertion(context =>
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext == null) return false;

            var isGrpc = httpContext.Request.ContentType?.StartsWith("application/grpc") == true;
            if (isGrpc) return true;
            
            return httpContext.User.Identity?.IsAuthenticated == true 
                   && httpContext.User.IsInRole(SecurityRole.User.ToString());
        });
    }
    internal static Task<IServiceCollection> AddIdentityServices(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddAuthentication(UsersAuthenticationOptions.DefaultScheme)
            .AddUsersAuthentication(item => {});
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(SecurityInfo.Admin, policy =>
            {
                policy.RequireClaim(ClaimTypes.Role, SecurityRole.Admin.ToString());
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy(SecurityInfo.Moderator, policy => policy.RequireClaim(ClaimTypes.Role, new []
            {
                SecurityRole.Moderator.ToString(),
                SecurityRole.Admin.ToString()
            }).RequireAuthenticatedUser());
            options.AddPolicy(SecurityInfo.User, policy => policy.RequireRestUserChecking());
        });
        return Task.FromResult(serviceCollection);
    }
    
}