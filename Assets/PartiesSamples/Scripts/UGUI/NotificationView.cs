using System.Collections;
using TMPro;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class NotificationView : MonoBehaviour
    {
        [SerializeField] TMP_Text m_PlayerName;
        [SerializeField] TMP_Text m_NotificationText;
        [SerializeField] CanvasGroup m_NotificationGroup;
        [SerializeField] float m_FadeTime = 1;

        float m_LifeTime = 1;

        public void Init(float lifeTime, string nameText, string notificationText)
        {
            m_LifeTime = lifeTime;
            m_PlayerName.text = nameText;
            m_NotificationText.text = notificationText;
            StartCoroutine(NotificationLifeTime());
        }

        IEnumerator NotificationLifeTime()
        {
            yield return new WaitForSeconds(m_LifeTime);

            var fadeTime = m_FadeTime;
            while (fadeTime >= 0)
            {
                m_NotificationGroup.alpha = fadeTime / m_FadeTime;
                yield return new WaitForEndOfFrame();
                fadeTime -= Time.deltaTime;
            }

            Destroy(gameObject);
        }
    }
}