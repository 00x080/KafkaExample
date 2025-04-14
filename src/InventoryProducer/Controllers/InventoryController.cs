using System.Text.Json;
using InventoryProducer.Models;
using InventoryProducer.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryProducer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventoryController : Controller
{
    private readonly ProducerService _producerService;
    private readonly ILogger<ProducerService> _logger;

    public InventoryController(ProducerService producerService, ILogger<ProducerService> logger)
    {
        _producerService = producerService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateInventory([FromBody] InventoryUpdateRequest request)
    {
        var message = JsonSerializer.Serialize(request);

        _logger.LogInformation($"Producer sending inventory update: {message}");

        await _producerService.ProduceAsync("InventoryUpdates", message);

        return Ok($"Producer Updated Inventory Successfully. {message}");
    }
}