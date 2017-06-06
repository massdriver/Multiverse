using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prophecy
{
    public abstract class AssetBase : ScriptableObject
    {
        public ulong id { get; private set; }
        public int index { get; private set; }

        public string nickname;

        public string itemName;
        public string itemDescription;
    }

    public abstract class AssetInventoryItem : AssetBase
    {
        public GradeType grade;
        public float volume;
        public int maxStack;
    }

    public abstract class AssetModule : AssetInventoryItem
    {
        public float mass;
        public float durability;
        public ModuleSize moduleSize;

        public int modSlots;
        public abstract ModSlotType requiredModType { get; }
    }

    public enum TurretType
    {
        Energy,
        Projectile,
        Missile
    }

    public sealed class AssetModuleTurret : AssetModule
    {
        public TurretType turretType;

        public override ModSlotType requiredModType
        {
            get
            {
                return ModSlotType.Turret;
            }
        }
    }

    public abstract class AssetAmmo : AssetInventoryItem
    {
        public bool raycastHit;
        public float raycastMaxDist;

        public float velocity;
        public float lifetime;

        public float damageEnergy;
        public float damageKinetic;
        public float damageExplosive;
        public float damageOmni;
    }

    public abstract class AssetHullModule : AssetModule
    {
        [Tooltip("Hull tech defines hull module connections")]
        public HullTechType hullTechType;

        public GameObject visualPrefab; // requires connection points to be set somehow + collision meshes
    }

    public sealed class AssetHullModuleCockpit : AssetHullModule
    {
        public float maxCargo;

        public override ModSlotType requiredModType
        {
            get
            {
                return ModSlotType.Cockpit;
            }
        }
    }

    public sealed class AssetHullModuleWing : AssetHullModule
    {
        [Range(0.0f, 1.0f)]
        public float resistEnergy;
        [Range(0.0f, 1.0f)]
        public float resistKinetic;
        [Range(0.0f, 1.0f)]
        public float resistExplosive;

        public int turretPoints;

        public override ModSlotType requiredModType
        {
            get
            {
                return ModSlotType.Wing;
            }
        }
    }

    public sealed class AssetHullModuleEngine : AssetHullModule
    {
        public float maxSpeed;
        public float thrust;
        public float mobility;

        public override ModSlotType requiredModType
        {
            get
            {
                return ModSlotType.Engine;
            }
        }
    }

    public enum HullTechType
    {
        Civilian,
        PieRat
    }

    public enum ModSlotType
    {
        Cockpit,
        Turret,
        MissileLauncher,
        Engine,
        Wing,
        PowerPlant,
        ShieldGenerator
    }

    public enum GradeType
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    public enum ModuleSize
    {
        Small,
        Medium,
        Large
    }

    public struct PercentModificator
    {
        public float Value { get; private set; }

        public const float MaxCap = 0.99f;
        public static readonly PercentModificator Default = new PercentModificator(0);

        public PercentModificator(float Value)
        {
            this.Value = Value;
        }

        public float Add(float val)
        {
            Value += (1.0f - Value) * val;

            if (Value > MaxCap)
                Value = MaxCap;

            return Value;
        }
    }

    public class Prophecy : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}