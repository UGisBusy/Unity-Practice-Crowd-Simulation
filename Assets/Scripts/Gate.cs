using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshLink))]
public class Gate : MonoBehaviour
{
    public float cycleInterval = 5f;

    private NavMeshLink link;
    private Renderer visual;
    private float timer;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        link = GetComponent<NavMeshLink>();
        visual = GetComponentInChildren<Renderer>();
        timer = cycleInterval;
        SetOpen(false);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer += cycleInterval;
            SetOpen(!IsOpen);
        }
    }

    private void SetOpen(bool open)
    {
        IsOpen = open;
        link.enabled = open;
        if (visual != null)
        {
            visual.enabled = open;
        }
    }
}
