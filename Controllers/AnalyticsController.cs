using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RolesAuth.Models;  // Assuming OrderEntity is in this namespace
using System.Collections.Generic;
using RolesAuth.Data;

namespace RolesAuth.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OrderAnalytics()
        {
            // Fetching all orders from the database
            var orders = _context.Order;

            // Grouping orders by date
            var groupedOrders = orders.GroupBy(o => o.Category)
                                     .Select(g => new
                                     {
                                         Category = g.Key,
                                         Count = g.Count(),
                                         TotalAmount = g.Sum(o => o.TotalAmount)
                                     }).ToList();

            // Create a ViewModel to pass the data to the view
            var viewModel = new OrderAnalyticsViewModel
            {
                Categories = groupedOrders.Select(g => g.Category).ToList(),
                OrderCounts = groupedOrders.Select(g => g.Count).ToList(),
                TotalAmounts = groupedOrders.Select(g => g.TotalAmount).ToList()
            };

            // Pass the data to the view
            return View(viewModel);
        }
    }
}
