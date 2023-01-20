using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyEntryView : MonoBehaviour
    {
        public event Action onKick;
        [SerializeField] GameObject m_PartyContentPanel;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] GameObject m_ReadyPanel;
        [SerializeField] Image m_PlayerPanel;
        [SerializeField] TMP_Text m_PartyLeaderText;
        [SerializeField] Button m_KickButton;

        Color m_PlayerColor = new Color(0.3f, 0.6f, 0.7f);
        Color m_DefaultColor = new Color(0.3f, 0.6f, 0.7f);

        public void Init()
        {
            m_KickButton.onClick.AddListener(() => onKick?.Invoke());
            m_KickButton.gameObject.SetActive(false);
            Clear();
        }

        public void Refresh(PartyPlayer playerData, bool imHost)
        {
            if (imHost && !playerData.IsLocalPlayer)
                m_KickButton.gameObject.SetActive(true);

            m_PartyContentPanel.SetActive(true);
            m_NameText.text = playerData.Name;
            m_ReadyPanel.gameObject.SetActive(playerData.IsReady);
            m_PlayerPanel.color = playerData.IsLocalPlayer ? m_PlayerColor : m_DefaultColor;
            m_PartyLeaderText.alpha = playerData.IsHost ? 1 : 0;
        }

        public void Clear()
        {
            m_KickButton.gameObject.SetActive(false);
            m_NameText.text = "Empty";
            m_PartyContentPanel.gameObject.SetActive(false);
            m_ReadyPanel.gameObject.SetActive(false);
            m_PlayerPanel.color = m_DefaultColor;
            m_PartyLeaderText.alpha = 0;
        }
    }
}