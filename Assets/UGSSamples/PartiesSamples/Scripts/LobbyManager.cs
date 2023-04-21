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
    /// Wraps around the Lobby SDK to create a party-like experience.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] LobbyJoinCreateView m_LobbyJoinCreateView;
        [SerializeField] LobbyView m_LobbyView;
        [SerializeField] LobbyListView m_LobbyListView;
        [SerializeField] LobbyJoinPopupView m_LobbyJoinPopupPopupView;
        [SerializeField] int m_MaxPartyMembers = 4;
        const string k_LobbyNamePrefix = "Party";

        Lobby m_PartyLobby;
        LobbyPlayer m_LocalPlayer;
        LobbyEventCallbacks m_PartyEventCallbacks;
        SamplePlayerProfileService m_SamplePlayerProfileService;

        async void Start()
        {
            await SampleAuthenticator.SignIn();
            m_SamplePlayerProfileService = new SamplePlayerProfileService();
            var playerID = AuthenticationService.Instance.PlayerId;
            var player = new Player(playerID);
            m_LocalPlayer = new LobbyPlayer(player);
            m_LocalPlayer.SetName(m_SamplePlayerProfileService.GetName(playerID));
            UIInit();
            m_PartyEventCallbacks = new LobbyEventCallbacks();
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
                var partyLobbyName = $"{k_LobbyNamePrefix}_{m_LocalPlayer.Id}";
                m_PartyLobby = await LobbyService.Instance.CreateLobbyAsync(partyLobbyName,
                    m_MaxPartyMembers,
                    partyLobbyOptions);
                await OnJoinedParty(m_PartyLobby);
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

                m_PartyLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinOptions);
                await OnJoinedParty(m_PartyLobby);
            }
            catch (LobbyServiceException e)
            {
                var joinFailMessage = FormatLobbyError(e);
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
                await LobbyService.Instance.UpdateLobbyAsync(m_PartyLobby.Id, setHostOptions);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
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
            try
            {
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, m_PartyEventCallbacks);
            }
            catch (LobbyServiceException e)
            {
                PopUpLobbyError(e);
            }
        }

        async void OnLeaveLobby()
        {
            await RemoveFromParty(m_LocalPlayer.Id);
            NotificationEvents.onNotify?.Invoke(
                new NotificationData(
                    "You", "Left the Party!", 1));

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
            m_PartyLobby = null;
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

        async void OnNameChanged(string newName)
        {
            m_LocalPlayer.SetName(newName);
            PlayerPrefs.SetString(LobbyPlayer.nameKey, newName);
            if (m_PartyLobby != null)
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
                    NotificationEvents.onNotify?.Invoke(
                        new NotificationData(leftPlayer.Name, "Left the Party!", 1));
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
            NotificationEvents.onNotify?.Invoke(
                new NotificationData(m_LocalPlayer.Name, "Removed from the Party!", 1));
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
