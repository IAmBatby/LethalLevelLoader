using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class UnlockableItemManager : ExtendedContentManager<ExtendedUnlockableItem, UnlockableItem>
    {
        protected override List<UnlockableItem> GetVanillaContent() => OriginalContent.UnlockableItems;
        protected override ExtendedUnlockableItem ExtendVanillaContent(UnlockableItem content) => ExtendedUnlockableItem.Create(content);

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);

            StartOfRound.unlockablesList.unlockables = [.. PatchedContent.ExtendedUnlockableItems.Select(u => u.UnlockableItem)];
            List<ExtendedUnlockableItem> unlockableItems = new List<ExtendedUnlockableItem>(PatchedContent.ExtendedUnlockableItems);
            for (int i = 0; i < unlockableItems.Count; i++)
                unlockableItems[i].SetGameID(i);

            foreach (ExtendedUnlockableItem item in unlockableItems)
            {
                TerminalManager.Keywords.Buy.TryAdd(item.BuyKeyword, item.BuyNode);
                TerminalManager.Keywords.Info.TryAdd(item.BuyKeyword, item.BuyInfoNode);
            }
        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedUnlockableItem content)
        {
            UnlockableItem unlock = content.Content;
            if (unlock.unlockableType == 1 && !unlock.alreadyUnlocked)
            {
                if (unlock.prefabObject == null)
                    return (false, "Unlockable Item Prefab Was Null Or Empty");
                else if (!unlock.prefabObject.TryGetComponent(out NetworkObject _))
                    return (false, "Unlockable Item Prefab Is Missing NetworkObject Component");
                else if (!unlock.prefabObject.TryGetComponent(out AutoParentToShip _))
                    return (false, "Unlockable Item Prefab Is Missing AutoParentToShip Component");
            }
            else if (unlock.unlockableType == 0 && unlock.suitMaterial == null)
                return (false, "Unlockable Suit Is Missing Suit Material");

            return (true, string.Empty);
        }

        protected override void PopulateContentTerminalData(ExtendedUnlockableItem content)
        {
            TerminalKeyword keyword = null;
            TerminalNode buyNode = null;
            TerminalNode buyConfirmNode = null;
            TerminalNode infoNode = null;

            if (content.UnlockableItem.shopSelectionNode != null)
            {
                buyNode = content.UnlockableItem.shopSelectionNode;
                buyConfirmNode = buyNode?.terminalOptions[1].result;
                if (Keywords.Buy.compatibleNouns.TryGet(buyNode, out TerminalKeyword noun))
                    keyword = noun;
                if (Keywords.Info.compatibleNouns.TryGet(keyword, out TerminalNode node))
                    infoNode = node;
            }
            else
            {
                string sanitisedName = content.UnlockableItem.unlockableName.StripSpecialCharacters().Sanitized();
                keyword = TerminalManager.CreateNewTerminalKeyword(sanitisedName + "Keyword", sanitisedName, Keywords.Buy);

                buyNode = TerminalManager.CreateNewTerminalNode(sanitisedName + "Buy");
                buyNode.itemCost = content.ItemCost;
                buyNode.isConfirmationNode = false;
                buyNode.overrideOptions = true;
                buyNode.clearPreviousText = true;
                buyNode.maxCharactersToType = 15;
                buyNode.creatureName = content.UnlockableItem.unlockableName;
                if (!string.IsNullOrEmpty(content.OverrideBuyNodeDescription))
                    buyNode.displayText = content.OverrideBuyNodeDescription;
                else
                {
                    buyNode.displayText = $"You have requested to order the {buyNode.creatureName}.";
                    buyNode.displayText += "\n Total cost of item: [totalCost].";
                    buyNode.displayText += "\n" + "\n" + "Please CONFIRM or DENY." + "\n" + "\n";
                }

                buyConfirmNode = TerminalManager.CreateNewTerminalNode(sanitisedName + "BuyConfirm");
                buyConfirmNode.itemCost = content.ItemCost;
                buyConfirmNode.isConfirmationNode = true;
                buyConfirmNode.clearPreviousText = true;
                buyConfirmNode.buyUnlockable = true;
                buyConfirmNode.maxCharactersToType = 35;
                buyConfirmNode.playSyncedClip = 0;
                buyConfirmNode.creatureName = content.UnlockableItem.unlockableName;
                if (!string.IsNullOrEmpty(content.OverrideBuyConfirmNodeDescription))
                    buyConfirmNode.displayText = content.OverrideBuyConfirmNodeDescription;
                else
                {
                    buyConfirmNode.displayText = $"Ordered the {buyConfirmNode.creatureName}! ";
                    buyConfirmNode.displayText += "Your new balance is [playerCredits]";
                }

                infoNode = TerminalManager.CreateNewTerminalNode(sanitisedName + "Info");
                infoNode.clearPreviousText = true;
                infoNode.maxCharactersToType = 35;
                infoNode.creatureName = content.UnlockableItem.unlockableName;
                infoNode.displayText = content.OverrideInfoNodeDescription;

                buyNode.AddNoun(Keywords.Confirm, buyConfirmNode);
                buyNode.AddNoun(Keywords.Deny, Nodes.CancelBuy);
            }

            content.BuyKeyword = keyword;
            content.UnlockableItem.shopSelectionNode = buyNode;
            content.BuyNode = buyNode;
            content.BuyConfirmNode = buyConfirmNode;
            content.BuyInfoNode = infoNode;
        }
    }
}