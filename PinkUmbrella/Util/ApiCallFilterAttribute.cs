using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Services;
using Poncho.Models.Auth;
using Poncho.Models.Crypto;

namespace PinkUmbrella.Util
{
    public class ApiCallFilterAttribute : ActionFilterAttribute
    {
        private static readonly string _API_HEADER_KEY = "X-Api-Key";
        private static readonly string _CIPHER_HEADER_KEY = "X-Api-Cipher";
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
                            var authHandler = _auth.GetHandler(at, key.Format);
                            context.HttpContext.Items.Add(_HANDLER_KEY, authHandler);

                            if (context.HttpContext.Request.ContentLength.HasValue && context.HttpContext.Request.ContentLength == 0)
                            {
                                // No body, no problem
                                return;
                            }

                            PrivateKey privateKey = null;
                            var apiKey = await _auth.GetApiKey(key);
                            if (apiKey != null)
                            {
                                context.HttpContext.Items.Add(_API_KEY, apiKey);
                                // privateKey = await _auth.GetPrivateKey(apiKey.ServerPublicKeyId, at);
                            }
                            else
                            {
                                var ip = await _auth.GetOrRememberIP(context.HttpContext.Connection.RemoteIpAddress);
                                if (ip != null)
                                {
                                    if (ip.PublicKeyId.HasValue)
                                    {
                                        if (ip.PublicKeyId == key.Id)
                                        {
                                            if (await _auth.IsTrusted(context.HttpContext.Connection.RemoteIpAddress, key))
                                            {
                                                privateKey = await _auth.GetCurrentPrivateKey();
                                            }
                                            else
                                            {
                                                context.Result = new JsonResult(new { Error = "You are not trusted" });
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            context.Result = new JsonResult(new { Error = "Key mismatch (are you using the right key?)" });
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        //context.Result = new JsonResult(new { Error = "Key not associated with your address" });
                                        //return;
                                    }
                                }
                                //privateKey = await _auth.GetPrivateKey(key.Id, key.Type);
                                // if (privateKey == null)
                                // {
                                //     var current = await _auth.GetCurrent();
                                //     privateKey = await _auth.GetPrivateKey(current.Id, current.Type);
                                // }
                            }

                            context.HttpContext.Request.EnableBuffering();
                            context.HttpContext.Request.Body.Position = 0;
                            using var encrypted = new MemoryStream();
                            await context.HttpContext.Request.Body.CopyToAsync(encrypted);
                            if (encrypted.Position == 0)
                            {
                                // No body, no problem
                                return;
                            }
                            encrypted.Position = 0;

                            if (privateKey != null)
                            {
                                var decrypted = new MemoryStream();
                                try
                                {
                                    if (context.HttpContext.Request.Headers.TryGetValue(_CIPHER_HEADER_KEY, out var apiCipher) && apiCipher.Count == 1)
                                    {
                                        var aesHelper = new AesHelper();
                                        using var apiCipherKeyEncrypted = new MemoryStream(Convert.FromBase64String(apiCipher[0]));
                                        using var apiCipherKeyDecrypted = new MemoryStream(aesHelper.Key);
                                        await authHandler.DecryptAndVerifyStreamAsync(apiCipherKeyEncrypted, apiCipherKeyDecrypted, privateKey, apiKey.ClientPublicKey, null);
                                        apiCipherKeyDecrypted.Position = 0;

                                        aesHelper.Decrypt(encrypted, decrypted);
                                    }
                                    else
                                    {
                                        await authHandler.DecryptAndVerifyStreamAsync(encrypted, decrypted, privateKey, apiKey.ClientPublicKey, null);
                                    }
                                    context.HttpContext.Request.Body = decrypted;
                                }
                                catch (Exception e)
                                {
                                    context.Result = new JsonResult(new { error = "Could not decrypt and verify payload", e.Message, e.StackTrace, e.Source });
                                }
                                return;
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

                        if (context.HttpContext.Request.Headers.TryGetValue(_CIPHER_HEADER_KEY, out var apiCipher) && apiCipher.Count == 1)
                        {
                            var aesHelper = new AesHelper();
                            aesHelper.Randomize();
                            using var apiCipherKeyDecrypted = new MemoryStream(aesHelper.Key);
                            using var apiCipherKeyEncrypted = new MemoryStream();

                            await authHandler.EncryptStreamAsync(apiCipherKeyDecrypted, apiCipherKeyEncrypted, apiKey.ClientPublicKey);
                            
                            context.HttpContext.Response.Headers.Add(_CIPHER_HEADER_KEY, Convert.ToBase64String(apiCipherKeyEncrypted.ToArray()));
                            encrypted.Write(aesHelper.IV);
                            aesHelper.Encrypt(jsonStream, encrypted);
                        }
                        else
                        {
                            //await authHandler.EncryptStreamAndSignAsync(jsonStream, encrypted, apiKey.ServerPrivateKey, apiKey.ClientPublicKey, null);
                        }
                        encrypted.Position = 0;
                        context.Result = new FileStreamResult(encrypted, "application/pink-umbrella");
                    }
                }
            }
            base.OnActionExecuted(context);
        }
    }
}