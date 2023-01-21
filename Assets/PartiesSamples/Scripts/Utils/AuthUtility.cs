using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ParrelSync;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Samples.Utilities
{
    public static class AuthUtility
    {
        public static InitializationOptions TryCreateEditorTestingProfile()
        {
#if UNITY_EDITOR
            var profileName = "";

            if (ClonesManager.IsClone())
            {
                var cloneNumber = ClonesManager.GetCurrentProject().name.Split('_').Last();
                profileName += $"_{cloneNumber}";
            }

            var unityAuthenticationInitOptions = new InitializationOptions();
            unityAuthenticationInitOptions.SetProfile(profileName);
            return unityAuthenticationInitOptions;
#else
            return new InitializationOptions();
#endif
        }
    }
}