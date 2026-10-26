using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private MinimapCamera _minimapCamera;

    public void OnClickZoomIn()
    {
        _minimapCamera.ZoomIn();
    }

    public void OnClickZoomOut()
    {
        _minimapCamera.ZoomOut();
    }
}
