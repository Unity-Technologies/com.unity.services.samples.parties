using System;
using TMPro;
using UnityEngine;

public class NameChangeView : MonoBehaviour
{
    public event Action<string> OnNameChanged;
    [SerializeField] TMP_InputField m_NameField;

    public void SetName(string name)
    {
        m_NameField.SetTextWithoutNotify(name);
    }
    public void Init(string startname)
    {
        SetName(startname);
        m_NameField.onEndEdit.AddListener((s) => OnNameChanged?.Invoke(s));
    }
}