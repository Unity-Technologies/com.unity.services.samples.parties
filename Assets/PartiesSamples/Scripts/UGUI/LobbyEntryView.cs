using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class LobbyEntryView : MonoBehaviour
    {
        public Action OnKickClicked;
        public Action OnHostClicked;

        [SerializeField] GameObject m_EmptyPartyContentPanel;
        [SerializeField] GameObject m_PlayerContentPanel;
        [SerializeField] GameObject m_NotReadyTextPanel;
        [SerializeField] GameObject m_ReadyPanelText;
        [SerializeField] GameObject m_ButtonPanel;
        [SerializeField] GameObject m_HostCrown;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] Button m_KickButton;
        [SerializeField] Button m_HostButton;

        public void Init()
        {
            m_KickButton.onClick.AddListener(() => OnKickClicked?.Invoke());
            m_HostButton.onClick.AddListener(() => OnHostClicked?.Invoke());
            SetEmpty();
        }

        //We only refresh active players,
        public void Refresh(LobbyPlayer playerData, bool imHost)
        {
            m_NameText.text = playerData.Name;
            ShowPlayer(true);
            SetReady(playerData.IsReady);
            m_HostCrown.SetActive(playerData.IsHost);

            if (imHost && !playerData.IsLocalPlayer)
            {
                m_ButtonPanel.SetActive(true);
            }
        }

        public void SetEmpty()
        {
            m_NameText.text = "Empty";
            ShowPlayer(false);
            SetReady(false);
            m_HostCrown.SetActive(false);
            m_ButtonPanel.SetActive(false);
        }

        void ShowPlayer(bool show)
        {
            m_PlayerContentPanel.SetActive(show);
            m_EmptyPartyContentPanel.SetActive(!show);
        }

        void SetReady(bool isReady)
        {
            m_ReadyPanelText.SetActive(isReady);
            m_NotReadyTextPanel.SetActive(!isReady);
        }
    }
}