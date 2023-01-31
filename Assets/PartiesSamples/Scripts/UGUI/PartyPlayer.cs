using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    /// <summary>
    /// In order to make the custom fields easier to work with, we extend the Lobby's player class
    /// </summary>
    public class PartyPlayer : Player
    {
        public string Name => Data[k_NameKey].Value;
        public bool IsReady => bool.Parse(Data[k_ReadyKey].Value);
        [field: SerializeField] public bool IsHost { get; private set; }
        [field: SerializeField] public bool IsLocalPlayer { get; private set; }
        
        const string k_NameKey = nameof(Name);
        const string k_ReadyKey = nameof(IsReady);

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
                    k_NameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name)
                },
                {
                    k_ReadyKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false")
                }
            };

            IsLocalPlayer = isLocalPlayer;
        }

        public void SetHost(bool isHost)
        {
            IsHost = isHost;
        }

        public void SetReady(bool ready)
        {
            Data[k_ReadyKey].Value = $"{ready}";
        }

        public void SetLocalPlayer(bool isLocalPlayer)
        {
            IsLocalPlayer = isLocalPlayer;
        }
    }
}
