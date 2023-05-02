using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Samples.UI;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    /// <summary>
    /// Wraps around the Lobby SDK to create a simple lobby experience
    /// (In the case of the Party Sample, The party is just a lobby)
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] LobbyJoinCreateView m_LobbyJoinCreateView;
        [SerializeField] LobbyView m_LobbyView;
        [SerializeField] LobbyListView m_LobbyListView;
        [SerializeField] LobbyJoinPopupView m_LobbyJoinPopupPopupView;
        [SerializeField] int m_MaxLobbyMembers = 4;
        [SerializeField] string m_LobbyNameSuffix;

        Lobby m_LocalLobby;
        LobbyPlayer m_LocalPlayer;
        LobbyEventCallbacks m_LobbyEventCallbacks;

        async void Start()
        {
            await Init();
        }

        async Task Init()
        {
            await LogIn();
            UIInit();
            m_LobbyEventCallbacks = new LobbyEventCallbacks();
        }

        async Task LogIn()
        {
            await UnityServiceAuthenticator.TrySignInAsync();
            var playerID = AuthenticationService.Instance.PlayerId;
            var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            m_LocalPlayer = new LobbyPlayer(playerID, playerName, true);
        }

        void UIInit()
        {
            //Join Lobby Popup
            m_LobbyJoinPopupPopupView.Init();
            m_LobbyJoinPopupPopupView.OnJoinClicked += TryLobbyJoin;

            //Lobby Join/Create
            m_LobbyJoinCreateView.Init();
            m_LobbyJoinCreateView.OnJoinClicked += () => m_LobbyJoinPopupPopupView.Show();
            m_LobbyJoinCreateView.OnCreateClicked += CreateLobby;

            //In-Lobby
            m_LobbyView.Init();
            m_LobbyView.OnLeaveClicked += OnLeaveLobby;
            m_LobbyView.OnReadyClicked += OnReadyClicked;

            //Lobby Member List
            m_LobbyListView.Init(m_MaxLobbyMembers);
            m_LobbyListView.OnKickClicked += OnKickFromLobby;
            m_LobbyListView.OnHostClicked += OnSetHost;
        }

        async void CreateLobby()
        {
            try
            {
                var createLobbyOptions = new CreateLobbyOptions()
                {
                    IsPrivate = true,
                    Player = m_LocalPlayer
                };
                var lobbyName = $"{m_LocalPlayer.Name}'s {m_LobbyNameSuffix}";
                m_LocalLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,
                    m_MaxLobbyMembers,
                    createLobbyOptions);
                await OnJoinedLobby(m_LocalLobby);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
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

                m_LocalLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinOptions);
                await OnJoinedLobby(m_LocalLobby);
            }
            catch (LobbyServiceException e)
            {
                var joinFailMessage = FormatLobbyError(e);
                m_LobbyJoinPopupPopupView.JoinLobbyFailed(joinFailMessage);
            }
        }

        async Task RemoveFromLobby(string playerID)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(m_LocalLobby.Id, playerID);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
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
                await LobbyService.Instance.UpdateLobbyAsync(m_LocalLobby.Id, setHostOptions);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
            }
        }

        async Task OnJoinedLobby(Lobby lobby)
        {
            m_LobbyView.JoinLobby(lobby.LobbyCode);
            m_LobbyJoinCreateView.Hide();
            m_LobbyJoinPopupPopupView.Hide();
            m_LobbyListView.Show();

            UpdatePlayers(lobby.Players, lobby.HostId);
            m_LobbyEventCallbacks.LobbyChanged += OnLobbyChanged;
            m_LobbyEventCallbacks.LobbyEventConnectionStateChanged += OnLobbyConnectionChanged;
            m_LobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;
            try
            {
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, m_LobbyEventCallbacks);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
            }
        }

        async void OnLeaveLobby()
        {
            await RemoveFromLobby(m_LocalPlayer.Id);
            NotificationEvents.onNotify?.Invoke(
                new NotificationData(
                    "You", "Left!", 1));

            //Leave Lobby Regardless of result
            OnLeftLobby();
        }

        void OnLobbyConnectionChanged(LobbyEventConnectionState state)
        {
            Debug.Log($"LobbyConnection Changed to {state}");
        }

        void OnLeftLobby()
        {
            m_LobbyEventCallbacks.LobbyChanged -= OnLobbyChanged;
            m_LobbyEventCallbacks.LobbyEventConnectionStateChanged -= OnLobbyConnectionChanged;
            m_LobbyEventCallbacks.KickedFromLobby -= OnKickedFromLobby;
            m_LobbyJoinCreateView.Show();
            m_LobbyView.LeftLobby();
            m_LobbyListView.Hide();
            m_LocalLobby = null;
        }

        void UpdatePlayers(List<Player> players, string hostID)
        {
            var lobbyPlayers = new List<LobbyPlayer>();
            int readyCount = 0;
            foreach (var player in players)
            {
                var lobbyPlayer = new LobbyPlayer(player);

                lobbyPlayer.SetLocalPlayer(player.Id == m_LocalPlayer.Id);
                if (lobbyPlayer.IsLocalPlayer)
                    m_LocalPlayer = lobbyPlayer;

                lobbyPlayer?.SetHost(lobbyPlayer.Id == hostID);

                if (lobbyPlayer.IsReady)
                    readyCount++;
                lobbyPlayers.Add(lobbyPlayer);
            }

            if (readyCount >= lobbyPlayers.Count)
                AllMembersReady(lobbyPlayers);
            m_LobbyView.SetPlayerCount(lobbyPlayers.Count, m_MaxLobbyMembers);
            m_LobbyListView.Refresh(lobbyPlayers, m_LocalPlayer.IsHost);
        }

        async void OnKickFromLobby(string playerId)
        {
            await RemoveFromLobby(playerId);
        }

        async void OnSetHost(string playerId)
        {
            await TrySetHost(playerId);
        }

        void OnLobbyChanged(ILobbyChanges changes)
        {
            if (changes.LobbyDeleted)
            {
                OnLeftLobby();
                return;
            }

            //We have to get the player data before we apply the Data to our local Lobby
            if (changes.PlayerLeft.Changed)
            {
                foreach (var player in changes.PlayerLeft.Value)
                {
                    var leftPlayer = new LobbyPlayer(m_LocalLobby.Players[player]);
                    NotificationEvents.onNotify?.Invoke(
                        new NotificationData(leftPlayer.Name, "Left!", 1));
                }
            }

            changes.ApplyToLobby(m_LocalLobby);

            UpdatePlayers(m_LocalLobby.Players, m_LocalLobby.HostId);
        }

        void AllMembersReady(List<LobbyPlayer> members)
        {
            Debug.Log($"All {members.Count} Members Ready!");
        }

        void OnKickedFromLobby()
        {
            NotificationEvents.onNotify?.Invoke(
                new NotificationData(m_LocalPlayer.Name, "Removed!", 1));
            OnLeftLobby();
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
                await LobbyService.Instance.UpdatePlayerAsync(m_LocalLobby.Id, m_LocalPlayer.Id,
                    localUpdatedPlayerData);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
            }
        }

        string FormatLobbyError(LobbyServiceException e)
        {
            return $"{e.Reason}({e.ErrorCode}) :\n {e.Message}";
        }

        void PopUpLobbyError(LobbyServiceException e)
        {
            var error = FormatLobbyError(e);
            PopUpEvents.Show?.Invoke(error);
        }
    }
}