using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    /// <summary>
    /// Wraps around the Lobby SDK to create a party-like experience.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] LobbyJoinCreateView m_LobbyJoinCreateView;
        [SerializeField] LobbyView m_LobbyView;
        [SerializeField] LobbyListView m_LobbyListView;
        [SerializeField] LobbyJoinPopupView m_LobbyJoinPopupPopupView;
        [SerializeField] NotificationStackView m_NotificationStackView;
        [SerializeField] NameChangeView m_NameChangeView;
        [SerializeField] string m_PlayerProfileName = "NewPlayer";
        [SerializeField] int m_MaxPartyMembers = 4;
        const string k_PartyNamePrefix = "Party";
        const string k_LocalPlayerNamePrefix = "Player";

        Lobby m_PartyLobby;
        LobbyPlayer m_LocalPlayer;
        LobbyEventCallbacks m_PartyEventCallbacks;

        async void Start()
        {
            await Authenticate(m_PlayerProfileName);
            CreateLocalPlayer(m_PlayerProfileName);
            UIInit();
            m_PartyEventCallbacks= new LobbyEventCallbacks();
        }

        /// <summary>
        /// If you are already Authenticating somewhere else, this step can be skipped.
        /// </summary>
        async Task Authenticate(string playerName)
        {
            //We can test locally out-of the box with one Editor and one Build.
            //Using things like ParrelSync, or multiple build from the same machine, requires unique InitializationOptions
            // per instance.
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(playerName);

            await UnityServices.InitializeAsync(initOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        string LoadPlayerName()
        {
            string playerName;
            if (PlayerPrefs.HasKey(LobbyPlayer.nameKey))
                playerName = PlayerPrefs.GetString(LobbyPlayer.nameKey);
            else
            {
                playerName = m_PlayerProfileName;
                PlayerPrefs.SetString(LobbyPlayer.nameKey, playerName);
            }

            return playerName;
        }

        void CreateLocalPlayer(string playerName)
        {
            var id = AuthenticationService.Instance.PlayerId;
            m_LocalPlayer = new LobbyPlayer(id, playerName, true);
        }

        void UIInit()
        {
            //Join Party Popup
            m_LobbyJoinPopupPopupView.Init();
            m_LobbyJoinPopupPopupView.OnJoinClicked += TryLobbyJoin;

            //Party Join/Create
            m_LobbyJoinCreateView.Init();
            m_LobbyJoinCreateView.OnJoinClicked += () => m_LobbyJoinPopupPopupView.Show();
            m_LobbyJoinCreateView.OnCreateClicked += CreateLobby;

            //In-Party
            m_LobbyView.Init();
            m_LobbyView.OnLeaveClicked += OnLeaveLobby;
            m_LobbyView.OnReadyClicked += OnReadyClicked;

            //Party List
            m_LobbyListView.Init(m_MaxPartyMembers);
            m_LobbyListView.OnKickClicked += OnKickFromLobby;
            m_LobbyListView.OnHostClicked += OnSetHost;

            m_NameChangeView.Init(LoadPlayerName());
            m_NameChangeView.OnNameChanged += OnNameChanged;
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

        async void TryLobbyJoin(string joinCode)
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
                m_LobbyJoinPopupPopupView.JoinPartyFailed(joinFailMessage);
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

        async Task TrySetHost(string playerId)
        {
            if (!m_LocalPlayer.IsHost)
                return;
            try
            {
                var setHostOptions = new UpdateLobbyOptions()
                {
                    HostId = playerId
                };
                await LobbyService.Instance.UpdateLobbyAsync(m_PartyLobby.Id, setHostOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        async Task OnJoinedParty(Lobby lobby)
        {
            m_LobbyView.JoinParty(lobby.LobbyCode);
            m_LobbyJoinCreateView.Hide();
            m_LobbyJoinPopupPopupView.Hide();
            m_LobbyListView.Show();

            UpdatePlayers(lobby.Players, lobby.HostId);
            m_PartyEventCallbacks.LobbyChanged += OnLobbyChanged;
            m_PartyEventCallbacks.LobbyEventConnectionStateChanged += OnLobbyConnectionChanged;
            m_PartyEventCallbacks.KickedFromLobby += OnKickedFromParty;

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, m_PartyEventCallbacks);
        }

        async void OnLeaveLobby()
        {
            await RemoveFromParty(m_LocalPlayer.Id);
            Debug.Log("Leaving Lobby");

            //Leave Lobby Regardless of result
            OnLeftParty();
        }

        void OnLobbyConnectionChanged(LobbyEventConnectionState state)
        {
            Debug.Log($"LobbyConnection Changed to {state}");
        }

        void OnLeftParty()
        {
            m_PartyEventCallbacks.LobbyChanged -= OnLobbyChanged;
            m_PartyEventCallbacks.LobbyEventConnectionStateChanged -= OnLobbyConnectionChanged;
            m_PartyEventCallbacks.KickedFromLobby -= OnKickedFromParty;
            m_LobbyJoinCreateView.Show();
            m_LobbyView.LeftParty();
            m_LobbyListView.Hide();
        }

        void UpdatePlayers(List<Player> players, string hostID)
        {
            var partyPlayers = new List<LobbyPlayer>();
            int readyCount = 0;
            foreach (var player in players)
            {
                var partyPlayer = new LobbyPlayer(player);

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

            m_LobbyListView.Refresh(partyPlayers, m_LocalPlayer.IsHost);
        }

        async void OnKickFromLobby(string playerId)
        {
            await RemoveFromParty(playerId);
        }

        async void OnSetHost(string playerId)
        {
            await TrySetHost(playerId);
        }

        async void OnNameChanged(string name)
        {
            m_LocalPlayer.SetName(name);
            PlayerPrefs.SetString(LobbyPlayer.nameKey, name);

            await UpdateLocalPlayer();
        }

        void OnLobbyChanged(ILobbyChanges changes)
        {
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
                    var leftPlayer = new LobbyPlayer(m_PartyLobby.Players[player]);
                    m_NotificationStackView.CreateNotification(1,
                        leftPlayer.Name,
                        "Left the Party!");
                }
            }

            changes.ApplyToLobby(m_PartyLobby);

            UpdatePlayers(m_PartyLobby.Players, m_PartyLobby.HostId);
        }

        void AllMembersReady(List<LobbyPlayer> members)
        {
            Debug.Log($"All {members.Count} party Members Ready!");
        }

        void OnKickedFromParty()
        {
            m_NotificationStackView.CreateNotification(1,
                m_LocalPlayer.Name,
                "You were removed from the Party!");
            Debug.Log($"Removed from party!");
            OnLeftParty();
            ;
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