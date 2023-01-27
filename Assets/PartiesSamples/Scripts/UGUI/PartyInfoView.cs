using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyInfoView : MonoBehaviour
    {
        public event Action OnReadyClicked;
        public event Action OnUnReadyClicked;
        public event Action OnJoinPartyClicked;
        public event Action OnCreateClicked;
        public event Action OnLeaveClicked;

        [SerializeField] GameObject m_JoinCreateView;
        [SerializeField] GameObject m_InPartyView;
        [SerializeField] LayoutElement m_ButtonPanel;

        [SerializeField] TMP_InputField m_InGameJoinCode;

        [SerializeField] Button m_ReadyButton;
        [SerializeField] Button m_UnReadyButton;

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
            m_ReadyButton.onClick.AddListener(OnReady);
            m_UnReadyButton.onClick.AddListener(OnUnready);
        }

        public void JoinParty(string partyCode)
        {
            m_InGameJoinCode.text = partyCode;
            m_JoinCreateView.SetActive(false);
            m_InPartyView.SetActive(true);
            m_ButtonPanel.gameObject.SetActive(true);
        }

        public void LeftParty()
        {
            m_JoinCreateView.SetActive(true);
            m_InPartyView.SetActive(false);
            m_ButtonPanel.gameObject.SetActive(false);
        }

        void OnReady()
        {
            OnReadyClicked?.Invoke();
            m_ReadyButton.gameObject.SetActive(false);
        }

        void OnUnready()
        {
            OnUnReadyClicked?.Invoke();
            m_ReadyButton.gameObject.SetActive(true);
        }
    }
}