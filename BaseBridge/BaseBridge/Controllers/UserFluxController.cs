using System.Text.RegularExpressions;
using BaseBridge.Models;
using BaseBridge.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using BaseBridge.Services;
using BaseBridge.Security;
using BaseBridge.Validations;
using Microsoft.AspNetCore.Cors;

namespace BaseBridge.Controllers;

[ApiController]
[Route("api/admin")]
[EnableCors("AllowSvelteFrontend")]
[ApiKeyAuth]
public class UserFluxController(
    EndpointService endpointService,
    AiManagerService aiManagerService,
    QueryValidator validator,
    DbConfigurationService dbConfigurationService) : ControllerBase
{

    [HttpPost("db-config")]
    public async Task<IActionResult> SaveDbConfig([FromBody] DbConfig config)
    {
        await dbConfigurationService.SaveConfigAsync(config);
        return Ok(new { Message = "Credentials for the database were saved successfully" });
    }

    [HttpPost("ai-config")]
    public async Task<IActionResult> SaveAiConfig([FromBody] AiConfig config)
    {
        await aiManagerService.SaveAiConfig(config);
        return Ok(new { Message = $"AI Provider '{config.Provider}' configured successfully" });
    }

    [HttpPost("generate-endpoint")]
    public async Task<IActionResult> Generate([FromBody] GenerateEndpointRequest request)
    {
        var dbConfig = await dbConfigurationService.GetConfigAsync();

        if (dbConfig == null)
            throw new ArgumentNullException(nameof(dbConfig), "Database not configured yet");
        var dbService = new DbService(dbConfig.DbType, dbConfig.Host, dbConfig.DbName, dbConfig.User,
            dbConfig.Password);
        var schema = await dbService.GetFullSchemaAsync();
        
        string sql = await aiManagerService.GenerateDynamicQueryAsync(request.EndpointDescription, schema, dbConfig.DbType, request.Parameters);
        
        validator.Validate(sql, dbConfig.DbType);

        var finalParameters = request.Parameters.Count > 0 ? request.Parameters :
            Regex.Matches(sql, @"@([a-zA-Z0-9_]+)")
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .Select(p => new EndpointParameter() { Name = p, Type = "string" })
                .ToList();
        
        return Ok(new
        {
            GeneratedQuery = sql,
            Parameters = finalParameters,
            Message = "Verify and save the query"
        });
    }

    [HttpPost("save-endpoint")]
    public async Task<IActionResult> Save([FromBody] SaveEndpointRequest request)
    {
        var dbConfig = await dbConfigurationService.GetConfigAsync();

        if (dbConfig == null)
            throw new Exception("Cannot save an endpoint until you set database credentials");
        
        validator.Validate(request.ValidatedQuery, dbConfig.DbType);

        var newEndpoint = new EndpointData
        {
            Name = request.Name,
            Path = $"/api/data/{request.Path}",
            Query = request.ValidatedQuery,
            Description = request.EndpointDescription,
            Parameters = request.Parameters
        };
        
        await endpointService.SaveEndpointAsync(newEndpoint);
        return Ok(new { Message = $"{request.Name} endpoint saved and active"});
    }

    [HttpDelete("delete-endpoint")]
    public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
    {
        await endpointService.DeleteEndpointAsync(request.Name);
        return Ok(new { Message = "Endpoint deleted successfully" });
    }

    [HttpGet("all-endpoints")]
    public async Task<IActionResult> GetAllEndpoints()
    {
        return Ok(await endpointService.GetAllEndpointsAsync());
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var dbConfig = await dbConfigurationService.GetConfigAsync();
        var aiConfig = await aiManagerService.GetAiConfig();

        return Ok(new
        {
            IsDbConfigured = dbConfig != null,
            IsAiConfigured = aiConfig != null
        });
    }

    [HttpGet("database-schema")]
    public async Task<IActionResult> GetDatabaseSchema()
    {
        var dbConfig = await dbConfigurationService.GetConfigAsync();
        if (dbConfig == null)
            return BadRequest("Database not configured");
        var dbService = new DbService(dbConfig.DbType, dbConfig.Host, dbConfig.DbName, dbConfig.User, dbConfig.Password);
        var schema = await dbService.GetFullSchemaAsync();
        return Ok(schema);
    }
}