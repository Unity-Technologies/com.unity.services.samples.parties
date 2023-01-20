using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class PartiesManager : MonoBehaviour
    {
        [SerializeField] PartiesViewUGUI m_PartiesViewUGUI;
        const int k_MaxPartyMembers = 4;
        const string k_PartyNamePrefix = "Party";
        const string k_LocalPlayerNamePrefix = "Player";

        PartyInfoView m_PartyInfoView;
        PartyListView m_PartyListView;

        Lobby m_PartyLobby;
        PartyPlayer m_LocalPlayer;
        LobbyEventCallbacks m_PartyEventCallbacks;

        async void Start()
        {
            await Authenticate();
            UIInit();
        }

        async Task Authenticate()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            CreateLocalPlayer();
        }

        void CreateLocalPlayer()
        {
            var id = AuthenticationService.Instance.PlayerId;
            var localPlayerName = $"{k_LocalPlayerNamePrefix}_{id}";

            m_LocalPlayer = new PartyPlayer(id, localPlayerName, true);
        }

        void UIInit()
        {
            m_PartiesViewUGUI.Init(k_MaxPartyMembers);

            //Party Info
            m_PartyInfoView = m_PartiesViewUGUI.PartyInfoView;
            m_PartyInfoView.onJoinPartyPressed += TryJoinLobby;
            m_PartyInfoView.onCreatePressed += CreateLobby;
            m_PartyInfoView.onLeavePressed += LeaveLobby;

            //Party List
            m_PartyListView = m_PartiesViewUGUI.PartyListView;
            m_PartyListView.onReadyClicked += OnReadyClicked;
            m_PartyListView.onUnReadyClicked += OnUnreadyClicked;
        }

        async void CreateLobby()
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
                await OnJoinedLobby(m_PartyLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        async void TryJoinLobby(string joinCode)
        {
            try
            {
                m_PartyLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
                await OnJoinedLobby(m_PartyLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        async void LeaveLobby()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(m_PartyLobby.Id, m_LocalPlayer.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            //Leave Lobby Regardless of call
            OnLeaveParty();
        }

        async Task OnJoinedLobby(Lobby lobby)
        {
            m_PartyInfoView.JoinParty(lobby.LobbyCode);

            UpdatePlayers(lobby.Players, lobby.HostId);

            //Subscribe to lobby updates
            await SubscribeToPartyEvents(lobby.Id);
        }

        void UpdatePlayers(List<Player> players, string hostID)
        {
            var partyPlayers = new List<PartyPlayer>();
            foreach (var player in players)
            {
                var partyPlayer = new PartyPlayer(player);

                partyPlayer?.SetLocalPlayer(player.Id == m_LocalPlayer.Id);
                if (partyPlayer.IsLocalPlayer)
                    m_LocalPlayer = partyPlayer;
                partyPlayer?.SetHost(partyPlayer.Id == hostID);

                partyPlayers.Add(partyPlayer);
            }

            m_PartyListView.UpdatePlayers(partyPlayers, m_LocalPlayer.IsHost);
        }

        async Task SubscribeToPartyEvents(string lobbyID)
        {
            m_PartyEventCallbacks = new LobbyEventCallbacks();
            m_PartyEventCallbacks.LobbyChanged += OnPartyChanged;
            m_PartyEventCallbacks.KickedFromLobby += KickedFromParty;
            m_PartyEventCallbacks.LobbyEventConnectionStateChanged += PartyConnectionStateChange;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID, m_PartyEventCallbacks);
        }

        void OnPartyChanged(ILobbyChanges changes)
        {
            if (changes.LobbyDeleted)
            {
                OnLeaveParty();
                return;
            }

            changes.ApplyToLobby(m_PartyLobby);
            UpdatePlayers(m_PartyLobby.Players, m_PartyLobby.HostId);
        }

        void KickedFromParty()
        {
            Debug.Log("Removed from party!");
            OnLeaveParty();
        }

        void OnLeaveParty()
        {
            m_PartyInfoView.LeftParty();
            m_PartyListView.ClearAll();
            m_PartyEventCallbacks = new LobbyEventCallbacks();
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
            Debug.Log($"Party Connection Changed:  {obj}");
        }
    }
}