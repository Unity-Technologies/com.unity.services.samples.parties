using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyEntryView : MonoBehaviour
    {
        public Action OnKickClicked;
        [SerializeField] GameObject m_EmptyPartyContentPanel;
        [SerializeField] GameObject m_PlayerContentPanel;
        [SerializeField] GameObject m_NotReadyTextPanel;
        [SerializeField] GameObject m_ReadyPanelText;
        [SerializeField] GameObject m_ButtonPanel;
        [SerializeField] GameObject m_HostCrown;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] Button m_KickButton;

        public void Init()
        {
            m_KickButton.onClick.AddListener(() => OnKickClicked?.Invoke());
            SetEmpty();
        }

        //We only refresh active players,
        public void Refresh(PartyPlayer playerData, bool imHost)
        {
            m_NameText.text = playerData.Name;

            m_EmptyPartyContentPanel.SetActive(false);

            m_PlayerContentPanel.SetActive(true);
            m_ReadyPanelText.SetActive(playerData.IsReady);
            m_NotReadyTextPanel.SetActive(!playerData.IsReady);
            m_HostCrown.SetActive(playerData.IsHost);

            if (imHost && !playerData.IsLocalPlayer)
                m_ButtonPanel.SetActive(true);
        }

        public void SetEmpty()
        {
            m_NameText.text = "Empty";
            m_EmptyPartyContentPanel.SetActive(true);

            m_PlayerContentPanel.SetActive(false);
            m_ReadyPanelText.SetActive(false);
            m_NotReadyTextPanel.SetActive(true);
            m_HostCrown.SetActive(false);
            m_ButtonPanel.SetActive(false);
        }
    }
}