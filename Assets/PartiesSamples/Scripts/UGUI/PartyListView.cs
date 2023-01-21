using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyListView : MonoBehaviour
    {
        public event Action onReadyClicked;
        public event Action onUnReadyClicked;
        public event Action<string> onKickClicked;

        [SerializeField] PartyEntryView m_PartyEntryPrefab;
        [SerializeField] LayoutElement m_ScrollElement;
        [SerializeField] LayoutElement m_ButtonPanel;
        [SerializeField] Button m_ReadyButton;
        [SerializeField] Button m_UnReadyButton;
        [SerializeField] VerticalLayoutGroup m_PartyLayoutGroup;
        [SerializeField] VerticalLayoutGroup m_EntryLayoutGroup;

        List<PartyEntryView> m_PartyEntryViews = new List<PartyEntryView>();
        Dictionary<int, PartyEntryView> m_PartyPlayerIndexMap = new Dictionary<int, PartyEntryView>();
        int m_MaxPartySize;

        public void Init(int maxPartySize)
        {
            for (int i = 0; i < maxPartySize; i++)
            {
                var entry = Instantiate(m_PartyEntryPrefab, m_EntryLayoutGroup.transform);
                m_PartyEntryViews.Add(entry);
                entry.Init();
            }

            m_ReadyButton.onClick.AddListener(OnReadyClicked);
            m_UnReadyButton.onClick.AddListener(OnUnreadyClicked);
            m_MaxPartySize = maxPartySize;
            HideParty();
        }

        public void ShowParty()
        {
            foreach (var entry in m_PartyEntryViews)
                entry.gameObject.SetActive(true);
            m_ButtonPanel.gameObject.SetActive(true);

            GrowPanelToEntryList();

            //I couldnt create am expandable List view with a scroller using only Canvas behaviours.
            //So we set the size of the Party window and list view manually.
            void GrowPanelToEntryList()
            {
                var entryHeight = m_PartyEntryPrefab.GetComponent<LayoutElement>().minHeight +
                    m_EntryLayoutGroup.spacing;
                m_ScrollElement.minHeight = m_MaxPartySize * entryHeight;

                var panelSpacing = m_PartyLayoutGroup.spacing;
                var panelPadding = m_PartyLayoutGroup.padding.top + m_PartyLayoutGroup.padding.bottom;
                var scrollSize = m_MaxPartySize * entryHeight + panelSpacing;
                var buttonHeight = m_ButtonPanel.minHeight + panelSpacing;
                var panelSize = scrollSize + buttonHeight + panelPadding;

                gameObject.GetComponent<RectTransform>()
                    .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize);
            }
        }

        public void HideParty()
        {
            foreach (var entry in m_PartyEntryViews)
                entry.gameObject.SetActive(false);
            m_ButtonPanel.gameObject.SetActive(false);
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        void SetAllEmpty()
        {
            foreach (var entry in m_PartyEntryViews)
                entry.SetEmpty();
        }

        /// <summary>
        /// Assumes an ordered player list from the lobby. We always want to show the player in EntryView 0, but need
        /// to keep the lobby index for removal purposes later.
        /// If you are the host, you get special actions visible to you.
        /// </summary>
        public void Refresh(List<PartyPlayer> players, bool imHost)
        {
            SetAllEmpty();
            m_PartyPlayerIndexMap = new Dictionary<int, PartyEntryView>();
            var localPlayerEntry = m_PartyEntryViews.First();

            //Isolate non-local players
            var nonLocalPlayerViews = new List<PartyEntryView>(m_PartyEntryViews);
            nonLocalPlayerViews.Remove(localPlayerEntry);

            foreach (var player in players)
            {
                var playerIndex = players.IndexOf(player);
                PartyEntryView finalEntryView = null;
                if (player.IsLocalPlayer)
                    finalEntryView = localPlayerEntry;
                else
                {
                    finalEntryView = nonLocalPlayerViews.First();
                    nonLocalPlayerViews.RemoveAt(0);
                }

                if (imHost)
                    finalEntryView.onKick = () => onKickClicked?.Invoke(player.Id);

                finalEntryView.Refresh(player, imHost);

                m_PartyPlayerIndexMap.Add(playerIndex, finalEntryView);
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