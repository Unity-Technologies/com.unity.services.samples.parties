using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    /// <summary>
    /// In order to make the custom fields easier to work with, we extend the Lobby's player class
    /// </summary>
    public class PartyPlayer : Player
    {
        public string Name => Data[nameof(Name)].Value;
        public bool IsReady => bool.Parse(Data[nameof(IsReady)].Value);
        [field: SerializeField] public bool IsHost { get; private set; }
        [field: SerializeField] public bool IsLocalPlayer { get; private set; }

        public PartyPlayer(Player player)
            : base(player.Id)
        {
            Data = player.Data;
        }

        public PartyPlayer(string playerID, string name, bool isLocalPlayer)
            : base(playerID)
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    nameof(Name), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name)
                },
                {
                    nameof(IsReady), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false")
                }
            };

            IsLocalPlayer = isLocalPlayer;
        }

        public void SetHost(bool isHost)
        {
            IsHost = isHost;
        }

        public void SetName(string name)
        {
            Data[nameof(Name)].Value = name;
        }

        public void SetReady(bool ready)
        {
            Data[nameof(IsReady)].Value = $"{ready}";
        }

        public void SetLocalPlayer(bool isLocalPlayer)
        {
            IsLocalPlayer = isLocalPlayer;
        }
    }
}