using System;
using UnityEngine;
using UnityEngine.UI;
namespace Unity.Services.Samples.Parties
{
    public class PartyJoinCreateView : MonoBehaviour
    {
        public event Action OnJoinPartyClicked;
        public event Action OnCreateClicked;
        [SerializeField] Button m_JoinButton;
        [SerializeField] Button m_CreateButton;

        public void Init()
        {
            m_JoinButton.onClick.AddListener(() => OnJoinPartyClicked?.Invoke());
            m_CreateButton.onClick.AddListener(() => OnCreateClicked?.Invoke());
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

}
