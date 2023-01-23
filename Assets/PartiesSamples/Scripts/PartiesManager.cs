using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Samples.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Services.Samples.Parties
{

    /// <summary>
    /// Wraps around the Lobby SDK to create a party-like experience.
    /// </summary>
    public class PartiesManager : MonoBehaviour
    {
        //Surfacing Events as gameplay hooks?
        public UnityEvent<List<PartyPlayer>> OnAllPartyMembersReady;
        [field: SerializeField] public PartyInfoView PartyInfoView { get; private set; }
        [field: SerializeField] public PartyListView PartyListView { get; private set; }
        const int k_MaxPartyMembers = 4;
        const string k_PartyNamePrefix = "Party";
        const string k_LocalPlayerNamePrefix = "Player";

        Lobby m_PartyLobby;
        PartyPlayer m_LocalPlayer;
        LobbyEventCallbacks m_PartyEventCallbacks;

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


            //Party Info
            PartyInfoView.Init();

            PartyInfoView.OnJoinPartyClicked += TryJoinParty;
            PartyInfoView.OnCreateClicked += CreateParty;
            PartyInfoView.OnLeaveClicked += LeaveParty;

            //Party List
            PartyListView.Init(k_MaxPartyMembers);

            PartyListView.OnReadyClicked += OnReadyClicked;
            PartyListView.OnUnReadyClicked += OnUnreadyClicked;
            PartyListView.OnKickClicked += OnKickedFromParty;
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
                m_PartyLobby =
                    await LobbyService.Instance.CreateLobbyAsync(partyLobbyName, k_MaxPartyMembers, partyLobbyOptions);
                await OnJoinedParty(m_PartyLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        async void TryJoinParty(string joinCode)
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
                Debug.Log(e);
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
            PartyInfoView.JoinParty(lobby.LobbyCode);

            PartyListView.ShowParty();

            UpdatePlayers(lobby.Players, lobby.HostId);

            await SubscribeToPartyEvents(lobby.Id);

            async Task SubscribeToPartyEvents(string lobbyID)
            {
                m_PartyEventCallbacks = new LobbyEventCallbacks();
                m_PartyEventCallbacks.LobbyChanged += OnPartyChanged;
                m_PartyEventCallbacks.KickedFromLobby += KickedFromParty;
                m_PartyEventCallbacks.LobbyEventConnectionStateChanged += PartyConnectionStateChange;
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID, m_PartyEventCallbacks);
            }
        }

        void OnLeftParty()
        {
            PartyInfoView.LeftParty();
            PartyListView.HideParty();
            m_PartyEventCallbacks = new LobbyEventCallbacks();
        }

        async void LeaveParty()
        {
            await RemoveFromParty(m_LocalPlayer.Id);
            //Leave Lobby Regardless of call
            OnLeftParty();
        }

        void UpdatePlayers(List<Player> players, string hostID)
        {
            var partyPlayers = new List<PartyPlayer>();
            int readyCount = 0;
            foreach (var player in players)
            {
                var partyPlayer = new PartyPlayer(player);

                partyPlayer?.SetLocalPlayer(player.Id == m_LocalPlayer.Id);
                if (partyPlayer.IsLocalPlayer)
                    m_LocalPlayer = partyPlayer;

                partyPlayer?.SetHost(partyPlayer.Id == hostID);

                if (partyPlayer.IsReady)
                    readyCount++;
                partyPlayers.Add(partyPlayer);
            }

            if (readyCount >= partyPlayers.Count)
                AllMembersReady(partyPlayers);

            PartyListView.Refresh(partyPlayers, m_LocalPlayer.IsHost);
        }

        async void OnKickedFromParty(string playerId)
        {
            await RemoveFromParty(playerId);
        }

        void OnPartyChanged(ILobbyChanges changes)
        {
            if (changes.LobbyDeleted)
            {
                OnLeftParty();
                return;
            }

            changes.ApplyToLobby(m_PartyLobby);
            UpdatePlayers(m_PartyLobby.Players, m_PartyLobby.HostId);
        }

        void AllMembersReady(List<PartyPlayer> members)
        {
            OnAllPartyMembersReady?.Invoke(members);
            Debug.Log($"All {members.Count} party Members Ready!");
        }

        void KickedFromParty()
        {
            Debug.Log("Removed from party!");
            OnLeftParty();
        }

        async void OnReadyClicked()
        {
            m_LocalPlayer.SetReady(true);
            await UpdateLocalPlayer();
        }

        async void OnUnreadyClicked()
        {
            m_LocalPlayer.SetReady(false);
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

        void PartyConnectionStateChange(LobbyEventConnectionState obj)
        {
            Debug.Log($"Party Connection Changed to {obj}");
        }
    }
}
