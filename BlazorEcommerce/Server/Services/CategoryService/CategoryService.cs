namespace BlazorEcommerce.Server.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly DataContext _context;

        public CategoryService(DataContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<List<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            var serviceResponse = new ServiceResponse<List<Category>>();
            serviceResponse.Data = categories;
            return serviceResponse;
        }

        public async Task<ServiceResponse<Category>> GetCategory(int categoryId)
        {
            throw new NotImplementedException();
        }
    }
}
