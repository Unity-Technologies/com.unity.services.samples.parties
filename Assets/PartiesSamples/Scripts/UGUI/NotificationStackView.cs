using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class NotificationStackView : MonoBehaviour
    {
        [SerializeField] NotificationView m_NotificationView;
        [SerializeField] Transform m_StackParent;

        public void CreateNotification(float lifeTime, string playerName, string notificationContent)
        {
            var notificationInstance = Instantiate(m_NotificationView, m_StackParent);
            notificationInstance.Init(lifeTime, playerName, notificationContent);
        }
    }
}