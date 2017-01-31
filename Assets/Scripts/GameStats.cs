using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameStats {

    public static bool LoadLevelSettings = false;

    public static int Level;

    public static int LvlWorldWidht;
    public static int LvlWorldHeight;
    public static float LvlIsleDensity;

    //Char stats
    public static bool LoadCharStats = false;

    public static float maxHealth;
    public static float health;
    public static float speed;
    public static float luck;
    public static float strength;

    //Shoot stats
    public static Vector3 shootOffset = new Vector3(0, 1, 0);
    public static float StartVerticalDegree = 45;
    public static float shotSpeed = 1.0f;
    public static float shotStrength = 5.0f;
    public static float fireRate = 0.1f;
    public static float fireRateDifference = 0;
    public static List<ShotEffect> possibleShotEffects;

    // other stuff
    public static int NumberOfKeys;
    public static bool HasSplitter;
    public static bool HasBluffer;

    public static void UpdateLevelSettings(int level)
    {
        Level = level;

        switch(level)
        {
            case 1: LvlWorldWidht = 3;
                    LvlWorldHeight = 3;
                    LvlIsleDensity = 1;
                    break;
            case 2: LvlWorldWidht = 4;
                    LvlWorldHeight = 4;
                    LvlIsleDensity = 0.5f;
                    break;
            case 3: LvlWorldWidht = 5;
                    LvlWorldHeight = 5;
                    LvlIsleDensity = 0.5f;
                    break;
        }
    }

    public static void ReadCharStats(Player player)
    {
        Stats stats = player.gameObject.GetComponent<Stats>();

        // load Player Stats

        stats.maxHealth = GameStats.maxHealth;
        stats.health = GameStats.health;
        stats.speed = GameStats.speed;
        stats.luck = GameStats.luck;
        stats.strength = GameStats.strength;

        //Shoot stats
        stats.shootOffset = GameStats.shootOffset;
        stats.StartVerticalDegree = GameStats.StartVerticalDegree;
        stats.shotSpeed = GameStats.shotSpeed;
        stats.shotStrength = GameStats.shotStrength;
        stats.fireRate = GameStats.fireRate;
        stats.fireRateDifference = GameStats.fireRateDifference;
        stats.possibleShotEffects = GameStats.possibleShotEffects;

        // other stuff

        player.NumberSmallKeys = GameStats.NumberOfKeys;
        player.HasSplitter = GameStats.HasSplitter;
        player.HasBluffer = GameStats.HasBluffer;

    }

    public static void SaveCharSets(Player player)
    {
        Stats stats = player.gameObject.GetComponent<Stats>();

        // save Player Stats

        GameStats.maxHealth = stats.maxHealth;
        GameStats.health = stats.health;
        GameStats.speed = stats.speed;
        GameStats.luck = stats.luck;
        GameStats.strength = stats.strength;

        //Shoot stats
        GameStats.shootOffset = stats.shootOffset;
        GameStats.StartVerticalDegree = stats.StartVerticalDegree;
        GameStats.shotSpeed = stats.shotSpeed;
        GameStats.shotStrength = stats.shotStrength;
        GameStats.fireRate = stats.fireRate;
        GameStats.fireRateDifference = stats.fireRateDifference;
        GameStats.possibleShotEffects = stats.possibleShotEffects;

        // other stuff

        GameStats.NumberOfKeys = player.NumberSmallKeys;
        GameStats.HasSplitter = player.HasSplitter;
        GameStats.HasBluffer = player.HasBluffer;
    }
}
