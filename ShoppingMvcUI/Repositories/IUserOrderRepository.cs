namespace ShoppingMvcUI.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOrders(bool getAll=false);
        Task<IEnumerable<Order>> GetUserOrderByOrderId(int orderId);

        Task TogglePaymentStatus (int orderId);

        Task ChangeOrderStatus(UpdateOrderStatusModel data);

        Task<Order?> GetOrderById(int id);

        Task<IEnumerable<OrderStatus>> GetOrderStatuses();


    }
}