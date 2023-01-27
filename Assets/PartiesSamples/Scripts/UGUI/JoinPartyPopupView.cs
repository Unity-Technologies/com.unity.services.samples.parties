using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Unity.Services.Samples.Parties
{
    public class JoinPartyPopupView : MonoBehaviour
    {
        [SerializeField] Button m_JoinButton = null;
        [SerializeField] Button m_CloseButton = null;
        [SerializeField] Button m_BackgroundButton = null;
        [SerializeField] TMP_InputField m_IdInputField = null;
        [SerializeField] TMP_Text m_JoinErrorText = null;
        public event Action<string> OnJoinClicked;

        public void Init()
        {
            var playerId = string.Empty;
            m_JoinButton.interactable = false;
            m_IdInputField.onValueChanged.AddListener((value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_JoinButton.interactable = false;
                    return;
                }
                m_JoinButton.interactable = true;

                playerId = value;
            });
            m_JoinButton.onClick.AddListener(() =>
            {
                m_JoinErrorText.text = string.Empty;
                OnJoinClicked?.Invoke(playerId);
            });
            m_BackgroundButton.onClick.AddListener(Hide);
            m_CloseButton.onClick.AddListener(Hide);
            Hide();
        }

        public void JoinPartySuccess()
        {
        }

        public void JoinPartyFailed(string failureReason)
        {
            m_JoinErrorText.text = failureReason;
        }

        public void Show()
        {
            m_JoinErrorText.text = string.Empty;
            m_IdInputField.SetTextWithoutNotify("");
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
