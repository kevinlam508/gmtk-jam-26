using System;
using System.Collections.Generic;
using UnityEngine;

public class HookTarget : MonoBehaviour
{
    public Transform hookSpot;

    public MeshRenderer renderer;
    public Material seenOutline;
    public Material inRangeOutline;
    public Material hookedOutline;
    public Animation hookHitAnim;

    public bool isInView;
    public bool isInRange;
    public bool isHooked;

    private List<Material> hookedMaterials;
    private List<Material> inRangeMaterials;
    private List<Material> seenMaterials;
    
    private List<Material> baseMaterials;

    public event Action captured;

    private void Awake()
    {
        baseMaterials = new List<Material>();
        seenMaterials = new List<Material>();
        inRangeMaterials = new List<Material>();
        hookedMaterials = new List<Material>();

        renderer.GetSharedMaterials(baseMaterials);
        renderer.GetSharedMaterials(seenMaterials);
        renderer.GetSharedMaterials(inRangeMaterials);
        renderer.GetSharedMaterials(hookedMaterials);
        
        seenMaterials.Add(seenOutline);
        hookedMaterials.Add(hookedOutline);
        inRangeMaterials.Add(inRangeOutline);

        if (hookSpot == null)
            hookSpot = transform;
    }
    public void SetInView(bool view)
    {
        isInView = view;
        if (isInView)
        {
            renderer.sharedMaterials = seenMaterials.ToArray();
        }
        else
        {
            renderer.sharedMaterials = baseMaterials.ToArray();
            isHooked = false;
            isInRange = false;
        }
    }

    public void SetInRange(bool ranged)
    {
        isInRange = ranged;
        if (isInRange)
        {
            renderer.sharedMaterials = inRangeMaterials.ToArray();
        }
        else
        {
            if (isInView && !isHooked)
                SetInView(true);
            else if (isHooked)
                SetHooked(true);
        }
    }

    public void SetHooked(bool hooked)
    {
        isHooked = hooked;
        if (isHooked)
        {
            renderer.sharedMaterials = hookedMaterials.ToArray();
            hookHitAnim.Play();
        }
    }

    public void OnCaptureComplete()
    {
        captured?.Invoke();
    }
}
