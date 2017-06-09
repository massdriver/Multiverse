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
        public ObjectSize moduleSize;

        public int modSlots;
        public abstract ModSlotType requiredModType { get; }
    }

    public enum TurretType
    {
        Energy,
        Projectile,
        Missile
    }

    public enum TurretFireMode
    {
        LaunchProjectile,
        RaycastShot
    }

    public sealed class AssetModuleTurret : AssetModule
    {
        public TurretType turretType;
        public TurretFireMode fireMode;

        [Range(0.0f,1.0f)]
        public float dispersion;

        public bool refireRate;
        public float overloadRefireRateBonus;

        public float overloadHeatGeneration; // per cycle
        public float maxHeat;

        public float energyPerShot;
        [Range(0.0f, 1.0f)]
        public float overloadEnergyConsumptionMod;

        public float reloadTime;

        public int maxAmmo;

        public float trackingSpeed;

        public float damageMultiplier;
        public float projectileVelocityMod;

        public override ModSlotType requiredModType
        {
            get
            {
                return ModSlotType.Turret;
            }
        }
    }

    public sealed class AssetAmmo : AssetInventoryItem
    {
        public ObjectSize requiredTurretSize;
        public TurretType requiredTurretType;

        public float velocity;
        [Tooltip("In case of RaycastShot this means max ray distance")]
        public float lifetime;

        public float damageEnergy;
        public float damageKinetic;
        public float damageExplosive;
        public float damageOmni;

        public GameObject projectilePrefab;
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
        [Range(0.0f, 0.99f)]
        public float resistEnergy;
        [Range(0.0f, 0.99f)]
        public float resistKinetic;
        [Range(0.0f, 0.99f)]
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
        Epic,
        Premium
    }

    public enum ObjectSize
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

    public abstract class HullModuleBase<HullAssetType> : Multiverse.UNetworkBehaviour
        where HullAssetType : AssetHullModule
    {
        public float mass { get; private set; }
        public float durability { get; private set; }
        public HullAssetType hullModuleAsset { get; private set; }
    }

    public sealed class HullModuleCockpit : HullModuleBase<AssetHullModuleCockpit>
    {
        public float maxCargoSpace { get; private set; }
    }

    public sealed class HullModuleWing : HullModuleBase<AssetHullModuleWing>
    {
        public float resistEnergy { get; private set; }
        public float resistKinetic { get; private set; }
        public float resistExplosive { get; private set; }
        public int turretPoints { get; private set; }
    }

    public sealed class HullModuleEngine : HullModuleBase<AssetHullModuleEngine>
    {
        public float maxSpeed { get; private set; }
        public float thrust { get; private set; }
        public float mobility { get; private set; }
    }

    public enum SolarEntityType
    {
        Static,
        ModularShip
    }

    public struct SolarEntityStats
    {
        public float mass;
        public float maxDurability;

        public float resistDurabilityEnergy;
        public float resistDurabilityKinetic;
        public float resistDurabilityExplosive;

        public float maxShield;
        public float regenRateShield;

        public float resistShieldEnergy;
        public float resistShieldKinetic;
        public float resistShieldExplosive;

        public float maxSpeed;
        public float thrust;
        public float mobility;

        public float regenRateEnergy;
        public float maxEnergy;
    }

    public struct SolarEntityOwnerInfo
    {
        public ulong ownerId;
    }


    public abstract class SolarEntity : Multiverse.UNetworkBehaviour
    {
        public SolarEntityOwnerInfo entityOwner { get; private set; }
        public string solarName { get; private set; }
        public abstract SolarEntityType entityType { get; }

        public SolarEntityStats baseStats { get; protected set; }
        public SolarEntityStats effectiveStats { get; private set; }

        public float mass { get; private set; }
        public float durability { get; private set; }
        public float shield { get; private set; }
        public float energy { get; private set; }

        public ObjectSize objectSize { get; private set; }

        public abstract GameObject ConstructVisualAppearance();
        public abstract SolarEntityStats ComputeEffectiveStats();

        public void UpdateStats()
        {

        }
    }

    public sealed class SolarEntityModularShip : SolarEntity
    {
        public HullModuleCockpit moduleCockpit { get; private set; }
        public HullModuleWing moduleWingLeft { get; private set; }
        public HullModuleWing moduleWingRight { get; private set; }
        public HullModuleEngine moduleEngineLeft { get; private set; }
        public HullModuleEngine moduleEngineRight { get; private set; }

        public override SolarEntityType entityType
        {
            get
            {
                return SolarEntityType.ModularShip;
            }
        }

        public override SolarEntityStats ComputeEffectiveStats()
        {
            throw new NotImplementedException();
        }

        public override GameObject ConstructVisualAppearance()
        {
            throw new NotImplementedException();
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