using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ShoppingMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext context , IHttpContextAccessor httpContextAccessor ,UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<int> AddItem(int bookId , int qty)
        {
            string userId = GetUserId();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                
                if (string.IsNullOrEmpty(userId)) { throw new Exception("User is not logged in"); }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                    };
                    _context.shoppingCarts.Add(cart);
                }
                _context.SaveChanges();
                var cartItem = _context.CartDetails.FirstOrDefault(obj => obj.ShoppingCartId == cart.Id && obj.BookId == bookId);
                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _context.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                       UnitPrice = book.Price
                    };
                    _context.CartDetails.Add(cartItem);
                }
                _context.SaveChanges();
                transaction.Commit();

                
            }
            catch (Exception ex)
            {
                throw new Exception("User is not logged in");
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
           
        }
        
        public async Task<int> RemoveItem(int bookId)
        {
            //using var transaction = _context.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                
                if (string.IsNullOrEmpty(userId)) { throw new Exception("user is not logged in"); }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new Exception("Invalid cart");
                }
                _context.SaveChanges();
                var cartItem = _context.CartDetails.FirstOrDefault(obj => obj.ShoppingCartId == cart.Id && obj.BookId == bookId);
                if (cartItem is null)
                {
                    throw new Exception("No Items in cart");
                }
                else if(cartItem.Quantity ==1)
                {
                    _context.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity-1;
                }
                _context.SaveChanges();
                //transaction.Commit();

            }
            catch (Exception ex)
            {
                throw new Exception("Somethingg went wrong");
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if(userId is null)
                throw new Exception("Invalid Userid");
            var shoppingCart = await _context.shoppingCarts
                                        .Include(obj => obj.CartDetails)
                                        .ThenInclude(obj => obj.Book)
                                        .ThenInclude(obj => obj.Genre)
                                        .Where(obj => obj.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart =await  _context.shoppingCarts.FirstOrDefaultAsync(obj => obj.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId="")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _context.shoppingCarts
                              join cartDetail in _context.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { cartDetail.Id }
                              ).ToListAsync();
            return data.Count;
        }

        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid Cart");
                var cartDetails = _context.CartDetails.Where(obj => obj.ShoppingCartId == cart.Id).ToList();

                if (cartDetails.Count == 0)
                    throw new Exception("Cart is empty");
                var pendingRecord = _context.OrderStatuses.FirstOrDefault(obj => obj.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new Exception("Order status does not have pending status");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.Now,
                    Name = model.Name,
                    Email= model.Email,
                    MobileNumber = model.MobileNumber,
                    Address = model.Address,
                    PaymentMethod = model.PaymentMethod,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
                foreach(var item in cartDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                    };
                    _context.OrderDetails.Add(orderDetail);
                }
                _context.SaveChanges();

                // remove cart details after creating order with pending state.

                _context.CartDetails.RemoveRange(cartDetails);
                _context.SaveChanges();
                transaction.Commit();
                return true;

            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private string GetUserId()
        {
            var userPrincipal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(userPrincipal);
            return userId;
        }
    }
}
