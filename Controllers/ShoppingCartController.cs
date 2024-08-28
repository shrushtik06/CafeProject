using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RolesAuth.Data;
using RolesAuth.Models;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RolesAuth.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly AppDbContext dbContext;

        public ShoppingCartController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET: ShoppingCartController
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
                // Handle the case where the user ID is not found
                return RedirectToAction("UserNotFound", "Error");
            }

            var currentUser = await dbContext.CustomerEntity
                .FirstOrDefaultAsync(c => c.UserId == currentUserId);

            if (currentUser == null)
            {
                // Handle the case where the user is not found
                return RedirectToAction("UserNotFound", "Error");
            }

            var product = await dbContext.Products
                .Include(p => p.Cafe)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                // Handle the case where the product is not found
                return RedirectToAction("ProductNotFound", "Error");
            }

            var cartItem = new CartItems
            {
                ProductId = product.ProductId,
                CustomerId = currentUser.CustomerId,
                CartFood_name = product.Name,
                Cafe_name = product.Cafe?.Name ?? "Unknown Cafe",
                Category = product.Category?.Name ?? "Unknown Category",
                Price = product.Prize,
                Quantity = 1
            };

            dbContext.CartItems.Add(cartItem);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "CartItems");
        }

        [HttpPost]
        public async Task<IActionResult> AddToOrders(int productId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
                // Handle the case where the user ID is not found
                return RedirectToAction("UserNotFound", "Error");
            }

            var currentUser = await dbContext.CustomerEntity
                .FirstOrDefaultAsync(c => c.UserId == currentUserId);

            if (currentUser == null)
            {
                return RedirectToAction("UserNotFound", "Error");
            }

            var product = await dbContext.Products
                .Include(p => p.Cafe)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return RedirectToAction("ProductNotFound", "Error");
            }

            var cartItems = await dbContext.CartItems
                .Where(item => item.CustomerId == currentUser.CustomerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                // Handle the case where the cart is empty
                return RedirectToAction("CartEmpty", "Error");
            }

            var order = new OrderEntity
            {
                OrderStatus = OrderEntity.Status.PENDING,
                TotalAmount = cartItems.Sum(item => item.Price * item.Quantity),
                CustomerId = currentUser.CustomerId,
                CafeId = product.CafeId,
                Category = product.Category?.Name ?? "Unknown Category",
                Food_name = product.Name,
            };

            dbContext.Order.Add(order);
            await dbContext.SaveChangesAsync();

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItemEntity
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Subtotal = cartItem.Price * cartItem.Quantity,
                };

                dbContext.OrderItem.Add(orderItem);
            }

            await dbContext.SaveChangesAsync();

            dbContext.CartItems.RemoveRange(cartItems);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("OrderSuccess", "Order");
        }

        public IActionResult OrderConfirmation()
        {
            var service = new SessionService();
            Session session = service.Get(TempData["Session"]?.ToString());

            if (session?.PaymentStatus == "paid")
            {
                var transaction = session.PaymentIntentId.ToString();
                return View("success");
            }

            return View("login");
        }

        [HttpPost]
        public IActionResult CheckOut()
        {
            var cartItems = dbContext.CartItems?.ToList() ?? new List<CartItems>();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "ShoppingCart");
            }

            var domain = "https://localhost:7211/";

            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"ShoppingCart/OrderConfirmation",
                CancelUrl = domain + "ShoppingCart/Login",
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>()
            };

            foreach (var item in cartItems)
            {
                var sessionListItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * item.Quantity * 100),
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.CartFood_name ?? "Product Name Not Available",
                        }
                    },
                    Quantity = item.Quantity
                };

                options.LineItems.Add(sessionListItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            TempData["Session"] = session.Id;

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }
    }
}
