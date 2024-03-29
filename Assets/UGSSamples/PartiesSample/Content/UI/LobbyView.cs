using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class LobbyView : MonoBehaviour
    {
        public event Action<bool> OnReadyClicked;
        public event Action OnLeaveClicked;

        [SerializeField] LayoutElement m_ButtonPanel;
        [SerializeField] TMP_InputField m_InGameJoinCode;
        [SerializeField] TMP_Text m_PlayerCount;

        [SerializeField] Button m_ReadyButton;
        [SerializeField] Button m_CancelButton;

        [SerializeField] Button m_CopyButton;

        [SerializeField] Button m_LeaveButton;

        public void Init()
        {
            m_CopyButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = m_InGameJoinCode.text);
            m_LeaveButton.onClick.AddListener(() => OnLeaveClicked?.Invoke());
            m_ReadyButton.onClick.AddListener(OnReady);
            m_CancelButton.onClick.AddListener(OnUnready);
        }

        public void SetPlayerCount(int count, int max)
        {
            m_PlayerCount.SetText($"({count}/{max})");
        }

        public void JoinLobby(string lobbyCode)
        {
            m_InGameJoinCode.text = lobbyCode;
            gameObject.SetActive(true);
            m_ButtonPanel.gameObject.SetActive(true);
        }

        public void LeftLobby()
        {
            gameObject.SetActive(false);
            m_ButtonPanel.gameObject.SetActive(false);
        }

        void OnReady()
        {
            OnReadyClicked?.Invoke(true);
            m_ReadyButton.gameObject.SetActive(false);
        }

        void OnUnready()
        {
            OnReadyClicked?.Invoke(false);
            m_ReadyButton.gameObject.SetActive(true);
        }
    }
}