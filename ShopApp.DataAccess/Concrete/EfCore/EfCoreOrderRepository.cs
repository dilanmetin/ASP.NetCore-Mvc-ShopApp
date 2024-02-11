﻿using Microsoft.EntityFrameworkCore;
using ShopApp.DataAccess.Abstract;
using ShopApp.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopApp.DataAccess.Concrete.EfCore
{
    public class EfCoreOrderRepository : EfCoreGenericRepository<Order>, IOrderRepository
    {
        public EfCoreOrderRepository(ShopContext context) : base(context)
        {

        }
        private ShopContext ShopContext
        {
            get { return context as ShopContext; }

        }
        public List<Order> GetOrders(string userId)
        {
            
                var orders= ShopContext.Orders.Include(i=>i.OrderItems).ThenInclude(i=>i.Product).AsQueryable();

                if(!string.IsNullOrEmpty(userId))
                {
                    orders=orders.Where(i=>i.UserId == userId);
                }
                return orders.ToList();
            
            
        }

    }
}
