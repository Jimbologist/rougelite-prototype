using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipPopup : MonoBehaviour
{
    private static TooltipPopup instance;

    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private RectTransform popupObject;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI notesText;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float padding;

    private Camera mainCamera;
    private void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        popupObject.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    private void DisplayTooltipInfo(Lootable lootable)
    {
        //StringBuilder if extra appending is needed.
        StringBuilder text = new StringBuilder();

        //Display title text.
        text.Append("<size=125%><align=center>").Append(lootable.GetColoredName()).Append("</size>");
        nameText.text = text.ToString();
        text.Clear();

        statsText.text = lootable.GetTooltipStatsText();
        notesText.text = lootable.GetTooltipNotesText();
        instance.gameObject.SetActive(true);
        popupObject.transform.position = RectTransformUtility.WorldToScreenPoint(mainCamera, lootable.transform.position + new Vector3(0, 1, 0));

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupObject);
    }

    private void HideTooltipInfo()
    {
        instance.gameObject.SetActive(false);
    }

    public static void DisplayInfo(Lootable lootable)
    {
        instance.DisplayTooltipInfo(lootable);
    }

    public static void HideInfo()
    {
        instance.HideTooltipInfo();
    }

    public static GameObject GetTooltipObject()
    {
        return instance.gameObject;
    }
}
