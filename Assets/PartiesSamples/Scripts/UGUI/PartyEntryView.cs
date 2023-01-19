using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Parties
{
    public class PartyEntryView : MonoBehaviour
    {
        [SerializeField] Image m_EntryFrame;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] Image m_ReadyFrame;
        [SerializeField] Image m_IsPlayerFrame;


        public void Refresh(PartyPlayer playerData)
        {
            m_EntryFrame.gameObject.SetActive(true);
            m_NameText.text = playerData.Name;
            m_ReadyFrame.gameObject.SetActive(playerData.IsReady);
            m_IsPlayerFrame.gameObject.SetActive(playerData.IsLocalPlayer);
        }


        public void Clear()
        {
            m_EntryFrame.gameObject.SetActive(false);
            m_ReadyFrame.gameObject.SetActive(false);
            m_IsPlayerFrame.gameObject.SetActive(false);

        }

    }
}
