using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.ViewModels.Account;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerAccount)]
    public partial class AccountController: BaseController
    {
        [HttpGet]
        public async Task<IActionResult> AuthKeys()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(AuthKeys);

            var user = await GetCurrentUserAsync();
            return View(new AuthKeysViewModel()
            {
                MyProfile = user,
                Keys = await _auth.GetForUser(user.Id),
            });
        }

        [HttpPost, FeatureGate(FeatureFlags.FunctionUserAddAuthKey)]
        public async Task<IActionResult> AddAuthKey(string value, AuthType type)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(AddAuthKey);
            var user = await GetCurrentUserAsync();

            var key = new PublicKey()
            {
                Value = value,
                Type = type,
            };
            var result = await _auth.TryAddUserKey(key, user);
            switch (result.Error)
            {
                case AuthKeyError.None: return RedirectToAction(nameof(AuthKeys));
                case AuthKeyError.InvalidFormat:
                ModelState.AddModelError("PublicKey", "Invalid format");
                break;
            }

            return View(nameof(AuthKeys), new AuthKeysViewModel()
            {
                MyProfile = user,
                NewKey = new AddKeyViewModel() {
                    PublicKey = key
                },
            });
        }

        [HttpPost, FeatureGate(FeatureFlags.FunctionUserGenAuthKey)]
        public async Task<IActionResult> GenAuthKey(AuthKeyOptions AuthKey)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(GenAuthKey);
            var user = await GetCurrentUserAsync();

            AuthKey.Format = AuthKeyFormat.Raw;
            var result = await _auth.GenKey(AuthKey, HandshakeMethod.Default);
            switch (result.Error)
            {
                case AuthKeyError.None:
                {
                    result = await _auth.TryAddUserKey(result.Keys.Public, user);
                    switch (result.Error)
                    {
                        case AuthKeyError.None: return RedirectToAction(nameof(AuthKeys));
                        case AuthKeyError.InvalidFormat:
                        ModelState.AddModelError("PublicKey", "Invalid format");
                        break;
                    }
                }
                break;
                case AuthKeyError.InvalidFormat:
                ModelState.AddModelError("PublicKey", "Invalid format");
                break;
            }

            return View(nameof(AuthKeys), new AuthKeysViewModel()
            {
                MyProfile = user,
                NewKey = new AddKeyViewModel() {
                    AuthKey = AuthKey
                },
            });
        }
    }
}