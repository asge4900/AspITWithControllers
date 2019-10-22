using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspIT.Lib.Models;
using AspIT.Web.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AspIT.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User) && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //[AcceptVerbs("Get", "Post")]
        //[AllowAnonymous]
        //public async Task<IActionResult> IsEmailInUse(string email)
        //{
        //    var user = await userManager.FindByEmailAsync(email);

        //    if (user == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //        return Json($"Email {email} is already in use");
        //}

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && user.Email == model.Email)
                {
                    ModelState.AddModelError("Email", "Email is already in use");
                    return View(model);
                }
                user = new ApplicationUser 
                { 
                    UserName = model.UserName, 
                    Email = model.Email,
                    City = model.City
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                           new { userId = user.Id, token = token}, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmationLink);

                    //var message = new MimeMessage();
                    //message.From.Add(new MailboxAddress("Test project", "asgerlassen@gmail.com"));
                    //message.To.Add(new MailboxAddress(user.UserName, user.Email));
                    //message.Subject = "Test mail in asp.net core";
                    //message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    //{                        

                    //    Text = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Link</a>"
                    //};

                    //using (var client = new SmtpClient())
                    //{
                    //    client.Connect("smtp.gmail.com", 587, false);

                    //    //Change Password 
                    //    client.Authenticate("asgerlassen@gmail.com", "Password");

                    //    client.Send(message);

                    //    client.Disconnect(true);
                    //}

                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    ViewBag.ErrorTitle = "Registration succesful";
                    ViewBag.ErrorMessage = $"Before you can login, please confirm you email, by clicking on the confirmation link we emailed you";
                    return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model.UserName.IndexOf("@") > -1)
            {
                string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                        @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(emailRegex);
                if (!re.IsMatch(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Email is not valid");
                }
            }
            else
            {
                string userNameRegex = @"^[a-zA-Z0-9]*$";
                Regex re = new Regex(userNameRegex);
                if (!re.IsMatch(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username is not valid");
                }
            }

            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var userName = model.UserName;               

                 var user = await userManager.FindByNameAsync(model.UserName);

                if (userName.IndexOf('@') > -1)
                {
                    user = await userManager.FindByEmailAsync(model.UserName);

                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                    else
                    {
                        userName = user.UserName;
                    }

                }

                if (user != null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();             
        }
    }
}