using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Unity.Services.Samples
{
    /// <summary>
    /// The Samples shared implementation of the Unity Authentication Service
    /// </summary>
    public static class UnityServiceAuthenticator
    {
        const int k_InitTimeout = 10000;
        static bool s_IsSigningIn;

        /// <summary>
        /// public Auth 
        /// </summary>
        /// <param name="profileName">Usually, auth shared profi</param>
        /// <returns></returns>
        public static async Task<bool> TryInitServicesAsync(string profileName = null)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return true;

            //Another Service is mid-initialization:
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                var task = WaitForInitialized();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                    return false; // We timed out

                return UnityServices.State == ServicesInitializationState.Initialized;
            }

            if (profileName != null)
            {
                //ProfileNames can't contain non-alphanumeric characters
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                profileName = rgx.Replace(profileName, "");
                var authProfile = new InitializationOptions().SetProfile(profileName);

                //If you are using multiple unity services, make sure to initialize it only once before using your services.
                await UnityServices.InitializeAsync(authProfile);
            }
            else
                await UnityServices.InitializeAsync();

            return UnityServices.State == ServicesInitializationState.Initialized;

            async Task WaitForInitialized()
            {
                while (UnityServices.State != ServicesInitializationState.Initialized)
                    await Task.Delay(100);
            }
        }

        public static async Task<bool> TrySignInAsync(string profileName = null)
        {
            if (!await TryInitServicesAsync(profileName))
                return false;
            if (s_IsSigningIn)
            {
                var task = WaitForSignedIn();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                    return false; // We timed out
                return AuthenticationService.Instance.IsSignedIn;
            }

            s_IsSigningIn = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            s_IsSigningIn = false;

            return AuthenticationService.Instance.IsSignedIn;

            async Task WaitForSignedIn()
            {
                while (!AuthenticationService.Instance.IsSignedIn)
                    await Task.Delay(100);
            }
        }
    }
}
