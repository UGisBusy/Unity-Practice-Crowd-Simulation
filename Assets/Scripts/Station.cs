using UnityEngine;

public class Station : MonoBehaviour
{
    private PlatformEnd[] platformEnds;
    private Gate gate;

    // TODO: assign id somewhere
    private int id = 0;

    private void InitializePlatormEnds()
    {
        platformEnds[0].Initilize(platformEnds[1], (Utils.PlatformEndId)(id * 2));
        platformEnds[1].Initilize(platformEnds[0], (Utils.PlatformEndId)(id * 2 + 1));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        platformEnds = GetComponentsInChildren<PlatformEnd>();
        if (platformEnds.Length != 2)
        {
            Debug.LogError($"Station {name} must have 2 PlatformEnd");
            return;
        }

        // gate = GetComponentInChildren<Gate>();
        // if (gate == null)
        // {
        //     Debug.LogError($"Station {name} must have 1 Gate");
        //     return;
        // }

        InitializePlatormEnds();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
