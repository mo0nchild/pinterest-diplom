﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CloudStorage.Application.Posts.Repositories;
using CloudStorage.Database.Posts.Contexts;
using CloudStorage.Database.Settings.Factories;
using CloudStorage.Database.Settings.Helpers;

namespace CloudStorage.Database.Posts;

public static class Bootstrapper
{
    private static readonly string DbSettingsSection = "Database";
    public static async Task<IServiceCollection> AddPostsDatabase(this IServiceCollection collection, 
        IConfiguration configuration)
    {
        var settings = collection.Configure<PostsDbContextSettings>(configuration.GetSection(DbSettingsSection))
            .BuildServiceProvider()
            .GetRequiredService<IOptions<PostsDbContextSettings>>();
        collection.AddDbContextFactory<PostsDbContext>(options =>
        {
            DbContextOptionsFactory<PostsDbContext>.Configure(settings.Value.ConnectionString, true).Invoke(options);
        });
        await collection.AddDbContextFactoryWrapper<IPostsRepository, PostsDbContext>();
        
        var serviceProvider = collection.BuildServiceProvider();
        var dbContextFactory = serviceProvider.GetService<IDbContextFactory<PostsDbContext>>()!;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync();
        return collection;
    }
}