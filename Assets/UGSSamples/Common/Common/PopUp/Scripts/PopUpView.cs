using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Samples.UI
{
    public class PopUpView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_MessageText;
        [SerializeField] private Button m_CloseButton;

        private void Awake()
        {
            PopUpEvents.Show += OnShow;
            m_CloseButton.onClick.AddListener(Close);
            Close();
        }

        private void OnDestroy()
        {
            PopUpEvents.Show -= OnShow;
        }

        private void OnShow(string message)
        {
            m_MessageText.text = message;
            gameObject.SetActive(true);
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}