using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;

    private float _accumulationX;

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        var mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * _rotationSpeed;

        transform.eulerAngles = new Vector3(0, _accumulationX, 0);
    }
}
