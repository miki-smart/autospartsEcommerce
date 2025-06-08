using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceManagementController : ControllerBase
{
    private readonly IServiceConfigurationService _serviceConfigService;
    private readonly ILogger<ServiceManagementController> _logger;

    public ServiceManagementController(IServiceConfigurationService serviceConfigService, ILogger<ServiceManagementController> logger)
    {
        _serviceConfigService = serviceConfigService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _serviceConfigService.GetAllServicesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _serviceConfigService.GetServiceByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ServiceConfiguration service)
    {
        var result = await _serviceConfigService.AddServiceAsync(service);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ServiceConfiguration service)
    {
        var result = await _serviceConfigService.UpdateServiceAsync(id, service);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _serviceConfigService.DeleteServiceAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> Toggle(string id)
    {
        var result = await _serviceConfigService.ToggleServiceAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("{id}/health")]
    public async Task<IActionResult> Health(string id)
    {
        var svcResult = await _serviceConfigService.GetServiceByIdAsync(id);
        if (!svcResult.Success || svcResult.Service == null) return NotFound(svcResult);
        var health = await _serviceConfigService.CheckServiceHealthAsync(svcResult.Service);
        return Ok(health);
    }

    [HttpPost("reload")]
    public async Task<IActionResult> Reload()
    {
        var result = await _serviceConfigService.ReloadOcelotConfigurationAsync();
        return Ok(new { Success = result });
    }
}
