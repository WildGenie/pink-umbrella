using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Services;

namespace PinkUmbrella.Util
{
    public class ApiCallFilterAttribute : ActionFilterAttribute
    {
        private static readonly string _API_HEADER_KEY = "X-Api-Key";
        private static readonly string _HANDLER_KEY = "AuthHandler";
        private static readonly string _API_KEY = "ApiKey";
        private static readonly Regex KeyExtractRgx = new Regex(@"(?<authType>\w+)(?:\s+)");
        private readonly IAuthService _auth;
        private readonly bool skipApiAuth = false;

        public ApiCallFilterAttribute(IAuthService auth)
        {
            _auth = auth;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Result != null)
            {
                return;
            }

            if (Debugger.IsAttached && skipApiAuth)
            {
                base.OnActionExecuting(context);
                return;
            }
            else if (context.HttpContext.User?.Identity?.IsAuthenticated ?? false)
            {
                base.OnActionExecuting(context);
                return;
            }
            else if (context.HttpContext.Request.Headers.TryGetValue(_API_HEADER_KEY, out var apiKeys) && apiKeys.Count == 1)
            {
                var keyToParse = apiKeys[0].Replace("%n%", "\n");
                var match = KeyExtractRgx.Match(keyToParse);
                if (Enum.TryParse(typeof(AuthType), match.Groups["authType"].Value, true, out var authType))
                {
                    var at = (AuthType)authType;
                    var matchKey = keyToParse.Substring(at.ToString().Length).Trim();
                    if (!string.IsNullOrWhiteSpace(matchKey))
                    {
                        var key = await _auth.GetPublicKey(matchKey, at);
                        if (key != null)
                        {
                            var apiKey = await _auth.GetApiKey(key);
                            if (apiKey != null)
                            {
                                var privateKey = await _auth.GetPrivateKey(apiKey.ServerPublicKeyId, at);
                                if (privateKey != null)
                                {
                                    var authHandler = _auth.GetHandler(at);
                                    context.HttpContext.Items.Add(_HANDLER_KEY, authHandler);
                                    context.HttpContext.Items.Add(_API_KEY, apiKey);

                                    context.HttpContext.Request.EnableBuffering();
                                    context.HttpContext.Request.Body.Position = 0;
                                    var decrypted = new MemoryStream();
                                    await authHandler.DecryptAndVerifyStreamAsync(context.HttpContext.Request.Body, decrypted, privateKey, apiKey.ClientPublicKey);
                                    context.HttpContext.Request.Body = decrypted;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            var ip = await _auth.GetOrRememberIP(context.HttpContext.Connection.RemoteIpAddress);
                            if (ip != null && ip.PublicKey == null)
                            {
                                context.Result = new JsonResult(new { Error = "You are not trusted" });
                                return;
                            }
                        }
                    }
                }
                else
                {
                    context.Result = new JsonResult(new { Error = $"Invalid {_API_HEADER_KEY} format" });
                    return;
                }
            }

            context.Result = new JsonResult(new { Error = "Not authorized" });
        }

        public override async void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result;
            if (result is JsonResult json)
            {
                var x = json.Value;
                var status = json.StatusCode;
                if ((status ?? 200) == 200)
                {
                    if (context.HttpContext.Items.TryGetValue(_HANDLER_KEY, out var authHandlerObj) &&
                        authHandlerObj is IAuthTypeHandler authHandler &&
                        context.HttpContext.Items.TryGetValue(_API_KEY, out var apiKeyObj) &&
                        apiKeyObj is ApiAuthKeyModel apiKey)
                    {
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                        using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
                        var encrypted = new MemoryStream();
                        await authHandler.EncryptStreamAndSignAsync(jsonStream, encrypted, apiKey.ServerPrivateKey, apiKey.ClientPublicKey);
                        encrypted.Position = 0;
                        context.Result = new FileStreamResult(encrypted, "application/pink-umbrella");
                    }
                }
            }
            base.OnActionExecuted(context);
        }
    }
}