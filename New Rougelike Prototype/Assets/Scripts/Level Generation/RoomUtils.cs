using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public const int DOOR_OFFSET = 2;
    public const int WALL_HEIGHT = 2;

    public static bool ValidateRoomSize(RoomData roomCheck)
    {
        Texture2D layout = roomCheck.Layout;
        if (layout.width == BASE_ROOM_X && layout.height == BASE_ROOM_Y)
            return true;
        else if (layout.width == (2 * BASE_ROOM_X) && layout.height == (2 * BASE_ROOM_Y))
            return true;
        else return false;
    }

    //Returns true if space is empty/is just a baseFloor.
    //tileCheck = color of tile in map to check.
    public static bool IsSpaceFree(Color tileCheck)
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

    public static int GetIndexFromColorOpacity(Color tileColor)
    {
        string colorStr = ColorUtility.ToHtmlStringRGBA(tileColor);
        
        colorStr = colorStr.Remove(0, colorStr.Length - 2);
        return (255 - Convert.ToInt32(colorStr, 16));
    }

    //String must already be converted to RGBA using ColorUtility.
    public static int GetIndexFromColorOpacity(string colorStr)
    {
        colorStr = colorStr.Remove(0, colorStr.Length - 2);
        //Debug.Log(colorStr + " converts to index " + (255 - Convert.ToInt32(colorStr, 16)));
        return (255 - Convert.ToInt32(colorStr, 16));
    }

    public static RuleTile GetLevelWallTile(Color tileColor)
    {
        int index = GetIndexFromColorOpacity(tileColor);
        return LevelGenerator.Instance.Floor.RoomVisuals[index].WallTile;
    }

    public static RuleTile GetLevelWallTile(string colorStr)
    {
        int index = GetIndexFromColorOpacity(colorStr);
        return LevelGenerator.Instance.Floor.RoomVisuals[index].WallTile;
    }

    public static RuleTile GetLevelWallTile(int index)
    {
        return LevelGenerator.Instance.Floor.RoomVisuals[index].WallTile;
    }

    public static RuleTile GetLevelFloorTile(Color tileColor)
    {
        int index = GetIndexFromColorOpacity(tileColor);
        return LevelGenerator.Instance.Floor.RoomVisuals[index].FloorTile;
    }

    public static RuleTile GetLevelFloorTile(string colorStr)
    {
        int index = GetIndexFromColorOpacity(colorStr);
        return LevelGenerator.Instance.Floor.RoomVisuals[index].FloorTile;
    }

    public static RuleTile GetLevelFloorTile(int index)
    {
        return LevelGenerator.Instance.Floor.RoomVisuals[index].FloorTile;
    }

    public static RuleTile GetLevelBoundTile(Color tileColor)
    {
        int index = GetIndexFromColorOpacity(tileColor);
        return LevelGenerator.Instance.Floor.RoomVisuals[index].BoundTile;
    }

    public static RuleTile GetLevelBoundTile(string colorStr)
    {
        int index = GetIndexFromColorOpacity(colorStr);
        return LevelGenerator.Instance.Floor.RoomVisuals[index].BoundTile;
    }

    public static RuleTile GetLevelBoundTile(int index)
    {
        return LevelGenerator.Instance.Floor.RoomVisuals[index].BoundTile;
    }

    //Integer parameter should be either 1 or 0, based on DoorBase.DoorDirection enum
    public static Sprite GetLevelDoorSprite(DoorBase.DoorDirection directionIndex)
    {
        return LevelGenerator.Instance.Floor.DoorSprites[(int)directionIndex];
    }

    public static int GetIndexFromTile(TileBase tile)
    {
        if (tile == null)
            Debug.Log("tile is null you fuck");
        int count = 0;
        foreach(var visual in LevelGenerator.Instance.Floor.RoomVisuals)
        {
            if (visual.BoundTile == tile)
                return count;
            if (visual.WallTile == tile)
                return count;
            if (visual.FloorTile == tile)
                return count;
            count++;
        }

        return -1;
    }
}
