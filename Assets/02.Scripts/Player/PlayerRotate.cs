using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;

    private float _accumulationX = 0f;

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * _rotationSpeed;

        transform.eulerAngles = new Vector3(0, _accumulationX, 0);
    }
}
