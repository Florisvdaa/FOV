using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material originalMat;
    [SerializeField] private Material targetMat;

    private bool isTargeted = false;

    private void Awake()
    {
        objectRenderer.material = originalMat;
    }

    private void Update()
    {
        if (isTargeted)
        {
            objectRenderer.material = targetMat;
        }
        else
        {
            objectRenderer.material = originalMat;
        }
    }

    public void Targeted()

    {
       isTargeted = true;
    }
    public void NotTargeted()
    {
        isTargeted = false;
    }
}
