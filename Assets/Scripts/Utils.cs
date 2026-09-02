using UnityEngine;

public static class Utils
{
    public const int STATION_COUNT = 1;
    public enum PlatformEndId
    {
        Station1A,
        Station1B,
        Station2A,
        Station2B,
    }

    public static bool IsSameStation(PlatformEndId id1, PlatformEndId id2)
    {
        return (int)id1 / 2 == (int)id2 / 2;
    }

}
