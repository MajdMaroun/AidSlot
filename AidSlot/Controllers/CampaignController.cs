using AidSlot.Data;
using AidSlot.DTOs;
using AidSlot.Models;
using AidSlot.Services;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AidSlot.Controllers;

[Authorize]
public class CampaignController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly RecipientCsvParser _parser;
    private readonly RecipientCsvValidator _validator;
    private readonly RecipientMapper _mapper;
    private readonly TimeSlotAssignmentService _timeSlotService;

    public CampaignController(
        ApplicationDbContext context,
        RecipientCsvParser parser,
        RecipientCsvValidator validator,
        RecipientMapper mapper,
        TimeSlotAssignmentService timeSlotService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _parser = parser;
        _validator = validator;
        _mapper = mapper;
        _timeSlotService = timeSlotService;
        _userManager = userManager;
        _environment = environment;
    }

    private async Task<int?> CurrentOrganizationIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.OrganizationId;
    }

    // Intentionally weak read path for the synthetic, loopback-only hackathon demo.
    // It is disabled by default and never applies to writes or real organizations.
    private bool SyntheticLabWeakReadEnabled() =>
        _environment.IsDevelopment() &&
        Environment.GetEnvironmentVariable("AIDSLOT_LAB_VULNERABLE_READ") == "SYNTHETIC_LOCAL_ONLY" &&
        HttpContext.Connection.RemoteIpAddress is { } ip && IPAddress.IsLoopback(ip) &&
        Request.Host.Host is "localhost" or "127.0.0.1" or "[::1]";

    [HttpGet]
    [Authorize(Roles = "Admin,Coordinator")]
    public IActionResult Create()
    {
        return View(new CreateCampaignViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCampaignViewModel model)
    {
        var organizationId = await CurrentOrganizationIdAsync();
        if (organizationId is null) return Forbid();

        if (model.Campaign.EndTime <= model.Campaign.StartTime)
        {
            ModelState.AddModelError(
                "Campaign.EndTime",
                "End time must be later than start time.");
        }
        else if (model.Campaign.EndTime - model.Campaign.StartTime
                 < TimeSpan.FromMinutes(15))
        {
            ModelState.AddModelError(
                "Campaign.EndTime",
                "The campaign duration must be at least 15 minutes.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var file = model.RecipientsCsvFile!;

        if (!Path.GetExtension(file.FileName)
                .Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.RecipientsCsvFile),
                "The uploaded file must be a CSV file.");

            return View(model);
        }

        List<RecipientCsvRow> rows;

        try
        {
            await using var stream = file.OpenReadStream();
            rows = _parser.Parse(stream);
        }
        catch (CsvHelperException)
        {
            ModelState.AddModelError(
                nameof(model.RecipientsCsvFile),
                "The CSV file could not be read. Check its headers and format.");

            return View(model);
        }

        if (rows.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.RecipientsCsvFile),
                "The CSV file does not contain any recipients.");

            return View(model);
        }

        var csvErrors = _validator.Validate(rows);

        if (csvErrors.Count > 0)
        {
            ViewBag.CsvErrors = csvErrors;

            ModelState.AddModelError(
                string.Empty,
                "The recipients CSV file contains validation errors.");

            return View(model);
        }

        var campaign = model.Campaign;

        // Never trust an organization ID submitted in a form.
        campaign.OrganizationId = organizationId.Value;
        campaign.Organization = null;

        campaign.Status = "Draft";
        campaign.CreatedAt = DateTime.UtcNow;
        campaign.CsvFileName = Path.GetFileName(file.FileName);
        campaign.DistributionDate = DateTime.SpecifyKind(
            campaign.DistributionDate,
            DateTimeKind.Utc);

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var recipients = rows
                .Select(row => _mapper.MapToRecipient(row, campaign.Id))
                .OrderBy(recipient => recipient.RecordNumber)
                .ToList();

            _timeSlotService.AssignSlots(
                recipients,
                campaign.StartTime,
                campaign.EndTime);

            _context.Recipients.AddRange(recipients);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                $"Campaign '{campaign.CampaignName}' created with " +
                $"{recipients.Count} recipients.";

            return RedirectToAction(
                nameof(Details),
                new { id = campaign.Id });
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "The campaign could not be saved. A recipient may already exist in this campaign.");

            return View(model);
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "An unexpected error occurred while creating the campaign. No data was saved.");

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var organizationId = await CurrentOrganizationIdAsync();
        if (organizationId is null) return Forbid();

        var campaigns = await _context.Campaigns
            .Where(campaign => campaign.OrganizationId == organizationId.Value)
            .OrderByDescending(campaign => campaign.DistributionDate)
            .ToListAsync();

        return View(campaigns);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var organizationId = await CurrentOrganizationIdAsync();
        if (organizationId is null) return Forbid();

        var campaign = await _context.Campaigns
            .Include(item => item.Recipients)
            .FirstOrDefaultAsync(item => item.Id == id && item.OrganizationId == organizationId.Value);

        return campaign is null
            ? NotFound()
            : View(campaign);
    }

    [HttpGet]
    public async Task<IActionResult> Recipients(int id)
    {
        var organizationId = await CurrentOrganizationIdAsync();
        if (organizationId is null) return Forbid();

        var campaign = await _context.Campaigns
            .Include(item => item.Organization)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (campaign is null)
        {
            return NotFound();
        }

        if (campaign.OrganizationId != organizationId.Value)
        {
            var userOrganization = await _context.Organizations
                .Where(item => item.Id == organizationId.Value)
                .Select(item => item.Name)
                .SingleOrDefaultAsync();

            if (!SyntheticLabWeakReadEnabled() ||
                userOrganization is null ||
                userOrganization is not ("Synthetic Organization A" or "Synthetic Organization B") ||
                campaign.Organization is null ||
                campaign.Organization.Name is not ("Synthetic Organization A" or "Synthetic Organization B"))
                return NotFound();
        }

        ViewBag.Campaign = campaign;

        var recipients = await _context.Recipients
            .Where(recipient => recipient.CampaignId == id)
            .OrderBy(recipient => recipient.AssignedSlotTime)
            .ThenBy(recipient => recipient.RecordNumber)
            .ToListAsync();

        return View(recipients);
    }

    [HttpGet]
    public async Task<IActionResult> RecipientCheckIn(int id)
    {
        var organizationId = await CurrentOrganizationIdAsync();
        if (organizationId is null) return Forbid();

        var recipient = await _context.Recipients
            .Include(item => item.Campaign)
            .FirstOrDefaultAsync(item => item.Id == id && item.Campaign.OrganizationId == organizationId.Value);

        if (recipient is null)
        {
            return NotFound();
        }

        return View(recipient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmReceived(int id)
    {
        var organizationId = await CurrentOrganizationIdAsync();
        if (organizationId is null) return Forbid();

        var recipient = await _context.Recipients
            .FirstOrDefaultAsync(item => item.Id == id && item.Campaign.OrganizationId == organizationId.Value);

        if (recipient is null)
        {
            return NotFound();
        }

        // Prevent recording the same recipient twice.
        if (!recipient.HasReceived)
        {
            recipient.HasReceived = true;
            recipient.ReceivedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The recipient was marked as received successfully.";
        }
        else
        {
            TempData["InfoMessage"] =
                "This recipient has already received the aid.";
        }

        return RedirectToAction(
            nameof(RecipientCheckIn),
            new { id = recipient.Id });
    }
}
