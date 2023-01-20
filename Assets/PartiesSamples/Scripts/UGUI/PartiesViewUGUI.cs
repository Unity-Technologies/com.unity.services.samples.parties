using System;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class PartiesViewUGUI : MonoBehaviour
    {
        [field: SerializeField] public PartyInfoView PartyInfoView { get; private set; }
        [field: SerializeField] public PartyListView PartyListView { get; private set; }

        public void Init(int maxPartysize)
        {
            PartyInfoView.Init();
            PartyListView.Init(maxPartysize);
        }
    }
}
