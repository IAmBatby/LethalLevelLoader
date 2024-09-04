using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedBuyableVehicle", menuName = "Lethal Level Loader/Extended Content/ExtendedBuyableVehicle", order = 21)]
    public class ExtendedBuyableVehicle : ExtendedContent
    {
        [field: SerializeField] public BuyableVehicle BuyableVehicle { get; set; }
        [field: SerializeField] public string TerminalKeywordName { get; set; } = string.Empty;

        public int VehicleID { get; set; }

        public TerminalNode VehicleBuyNode { get; set; }
        public TerminalNode VehicleBuyConfirmNode { get; set; }
        public TerminalNode VehicleInfoNode { get; set; }

        internal static ExtendedBuyableVehicle Create(BuyableVehicle newBuyableVehicle)
        {
            ExtendedBuyableVehicle newExtendedBuyableVehicle = ScriptableObject.CreateInstance<ExtendedBuyableVehicle>();
            newExtendedBuyableVehicle.name = newBuyableVehicle.vehiclePrefab.name;
            newExtendedBuyableVehicle.BuyableVehicle = newBuyableVehicle;

            return (newExtendedBuyableVehicle);
        }

        internal override (bool result, string log) Validate()
        {
            return (true, string.Empty);
        }
    }
}
