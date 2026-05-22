# Examen Parcial — Daniel Roland Peñaranda Colque

## Sección 1 — Identificación

- **Nombre completo:** Daniel Roland Peñaranda Colque
- **Pareja asignada para el sábado:**
- **Repositorio de Inventario:** [https://github.com/DanyElAlgo/ISW-312-inventory]
- **Repositorio de Ventas:** [https://github.com/DanyElAlgo/ISW-312-inventory] (este mismo)
- **Contrato API acordado en grupo:** [link al archivo contrato-api.yaml en este repo]
- **URL del Swagger autogenerado** (cuando levantás el backend localmente): http://localhost:5002/swagger/index.html

## Sección 2 — Decisiones técnicas con snippets

### 2.1 Árbol de carpetas del backend de Ventas

Pegá la estructura de carpetas de tu proyecto de Ventas. Ejemplo:

```
Sales.API/
├── Controllers/
├── DTOs/
├── HttpClients/
├── Migrations/
├── Models/
├── Repositories/
├── Services/
```

Explicá en 2-3 líneas por qué la organizaste así.

Lo hice de esta forma para tener una estructura simple y directa (parecida a Clean Architecture, pero sin interfaces porque a mi parecer son redundantes). Controllers, DTOs, Servicios y Modelos. La carpeta HttpClients aloja las solicitudes que se enviarán a inventario para mantenerlo separado del resto y así conservar la claridad.

### 2.2 Flujo de "registrar una venta"

Pegá los snippets del código que se ejecuta cuando un usuario confirma una venta, en orden:

1. El endpoint que recibe el request (Controller).
```c#
[HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/payment")]
[ProducesResponseType(typeof(PayTicketContractResponse), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(404)]
[ProducesResponseType(typeof(ProcessRestaurantOrderPaymentResultDto), 409)]
public async Task<IActionResult> PayTicket(
    string companyCen,
    string ticketCen,
    [FromBody] PayTicketContractRequest request)
{
    if (string.IsNullOrWhiteSpace(request.PaymentMethodCode))
        return BadRequest("paymentMethodCode is required.");

    try
    {
        var (success, conflict) = await _paymentsService.PayTicketAsync(companyCen, ticketCen, request.PaymentMethodCode);

        if (conflict != null)
            return Conflict(conflict);

        if (success == null)
            return NotFound();

        return Ok(success);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(ex.Message);
    }
}
```
2. La capa intermedia que procesa la lógica (Service / Use Case / Handler).
```c#
public async Task<(PayTicketContractResponse? success, ProcessRestaurantOrderPaymentResultDto? conflict)>
    PayTicketAsync(string companyCen, string ticketCen, string paymentMethodCode)
{
    if (!int.TryParse(ticketCen, out var ticketId))
        throw new InvalidOperationException("Invalid ticketCen.");

    var ticket = await _tickets.GetByIdAsync(ticketId, includeItems: true, includeStatus: true);
    if (ticket == null)
        throw new InvalidOperationException("Ticket not found.");

    var statusName = ticket.Status?.Name?.ToLower() ?? "";
    if (statusName is not ("open" or "abierto"))
        throw new InvalidOperationException("Ticket is not open.");

    var waiter = await _orderTicketsService.GetAssignedWaiterAsync(ticketId);
    if (waiter == null)
        throw new InvalidOperationException("A waiter must be assigned before payment.");

    var items = ticket.OrderItems.Where(i => !string.IsNullOrWhiteSpace(i.ProductCen)).ToList();
    if (!items.Any())
        throw new InvalidOperationException("Ticket has no items.");

    var paymentType = await _paymentTypes.FindByCodeOrNameAsync(paymentMethodCode);
    if (paymentType == null)
        throw new InvalidOperationException($"Payment method '{paymentMethodCode}' not found.");

    var validateDto = new StockValidationRequest
    {
        WarehouseCen = _integrationOptions.WarehouseCen,
        Source = _integrationOptions.Source,
        ReferenceCen = BuildAccountNumber(ticketId),
        Items = items.Select(i => new StockValidationItemDto
        {
            ProductCen = i.ProductCen!,
            Quantity = (decimal)(i.Qty ?? 0)
        }).ToList()
    };

    var validation = await _inventoryClient.ValidateStockAsync(_integrationOptions.CompanyCen, validateDto);
    if (validation == null)
        throw new InvalidOperationException("Could not validate stock.");

    if (!validation.IsValid)
    {
        var subtotal = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var tax = subtotal * (ticket.TaxRateSnapshot ?? 0);
        var conflict = new ProcessRestaurantOrderPaymentResultDto
        {
            IsSuccess = false,
            Message = "Insufficient stock.",
            Subtotal = subtotal,
            TaxAmount = tax,
            Total = subtotal + tax,
            Insufficiencies = validation.Requirements.Select(r => new StockInsufficiencyResponseDto
            {
                ProductCen = r.ProductCen,
                ProductName = r.ProductName,
                WarehouseCen = r.WarehouseCen,
                RequestedQuantity = (int)r.RequestedQuantity,
                AvailableQuantity = (int)r.AvailableQuantity,
                MissingQuantity = (int)r.MissingQuantity
            }).ToList()
        };
        return (null, conflict);
    }

    var consumeDto = new StockConsumeRequest
    {
        WarehouseCen = _integrationOptions.WarehouseCen,
        Source = _integrationOptions.Source,
        ReferenceCen = BuildAccountNumber(ticketId),
        Reason = $"Sale — ticket #{ticketId}",
        Items = items.Select(i => new StockConsumeItemDto
        {
            ProductCen = i.ProductCen!,
            Quantity = (decimal)(i.Qty ?? 0)
        }).ToList()
    };

    var deduction = await _inventoryClient.ConsumeStockAsync(_integrationOptions.CompanyCen, consumeDto);
    if (deduction == null || !deduction.Success)
        throw new InvalidOperationException(deduction?.Message ?? "Stock deduction failed.");

    var payment = _payments.Add(new Payment
    {
        OrderId = ticketId,
        PaymentTypeId = paymentType.Id,
        PaidAt = DateTime.UtcNow
    });
    ticket.StatusId = await _statuses.GetPaidStatusIdAsync();
    await _uow.SaveChangesAsync();

    var sub = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
    var taxAmount = sub * (ticket.TaxRateSnapshot ?? 0);

    return (new PayTicketContractResponse
    {
        SaleCen = $"SALE-{payment.Id}",
        TicketCen = ticketId.ToString(),
        Status = "Pagado",
        Subtotal = sub,
        TaxAmount = taxAmount,
        Total = sub + taxAmount,
        InventoryDocumentCen = deduction.DocumentCen
    }, null);
}
```
3. La parte que llama al Inventario del compañero (HttpClient o equivalente).
```c#
public async Task<StockConsumeResponse?> ConsumeStockAsync(string companyCen, StockConsumeRequest dto)
{
    try
    {
        var response = await _http.PostAsJsonAsync($"api/inventory/companies/{companyCen}/stock/consume", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StockConsumeResponse>();
    }
    catch (HttpRequestException)
    {
        throw new InvalidOperationException(
            "Inventory service is unavailable. Cannot deduct stock at this time.");
    }
}
```
4. La parte que persiste la venta en tu BD.
```c#
var payment = _payments.Add(new Payment
    {
        OrderId = ticketId,
        PaymentTypeId = paymentType.Id,
        PaidAt = DateTime.UtcNow
    });
    ticket.StatusId = await _statuses.GetPaidStatusIdAsync();
    await _uow.SaveChangesAsync();
```

Explicá en 3-5 líneas por qué dividiste así las responsabilidades.

Para mantener simple el repositorio mientras se conserva la legibilidad. Por eso separé las llamadas a APIs externos en su propia carpeta. Al separar las lógicas, el mantenimiento también se vuelve más fácil de cubrir.

### 2.3 Llamada al Inventario del compañero

Pegá el código exacto donde tu Ventas llama al API del Inventario del compañero.

```c#
public async Task<StockConsumeResponse?> ConsumeStockAsync(string companyCen, StockConsumeRequest dto)
{
    try
    {
        var response = await _http.PostAsJsonAsync($"api/inventory/companies/{companyCen}/stock/consume", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StockConsumeResponse>();
    }
    catch (HttpRequestException)
    {
        throw new InvalidOperationException(
            "Inventory service is unavailable. Cannot deduct stock at this time.");
    }
}
```

Respondé brevemente:
- ¿Qué pasa si el compañero responde con código 200 OK?
Se continúa el servicio y se retorna el `PayTicketContractResponse` al módulo de Ventas
- ¿Qué pasa si responde con 404 o 500?
El proceso de venta se detiene, se lanza una `InvalidOperationException`. 
- ¿Qué pasa si el compañero está caído (timeout)?
Este es el peor caso, ahora mismo no hay un soporte para ello, lo cual causará que el sistema retorne `InvalidOperationException` y un problema de concurrencia dependiendo de lo que pudo hacer el API de Inventario antes de colgarse.

### 2.4 Configuración de la URL del compañero

Pegá:
- La línea relevante de tu `.env.example` o `appsettings.json`.
```json
"Urls": "http://localhost:5002",
"InventoryApi": {
"BaseUrl": "http://localhost:5001"
}
```
- El código que lee esa configuración y la usa para construir la llamada HTTP.
```c#
builder.Services.AddHttpClient<InventoryClient>(client =>
{
    var baseUrl = builder.Configuration["InventoryApi:BaseUrl"]
        ?? throw new InvalidOperationException("InventoryApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});
```

Explicá en 1 línea cómo cambiarías esa URL si el sábado tu pareja levanta su backend en otra IP.

Cambiando el `appsettings.Development.json`, la línea debajo de la declaración de `"InventoryApi"`.

## Sección 3 — Sobre el trabajo en grupo del contrato API

- **3.1** ¿Hubo desacuerdos al definir el contrato? ¿Cuáles?
- **3.2** ¿Cómo se resolvieron?
- **3.3** ¿Qué propusiste vos específicamente que quedó en el contrato final?

## Sección 4 — Teoría aplicada

Respondé cada pregunta en 1-2 párrafos. Está permitido usar IA para mejorar redacción, pero las respuestas deben hacer referencia explícita a tu propio código o decisiones.

**4.1** Tu compañero te avisa que va a cambiar el campo `cantidad` por `qty` en su respuesta del endpoint de stock. Tu sistema ya consume ese endpoint. Explicá qué riesgos genera ese cambio y qué prácticas conocés para evitar que un cambio así rompa los sistemas que dependen de su API.

Usar qty, aunque suene más sencillo y hasta obvio, es mala práctica porque es un diminutivo, qué tal si este diminutivo tiene otro significado en otros sistemas? Es por eso que se sugiere siempre utilizar nombre completos. Y esto es solo la nomenclatura, aún no se considera el hecho de que los sistemas ya consumen el endpoint, en casos así, es mucho peor ya que los otros sistemas no detectarán el nombre al ser este distinto, causando nulos, perdidas de información y caídas de sistema si los campos son obligatorios.

Si se va a realizar un cambio de ese tipo, no solo hay que informar al equipo completo, también hay que llegar a un acuerdo sobre si es verdaderamente necesario (para que TODOS apliquen el mismo cambio) o si es irrelevante (nadie cambia nada, sigue siendo `cantidad`).

**4.2** Tu sistema de Ventas hace una petición al Inventario para descontar stock. La red se cae justo después de que Inventario procesó el descuento pero antes de que la respuesta llegue a Ventas. ¿Qué problema se genera? ¿Cómo lo manejarías?

Si no llega la respuesta de vuelta a ventas, habrá un error de concurrencia, donde el módulo de Ventas pensará que Inventario no realizó ninguna transacción, por lo tanto, cancelando la de Ventas. Esto hace que Inventario tenga un registro de stock reducido mientras Ventas dice que nunca se generó dicha venta.

Una solución adecuada es utilizar un identificador de la transacción realizada y verificar si esta ya se encuentra en Inventario, si se valida que ya existe la venta, Inventario debe retornar un mensaje existoso a Ventas, devolviendo así la paridad entre ambos.

**4.3** Si el Inventario del compañero está caído, ¿debería tu Ventas permitir seguir registrando ventas? Justificá considerando ventajas y desventajas de cada postura. ¿Qué hace TU sistema hoy en ese caso?

Desde mi punto de vista, es más seguro y directo no permitir ventas cuando el Inventario está caído. Así evito acumular problemas de concurrencia. Claro está que al hacer esto, también dejas al cliente en espera.

Mi sistema actualmente envía una solicitud de Ventas a Inventario, si recibe un error de solicitud HTTP, entonces cancela su propia transacción. Simple y efectivo si el sistema de Inventario está completamente caído.

**4.4** Explicá por qué tener la URL del compañero hardcodeada como `http://localhost:5000` es un problema. ¿Cuál es la solución correcta y cómo la implementaste vos?

Cualquier URL que tenga 'localhost' es exclusiva de la máquina en la que se abrió el API, lo que significa que nadie, ni siquiera en una misma red local, puede acceder a dicho API.

Mi solución temporal fue reemplazar los `appsettings.json` para que usen una ruta wildcard (`"Urls": "http://*:5002"`), sin embargo, esto sigue siendo hardcodeado y no es visible para los compañeros que quieren acceder a mi API, lo correcto es usar archivos `.env` para alojar las rutas.