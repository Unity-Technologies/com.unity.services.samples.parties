using System;
using UnityEngine;

namespace Unity.Samples.UI
{
    /// <summary>
    /// Requires the NotificationManager Prefab to be in the scene to work.
    /// </summary>
    public class NotificationEvents
    {
        public static Action<NotificationData> onNotify;
    }
}
