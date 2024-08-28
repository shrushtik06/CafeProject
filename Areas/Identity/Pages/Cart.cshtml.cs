using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RolesAuth.Areas.Identity.Pages
{
    public class CartModel : PageModel
    {
        public string NotificationMessage { get; set; }

        public void OnPostAddToCart(int itemId)
        {
            // Logic to add item to cart

            NotificationMessage = "Item successfully added to your cart!";
        }
    }
}