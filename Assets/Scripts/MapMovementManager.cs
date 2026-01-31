using UnityEngine;

public class MapMovementManager : MonoBehaviour
{
    public float movementSpeed = 1f;
    public GameObject map;

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        map.transform.position += Vector3.left * movementSpeed * Time.deltaTime;
    }
}
