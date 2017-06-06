using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prophecy
{
    public class InventoryItem
    {
        public float baseMass;
        public float baseVolume;
        public int count;
        public bool stackable;
        public GradeType grade;
    }

    public enum ModuleType
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

    // Base values
    // Mods
    // Final values

    public class ItemStatValues
    {
        public float mass;
        public float durability;
    }

    public abstract class InventoryItemModificator : InventoryItem
    {
        public float massReduction;
        public float durabilityMod;
    }

    public abstract class InventoryItemModule<StatValueType, ModificatorType> : InventoryItem
        where StatValueType : ItemStatValues, new()
        where ModificatorType : InventoryItemModificator
    {
        public InventoryItemModule(StatValueType baseValues, IEnumerable<ModificatorType> modificators, ModuleSize moduleSize)
        {
            this.moduleSize = moduleSize;
            this.baseValues = baseValues;
            this.modificatorList = new List<ModificatorType>(modificators);
            this.finalValues = ComputeFinalStats(this.baseValues, this.modificatorList);
        }

        public ModuleType moduleType { get; protected set; }
        public ModuleSize moduleSize { get; private set; }
        public StatValueType baseValues { get; private set; }
        public StatValueType finalValues { get; private set; }

        private List<ModificatorType> modificatorList { get; set; }

        public void AddModificator(ModificatorType mod)
        {
            modificatorList.Add(mod);
            finalValues = ComputeFinalStats(baseValues, modificatorList);
        }

        public int numModificators { get { return modificatorList.Count; } }

        public void RemoveModificator(int id)
        {
            modificatorList.RemoveAt(id);
            finalValues = ComputeFinalStats(baseValues, modificatorList);
        }

        public IEnumerable<ModificatorType> modificators { get { return modificatorList; } }

        protected virtual StatValueType ComputeFinalStats(StatValueType baseStats, IEnumerable<ModificatorType> mods)
        {
            StatValueType values = new StatValueType();

            values.mass = baseStats.mass;
            values.durability = baseStats.durability;

            PercentModificator pm = PercentModificator.Default;
            foreach (var m in mods) { pm.Add(m.massReduction); values.durability += m.durabilityMod; }
            values.mass *= (1.0f - pm.Value);

            return values;
        }
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


    public class ItemStatValuesCockpit : ItemStatValues
    {
        public float cargoSpace;
    }

    public class InventoryItemModificatorCockpit : InventoryItemModificator
    {
        public float additionalCargoSpace;
    }

    public sealed class InventoryItemModuleCockpit : InventoryItemModule<ItemStatValuesCockpit, InventoryItemModificatorCockpit>
    {
        public InventoryItemModuleCockpit(ItemStatValuesCockpit baseValues, IEnumerable<InventoryItemModificatorCockpit> modificators, ModuleSize moduleSize) :
            base(baseValues, modificators, moduleSize)
        {
            this.moduleType = ModuleType.Cockpit;
        }

        protected override ItemStatValuesCockpit ComputeFinalStats(ItemStatValuesCockpit baseStats, IEnumerable<InventoryItemModificatorCockpit> mods)
        {
            var stats = base.ComputeFinalStats(baseStats, mods);

            foreach (var m in mods) stats.cargoSpace += m.additionalCargoSpace;

            return stats;
        }
    }

    public struct ResistanceValues
    {
        public float energy;
        public float kinetic;
        public float explosive;
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