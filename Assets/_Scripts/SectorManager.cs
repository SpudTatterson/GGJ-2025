using System;
using UnityEngine;

public class SectorManager : MonoBehaviour
{
    [SerializeField] Sector[] sectors;

    int germSectors;
    int bubbleSectors;


    void Awake()
    {
        foreach (Sector s in sectors)
        {
            s.OnGermCapture += CheckCaptureState;
            s.OnBubbleCapture += CheckCaptureState;
        }
    }

    private void CheckCaptureState()
    {
        germSectors = 0;
        bubbleSectors = 0;

        foreach (Sector s in sectors)
        {
            if (s.IsCapturedByBubbles()) bubbleSectors++;
            if (s.IsCapturedByGerms()) germSectors++;
        }

        float  germFillAmount = Mathf.InverseLerp(0, sectors.Length, germSectors);
        float  bubbleFillCount = Mathf.InverseLerp(0, sectors.Length, bubbleSectors);

        UIManager.Instance.sectorFillUpBarBubble.fillAmount = bubbleFillCount;
        UIManager.Instance.sectorFillUpBarGerm.fillAmount = germFillAmount;

    }
}
