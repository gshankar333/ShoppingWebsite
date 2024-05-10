using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoppingMvcUI.Models;

namespace ShoppingMvcUI.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserOrderRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager) 
        {
            _context = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("user is not logged in");
            var orders = await _context.Orders
                                .Include(obj => obj.OrderStatus)
                                .Include(obj => obj.OrderDetail)
                                
                                .ThenInclude(obj => obj.Book)
                                .ThenInclude(obj => obj.Genre)
                                .Where(obj => obj.UserId==userId)
                                .ToListAsync();
            return orders;
        }
        public async Task<IEnumerable<Order>> GetUserOrderByOrderId(int orderId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("user is not logged in");
            var orders = await _context.Orders
                                .Include(obj => obj.OrderStatus)
                                .Include(obj => obj.OrderDetail)

                                .ThenInclude(obj => obj.Book)
                                .ThenInclude(obj => obj.Genre)
                                .Where(obj => obj.UserId == userId && obj.Id == orderId)
                                .ToListAsync();
            return orders;
        }
        private string GetUserId()
        {
            var userPrincipal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(userPrincipal);
            return userId;
        }

        public async Task<IEnumerable<Order>> UserOrders(bool getAll = false)
        {
            var orders = _context.Orders
                                .Include(obj => obj.OrderStatus)
                                .Include(obj => obj.OrderDetail)
                                .ThenInclude(obj => obj.Book)
                                .ThenInclude(obj => obj.Genre).AsQueryable();
            if (!getAll)
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId)) throw new Exception("User is not logged in");
                orders = orders.Where(obj => obj.UserId == userId);
                return await orders.ToListAsync();
            }
            return await orders.ToListAsync();
        }
        public async Task ChangeOrderStatus(UpdateOrderStatusModel data)
        {
            var order = await _context.Orders.FindAsync(data.OrderId);
            if(order is null)
            {
                throw new InvalidOperationException($"Order with the id: {data.OrderId} does not found");
            }
            order.OrderStatusId = data.OrderStatusId;
            await _context.SaveChangesAsync();
        }
        public async Task TogglePaymentStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if(order is null)
            {
                throw new InvalidOperationException($"Order with id: {orderId} does not found");
            }
            order.IsPaid= !order.IsPaid;
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
            return await _context.OrderStatuses.ToListAsync();
        }
    }
}
