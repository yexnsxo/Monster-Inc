using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{
    public float jumpSpeed = 5f;
    public GameObject player;

    private Vector3 _originalScale;
    private float _verticalVelocity;

    private float _groundYPos;

    private void Awake()
    {
        _originalScale = player.transform.localScale;
        _verticalVelocity = 0f;
        _groundYPos = player.transform.position.y;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }

        ApplyVerticalVelocity();

        bool isShiftHeld = Keyboard.current != null
            && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        if (isShiftHeld)
        {
            player.transform.localScale = new Vector3(_originalScale.x, _originalScale.y * 0.5f, _originalScale.z);
        }
        else
        {
            player.transform.localScale = _originalScale;
        }
    }

    private void Jump()
    {
        _verticalVelocity = jumpSpeed;
    }

    private void ApplyVerticalVelocity()
    {
        if (player.transform.position.y < _groundYPos)
        {
            _verticalVelocity = 0f;
            player.transform.position = new Vector3(player.transform.position.x, _groundYPos, player.transform.position.z);
            return;
        }

        _verticalVelocity += Physics2D.gravity.y * Time.deltaTime;
        player.transform.position += Vector3.up * _verticalVelocity * Time.deltaTime;
    }
}
