using UnityEngine;
using UnityVolumeRendering;

public class VolumeGUIController : MonoBehaviour
{
    [Header("GUI de rotación")]
    public GameObject rotationGUI;

    void Start()
    {
        if(rotationGUI != null)
            rotationGUI.SetActive(false); // Por si quieres que esté oculta al iniciar
    }

    public void OnVolumeLoaded(VolumeRenderedObject volObj)
    {
        if (rotationGUI != null)
            rotationGUI.SetActive(true); // Activar GUI de rotación
    }
    public void OnVolumeRemoved()
{
    if(rotationGUI != null)
        rotationGUI.SetActive(false);
}
}