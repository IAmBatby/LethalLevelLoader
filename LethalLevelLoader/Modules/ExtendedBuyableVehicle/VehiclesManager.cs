using LethalFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class VehiclesManager : ExtendedContentManager<ExtendedBuyableVehicle, BuyableVehicle>
    {
        protected override List<BuyableVehicle> GetVanillaContent() => new List<BuyableVehicle>(Refs.BuyableVehicles);
        protected override ExtendedBuyableVehicle ExtendVanillaContent(BuyableVehicle content) => ExtendedBuyableVehicle.Create(content);

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);

            Terminal.buyableVehicles = PatchedContent.ExtendedBuyableVehicles.Select(v => v.BuyableVehicle).ToArray();
            StartOfRound.VehiclesList = PatchedContent.ExtendedBuyableVehicles.Select(v => v.BuyableVehicle.vehiclePrefab).ToArray();

            List<ExtendedBuyableVehicle> vehicles = new List<ExtendedBuyableVehicle>(PatchedContent.ExtendedBuyableVehicles);
            for (int i = 0; i < vehicles.Count; i++)
                vehicles[i].SetGameID(i);

            foreach (ExtendedBuyableVehicle vehicle in vehicles)
                if (!TerminalManager.Keywords.Buy.Contains(vehicle.NounKeyword, vehicle.PurchasePromptNode))
                    TerminalManager.Keywords.Buy.AddNoun(vehicle.NounKeyword, vehicle.PurchasePromptNode);
        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedBuyableVehicle extendedBuyableVehicle)
        {
            if (extendedBuyableVehicle.BuyableVehicle.vehiclePrefab == null)
                return (false, "Vehicle Prefab Was Null Or Empty");
            else if (extendedBuyableVehicle.BuyableVehicle.secondaryPrefab == null)
                return (false, "Vehicle Secondary Prefab Was Null Or Empty");
            else if (extendedBuyableVehicle.BuyableVehicle.vehiclePrefab.GetComponent<NetworkObject>() == null)
                return (false, "Vehicle Prefab Is Missing NetworkObject Component");
            else if (extendedBuyableVehicle.BuyableVehicle.secondaryPrefab.GetComponent<NetworkObject>() == null)
                return (false, "Vehicle Secondary Prefab Is Missing NetworkObject Component");

            return (true, string.Empty);
        }

        protected override void PopulateContentTerminalData(ExtendedBuyableVehicle content)
        {
            TerminalKeyword infoKeyword = null;
            TerminalNode infoNode = null;
            TerminalNode buyNode = null;
            TerminalNode buyConfirmNode = null;
            //if is custom check
            BuyableVehicle vehicle = content.BuyableVehicle;
            string displayName = vehicle.vehicleDisplayName;
            infoKeyword = TerminalManager.CreateNewTerminalKeyword(content.name + "Keyword", content.TerminalKeywordName.ToLower(), TerminalManager.Keywords.Buy);

            buyNode = TerminalManager.CreateNewTerminalNode(content.name + "Buy");
            buyNode.itemCost = vehicle.creditsWorth;
            buyNode.isConfirmationNode = true;
            buyNode.overrideOptions = true;
            buyNode.clearPreviousText = true;
            buyNode.maxCharactersToType = 15;
            buyNode.displayText =
                "You have requested to order the " + displayName + "." + "\n" +
                "[warranty] Total cost of items: [totalCost]." + "\n\n" +
                "Please CONFIRM or DENY." + "\n\n";

            buyConfirmNode = TerminalManager.CreateNewTerminalNode(content.name + "BuyConfirm");
            buyConfirmNode.itemCost = vehicle.creditsWorth;
            buyConfirmNode.clearPreviousText = true;
            buyConfirmNode.maxCharactersToType = 35;
            buyConfirmNode.playSyncedClip = 0;
            buyConfirmNode.displayText =
                "Ordered the " + displayName + ". Your new balance is [playerCredits]." + "\n\n" +
                "We are so confident in the quality of this product, it comes with a life-time warranty! If your " + displayName + " is lost or destroyed, you can get one free replacement. Items cannot be purchased while the vehicle is en route." + "\n\n";
            infoNode = TerminalManager.CreateNewTerminalNode(content.name + "Info");

            buyNode.AddNoun(TerminalManager.Keywords.Confirm, buyConfirmNode);
            buyNode.AddNoun(TerminalManager.Keywords.Deny, TerminalManager.Nodes.CancelBuy);

            content.NounKeyword = infoKeyword;
            content.InfoNode = infoNode;
            content.PurchasePromptNode = buyNode;
            content.PurchaseConfirmNode = buyConfirmNode;
        }
    }
}
