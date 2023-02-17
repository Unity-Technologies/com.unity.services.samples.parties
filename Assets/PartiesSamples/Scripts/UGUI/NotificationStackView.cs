using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class NotificationStackView : MonoBehaviour
    {
        [SerializeField] NotificationView m_NotificationView;
        [SerializeField] Transform m_StackParent;

        //Performance Note : Should probably be pooled
        public void CreateNotification(float lifeTime, string playerName, string notificationContent)
        {
            var notificationInstance = Instantiate(m_NotificationView, m_StackParent);
            notificationInstance.Init(lifeTime, playerName, notificationContent);
        }

        /// <summary>
        /// Throw this into a UI button to quickly test how it looks
        /// </summary>
        public void TestNotify()
        {
            CreateNotification(1,"Unity", "Test Test Test Test");
        }
    }
}