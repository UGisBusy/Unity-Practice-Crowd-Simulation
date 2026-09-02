using UnityEngine;
using UnityEngine.InputSystem;

public class Camera : MonoBehaviour
{
    Vector3[] positions;

    int current = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        positions = new Vector3[2];
        positions[0] = new Vector3(60, 106.963608f, -36.7235031f);
        positions[1] = new Vector3(300, 106.963608f, -36.7235031f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            current = (current + 1) % positions.Length;
            transform.position = positions[current];
        }

    }
}
