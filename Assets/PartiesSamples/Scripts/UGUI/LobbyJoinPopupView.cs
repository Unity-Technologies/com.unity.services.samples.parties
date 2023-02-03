using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class LobbyJoinPopupView : MonoBehaviour
    {
        [SerializeField] Button m_JoinButton = null;
        [SerializeField] Button m_CloseButton = null;
        [SerializeField] Button m_BackgroundButton = null;
        [SerializeField] TMP_InputField m_LobbyCodeField = null;
        [SerializeField] TMP_Text m_JoinErrorText = null;
        [SerializeField] GameObject m_ErrorPanel;
        public event Action<string> OnJoinClicked;

        const int k_GameCodeLength = 6;
        readonly Regex k_AlphaNumeric = new Regex("^[A-Z0-9]*$");

        public void Init()
        {
            m_JoinButton.interactable = false;
            m_LobbyCodeField.onValidateInput += (inputString, characterCount, character) => char.ToUpper(character);

            m_LobbyCodeField.onValueChanged.AddListener(ValidateInputCode);

            m_JoinButton.onClick.AddListener(() =>
            {
                OnJoinClicked?.Invoke(m_LobbyCodeField.text);
            });
            m_BackgroundButton.onClick.AddListener(Hide);
            m_CloseButton.onClick.AddListener(Hide);
            Hide();
        }

        public void JoinPartyFailed(string failureReason)
        {
            m_ErrorPanel.SetActive(true);
            m_JoinErrorText.text = failureReason;
        }

        public void Show()
        {
            m_JoinErrorText.text = string.Empty;
            m_LobbyCodeField.SetTextWithoutNotify("");
            gameObject.SetActive(true);
            m_ErrorPanel.SetActive(false);
        }

        void ValidateInputCode(string inputCode)
        {
            m_JoinButton.interactable = inputCode.Length == k_GameCodeLength && k_AlphaNumeric.IsMatch(inputCode);
        }

        void Hide()
        {
            gameObject.SetActive(false);
            m_ErrorPanel.SetActive(false);
        }
    }
}