using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LethalLevelLoader
{
    public static class VehiclesManager
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
    }
}
