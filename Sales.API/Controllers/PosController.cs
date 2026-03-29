using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PosController : ControllerBase
{
    private readonly PosService _service;

    public PosController(PosService service)
    {
        _service = service;
    }

    [HttpGet("tax")]
    public async Task<ActionResult<GlobalTaxConfigDto>> GetGlobalTax()
    {
        var tax = await _service.GetGlobalTaxAsync();
        return Ok(tax);
    }

    [HttpPut("tax")]
    public async Task<ActionResult<GlobalTaxConfigDto>> UpdateGlobalTax([FromBody] GlobalTaxConfigDto dto)
    {
        try
        {
            var tax = await _service.UpdateGlobalTaxAsync(dto);
            return Ok(tax);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accounts")]
    public async Task<ActionResult<PosAccountDto>> CreateAccount([FromBody] OpenAccountCreateDto dto)
    {
        var account = await _service.CreateAccountAsync(dto);
        return CreatedAtAction(nameof(GetAccountById), new { id = account.TicketId }, account);
    }

    [HttpGet("accounts/open")]
    public async Task<ActionResult<IEnumerable<PosAccountDto>>> GetOpenAccounts()
    {
        var accounts = await _service.GetOpenAccountsAsync();
        return Ok(accounts);
    }

    [HttpGet("accounts/{id}")]
    public async Task<ActionResult<PosAccountDto>> GetAccountById(int id)
    {
        var account = await _service.GetAccountAsync(id);
        if (account == null)
            return NotFound(new { message = "Account not found." });

        return Ok(account);
    }

    [HttpPost("accounts/{id}/waiter")]
    public async Task<ActionResult<PosAccountDto>> AssignWaiter(int id, [FromBody] AssignWaiterDto dto)
    {
        try
        {
            var account = await _service.AssignWaiterAsync(id, dto.WaiterId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accounts/{id}/items")]
    public async Task<ActionResult<PosAccountDto>> AddItem(int id, [FromBody] AddOrderItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var account = await _service.AddItemAsync(id, dto);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("accounts/{accountId}/items/{itemId}")]
    public async Task<ActionResult<PosAccountDto>> UpdateItem(int accountId, int itemId, [FromBody] UpdateOrderItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var account = await _service.UpdateItemAsync(accountId, itemId, dto);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accounts/{id}/validate-checkout")]
    public async Task<ActionResult> ValidateCheckout(int id)
    {
        try
        {
            await _service.ValidateCheckoutAsync(id);
            return Ok(new { message = "Checkout validation passed." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accounts/{id}/send-command")]
    public async Task<ActionResult<CommandSendResultDto>> SendCommand(int id)
    {
        var result = await _service.SendCommandAsync(id);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("accounts/{accountId}/items/{itemId}")]
    public async Task<ActionResult<PosAccountDto>> RemoveItem(int accountId, int itemId)
    {
        try
        {
            var account = await _service.RemoveItemAsync(accountId, itemId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("kds/{stationType}/pending")]
    public async Task<ActionResult<IEnumerable<KdsItemDto>>> GetKdsPending(string stationType)
    {
        var items = await _service.GetKdsPendingByStationTypeAsync(stationType);
        return Ok(items);
    }

    [HttpPatch("kds/items/{orderItemId}/status")]
    public async Task<ActionResult<KdsItemDto>> AdvanceKdsItemStatus(int orderItemId)
    {
        try
        {
            var result = await _service.AdvanceKdsItemStatusAsync(orderItemId);
            if (result == null)
                return NotFound(new { message = "Order item not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("commands/{commandId}/reprint")]
    public async Task<ActionResult<CommandReprintDto>> GetCommandReprint(int commandId)
    {
        var result = await _service.GetCommandReprintAsync(commandId);
        if (result == null)
            return NotFound(new { message = "Command not found." });

        return Ok(result);
    }

    [HttpPost("accounts/{id}/checkout")]
    public async Task<ActionResult<CheckoutResultDto>> Checkout(int id, [FromBody] CheckoutDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CheckoutAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("accounts/{id}")]
    public async Task<ActionResult<PosAccountDto>> CancelAccount(int id)
    {
        try
        {
            var account = await _service.CancelAccountAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
