using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.ViewModels.Account;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Models.Auth;
using Tides.Models.Auth;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerAccount)]
    public partial class AccountController: ActivityStreamController
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
                Keys = await _auth.GetForUser(user.UserId.Value),
            });
        }

        [HttpPost, FeatureGate(FeatureFlags.FunctionUserAddAuthKey)]
        public async Task<IActionResult> AddAuthKey(string value, AuthType type)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(AddAuthKey);
            var user = await GetCurrentLocalUserAsync();

            var key = new PublicKey()
            {
                Value = _auth.CleanupKeyString(value),
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
                MyProfile = await _publicProfiles.Transform(user, 0, user.Id),
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
            var user = await GetCurrentLocalUserAsync();

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
                MyProfile = await _publicProfiles.Transform(user, 0, user.Id),
                NewKey = new AddKeyViewModel() {
                    AuthKey = AuthKey
                },
            });
        }
    }
}