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

            m_LocalPlayer = new PartyPlayer(id, localPlayerName);
        }

        void UIInit()
        {

            m_PartiesViewUGUI.Init();
            //Party Info
            m_PartyInfoView = m_PartiesViewUGUI.partyInfoView;
            m_PartyInfoView.onJoinPartyPressed += TryJoinLobby;
            m_PartyInfoView.onCreatePressed += CreateLobby;
            m_PartyInfoView.onLeavePressed += LeaveLobby;

            //Party List
            m_PartyListView = m_PartiesViewUGUI.partyListView;
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
                m_PartyLobby = await LobbyService.Instance.CreateLobbyAsync(partyLobbyName, k_MaxPartyMembers, partyLobbyOptions);
                OnJoinedLobby(m_PartyLobby);
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
                OnJoinedLobby(m_PartyLobby);
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
        }

        void OnJoinedLobby(Lobby lobby)
        {
            m_PartyInfoView.JoinParty(lobby.LobbyCode);
            foreach (var player in lobby.Players)
            {
                var partyPlayer = player as PartyPlayer;
                if (player.Id == m_LocalPlayer.Id)
                    partyPlayer.SetLocalPlayer();
                m_PartyListView.UpdatePlayer((PartyPlayer)player);
            }
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
                await LobbyService.Instance.UpdatePlayerAsync(m_PartyLobby.Id, m_LocalPlayer.Id, localUpdatedPlayerData);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
