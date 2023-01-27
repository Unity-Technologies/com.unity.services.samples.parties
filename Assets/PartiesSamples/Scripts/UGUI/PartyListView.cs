using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyListView : MonoBehaviour
    {
        public event Action<string> OnKickClicked;

        [SerializeField] VerticalLayoutGroup m_EntryLayoutGroup;
        [SerializeField] PartyEntryView m_PartyEntryPrefab;
        [SerializeField] LayoutElement m_ScrollLayout;

        List<PartyEntryView> m_PartyEntryViews = new List<PartyEntryView>();
        int m_Partysize = 1;
        public void Init(int maxPartySize)
        {
            for (int i = 0; i < maxPartySize; i++)
            {
                var entry = Instantiate(m_PartyEntryPrefab, m_EntryLayoutGroup.transform);
                m_PartyEntryViews.Add(entry);
                entry.Init();
            }

            m_Partysize = maxPartySize;
            Hide();
        }

        public void Show()
        {
            m_ScrollLayout.gameObject.SetActive(true);
            GrowScrollListToContent();
        }

        public void Hide()
        {
            m_ScrollLayout.gameObject.SetActive(false);
            m_ScrollLayout.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        //Combine the heights of the elements to create the right size of the party
        void GrowScrollListToContent()
        {
            var contentSize = m_EntryLayoutGroup.padding.vertical +
                m_Partysize *
                (m_EntryLayoutGroup.spacing +
                    m_PartyEntryPrefab.GetComponent<RectTransform>().rect.height);

            m_ScrollLayout.minHeight = contentSize;
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
    }
}