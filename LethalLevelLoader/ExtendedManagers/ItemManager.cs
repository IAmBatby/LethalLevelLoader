using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public class ItemManager : ExtendedContentManager<ExtendedItem, Item>
    {

        protected override List<Item> GetVanillaContent() => Patches.StartOfRound.allItemsList.itemsList;

        protected override ExtendedItem ExtendVanillaContent(Item content)
        {
            ExtendedItem extendedVanillaItem = ExtendedItem.Create(content);
            extendedVanillaItem.IsBuyableItem = Patches.Terminal.buyableItemsList.Contains(content);

            //Terminal finding stuff

            return (extendedVanillaItem);
        }

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);

            List<ExtendedItem> items = new List<ExtendedItem>(ExtendedContents);
            foreach (ExtendedItem item in items)
            {
                if (!StartOfRound.allItemsList.itemsList.Contains(item.Item)) //Dunno about this one
                    StartOfRound.allItemsList.itemsList.Add(item.Item);
                if (!item.IsBuyableItem) continue;
                TerminalManager.Keyword_Buy.TryAdd(item.BuyKeyword, item.BuyNode);
                TerminalManager.Keyword_Info.TryAdd(item.BuyKeyword, item.BuyInfoNode);
                if (!Terminal.buyableItemsList.Contains(item.Item))
                    Terminal.buyableItemsList.Add(item.Item);
            }
        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

        public static void RefreshDynamicItemRarityOnAllExtendedLevels()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                InjectCustomItemsIntoLevelViaDynamicRarity(extendedLevel);
        }
        public static void InjectCustomItemsIntoLevelViaDynamicRarity(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            foreach (ExtendedItem extendedItem in PatchedContent.CustomExtendedItems.Where(i => i.Item.isScrap))
            {
                string debugString = string.Empty;
                int returnRarity = extendedItem.LevelMatchingProperties.GetDynamicRarity(extendedLevel);
                SpawnableItemWithRarity alreadyInjectedItem = extendedLevel.SelectableLevel.spawnableScrap.Where(s => s.spawnableItem == extendedItem).FirstOrDefault();

                if (alreadyInjectedItem != null)
                {
                    if (returnRarity > 0)
                    {
                        alreadyInjectedItem.rarity = returnRarity;
                        debugString = "Updated Rarity Of: " + extendedItem.Item.itemName + " To: " + returnRarity + " On Planet: " + extendedLevel.NumberlessPlanetName;
                    }
                    else
                    {
                        extendedLevel.SelectableLevel.spawnableScrap.Remove(alreadyInjectedItem);
                        debugString = "Removed " + extendedItem.Item.itemName + " From Planet: " + extendedLevel.NumberlessPlanetName;
                    }
                }
                else
                {
                    SpawnableItemWithRarity newSpawnableItem = new SpawnableItemWithRarity();
                    newSpawnableItem.spawnableItem = extendedItem.Item;
                    newSpawnableItem.rarity = returnRarity;
                    extendedLevel.SelectableLevel.spawnableScrap.Add(newSpawnableItem);
                    debugString = "Added " + extendedItem.Item.itemName + " To Planet: " + extendedLevel.NumberlessPlanetName + " With A Rarity Of: " + returnRarity;
                }
                if (debugResults == true)
                    DebugHelper.Log(debugString, DebugType.Developer);
            }
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedItem extendedItem)
        {
            if (extendedItem.Item.spawnPrefab == null)
                return (false, "SpawnPrefab Was Null");
            else
                return (true, string.Empty);
        }

        internal static int GetAverageScrapValue(ExtendedItem extendedItem)
        {
            return (Mathf.RoundToInt(Mathf.Lerp(extendedItem.Item.minValue, extendedItem.Item.maxValue, 0.5f)));
        }

        protected override void PopulateContentTerminalData(ExtendedItem content)
        {
            if (content.IsBuyableItem == false) return;

            TerminalKeyword keyword = null;
            TerminalNode buyNode = null;
            TerminalNode buyConfirmNode = null;
            TerminalNode buyInfoNode = null;

            if (Terminal.buyableItemsList.Contains(content.Item))
            {
                CompatibleNoun pair = TerminalManager.Keyword_Buy.compatibleNouns.Where(b => b.result.buyItemIndex == content.GameID).FirstOrDefault();
                keyword = pair?.noun;
                buyNode = pair?.result;
                buyConfirmNode = buyNode?.terminalOptions[1].result;
                if (TerminalManager.Keyword_Info.compatibleNouns.TryGet(keyword, out TerminalNode result))
                    buyInfoNode = result;
            }
            else
            {
                string sanitised = content.Item.itemName.StripSpecialCharacters().Sanitized();
                string plural = !string.IsNullOrEmpty(content.PluralisedItemName) ? content.PluralisedItemName : content.Item.itemName;
                keyword = TerminalManager.CreateNewTerminalKeyword(sanitised + "Keyword", sanitised, TerminalManager.Keyword_Buy);

                buyNode = TerminalManager.CreateNewTerminalNode(sanitised + "Buy");
                if (!string.IsNullOrEmpty(content.OverrideBuyNodeDescription))
                    buyNode.displayText = content.OverrideBuyNodeDescription;
                else
                {
                    buyNode.displayText = "You have requested to order " + plural + ". Amount: [variableAmount].";
                    buyNode.displayText += "\n Total cost of items: [totalCost].";
                    buyNode.displayText += "\n" + "\n" + "Please CONFIRM or DENY." + "\n" + "\n";
                }
                buyNode.clearPreviousText = true;
                buyNode.maxCharactersToType = 15;
                buyNode.isConfirmationNode = true;
                buyNode.itemCost = content.Item.creditsWorth;
                buyNode.overrideOptions = true;

                buyConfirmNode = TerminalManager.CreateNewTerminalNode(sanitised + "BuyConfirm");
                if (!string.IsNullOrEmpty(content.OverrideBuyConfirmNodeDescription))
                    buyConfirmNode.displayText = content.OverrideBuyConfirmNodeDescription;
                else
                {
                    buyConfirmNode.displayText = "Ordered [variableAmount] " + plural + ". Your new balance is";
                    buyConfirmNode.displayText += "[playerCredits]";
                    buyConfirmNode.displayText += "\n" + "\n" + "Our contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.";
                }
                buyConfirmNode.clearPreviousText = true;
                buyConfirmNode.maxCharactersToType = 35;
                buyConfirmNode.isConfirmationNode = false;
                buyConfirmNode.playSyncedClip = 0;

                buyInfoNode = TerminalManager.CreateNewTerminalNode(sanitised + "Info");
                buyInfoNode.clearPreviousText = true;
                buyInfoNode.maxCharactersToType = 25;
                buyInfoNode.displayText = "\n" + content.OverrideInfoNodeDescription;

                buyNode.AddCompatibleNoun(TerminalManager.Keyword_Confirm, buyConfirmNode);
                buyNode.AddCompatibleNoun(TerminalManager.Keyword_Deny, TerminalManager.Node_CancelPurchase);
            }

            content.BuyKeyword = keyword;
            content.BuyNode = buyNode;
            content.BuyConfirmNode = buyConfirmNode;
            content.BuyInfoNode = buyInfoNode;
        }
    }
}
