using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class CookieStorageService<T> : ICookieStorageService<T>
    {
        private readonly HttpContext _httpContext;
        private readonly IDataProtector _protector;
        private const string ProtectorPurpose = "CookieStorageService";

        public CookieStorageService(
            HttpContext httpContext,
            IDataProtectionProvider provider)
        {
            _httpContext = httpContext;
            _protector = provider.CreateProtector(ProtectorPurpose);
        }

        public void Create(T item, string cookieName, int expiryDays = 1)
        {
            var cookieContent = JsonConvert.SerializeObject(item);
            var encodedContent = Convert.ToBase64String(
                _protector.Protect(
                    System.Text.Encoding.UTF8.GetBytes(cookieContent)));

            var options = new CookieOptions
            {
                IsEssential = true,
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddDays(expiryDays)
            };

            _httpContext.Response.Cookies.Append(cookieName, encodedContent, options);
        }

        public T Get(string cookieName)
        {
            var cookie = _httpContext.Request.Cookies[cookieName];
            if (cookie == null)
                return default(T);

            var base64EncodedBytes = Convert.FromBase64String(cookie);
            return JsonConvert.DeserializeObject<T>(
                System.Text.Encoding.UTF8.GetString(
                    _protector.Unprotect(base64EncodedBytes)));
        }

        public void Delete(string cookieName)
        {
            if (_httpContext.Request.Cookies[cookieName] != null)
            {
                _httpContext.Response.Cookies.Delete(cookieName);
            }
        }

        public void Update(string cookieName, T item, int expiryDays = 1)
        {
            Delete(cookieName);
            Create(item, cookieName, expiryDays);
        }
    }
}
