using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Samples.UI
{
    public struct NotificationData
    {
        public string titleText;
        public string bodyText;
        public float lifeTime;
        public Sprite image;

        public NotificationData(string titleText, string bodyText, float lifeTime, Sprite image = null)
        {
            this.titleText = titleText;
            this.bodyText = bodyText;
            this.lifeTime = lifeTime;
            this.image = image;
        }
    }

    public class NotificationView : MonoBehaviour
    {
        [SerializeField] Image m_NotificationImage;
        [SerializeField] TMP_Text m_NotificationTitle;
        [SerializeField] TMP_Text m_NotificationBodyText;
        [SerializeField] CanvasGroup m_NotificationGroup;
        [SerializeField] float m_FadeTime = 1;

        float m_LifeTime = 1;

        public void Init(NotificationData notification)
        {
            if (notification.image != null)
            {
                m_NotificationImage.gameObject.SetActive(true);
                m_NotificationImage.sprite = notification.image;
            }
            else
            {
                m_NotificationImage.gameObject.SetActive(false);
            }

            m_LifeTime = notification.lifeTime;
            m_NotificationTitle.text = notification.titleText;
            m_NotificationBodyText.text = notification.bodyText;
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