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

        [SerializeField] Transform m_ContentTransform;
        [SerializeField] PartyEntryView m_PartyEntryPrefab;

        [SerializeField] Button m_ReadyButton;
        [SerializeField] Button m_UnReadyButton;

        Dictionary<string, PartyEntryView> m_PartyEntryViews = new Dictionary<string, PartyEntryView>();

        public void Init()
        {
            m_ReadyButton.onClick.AddListener(OnReadyClicked);
            m_UnReadyButton.onClick.AddListener(OnUnreadyClicked);
        }

        public void Clear()
        {
            foreach(var entry in m_PartyEntryViews.Values)
                entry.Clear();
        }

        public void UpdatePlayer(PartyPlayer playerData)
        {
            if (m_PartyEntryViews.ContainsKey(playerData.Id))
            {
                m_PartyEntryViews[playerData.Id].Refresh(playerData);
            }
            else
            {
                var entry = Instantiate(m_PartyEntryPrefab, m_ContentTransform.transform);
                m_PartyEntryViews.Add(playerData.Id, entry);
            }
        }

        void OnReadyClicked()
        {
            onReadyClicked?.Invoke();
            m_ReadyButton.gameObject.SetActive(false);
        }

        void OnUnreadyClicked()
        {
            onUnReadyClicked?.Invoke();
            m_ReadyButton.gameObject.SetActive(true);
        }
    }
}