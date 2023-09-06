using App.Areas.Product.Model;
using Newtonsoft.Json;

public class CartService
{
    // Key lưu chuỗi json của Cart
    public const string CARTKEY = "cart";
    private readonly IHttpContextAccessor _context;
    private readonly HttpContext HttpContext;

    public CartService(IHttpContextAccessor context)
    {
        _context = context;
        HttpContext = context.HttpContext;
    }

    // Lấy cart từ Sesion
    public List<CartItem> GetCartItems()
    {
        var session = HttpContext.Session;
        string jsonCartItems = session.GetString(CARTKEY);
        if (!string.IsNullOrEmpty(jsonCartItems))
        {
            var listCartItems = JsonConvert.DeserializeObject<List<CartItem>>(jsonCartItems);
            return listCartItems;
        }
        return new List<CartItem>();
    }

    // Xóa cart khỏi Session
    public void ClearCart()
    {
        HttpContext.Session.Remove(CARTKEY);
    }

    // Lưu cart vào Sesion
    public void SaveCartSession(List<CartItem> listCartItems)
    {
        string jsonCartItems = JsonConvert.SerializeObject(listCartItems);
        HttpContext.Session.SetString(CARTKEY, jsonCartItems); // key/value
    }
}