using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoppingMvcUI.Constants;
using System.Linq.Expressions;

namespace ShoppingMvcUI.Controllers
{
    [Authorize(Roles =nameof(Roles.Admin))]
    public class AdminOperationsController : Controller
    {
        private readonly IUserOrderRepository _UserOrderRepository;
        public AdminOperationsController(IUserOrderRepository userOrderRepository)
        {
            _UserOrderRepository = userOrderRepository;
        }
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _UserOrderRepository.UserOrders(true);
            return View(orders);
        }

        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            try
            {
                await _UserOrderRepository.TogglePaymentStatus(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong");
            }
            return RedirectToAction(nameof(AllOrders));
        }

        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var order = await _UserOrderRepository.GetOrderById(orderId);

            if(order is null)
            {
                throw new InvalidOperationException($"Order with id: {orderId} does not found.");
            }

            var orderStatusList = (
                    await _UserOrderRepository.GetOrderStatuses()).Select(orderStatus
                    =>
                    {
                        return new SelectListItem
                        {
                            Value = orderStatus.Id.ToString(),
                            Text = orderStatus.StatusName,
                            Selected = order.OrderStatusId == orderStatus.Id
                        };
                    }
                    );
            var data = new UpdateOrderStatusModel
            {
                OrderId = orderId,
                OrderStatusId = order.OrderStatusId,
                OrderStatusList = orderStatusList
            };

            return View(data);

        }

        [HttpPost]

        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    data.OrderStatusList = (
                        await _UserOrderRepository.GetOrderStatuses()).Select(orderStatus =>
                        {
                            return new SelectListItem
                            {
                                Value = orderStatus.Id.ToString(),
                                Text = orderStatus.StatusName,
                                Selected = orderStatus.Id == orderStatus.Id
                            };
                        });
                    return View(data);

                }
                await _UserOrderRepository.ChangeOrderStatus(data);
                TempData["msg"] = "Updated Successfully";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Something went wrong";
            }
            return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });
        }

    }
}
