using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedBuyableVehicle", menuName = "Lethal Level Loader/Extended Content/ExtendedBuyableVehicle", order = 21)]
    public class ExtendedBuyableVehicle : ExtendedContent<ExtendedBuyableVehicle, BuyableVehicle, VehiclesManager>
    {
        public override RestorationPeriod RestorationPeriod => RestorationPeriod.MainMenu;
        public override BuyableVehicle Content => BuyableVehicle;
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

        internal override List<GameObject> GetNetworkPrefabsForRegistration()
        {
            return new List<GameObject>() { BuyableVehicle.vehiclePrefab, BuyableVehicle.secondaryPrefab };
        }
        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
    }
}
