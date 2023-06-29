namespace BlazorEcommerce.Server.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly DataContext _context;

        private readonly IAuthService _authService;

        public CartService(DataContext context, IAuthService authService)
        {
            _context = context;

            _authService = authService;
        }


        public async Task<ServiceResponse<List<CartProductResponse>>> GetCartProducts(List<CartItem> cartItems)
        {
            var result = new ServiceResponse<List<CartProductResponse>>
            {
                Data = new List<CartProductResponse>()
            };

            foreach (var item in cartItems)
            {
                var product = await _context.Products
                    .Where(p => p.Id == item.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    continue;
                }

                var productVariant = await _context.ProductVariants
                    .Where(pv => pv.ProductId == item.ProductId && pv.ProductTypeId == item.ProductTypeId)
                    .Include(pv => pv.ProductType)
                    .FirstOrDefaultAsync();
                if (productVariant == null)
                {
                    continue;
                }

                var cartProduct = new CartProductResponse
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    ImageUrl = product.ImageUrl,
                    Price = productVariant.Price,
                    ProductType = productVariant.ProductType.Name,
                    ProductTypeId = productVariant.ProductTypeId,
                    Quantity = item.Quantity
                };
                result.Data.Add(cartProduct);
            }
            return result;

        }

        public async Task<ServiceResponse<List<CartProductResponse>>> StoreCartItems(List<CartItem> cartItems)
        {
            var userId = _authService.GetCurrentUserId();
            cartItems.ForEach(cartItem => cartItem.UserId = userId);
            _context.CartItems.AddRange(cartItems);
            await _context.SaveChangesAsync();

            return await GetDbCartProducts();
        }

        public async Task<ServiceResponse<int>> GetCartItemsCount()
        {
            var count = (await _context.CartItems
                .Where(ci => ci.UserId == _authService.GetCurrentUserId()).ToListAsync()).Count;
            return new ServiceResponse<int>
            {
                Data = count,
                Success = true,
                Message = "Cart items count retrieved successfully"
            };

        }

        public async Task<ServiceResponse<List<CartProductResponse>>> GetDbCartProducts(int? userId = null)
        {
            if (userId == null)
            {
                userId = _authService.GetCurrentUserId();
            }
            return await GetCartProducts(await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync());
        }

        public async Task<ServiceResponse<bool>> AddToCart(CartItem cartItem)
        {
            cartItem.UserId = _authService.GetCurrentUserId();
            var sameItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == cartItem.UserId
                && ci.ProductId == cartItem.ProductId
                && ci.ProductTypeId == cartItem.ProductTypeId);
            if (sameItem == null)
            {
                _context.CartItems.Add(cartItem);
            }
            else
            {
                sameItem.Quantity += cartItem.Quantity;
            }
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = "Cart item added successfully"
            };
        }

        public async Task<ServiceResponse<bool>> UpdateQuantity(CartItem cartItem)
        {
            var dbCartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == _authService.GetCurrentUserId()
                && ci.ProductId == cartItem.ProductId
                && ci.ProductTypeId == cartItem.ProductTypeId);
            if (dbCartItem == null)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = "Cart item not found"
                };
            }
            dbCartItem.Quantity = cartItem.Quantity;
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = "Cart item updated successfully"
            };
        }

        public async Task<ServiceResponse<bool>> RemoveItemFromCart(int productId, int productTypeId)
        {
            var dbCartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.ProductId == productId
                && ci.ProductTypeId == productTypeId && ci.UserId == _authService.GetCurrentUserId());
            if (dbCartItem == null)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = "Cart item not found"
                };
            }
            _context.CartItems.Remove(dbCartItem);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = "Cart item removed successfully"
            };
        }
    }
}
