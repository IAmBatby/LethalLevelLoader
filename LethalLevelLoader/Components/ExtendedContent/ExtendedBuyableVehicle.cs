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

        public int VehicleID => GameID;

        public TerminalKeyword TerminalKeyword { get; internal set; }
        public TerminalNode PurchasePromptNode { get; internal set; }
        public TerminalNode PurchaseConfirmNode { get; internal set; }
        public TerminalNode TerminalEntryNode { get; internal set; }

        public VehicleController VehicleController { get; private set; }

        internal static ExtendedBuyableVehicle Create(BuyableVehicle newBuyableVehicle)
        {
            ExtendedBuyableVehicle newExtendedBuyableVehicle = ScriptableObject.CreateInstance<ExtendedBuyableVehicle>();
            newExtendedBuyableVehicle.name = newBuyableVehicle.vehiclePrefab.name;
            newExtendedBuyableVehicle.BuyableVehicle = newBuyableVehicle;

            return (newExtendedBuyableVehicle);
        }

        internal override void Initialize()
        {
            VehicleController = BuyableVehicle.vehiclePrefab.GetComponent<VehicleController>();
        }

        protected override void OnGameIDChanged()
        {
            if (VehicleController != null) VehicleController.vehicleID = GameID;
            if (PurchasePromptNode != null) PurchasePromptNode.buyItemIndex = GameID;
            if (PurchaseConfirmNode != null) PurchaseConfirmNode.buyItemIndex = GameID;
        }

        internal override List<GameObject> GetNetworkPrefabsForRegistration()
        {
            return new List<GameObject>() { BuyableVehicle.vehiclePrefab, BuyableVehicle.secondaryPrefab };
        }
        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
    }
}
