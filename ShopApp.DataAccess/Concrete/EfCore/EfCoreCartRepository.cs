using Microsoft.EntityFrameworkCore;
using ShopApp.DataAccess.Abstract;
using ShopApp.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopApp.DataAccess.Concrete.EfCore
{
    public class EfCoreCartRepository : EfCoreGenericRepository<Cart>, ICartRepository
    {
        public EfCoreCartRepository(ShopContext context) : base(context)
        {

        }
        private ShopContext ShopContext
        {
            get { return context as ShopContext; }

        }
        public void ClearCart(int cartId)
        {
            
                var cmd = @"delete from CartItems where CartId=@p0 ";
            ShopContext.Database.ExecuteSqlRaw(cmd, cartId);
            
        }

        public void DeleteFromCart(int cartId, int productId)
        {
            
                var cmd = @"delete from CartItems where CartId=@p0 and ProductId=@p1";
            ShopContext.Database.ExecuteSqlRaw(cmd,cartId,productId);
            
        }

        public Cart GetByUserId(string userId)
        {
                return ShopContext.Carts
                .Include(i => i.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(i => i.UserId == userId);
                
        }

        public override void Update(Cart entity)
        {

            ShopContext.Carts.Update(entity);          
           
        }
    }
}
