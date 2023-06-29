using Stripe;
using Stripe.Checkout;

namespace BlazorEcommerce.Server.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;
        const string secret = "whsec_cd045d6d815dee2b00a799652e12517449a674f258fe130060e78f274c35e0cb";

        public PaymentService(ICartService cartService, IAuthService authService, IOrderService orderService)
        {
            StripeConfiguration.ApiKey = "sk_test_51MlyhNHcSFVskGXvM2lQcnvNWj7DJIGycagdUSsCAoRgCFw5pzTqQP3zRA0qL7GCmMVV6qo9MOHzDrDzScux33PW00tAZlj06y";
            _cartService = cartService;
            _authService = authService;
            _orderService = orderService;

        }

        public async Task<Session> CreateCheckoutSession()
        {
            var products = (await _cartService.GetDbCartProducts()).Data;
            var lineItem = new List<SessionLineItemOptions>();
            products.ForEach(product => lineItem.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = product.Price * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Title,
                        Images = new List<string> { product.ImageUrl }
                    }
                },
                Quantity = product.Quantity

            }));

            var options = new SessionCreateOptions
            {
                CustomerEmail = _authService.GetUserEmail(),
                ShippingAddressCollection =
                    new SessionShippingAddressCollectionOptions
                    {
                        AllowedCountries = new List<string> { "US" }
                    },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = lineItem,
                Mode = "payment",
                SuccessUrl = "https://localhost:7226/order-success",
                CancelUrl = "https://localhost:7226/cart"

            };
            var service = new SessionService();
            Session session = service.Create(options);
            return session;
        }

        public async Task<ServiceResponse<bool>> FulFillOrder(HttpRequest request)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, request.Headers["Stripe-Signature"], secret);
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    var user = await _authService.GetUserByEmail(session.CustomerEmail);
                    await _orderService.PlaceOrder(user.Id);
                }
                return new ServiceResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = "Payment was successful"
                };
            }
            catch (StripeException e)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = e.Message
                };
            }
        }
    }
}
