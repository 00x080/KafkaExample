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

    public InventoryController(ProducerService producerService)
    {
        _producerService = producerService;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateInventory([FromBody] InventoryUpdateRequest request)
    {
        var message = JsonSerializer.Serialize(request);

        await _producerService.ProduceAsync("InventoryUpdates", message);

        return Ok($"Producer Updated Inventory Successfully. {message}");
    }
}