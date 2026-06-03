using BaseBridge.Database;
using BaseBridge.Models;
using BaseBridge.Utils.Exception;
using Microsoft.EntityFrameworkCore;

namespace BaseBridge.Services;

public class EndpointService(
    AppDbContext dbContext)
{
    public async Task SaveEndpointAsync(EndpointData endpoint)
    {
        var exists = await dbContext.Endpoints.AnyAsync(e => e.Name == endpoint.Name || e.Path == endpoint.Path);

        if (exists)
            throw new EndpointAlreadyExistsException();
        await dbContext.Endpoints.AddAsync(endpoint);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<EndpointData>> GetAllEndpointsAsync()
    {
        return await dbContext.Endpoints.Include(e => e.Parameters).ToListAsync();
    }

    public async Task<EndpointData> GetEndpointByPathAsync(string path)
    {
        var endpoint = await dbContext.Endpoints.Include(e => e.Parameters).FirstOrDefaultAsync(e => e.Path == path);

        if (endpoint == null)
            throw new EndpointNotFoundException(path);

        return endpoint;
    }

    public async Task DeleteEndpointAsync(string name)
    {
        var endpointToRemove = await dbContext.Endpoints.FirstOrDefaultAsync(e => e.Name == name);
        if (endpointToRemove == null)
            throw new EndpointNotFoundException();

        dbContext.Endpoints.Remove(endpointToRemove);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateEndpointAsync(string originalName, EndpointData updatedEndpoint)
    {
        var existingEndpoint = await dbContext.Endpoints.Include(e => e.Parameters)
            .FirstOrDefaultAsync(e => e.Name == originalName);
        if (existingEndpoint == null)
            throw new EndpointNotFoundException();

        if (originalName != updatedEndpoint.Name)
        {
            var exists = await dbContext.Endpoints.AnyAsync(e => e.Name == updatedEndpoint.Name);
            if (exists)
                throw new EndpointAlreadyExistsException();
        }

        existingEndpoint.Name = updatedEndpoint.Name;
        existingEndpoint.Path = updatedEndpoint.Path;
        existingEndpoint.Description = updatedEndpoint.Description;
        existingEndpoint.Query = updatedEndpoint.Query;
        existingEndpoint.IsPaginationMandatory = updatedEndpoint.IsPaginationMandatory;
        
        dbContext.RemoveRange(existingEndpoint.Parameters);
        existingEndpoint.Parameters = updatedEndpoint.Parameters;

        await dbContext.SaveChangesAsync();
    }
}