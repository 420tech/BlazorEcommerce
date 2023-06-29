using Microsoft.AspNetCore.Components;

namespace BlazorEcommerce.Client.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly IAuthService _authService;

        public OrderService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, NavigationManager navigationManager, IAuthService authService)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
            _authService = authService;
        }

        public async Task<OrderDetailsResponse> GetOrderDetails(int orderId)
        {
            var result = await _httpClient.GetFromJsonAsync<ServiceResponse<OrderDetailsResponse>>($"api/order/{orderId}");
            return result.Data;
        }

        public async Task<List<OrderOverviewResponse>> GetOrders()
        {
            var results = await _httpClient.GetFromJsonAsync<ServiceResponse<List<OrderOverviewResponse>>>("api/order");
            return results.Data;
        }

        public async Task<string> PlaceOrder()
        {
            if (await _authService.IsUserAuthenticated())
            {
                var result = await _httpClient.PostAsync("api/payment/checkout", null);
                var url = await result.Content.ReadAsStringAsync();
                return url;
            }
            else
            {
                return "login";
            }
        }
    }
}
