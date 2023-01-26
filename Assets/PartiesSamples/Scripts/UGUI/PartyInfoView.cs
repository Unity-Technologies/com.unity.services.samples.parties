using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyInfoView : MonoBehaviour
    {
        public event Action OnJoinPartyClicked;
        public event Action OnCreateClicked;
        public event Action OnLeaveClicked;

        [SerializeField] CanvasGroup m_JoinCreateView;
        [SerializeField] CanvasGroup m_InPartyView;

        [SerializeField] TMP_InputField m_InGameJoinCode;

        [SerializeField] Button m_CopyButton;
        [SerializeField] Button m_JoinButton;
        [SerializeField] Button m_CreateButton;
        [SerializeField] Button m_LeaveButton;

        public void Init()
        {
            m_CopyButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = m_InGameJoinCode.text);
            m_JoinButton.onClick.AddListener(() => OnJoinPartyClicked?.Invoke());
            m_CreateButton.onClick.AddListener(() => OnCreateClicked?.Invoke());
            m_LeaveButton.onClick.AddListener(() => OnLeaveClicked?.Invoke());
        }

        public void JoinParty(string partyCode)
        {
            ShowPartyInfo();
            m_InGameJoinCode.text = partyCode;

            void ShowPartyInfo()
            {
                m_JoinCreateView.alpha = 0;
                m_JoinCreateView.interactable = false;
                m_JoinCreateView.blocksRaycasts = false;
                m_InPartyView.alpha = 1;
                m_InPartyView.interactable = true;
                m_InPartyView.blocksRaycasts = true;
            }
        }

        public void LeftParty()
        {
            HidePartyInfo();

            void HidePartyInfo()
            {
                m_JoinCreateView.alpha = 1;
                m_JoinCreateView.interactable = true;
                m_JoinCreateView.blocksRaycasts = true;
                m_InPartyView.alpha = 0;
                m_InPartyView.interactable = false;
                m_InPartyView.blocksRaycasts = false;
            }
        }
    }
}
