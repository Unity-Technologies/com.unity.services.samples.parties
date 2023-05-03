using System;
using UnityEngine;

namespace Unity.Samples.UI
{
    /// <summary>
    /// This needs to be in the scene for Notifications to fire
    /// </summary>
    public class NotificationStackView : MonoBehaviour
    {
        [SerializeField] NotificationView m_NotificationView;
        [SerializeField] Transform m_StackParent;

        void OnEnable()
        {
            NotificationEvents.onNotify += CreateNotification;
        }

        void OnDisable()
        {
            NotificationEvents.onNotify -= CreateNotification;
        }

        public void CreateNotification(NotificationData notification)
        {
            var notificationInstance = Instantiate(m_NotificationView, m_StackParent);
            notificationInstance.Init(notification);
        }
    }
}