using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class VehiclesManager : ExtendedContentManager<ExtendedBuyableVehicle, BuyableVehicle, VehiclesManager>
    {
        internal static void PatchVanillaVehiclesLists()
        {
            Patches.Terminal.buyableVehicles = PatchedContent.ExtendedBuyableVehicles.Select(v => v.BuyableVehicle).ToArray();
            Patches.StartOfRound.VehiclesList = PatchedContent.ExtendedBuyableVehicles.Select(v => v.BuyableVehicle.vehiclePrefab).ToArray();
        }

        internal static void SetBuyableVehicleIDs()
        {
            foreach (ExtendedBuyableVehicle extendedBuyableVehicle in PatchedContent.ExtendedBuyableVehicles)
                extendedBuyableVehicle.VehicleID = -1;

            int vehicleID = 0;
            foreach (ExtendedBuyableVehicle vanillaBuyableVehicle in PatchedContent.VanillaExtendedBuyableVehicles)
            {
                vanillaBuyableVehicle.VehicleID = vehicleID;
                vehicleID++;
            }

            foreach (ExtendedBuyableVehicle customBuyableVehicle in PatchedContent.CustomExtendedBuyableVehicles)
            {
                customBuyableVehicle.VehicleID = vehicleID;
                vehicleID++;
            }

            foreach (ExtendedBuyableVehicle extendedBuyableVehicle in PatchedContent.ExtendedBuyableVehicles)
                if (extendedBuyableVehicle.BuyableVehicle.vehiclePrefab.TryGetComponent(out VehicleController vehicleController))
                    vehicleController.vehicleID = extendedBuyableVehicle.VehicleID;

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
    }
}
