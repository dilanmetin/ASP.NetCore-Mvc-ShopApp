using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopApp.Business.Abstract;
using ShopApp.WebUI.Identity;
using ShopApp.WebUI.Models;
using Iyzipay;
using Iyzipay.Request;
using System.Net;
using System;
using Iyzipay.Model;
using ShopApp.Entity;
using OrderItem = ShopApp.Entity.OrderItem;
using ShopApp.WebUI.Extensions;

namespace ShopApp.WebUI.Controllers
{
    [Authorize]
    public class CartController: Controller 
    {
        private ICartService _cartService;
        private IOrderService _orderService;
        private UserManager<User> _userManager;
        public CartController(ICartService cartSevice, UserManager<User> userManager, IOrderService orderService)
        {
            _cartService = cartSevice;
            _orderService = orderService;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));
            return View(new CartModel()
            {
                CartId = cart.Id,   
                CartItems = cart.CartItems.Select(i=>new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId=i.ProductId,
                    Name=i.Product.Name,
                    Price=(double)i.Product.Price,
                    ImageUrl=i.Product.ImageUrl,
                    Quantity=i.Quantity

                }).ToList()
            } );
        }
        [HttpPost]
        public IActionResult AddToCart(int productId,int quantity)
        {
            var userId = _userManager.GetUserId(User);
            _cartService.AddToCart(userId, productId, quantity);
            return RedirectToAction("Index");
        }
       
        
        
        [HttpPost]
        public IActionResult DeleteFromCart(int productId)
        {
            var userId = _userManager.GetUserId(User);
            _cartService.DeleteFromCart(userId, productId);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));
            var orderModel= new OrderModel();
            orderModel.CartModel = new CartModel()
            {
                CartId = cart.Id,
                CartItems = cart.CartItems.Select(i => new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = (double)i.Product.Price,
                    ImageUrl = i.Product.ImageUrl,
                    Quantity = i.Quantity

                }).ToList()
            };
            return View(orderModel);
          
        }
        [HttpPost]
        public IActionResult Checkout(OrderModel model)
        {
            var userId = _userManager.GetUserId(User);
            var cart = _cartService.GetCartByUserId(userId);
            model.CartModel = new CartModel()
            {
                CartId = cart.Id,
                CartItems = cart.CartItems.Select(i => new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = (double)i.Product.Price,
                    ImageUrl = i.Product.ImageUrl,
                    Quantity = i.Quantity

                }).ToList()
            };
            if (!ModelState.IsValid)
            {

                var payment = PaymentProcess(model);

                if (payment.Status == "success")
                {
                    SaveOrder(model, payment, userId);
                    ClearCart(model.CartModel.CartId);
                    return View("Success");

                }
                else
                {
                    TempData.Put("message", new AlertMessage()
                    {
                        Title = "Hata",
                        Message = $"{payment.ErrorMessage}",
                        AlertType = "danger"
                    });
                }
            }
            return View(model);



        }

        public IActionResult GetOrders()
        {
            var userId= _userManager.GetUserId(User);
            var orders=  _orderService.GetOrders(userId);

            var orderListModel =new List<OrderListModel>();
            OrderListModel orderModel;
            foreach (var order in orders)
            {
                orderModel = new OrderListModel();
                orderModel.OrderId = order.OrderId; 
                orderModel.OrderNumber = order.OrderNumber;
                orderModel.OrderDate= order.OrderDate;
                orderModel.Phone=order.Phone;
                orderModel.FirstName= order.FirstName;
                orderModel.LastName= order.LastName;    
                orderModel.Email= order.Email;
                orderModel.Address= order.Address;
                orderModel.City= order.City;
                orderModel.OrderState= order.OrderState;
                orderModel.PaymentType= order.PaymentType;
                
                orderModel.OrderItems = order.OrderItems.Select(i=>new OrderItemModel()
                {
                    OrderItemId = i.Id,
                    Name=i.Product.Name,
                    Price=(double)i.Product.Price,
                    Quantity=i.Quantity,
                    ImageUrl=i.Product.ImageUrl

                }).ToList();
                orderListModel.Add(orderModel);



            }
            return View("Orders", orderListModel);
        }
        private void ClearCart(int cartId)
        {
            _cartService.ClearCart(cartId);
        }

        private void SaveOrder(OrderModel model, Payment payment, string userId)
        {
            var order = new Order();
            order.OrderNumber = new Random().Next(111111, 999999).ToString();
            order.OrderState = EnumOrderState.complated;
            order.PaymentType = EnumPaymentType.CreditCard;
            order.PaymentId= payment.PaymentId;
            order.ConversationId = payment.ConversationId;
            order.OrderDate = new DateTime();
            order.FirstName = model.FirstName;
            order.LastName = model.LastName;
            order.UserId = userId;
            order.Address = model.Address;
            order.City = model.City;
            order.Phone = model.Phone;
            order.Email= model.Email;
         
            order.OrderItems = new List<OrderItem>();

            foreach (var item in model.CartModel.CartItems)
            {
                var orderItem = new ShopApp.Entity.OrderItem()
                {
                    Price= item.Price,
                    Quantity= item.Quantity,
                    ProductId= item.ProductId

                };
                order.OrderItems.Add(orderItem);
            }
            _orderService.Create(order);

        }

        private Payment PaymentProcess(OrderModel model)
        {
            Iyzipay.Options options = new Iyzipay.Options();
            options.ApiKey = "sandbox-Vnai6yRCxyUzrzLNAtp7bI8aT4ccBbol";
            options.SecretKey = "sandbox-ZeQwaqz5q5PPhHVSzm8c9qjtPrjaO9vo";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = new Random().Next(111111111, 999999999).ToString() ;
            request.Price = model.CartModel.TotalPrice().ToString() ;
            request.PaidPrice = model.CartModel.TotalPrice().ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B67832";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = model.CardName;
            paymentCard.CardNumber = model.CardNumber ;
            paymentCard.ExpireMonth = model.ExpirationMonth;
            paymentCard.ExpireYear = model.ExpirationYear;
            paymentCard.Cvc = model.Cvc;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            //paymentCard.CardNumber = "5528790000000008";
            //paymentCard.ExpireMonth = "12";
            //paymentCard.ExpireYear = "2030";
            //paymentCard.Cvc = "123";

            Buyer buyer = new Buyer();
            buyer.Id = "BY789";
            buyer.Name = model.FirstName;
            buyer.Surname = model.LastName;
            buyer.GsmNumber = model.Phone;
            buyer.Email = model.Email;
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            buyer.Ip = "85.34.78.112";
            buyer.City = "Istanbul";
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem basketItem;
            foreach (var item in model.CartModel.CartItems)
            {
                basketItem = new BasketItem();
                basketItem.Id = item.ProductId.ToString();
                basketItem.Name = item.Name;
                basketItem.Category1 = "Makyaj";
                basketItem.Price = (item.Price*item.Quantity).ToString();
                basketItem.ItemType= BasketItemType.PHYSICAL.ToString();
                basketItems.Add(basketItem);
            }
            
           request.BasketItems = basketItems;  
           return Payment.Create(request, options);
            
        }
    }
}
