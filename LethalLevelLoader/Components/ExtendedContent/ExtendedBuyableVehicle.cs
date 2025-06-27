using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedBuyableVehicle", menuName = "Lethal Level Loader/Extended Content/ExtendedBuyableVehicle", order = 21)]
    public class ExtendedBuyableVehicle : ExtendedContent<ExtendedBuyableVehicle, BuyableVehicle, VehiclesManager>, ITerminalInfoEntry, ITerminalPurchasableEntry
    {
        public override BuyableVehicle Content { get => BuyableVehicle; protected set => BuyableVehicle = value; }
        [field: SerializeField] public BuyableVehicle BuyableVehicle { get; set; }
        [field: SerializeField] public string TerminalKeywordName { get; set; } = string.Empty;

        public int VehicleID => GameID;
        public int PurchasePrice { get; private set; }

        public TerminalKeyword NounKeyword { get; internal set; }
        public TerminalNode PurchasePromptNode { get; internal set; }
        public TerminalNode PurchaseConfirmNode { get; internal set; }
        public TerminalNode InfoNode { get; internal set; }

        TerminalKeyword ITerminalPurchasableEntry.RegistryKeyword => TerminalManager.Keyword_Buy;
        TerminalKeyword ITerminalInfoEntry.RegistryKeyword => TerminalManager.Keyword_Info;
        public List<CompatibleNoun> GetRegistrations() => new() { (this as ITerminalInfoEntry).GetPair(), (this as ITerminalPurchasableEntry).GetPair() };

        public VehicleController VehicleController { get; private set; }

        internal static ExtendedBuyableVehicle Create(BuyableVehicle newBuyableVehicle) => Create<ExtendedBuyableVehicle, BuyableVehicle, VehiclesManager>(newBuyableVehicle.vehiclePrefab.name, newBuyableVehicle);

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

        public void SetPurchasePrice(int newPrice)
        {
            PurchasePrice = newPrice;
            BuyableVehicle.creditsWorth = newPrice;
            if (PurchasePromptNode != null) PurchasePromptNode.itemCost = newPrice;
            if (PurchaseConfirmNode != null) PurchaseConfirmNode.itemCost = newPrice;
        }

        internal override List<GameObject> GetNetworkPrefabsForRegistration() => new () { BuyableVehicle.vehiclePrefab, BuyableVehicle.secondaryPrefab };
        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
    }
}
