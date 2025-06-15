# Coding Conventions for AutoParts E-Commerce Development

## 1. Naming Conventions

- **Classes**: PascalCase (e.g., `ProductService`)
- **Interfaces**: Prefix with `I` (e.g., `IProductRepository`)
- **Private fields**: `_camelCase` (e.g., `_productRepository`)
- **Properties**: PascalCase (e.g., `ProductName`)
- **Constants**: ALL_CAPS_WITH_UNDERSCORES (e.g., `DEFAULT_CATEGORY`)
- **File names**: Match class names exactly (e.g., `ProductService.cs` for `ProductService`)

## 2. Architecture Guidelines

- **Domain Layer**: Pure business logic, no dependencies on external systems
- **Application Layer**: Orchestrates domain objects, contains use cases
- **Infrastructure Layer**: Implementation of interfaces defined in domain/application
- **API Layer**: Thin controllers, minimal business logic, validation and auth concerns

## 3. API Design

- Use REST conventions consistently (GET, POST, PUT, DELETE)
- Version APIs in URI path: `/api/v1/products`
- Return appropriate status codes (200, 201, 400, 401, 403, 404, 500)
- Use consistent response objects with metadata: `{ data: {}, error: null, success: true }`

## 4. Documentation

```csharp
/// <summary>
/// Creates a new product with the specified details
/// </summary>
/// <param name="request">Product creation details</param>
/// <returns>Newly created product with ID</returns>
/// <exception cref="ValidationException">Thrown when request is invalid</exception>
```

## 5. Error Handling & Logging

```csharp
try
{
    // Operation that might fail
    _logger.LogDebug("Attempting to create product {ProductName}", request.Name);
    var result = await _productService.CreateProductAsync(request);
    _logger.LogInformation("Product {ProductId} created successfully", result.Id);
    return result;
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed for product creation");
    return BadRequest(new ErrorResponse(ex.Message));
}
```

---

## Customizing Copilot Responses

To make GitHub Copilot follow your conventions consistently:

### 1. Use Structured Prompts

```
@copilot Create a controller method for updating a product with CQRS pattern that:
- Uses MediatR to dispatch the command
- Includes proper validation
- Returns appropriate status codes
- Follows our logging convention
```

### 2. Include Examples in Prompts

```
@copilot Create a method like this example but for Product category:
```csharp
public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
{
    _logger.LogInformation("Getting order with ID {OrderId}", id);
    var query = new GetOrderByIdQuery(id);
    var result = await _mediator.Send(query);
    return result != null ? Ok(result) : NotFound();
}
```

### 3. Use Follow-up Instructions

After receiving code from Copilot:
- "Apply our error handling pattern to this code"
- "Refactor this to follow our repository pattern"
- "Add appropriate XML documentation to this class"


