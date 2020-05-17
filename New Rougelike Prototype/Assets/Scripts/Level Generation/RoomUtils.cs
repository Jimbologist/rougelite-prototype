using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomUtils
{
    /**All base colors for tiles; alpha value is excluded.
     * To include Alpha values, check if RGB matches, then 
     * concatenate hex value of alpha from ID OF OBJECT TO SPAWN.
     * If there is a match, spawn that type instead.
     */
    public const string BASE_WALL = "000000";
    public const string BASE_FLOOR = "FFFFFF";

    //Base sizes of rooms REQUIRED by editor.
    public const int BASE_ROOM_X = 28;
    public const int BASE_ROOM_Y = 16;

    //Size of connector doors/halls between rooms.
    public const int BASE_DOOR_OFFSET = 1;
    public const int BASE_WALL_HEIGHT = 2;

    public static bool ValidateRoomSize(RoomData roomCheck)
    {
        Texture2D layout = roomCheck.RoomLayout;
        if (layout.width == BASE_ROOM_X && layout.height == BASE_ROOM_Y)
            return true;
        else if (layout.width == (2 * BASE_ROOM_X) && layout.height == (2 * BASE_ROOM_Y))
            return true;
        else return false;
    }

    //Returns true if space is empty/is just a baseFloor.
    //tileCheck = color of tile in map to check.
    public static bool IsSpaceEmpty(Color tileCheck)
    {
        if (tileCheck == null) return false;

        return (MatchRGB(tileCheck, BASE_FLOOR));
    }

    //Check if passed HTML color string matches base tile color
    //Alpha isn't checked, so only base tile type is checked.
    public static bool MatchRGB(Color tileColor, string baseString)
    {
        return (ColorUtility.ToHtmlStringRGB(tileColor) == baseString);
    }

    public static bool MatchRGBA(Color tileColor, string baseString)
    {
        return (ColorUtility.ToHtmlStringRGBA(tileColor) == baseString);
    }
}
