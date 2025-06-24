using FinanceCalc.Data;
using FinanceCalc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class BankController : Controller
{
    private readonly ISaltEdgeService _saltEdge;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BankController(ISaltEdgeService saltEdge, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _saltEdge = saltEdge;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Connect()
    {
        var user = await _userManager.GetUserAsync(User);
        var customerId = await _saltEdge.EnsureCustomerAsync(user.Id); // create customer if needed

        var connectUrl = await _saltEdge.BuildConnectUrlAsync(customerId);
        return Redirect(connectUrl);
    }

    public async Task<IActionResult> Callback(string connection_id, string customer_id)
    {
        var user = await _userManager.GetUserAsync(User);

        _context.BankConnections.Add(new BankConnection
        {
            ConnectionId = connection_id,
            CustomerId = customer_id,
            ConnectedAt = DateTime.UtcNow,
            ApplicationUserId = user.Id
        });

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Dashboard");
    }
}
