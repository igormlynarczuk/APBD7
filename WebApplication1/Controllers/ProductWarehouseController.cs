namespace DefaultNamespace;

[ApiController]
[Route("[controller]")]
public class ProductWarehouseController : ControllerBase
{
    private readonly ProductWarehouseRepository _repository;

    public ProductWarehouseController(ProductWarehouseRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("AddProductToWarehouse")]
    public IActionResult AddProductToWarehouse([FromBody] ProductToWarehouseRequest request)
    {
        try
        {
            // Wywołanie metody z ProductWarehouseRepository do dodawania produktu do magazynu
            var newProductId = _repository.AddProductToWarehouse(request.IdProduct, request.IdWarehouse, request.Amount, request.CreatedAt);
            return Ok(newProductId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("AddProductToWarehouseFromProcedure")]
    public IActionResult AddProductToWarehouseFromProcedure([FromBody] ProductToWarehouseRequest request)
    {
        // Implementacja logiki dla wywołania procedury składowanej z ProductWarehouseRepository
    }
}
