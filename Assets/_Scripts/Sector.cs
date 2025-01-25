using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Sector : MonoBehaviour
{
    [SerializeField] float captureTime = 5f;
    [SerializeField] Vector3 sectorHalfExtents = new Vector3(1, 1, 1);
    [SerializeField] Vector3 centerOffset = new Vector3(0, 0, 0);
    [SerializeField] LayerMask troopLayers;

    [SerializeField] Sector previousSectorGerm;   // The previous sector for Germs
    [SerializeField] Sector previousSectorBubble; // The previous sector for Bubbles

    private int germCount;
    private int bubbleCount;

    [SerializeField, ReadOnly] bool bubbleSector;
    [SerializeField, ReadOnly] bool germSector;

    private bool bubblesCapturing;
    private bool germsCapturing;

    private float bubbleTimer;
    private float germTimer;

    [SerializeField] float normalDecayRate = 0.5f;
    [SerializeField] float acceleratedDecayRate = 2f;

    MeshRenderer meshRenderer;

    public event Action OnGermCapture;
    public event Action OnBubbleCapture;

    void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    void Update()
    {
        GetTroopsInSector();

        if (CanGermsCapture())
        {
            StartGermCapture();
        }
        else if (CanBubblesCapture())
        {
            StartBubbleCapture();
        }
        else
        {
            ResetCaptureStates();
        }

        UpdateTimers();
    }

    private void GetTroopsInSector()
    {
        List<BaseTroop> troops = ComponentUtility.GetComponentsInBox<BaseTroop>(transform.position + centerOffset, sectorHalfExtents, troopLayers);
        germCount = 0;
        bubbleCount = 0;

        foreach (BaseTroop troop in troops)
        {
            if (troop is BaseEnemyTroop)
            {
                bubbleCount++;
            }
            else
            {
                germCount++;
            }
        }
    }

    private bool CanGermsCapture()
    {
        return germCount > 0 && bubbleCount == 0 && !bubblesCapturing && CanCaptureSector(germ: true) && !germSector;
    }

    private bool CanBubblesCapture()
    {
        return bubbleCount > 0 && germCount == 0 && !germsCapturing && CanCaptureSector(germ: false) && !bubbleSector;
    }

    private bool CanCaptureSector(bool germ)
    {
        if (germ)
        {
            if (previousSectorGerm == null) return true; // No previous sector for Germs
            return previousSectorGerm.germSector;       // Check if the previous sector is captured by Germs
        }
        else
        {
            if (previousSectorBubble == null) return true; // No previous sector for Bubbles
            return previousSectorBubble.bubbleSector;      // Check if the previous sector is captured by Bubbles
        }
    }

    private void StartGermCapture()
    {
        germsCapturing = true;
        bubblesCapturing = false;
        bubbleTimer = 0; // Reset the opposing timer
    }

    private void StartBubbleCapture()
    {
        bubblesCapturing = true;
        germsCapturing = false;
        germTimer = 0; // Reset the opposing timer
    }

    private void ResetCaptureStates()
    {
        bubblesCapturing = false;
        germsCapturing = false;
    }

    private void UpdateTimers()
    {
        float bubbleDecayRate = normalDecayRate;
        float germDecayRate = normalDecayRate;

        // Accelerate decay if the other team is alone in the sector
        if (germCount > 0 && bubbleCount == 0)
        {
            bubbleDecayRate = acceleratedDecayRate;
        }
        else if (bubbleCount > 0 && germCount == 0)
        {
            germDecayRate = acceleratedDecayRate;
        }

        if (germsCapturing)
        {
            germTimer += Time.deltaTime;
            if (germTimer >= captureTime)
            {
                CaptureGermSector();
            }
        }
        else if (bubblesCapturing)
        {
            bubbleTimer += Time.deltaTime;
            if (bubbleTimer >= captureTime)
            {
                CaptureBubbleSector();
            }
        }
        else
        {
            // Gradual decay with adjusted rates
            germTimer = Mathf.Max(0, germTimer - Time.deltaTime * germDecayRate);
            bubbleTimer = Mathf.Max(0, bubbleTimer - Time.deltaTime * bubbleDecayRate);

            if (germTimer == 0 && bubbleTimer == 0 && !bubblesCapturing && !germsCapturing && !germSector && !bubbleSector)
            {
                meshRenderer.material.color = Color.white;
            }
        }
    }

    private void CaptureGermSector()
    {
        Debug.Log("Germ Sector Captured");
        germSector = true;
        bubbleSector = false;
        germTimer = 0;
        meshRenderer.material.color = Color.green;
        OnGermCapture?.Invoke();
        ResetCaptureStates();
    }

    private void CaptureBubbleSector()
    {
        Debug.Log("Bubble Sector Captured");
        bubbleSector = true;
        germSector = false;
        bubbleTimer = 0;
        meshRenderer.material.color = Color.cyan;
        OnBubbleCapture?.Invoke();
        ResetCaptureStates();
    }
    public Sector GetNextSector(bool germ)
    {
        if (germ) return previousSectorBubble;
        else return previousSectorGerm;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + centerOffset, sectorHalfExtents * 2);

        // Visualize previous sector connections
        if (previousSectorGerm != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, previousSectorGerm.transform.position);
        }

        if (previousSectorBubble != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, previousSectorBubble.transform.position);
        }
    }
}
