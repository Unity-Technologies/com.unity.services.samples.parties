using System;
using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class PartiesViewUGUI : MonoBehaviour
    {
        [field: SerializeField] public PartyInfoView partyInfoView { get; private set; }

        public void Init()
        {
            partyInfoView.Init();
        }
    }
}