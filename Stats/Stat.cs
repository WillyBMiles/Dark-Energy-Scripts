using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stat
{
    //! means this is implemented as a stat using status effects

    public enum StatEnum
    {
        None = -1,

        //Main Visible Stats
        Hull = 0, //!
        Power, //!
        Shield,//!
        Plasma,//!
        Energy,//!
        Quantum,//!
        Capacity,//!

        DarkEnergy,//!



        //Main Derived Stats
        MaxHealth = 50, //!
        MaxPower, //!

        ShieldEfficiency, //!

        ToolCapacity, //!
        ModuleCapacity, //!

        //Damage types
        PhysicalDamage = 100, //!
        PhysicalIncoming, //!

        PlasmaDamage, //!
        PlasmaIncoming, //!

        EnergyDamage, //!
        EnergyIncoming, //!

        QuantumDamage, //!
        QuantumIncoming, //!

        DarkEnergyDamage, //!
        DarkEnergyIncoming, //!

        AllDamage, //!
        AllDamageIncoming, //!

        //Other Damage Types

        LightDamage = 150, //!
        LightDamageIncoming, //!

        HeavyDamage, //!
        HeavyDamageIncoming, //!

        SustainDamage, //!
        SustainDamageIncoming, //!

        MissileDamage, //!
        MissileDamageIncoming, //!

        //Aux Stats
        PowerRegen = 200, //!
        PowerDelay, //!
        ShieldPowerMultiplier, //!
        NumberOfCells, //!
        CellHealing, //!

        ControlResistance, //!
        ControlRechargeSpeed, //!

        ControlDamage, //!

        AnimationSpeed, //!

        ContractCapacity, //!

        //Physics
        MoveSpeed = 300, //!
        TurnSpeed, //!
        Size, //!
        Mass, //!

        //Odd Stats
        KillReward = 1000, //apply this to enemies !
        KillRewardEarned, //apply this to players !
        DropChancePercent, //apply this to enemies !
        DropChancePercentEarned, //apply this to players !
        
    }



    public static readonly Stat Hull = new()
    {
        name = "Hull",
        id = "HUL",
        description = "Increases number of hits before your ship will be destroyed.",
        startingValue = 10,

    };
    public static readonly Stat Power = new()
    {
        name = "Power",
        id = "POW",
        description = "Increases amount of power usable.",
        startingValue = 10,
    };
    public static readonly Stat Shield = new()
    {
        name = "Shield",
        id = "SHI",
        description = "Increases effectiveness of shields.",
        startingValue = 10,
    };


    public static readonly Stat Plasma = new()
    {
        name = "Plasma",
        id = "PSM",
        description = "Increases effectiveness with plasma weapons and tools.",
        startingValue = 10,
    };
    public static readonly Stat Energy = new()
    {
        name = "Energy",
        id = "NRG",
        description = "Increases effectiveness with energy weapons and tools.",
        startingValue = 10,
    };
    public static readonly Stat Quantum = new()
    {
        name = "Quantum",
        id = "QUA",
        description = "Increases effectiveness with quantum weapons and tools.",
        startingValue = 10,
    };
    public static readonly Stat Capacity = new()
    {
        name = "Capacity",
        id = "CAP",
        description = "Increases number of missiles and other tools that can be equipped.",
        startingValue = 10,
    };

    public static readonly Dictionary<StatEnum, Stat> allStats = new()
    {
        { StatEnum.Hull, Hull },
        { StatEnum.Power, Power },
        { StatEnum.Shield, Shield },
        { StatEnum.Plasma, Plasma },
        { StatEnum.Energy, Energy },
        { StatEnum.Quantum, Quantum },
        { StatEnum.Capacity, Capacity },
    };

    public string name;
    public string id;
    public string description;
    public int startingValue;
}
