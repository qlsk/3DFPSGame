using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaSlider : MonoBehaviour
{
    private Slider _staminaSlider;
    private void Start()
    {
        _staminaSlider = GetComponent<Slider>();
    }

    private void Update()
    {

    }

    public void UpdateStaminaSlider(float value)
    {
        _staminaSlider.value = value;
    }
}
