using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Samples.Utilities;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    /// <summary>
    /// Wraps around the Lobby SDK to create a party-like experience.
    /// </summary>
    public class PartiesManager : MonoBehaviour
    {
        [SerializeField] PartyJoinCreateView m_PartyJoinCreateView;
        [SerializeField] PartyView m_PartyView;
        [SerializeField] PartyListView m_PartyListView;
        [SerializeField] JoinPartyPopupView m_JoinPartyPopupPopupView;
        [SerializeField] NotificationStackView m_NotificationStackView;
        [SerializeField] int m_MaxPartyMembers = 4;
        const string k_PartyNamePrefix = "Party";
        const string k_LocalPlayerNamePrefix = "Player";

        Lobby m_PartyLobby;
        PartyPlayer m_LocalPlayer;
        LobbyEventCallbacks m_PartyEventCallbacks = new LobbyEventCallbacks();

        async void Start()
        {
            await Authenticate();
            CreateLocalPlayer();
            UIInit();
        }

        /// <summary>
        /// If you are already Authenticating somewhere else, this step can be skipped.
        /// </summary>
        async Task Authenticate()
        {
            var authOptions = AuthUtility.TryCreateEditorTestingProfile();
            await UnityServices.InitializeAsync(authOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        void CreateLocalPlayer()
        {
            var id = AuthenticationService.Instance.PlayerId;
            var localPlayerName = $"{k_LocalPlayerNamePrefix}_{id}";
            m_LocalPlayer = new PartyPlayer(id, localPlayerName, true);
        }

        void UIInit()
        {
            //Join Party Popup
            m_JoinPartyPopupPopupView.Init();
            m_JoinPartyPopupPopupView.OnJoinClicked += TryJoin;

            //Party Join/Create
            m_PartyJoinCreateView.Init();
            m_PartyJoinCreateView.OnJoinPartyClicked += () => m_JoinPartyPopupPopupView.Show();
            m_PartyJoinCreateView.OnCreateClicked += CreateParty;

            //In-Party
            m_PartyView.Init();
            m_PartyView.OnLeaveClicked += OnLeaveParty;
            m_PartyView.OnReadyClicked += OnReadyClicked;
            //Party List
            m_PartyListView.Init(m_MaxPartyMembers);
            m_PartyListView.OnKickClicked += OnKickedFromParty;
        }

        async void CreateParty()
        {
            try
            {
                var partyLobbyOptions = new CreateLobbyOptions()
                {
                    IsPrivate = true,
                    Player = m_LocalPlayer
                };
                var partyLobbyName = $"{k_PartyNamePrefix}_{AuthenticationService.Instance.PlayerId}";
                m_PartyLobby = await LobbyService.Instance.CreateLobbyAsync(partyLobbyName,
                        m_MaxPartyMembers,
                        partyLobbyOptions);
                await OnJoinedParty(m_PartyLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        async void TryJoin(string joinCode)
        {
            try
            {
                var joinOptions = new JoinLobbyByCodeOptions()
                {
                    Player = m_LocalPlayer
                };

                m_PartyLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinOptions);
                await OnJoinedParty(m_PartyLobby);
            }
            catch (LobbyServiceException e)
            {
                var joinFailMessage = $"{e.Reason}, {e.Message}";
                m_JoinPartyPopupPopupView.JoinPartyFailed(joinFailMessage);
            }
        }

        async Task RemoveFromParty(string playerID)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(m_PartyLobby.Id, playerID);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        async Task OnJoinedParty(Lobby lobby)
        {
            m_PartyView.JoinParty(lobby.LobbyCode);
            m_PartyJoinCreateView.Hide();
            m_PartyListView.Show();

            UpdatePlayers(lobby.Players, lobby.HostId);
            m_PartyEventCallbacks.LobbyChanged += OnPartyChanged;
            m_PartyEventCallbacks.KickedFromLobby += OnKickedFromParty;

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, m_PartyEventCallbacks);
        }

        async void OnLeaveParty()
        {
            await RemoveFromParty(m_LocalPlayer.Id);
            Debug.Log($"Lobby changed");

            //Leave Lobby Regardless of result
            OnLeftParty();
        }

        void OnLeftParty()
        {
            m_PartyEventCallbacks.LobbyChanged -= OnPartyChanged;
            m_PartyEventCallbacks.KickedFromLobby -= OnKickedFromParty;
            m_PartyJoinCreateView.Show();
            m_PartyView.LeftParty();
            m_PartyListView.Hide();
        }

        void UpdatePlayers(List<Player> players, string hostID)
        {
            var partyPlayers = new List<PartyPlayer>();
            int readyCount = 0;
            foreach (var player in players)
            {
                var partyPlayer = new PartyPlayer(player);

                partyPlayer.SetLocalPlayer(player.Id == m_LocalPlayer.Id);
                if (partyPlayer.IsLocalPlayer)
                    m_LocalPlayer = partyPlayer;

                partyPlayer?.SetHost(partyPlayer.Id == hostID);

                if (partyPlayer.IsReady)
                    readyCount++;
                partyPlayers.Add(partyPlayer);
            }

            if (readyCount >= partyPlayers.Count)
                AllMembersReady(partyPlayers);

            m_PartyListView.Refresh(partyPlayers, m_LocalPlayer.IsHost);
        }

        async void OnKickedFromParty(string playerId)
        {
            await RemoveFromParty(playerId);
        }

        void OnPartyChanged(ILobbyChanges changes)
        {
            Debug.Log($"Lobby changed");
            if (changes.LobbyDeleted)
            {
                OnLeftParty();
                return;
            }

            //We have to get the player data before we apply the Data to our local Lobby
            if (changes.PlayerLeft.Changed)
            {
                foreach (var player in changes.PlayerLeft.Value)
                {
                    var leftPlayer = new PartyPlayer(m_PartyLobby.Players[player]);
                    m_NotificationStackView.CreateNotification(1,
                        leftPlayer.Name,
                        "Left the Party!");
                }
            }

            changes.ApplyToLobby(m_PartyLobby);

            UpdatePlayers(m_PartyLobby.Players, m_PartyLobby.HostId);
        }

        void AllMembersReady(List<PartyPlayer> members)
        {
            Debug.Log($"All {members.Count} party Members Ready!");
        }

        void OnKickedFromParty()
        {
            m_NotificationStackView.CreateNotification(1,
                m_LocalPlayer.Name,
                "You were removed from the Party!");
            OnLeftParty();
        }

        async void OnReadyClicked(bool ready)
        {
            m_LocalPlayer.SetReady(ready);
            await UpdateLocalPlayer();
        }

        async Task UpdateLocalPlayer()
        {
            try
            {
                var localUpdatedPlayerData = new UpdatePlayerOptions() { Data = m_LocalPlayer.Data };
                await LobbyService.Instance.UpdatePlayerAsync(m_PartyLobby.Id, m_LocalPlayer.Id,
                    localUpdatedPlayerData);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

    }
}
