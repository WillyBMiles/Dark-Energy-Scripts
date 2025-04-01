using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUI : MonoBehaviour
{
    public Dictionary<Stat.StatEnum, int> offset = new();
    public int cost { get; private set; }
    public int levelOffset { get; private set; } = 0;

    public AudioSource levelUpSource;

    private void Start()
    {
        Open();
    }
    public void Open()
    {
        if (PlayerRPG.localRpg == null)
            return;
        levelOffset = 0;
        cost = 0;
        offset.Clear();
    }
    private void OnEnable()
    {
        Start();
    }

    public void Confirm()
    {
        if (PlayerRPG.localRpg == null || levelOffset == 0)
            return;
        foreach (var pair in offset)
        {
            for (int i =0; i < pair.Value; i++)
            {
                PlayerRPG.localRpg.ImproveStat(pair.Key);
            }
        }
        levelUpSource.Play();
        PlayerRPG.localRpg.Cost(cost);
        Open();
        PlayerRPG.localRpg.UpdateStats();
        
    }

    public void MakeRepairs()
    {
        Station.Rest();
    }

    public void AddLevel(Stat.StatEnum stat)
    {
        if (PlayerRPG.localRpg == null)
            return;
        int thisCost = PlayerRPG.GetCost(PlayerRPG.localRpg.level + levelOffset);
        if (PlayerRPG.localRpg.currency < cost + thisCost)
        {
            return;
        }
        cost += thisCost;
        levelOffset++;

        if (offset.ContainsKey(stat))
        {
            offset[stat]++;
        }
        else
            offset[stat] = 1;


    }

    public void RemoveLevel(Stat.StatEnum stat)
    {
        if (PlayerRPG.localRpg == null)
            return;

        if (levelOffset <= 0)
            return;

        if (offset.ContainsKey(stat))
        {
            if (offset[stat] == 1)
                offset.Remove(stat);
            else
                offset[stat]--;

            levelOffset--;
            cost -= PlayerRPG.GetCost(PlayerRPG.localRpg.level + levelOffset);
        }

    }

    public int GetLevel(Stat.StatEnum stat)
    {
        if (PlayerRPG.localRpg == null)
            return 0;
        int start = PlayerRPG.localRpg.GetBaseStat(stat);
        if (offset.ContainsKey(stat))
            start += offset[stat];

        return start;
    }

    public bool CanAdd(Stat.StatEnum stat)
    {
        if (PlayerRPG.localRpg == null)
            return false;
        int thisCost = PlayerRPG.GetCost(PlayerRPG.localRpg.level + levelOffset);
        if (PlayerRPG.localRpg.currency < cost + thisCost)
        {
            return false;
        }
        return true;
    }

    public bool CanSubtract(Stat.StatEnum stat)
    {
        return offset.ContainsKey(stat);
    }
}
