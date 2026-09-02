using UnityEngine;

public static class Utils
{
    public const int STATION_COUNT = 4;
    public enum PlatformEndId
    {
        Station1A,
        Station1B,
        Station2A,
        Station2B,
        Station3A,
        Station3B,
        Station4A,
        Station4B,
    }

    public static bool IsSameStation(PlatformEndId id1, PlatformEndId id2)
    {
        return (int)id1 / 2 == (int)id2 / 2;
    }

}
