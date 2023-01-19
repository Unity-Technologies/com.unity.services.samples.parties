using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Unity.Services.Samples.Parties
{
    public class PartyListView : MonoBehaviour
    {
        public event Action onReadyClicked;
        public event Action onUnReadyClicked;

        [SerializeField] ScrollRect m_PlayerListScroller;
        [SerializeField] PartyEntryView m_PlayerEntryPrefab;


        [SerializeField] Button m_ReadyButton;
        [SerializeField] Button m_UnReadyButton;

        Dictionary<string, PartyEntryView> m_PartyEntryViews = new Dictionary<string, PartyEntryView>();

        public void Init()
        {
            m_ReadyButton.onClick.AddListener(OnReadyClicked);
            m_UnReadyButton.onClick.AddListener(OnUnreadyClicked);
        }

        public void UpdatePlayer(PartyPlayer playerData)
        {

            if (m_PartyEntryViews.ContainsKey(playerData.Id))
            {
                m_PartyEntryViews[playerData.Id].Refresh(playerData);
            }
            else
            {
                var entry = Instantiate(m_PlayerEntryPrefab, m_PlayerListScroller.transform);
                m_PartyEntryViews.Add(playerData.Id, entry);
            }

        }

        public void Refresh(List<PartyPlayer> players)
        {
            foreach (var partyPLayer in players)
            {
                UpdatePlayer(partyPLayer);
            }
        }


        void OnReadyClicked()
        {
            onReadyClicked?.Invoke();
            m_ReadyButton.gameObject.SetActive(false);
            m_UnReadyButton.gameObject.SetActive(true);

        }

        void OnUnreadyClicked()
        {
            onUnReadyClicked?.Invoke();
            m_ReadyButton.gameObject.SetActive(true);
            m_UnReadyButton.gameObject.SetActive(false);

        }
    }
}
