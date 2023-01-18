using UnityEngine;

namespace Unity.Services.Samples.Parties
{
    public class PartiesManager : MonoBehaviour
    {
        [SerializeField] PartiesViewUGUI m_PartiesViewUGUI;

        PartyInfoView m_PartyInfoView;
        void Start()
        {
            UIInit();
        }

        void UIInit()
        {

            m_PartiesViewUGUI.Init();
            m_PartyInfoView = m_PartiesViewUGUI.partyInfoView;


        }
    }
}