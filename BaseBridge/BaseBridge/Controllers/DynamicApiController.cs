using System.Data.Common;
using BaseBridge.Services;
using BaseBridge.Utils.Exception;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BaseBridge.Controllers;

[ApiController]
[EnableCors("AllowAll")]
public class DynamicApiController(
    EndpointService endpointService,
    DbConfigurationService dbConfigService,
    ILogger<DynamicApiController> logger)
    : ControllerBase
{
    [HttpGet("api/data/{**endpointPath}")]
    public async Task<IActionResult> ExecuteEndpoint(string endpointPath)
    {
        var normalizePath = "/api/data/" + endpointPath;
        var config = await dbConfigService.GetConfigAsync();

        if (config == null)
            throw new DbNotConfiguredException();
        
        var endpoint = await endpointService.GetEndpointByPathAsync(normalizePath);
        var queryParams = new Dictionary<string, object>();
        foreach (var expectedParam in endpoint.Parameters)
        {
            if (!Request.Query.ContainsKey(expectedParam.Name))
                return BadRequest(new { Message = $"Missing parameter: {expectedParam.Name}" });
            var rawValue = Request.Query[expectedParam.Name].ToString();
            object typedValue = expectedParam.Type.ToLower() switch
            {
                "int"     => int.TryParse(rawValue, out var i) ? i : throw new ArgumentException($"Parameter '{expectedParam.Name}' must be an integer."),
                "decimal" => decimal.TryParse(rawValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : throw new ArgumentException($"Parameter '{expectedParam.Name}' must be a decimal."),
                "bool"    => bool.TryParse(rawValue, out var b) ? b : throw new ArgumentException($"Parameter '{expectedParam.Name}' must be a boolean."),
                "date"    => DateTime.TryParse(rawValue, out var dt) ? dt : throw new ArgumentException($"Parameter '{expectedParam.Name}' must be a valid date."),
                _         => rawValue
            };
            
            queryParams.Add(expectedParam.Name, typedValue);
        }

        try
        {
            var dbService = new DbService(config.DbType, config.Host, config.DbName, config.User, config.Password);
            var data = await dbService.ExecuteDynamicQueryAsync(endpoint.Query, queryParams);
            return Ok(data);
        }
        catch (ArgumentException argEx)
        {
            return BadRequest(new { Message = argEx.Message });
        }
        catch(DbException dbEx)
        {
            logger.LogWarning(dbEx, "Database execution error for endpoint {Path}", normalizePath);
            return BadRequest(new
            {
                Message = "Query execution error. Verify if you send the necessary parameters(e.g, ?parameter=value)",
                Details = dbEx.Message
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error executing the endpoint");
            return StatusCode(500, "Error executing the endpoint");
        }
    }
    
}