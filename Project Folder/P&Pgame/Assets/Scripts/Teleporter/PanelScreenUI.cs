using TMPro;
using UnityEngine;

public class PanelScreenUI : MonoBehaviour
{
    [SerializeField] TMP_Text screenText;

    public void SetText(string msg)
    {
        if (!screenText) return;
        screenText.text = msg; 
    }

    public void Clear()
    {
        if (!screenText) return ;
        screenText.text = "";
    }
}
