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

    public static int GetStationId(PlatformEndId id)
    {
        return (int)id / 2;
    }

    public static Color GetPlatformColor(PlatformEndId id)
    {
        float hue = (float)id / (float)STATION_COUNT / 2;
        return Color.HSVToRGB(hue, 1f, 1f);
    }

}
