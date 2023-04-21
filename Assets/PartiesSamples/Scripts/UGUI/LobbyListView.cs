using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class LobbyListView : MonoBehaviour
    {
        public event Action<string> OnKickClicked;
        public event Action<string> OnHostClicked;

        [SerializeField] VerticalLayoutGroup m_ContentLayoutGroup;
        [SerializeField] LobbyEntryView m_LobbyEntryPrefab;
        [SerializeField] LayoutElement m_ScrollLayout;
        [SerializeField] int m_MaxLobbyWindowHeight = 500;

        List<LobbyEntryView> m_PartyEntryViews = new List<LobbyEntryView>();

        public void Init(int maxPartySize)
        {
            for (int i = 0; i < maxPartySize; i++)
            {
                var entry = Instantiate(m_LobbyEntryPrefab, m_ContentLayoutGroup.transform);
                m_PartyEntryViews.Add(entry);
                entry.Init();
            }

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            StartCoroutine(GrowScrollListToContent());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            m_ScrollLayout.minHeight = 0;
        }

        //Combine the heights of the elements to create the right size of the party
        //Needs to be run on the next frame after canvas has settled the sizes.
        IEnumerator GrowScrollListToContent()
        {
            yield return new WaitForEndOfFrame();
            float contentSize = m_ContentLayoutGroup.padding.vertical;
            foreach (var entry in m_PartyEntryViews)
            {
                contentSize += entry.RectTransform.rect.height;
                contentSize += m_ContentLayoutGroup.spacing;
            }

            //Leave Space for Ready Button on bottom
            contentSize = Mathf.Clamp(contentSize, 0, m_MaxLobbyWindowHeight);
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
        public void Refresh(List<LobbyPlayer> players, bool imHost)
        {
            SetAllEmpty();
            var localPlayerEntry = m_PartyEntryViews.First();

            //Copy the view list without the player
            var nonLocalPlayerViews = new List<LobbyEntryView>(m_PartyEntryViews);
            nonLocalPlayerViews.Remove(localPlayerEntry);

            //Players are ordered by the Lobby SDK
            foreach (var player in players)
            {
                LobbyEntryView remotePlayerView = null;
                if (player.IsLocalPlayer)
                    remotePlayerView = localPlayerEntry;
                else
                {
                    remotePlayerView = nonLocalPlayerViews.First();
                    nonLocalPlayerViews.RemoveAt(0);
                }

                //Overwrite the Kick action, since the players could shuffle around, we want to make sure the button kicks the current player.
                if (imHost)
                {
                    remotePlayerView.OnKickClicked = () => OnKickClicked?.Invoke(player.Id);
                    remotePlayerView.OnHostClicked = () => OnHostClicked?.Invoke(player.Id);
                }

                remotePlayerView.Refresh(player, imHost);
            }

            StartCoroutine(GrowScrollListToContent());
        }
    }
}