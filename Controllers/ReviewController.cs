using Microsoft.AspNetCore.Mvc;
using RolesAuth.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RolesAuth.Data;

public class ReviewController : Controller
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    // GET: ReviewFeedback
    public async Task<IActionResult> Index()
    {
        return View(await _context.Review.ToListAsync());
    }

    // GET: ReviewFeedback/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ReviewFeedback/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserName,Feedback,Rating")] Review reviewFeedback)
    {
        if (ModelState.IsValid)
        {
            _context.Add(reviewFeedback);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(reviewFeedback);
    }
}
