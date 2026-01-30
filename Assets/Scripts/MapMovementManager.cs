using UnityEngine;

public class MapMovementManager : MonoBehaviour
{
    public float movementSpeed = 1f;
    public GameObject map;

    private void Update()
    {
        map.transform.position += Vector3.left * movementSpeed * Time.deltaTime;
    }
}
