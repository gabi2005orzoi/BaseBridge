using System.Text.Json;
using BaseBridge.Database;
using BaseBridge.Models;
using BaseBridge.Utils;
using Microsoft.EntityFrameworkCore;

namespace BaseBridge.Services;

public class DbConfigurationService(AppDbContext dbContext)
{

    public async Task SaveConfigAsync(DbConfig config)
    {
        var existing = await dbContext.DbConfigs.FirstOrDefaultAsync();
        if (existing != null)
            dbContext.DbConfigs.Remove(existing);

        config.Password = EncryptionUtils.Encrypt(config.Password);
        await dbContext.DbConfigs.AddAsync(config);
        await dbContext.SaveChangesAsync();
    }

    public async Task<DbConfig?> GetConfigAsync()
    {
        var config = await dbContext.DbConfigs.AsNoTracking().FirstOrDefaultAsync();

        if (config != null)
        {
            try
            {
                config.Password = EncryptionUtils.Decrypt(config.Password);
            }
            catch
            {
                return null;
            }
        }
        return config;
    }
}