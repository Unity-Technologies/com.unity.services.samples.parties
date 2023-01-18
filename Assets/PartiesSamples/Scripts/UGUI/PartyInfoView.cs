using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyInfoView : MonoBehaviour
    {
        public event Action<string> onJoinPartyPressed;
        public event Action onCreatePressed;
        public event Action onLeavePressed;

        [SerializeField] CanvasGroup m_JoinCreateView;
        [SerializeField] CanvasGroup m_InPartyView;

        [SerializeField] TMP_InputField m_JoinPartyInput;
        [SerializeField] TMP_InputField m_InGameJoinCode;

        [SerializeField] Button m_JoinButton;
        [SerializeField] Button m_CreateButton;
        [SerializeField] Button m_LeaveButton;

        public void Init()
        {
            m_JoinButton.onClick.AddListener(OnJoinPartyPressed);
            m_CreateButton.onClick.AddListener(() => onCreatePressed?.Invoke());
            m_LeaveButton.onClick.AddListener(() => onLeavePressed?.Invoke());
        }

        public void JoinParty(string partyCode)
        {
            SetInPartyView();
            m_InGameJoinCode.text = partyCode;
        }

        public void LeftParty()
        {
            SetOutsidePartyView();
        }

        void SetInPartyView()
        {
            m_JoinCreateView.alpha = 0;
            m_JoinCreateView.interactable = false;
            m_JoinCreateView.blocksRaycasts = false;
            m_InPartyView.alpha = 1;
            m_InPartyView.interactable = true;
            m_InPartyView.blocksRaycasts = true;
        }

        void SetOutsidePartyView()
        {
            m_JoinCreateView.alpha = 1;
            m_JoinCreateView.interactable = true;
            m_JoinCreateView.blocksRaycasts = true;
            m_InPartyView.alpha = 0;
            m_InPartyView.interactable = false;
            m_InPartyView.blocksRaycasts = false;
        }

        void OnJoinPartyPressed()
        {
            onJoinPartyPressed?.Invoke(m_JoinPartyInput.text);
        }
    }
}