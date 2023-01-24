using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyListView : MonoBehaviour
    {
        public event Action OnReadyClicked;
        public event Action OnUnReadyClicked;
        public event Action<string> OnKickClicked;

        [SerializeField] PartyEntryView m_PartyEntryPrefab;
        [SerializeField] LayoutElement m_ScrollElement;
        [SerializeField] LayoutElement m_ButtonPanel;
        [SerializeField] Button m_ReadyButton;
        [SerializeField] Button m_UnReadyButton;
        [SerializeField] VerticalLayoutGroup m_PartyLayoutGroup;
        [SerializeField] VerticalLayoutGroup m_EntryLayoutGroup;

        List<PartyEntryView> m_PartyEntryViews = new List<PartyEntryView>();
        int m_MaxPartySize;

        public void Init(int maxPartySize)
        {
            for (int i = 0; i < maxPartySize; i++)
            {
                var entry = Instantiate(m_PartyEntryPrefab, m_EntryLayoutGroup.transform);
                m_PartyEntryViews.Add(entry);
                entry.Init();
            }

            m_ReadyButton.onClick.AddListener(OnReady);
            m_UnReadyButton.onClick.AddListener(OnUnready);
            m_MaxPartySize = maxPartySize;
            HideParty();
        }

        public void ShowParty()
        {
            foreach (var entry in m_PartyEntryViews)
                entry.gameObject.SetActive(true);
            m_ButtonPanel.gameObject.SetActive(true);

            GrowPanelToEntryList();
        }

        public void HideParty()
        {
            foreach (var entry in m_PartyEntryViews)
                entry.gameObject.SetActive(false);
            m_ButtonPanel.gameObject.SetActive(false);
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

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
        void SetAllEmpty()
        {
            foreach (var entry in m_PartyEntryViews)
                entry.SetEmpty();
        }

        /// <summary>
        /// Assumes an ordered player list from the lobby. We always want to show the player in EntryView 0,
        /// If you are the host, you get special actions visible to you.
        /// </summary>
        public void Refresh(List<PartyPlayer> players, bool imHost)
        {
            SetAllEmpty();
            var localPlayerEntry = m_PartyEntryViews.First();

            //Copy the view list without the player
            var nonLocalPlayerViews = new List<PartyEntryView>(m_PartyEntryViews);
            nonLocalPlayerViews.Remove(localPlayerEntry);

            //Players are ordered by the Lobby SDK
            foreach (var player in players)
            {
                PartyEntryView remotePlayerView = null;
                if (player.IsLocalPlayer)
                    remotePlayerView = localPlayerEntry;
                else
                {
                    remotePlayerView = nonLocalPlayerViews.First();
                    nonLocalPlayerViews.RemoveAt(0);
                }
                //Overwrite the Kick action, since the players could shuffle around, we want to make sure the button kicks the current player.
                if (imHost)
                    remotePlayerView.OnKickClicked = () => OnKickClicked?.Invoke(player.Id);

                remotePlayerView.Refresh(player, imHost);

            }
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
