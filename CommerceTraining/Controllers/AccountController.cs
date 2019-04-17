using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using EPiServer.Web.Routing;
using CommerceTraining.Models.ViewModels;
using CommerceTraining.SupportingClasses;
using Mediachase.Commerce.Customers;
using System.Web.Security;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Customers.Profile;
using Mediachase.Commerce.Core;
using System.Web;
using System;
using EPiServer.Filters;

namespace CommerceTraining.Controllers
{
    public class AccountController : PageController<AccountPage>
    {
        private readonly IContentRepository _contentRepository;
        private readonly UrlResolver _urlResolver;

        public AccountController(IContentRepository contentRepository, UrlResolver urlResolver)
        {
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
        }


        public ActionResult Index(AccountPage currentPage)
        {
            AccountViewModel model = new AccountViewModel();

            return View(model);
        }

        public ActionResult Login(AccountPage currentPage, string userName, string passWord)
        {
            if (Membership.ValidateUser(userName, passWord))
            {
                MembershipUser account = Membership.GetUser(userName);
                if (userName != null)
                {
                    var profile = SecurityContext.Current.CurrentUserProfile as CustomerProfileWrapper;
                    if (profile != null)
                    {
                        CreateAuthenticationCookie(ControllerContext.HttpContext, userName, Mediachase.Commerce.Core.AppContext.Current.ApplicationName, false);
                        //return Redirect(url);
                    }
                }
            }

            // ...just for a check
            return RedirectToAction("Index", new { page = ContentReference.StartPage }.page.ToPageReference());
        }

        // ToDo: Exercises in customers module
        public ActionResult CreateAccount(AccountPage currentPage, string userName, string passWord)
        {
            var firstName = userName;
            var lastName = userName;
            string email = firstName + "." + lastName + "@epi.com";
            
            var user = Membership.CreateUser(email,passWord,email,null,null,true, out MembershipCreateStatus status);

            // Create the Contact in ECF
            var customerContact = CustomerContact.CreateInstance(user);
            customerContact.FirstName = firstName;
            customerContact.LastName = lastName;
            customerContact.RegistrationSource = $"{this.Request.Url.Host},{SiteContext.Current}";

            // customerContact["Email"] = email; 
            customerContact.Email = email;

            // Do the "SaveChanges()" before setting ECF-"Roles" 
            customerContact.SaveChanges();


            // These Roles are ECF specific ... saved automatically ... and obsolete in 9
            //SecurityContext.Current.AssignUserToGlobalRole(membershipGuy, AppRoles.EveryoneRole); // For CM
            //SecurityContext.Current.AssignUserToGlobalRole(membershipGuy, AppRoles.RegisteredRole); // For CM

            Roles.AddUserToRole(user.UserName, AppRoles.EveryoneRole);
            Roles.AddUserToRole(user.UserName, AppRoles.RegisteredRole);

            // Call for further properties to be set
            SetContactProperties(customerContact);

            return null; // for now
        }

        protected void SetContactProperties(CustomerContact contact)
        {
            Organization org = Organization.CreateInstance();
            org.Name = "ParentOrg";
            org.SaveChanges();

            // Remember the EffectiveCustomerGroup!
            contact.CustomerGroup = "MyBuddies";

            // The custom field
            contact["Geography"] = "West";
            contact.OwnerId = org.PrimaryKeyId;

            contact.SaveChanges();
        }

        public static void CreateAuthenticationCookie(HttpContextBase httpContext, string username, string domain, bool remember)
        {
            // ... needed for cookieless authentication
            FormsAuthentication.SetAuthCookie(username, remember);
            var expirationDate = FormsAuthentication.GetAuthCookie(username, remember).Expires;

            // we need to handle ticket ourselves since we need to save session paremeters as well
            int timeout = httpContext.Session != null ? httpContext.Session.Timeout : 20;
            var ticket = new FormsAuthenticationTicket(2,
                    username,
                    DateTime.Now,
                    expirationDate == DateTime.MinValue ? DateTime.Now.AddMinutes(timeout) : expirationDate,
                    remember,
                    domain,
                    FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // remove the cookie, if one already exists with the same cookie name
            if (httpContext.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                httpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            }

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.HttpOnly = true;

            cookie.Path = FormsAuthentication.FormsCookiePath;
            cookie.Secure = FormsAuthentication.RequireSSL;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }

            // Create the cookie.
            httpContext.Response.Cookies.Set(cookie);
        }

    }
}