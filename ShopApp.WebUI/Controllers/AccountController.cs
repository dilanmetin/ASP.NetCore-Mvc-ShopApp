using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopApp.Business.Abstract;
using ShopApp.WebUI.EmailServices;
using ShopApp.WebUI.Extensions;
using ShopApp.WebUI.Identity;
using ShopApp.WebUI.Models;
using System;
using System.Security.Claims;

namespace ShopApp.WebUI.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController:Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IEmailSender _emailSender;
        private ICartService _cartService;
        public AccountController(ICartService cartService, UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender= emailSender;
            _cartService = cartService;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult>  Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);

            }
            var user= await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu kullanıcı adı ile hesap oluşturulmamış");
                return View(model);
            }

            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "E-mail hesabınıza gelen link ile üyeliğinizi onaylayınız");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user,model.Password,false,false);
            if (result.Succeeded)
            {
                return Redirect("~/"); 
            }
            ModelState.AddModelError("", "Girilen kullanıcı adı veya parola hatalı");
            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>  Register(RegisterModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);

            }
            var user = new User()
            {
                FirstName=model.FirstName,
                LastName=model.LastName,
                UserName=model.UserName,
                Email=model.Email,
            };

            var result = await _userManager.CreateAsync(user,model.Password);
            if(result.Succeeded)
            {
                //generate token
                var code= await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = code
                });
                //mail
                await _emailSender.SendEmailAsync(model.Email, "Hesabınızı onaylayınız", $"Lütfen e-mail hesabınızı onaylamak için linke <a href='https://localhost:7001{url}'>tıklayınız </a>");
                //Console.WriteLine(url);
                return RedirectToAction("Login", "Account");
            }
            ModelState.AddModelError("", "Bilinmeyen bir hata oldu. Lütfen tekrar deneyiniz");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData.Put("message", new AlertMessage()
            {
                Title = "Oturum kapatıldı",
                Message = "Oturum güvenli bir şekilde kapatıldı",
                AlertType = "warning"
            });
            return Redirect("~/");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null|| token ==null )
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Geçersiz token",
                    Message = "Geçersiz token",
                    AlertType = "danger"
                }); 
                            
                return View();
            }
            var user = await _userManager.FindByIdAsync(userId);    
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _cartService.InitializeCart(user.Id);
                    TempData.Put("message", new AlertMessage()
                    {
                        Title = "Hesabınız onaylandı",
                        Message = "Hesabınız onaylandı",
                        AlertType = "success"
                    });     
                    return View();
                }    
            }
            TempData.Put("message", new AlertMessage()
            {
                Title = "Hesabınız onaylanmadı",
                Message = "Hesabınız onaylanmadı",
                AlertType = "warning"
            });
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>  ForgotPassword(string Email)
        {
            if(string.IsNullOrEmpty(Email))
            {
                return View();
             }
            var user = await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                return View();
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword", "Account", new
            {
                userId = user.Id,
                token = code
            });
            //mail
            await _emailSender.SendEmailAsync(Email, "Reset Password", $"Parolanızı yenilemek için linke <a href='https://localhost:7001{url}'>tıklayınız </a>");
           
            return View();
        }
        public  IActionResult ResetPassword(string userId,string token)
        {
            if(userId == null|| token==null)
            {
                return RedirectToAction("Home", "Index");
            }
            var model = new ResetPasswordModel { Token = token };
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return RedirectToAction("Home", "Index");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if(result.Succeeded){
                return RedirectToAction("Login","Account");
            }
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult FacebookLogin(string returnUrl)
        {
            string redirectUrl = Url.Action("SocialMediaResponse", "Home", new { returnUrl = returnUrl });
            var properties=  _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
            return new ChallengeResult("Facebook",properties);

        }
        public async Task<IActionResult>  SocialMediaResponse(string returnUrl)
        {
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (loginInfo==null)
            {
                return RedirectToAction("Register");
            }
            else
            {
                var result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey,true);
                if (result.Succeeded)
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    if (loginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                    {
                        User user = new User()
                        {
                            Email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email),
                            UserName = loginInfo.Principal.FindFirstValue(ClaimTypes.Name)   
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if(createResult.Succeeded)
                        {
                            var identityLogin = await _userManager.AddLoginAsync(user, loginInfo);
                            if(identityLogin.Succeeded) {
                                await _signInManager.SignInAsync(user, true);
                                return Redirect(returnUrl);

                            }

                        }
                       

                    }
                }
            }
            return RedirectToAction("Register");
        }
    }
}
