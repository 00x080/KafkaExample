using InventoryConsumer.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryConsumer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventoryConsumerController : ControllerBase
{
    private readonly ILogger<InventoryConsumerController> _logger;

    public InventoryConsumerController(ILogger<InventoryConsumerController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult ProcessInventoryUpdate([FromBody] InventoryUpdateRequest request)
    {
        return Ok("Inventory update processed successfully.");
    }
}