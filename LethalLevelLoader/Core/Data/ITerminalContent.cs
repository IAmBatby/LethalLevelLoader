using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public interface ITerminalEntry
    {
        public TerminalKeyword NounKeyword { get; }
        public List<CompatibleNoun> GetRegistrations();
        public static CompatibleNoun Create(TerminalKeyword registry, TerminalNode value) => (Utilities.Create(registry, value));

        public void TryRegister()
        {
            foreach (CompatibleNoun noun in GetRegistrations())
                if (!noun.noun.Contains(NounKeyword,noun.result))
                    noun.noun.AddNoun(NounKeyword,noun.result);
        }
    }

    public interface ITerminalInfoEntry : ITerminalEntry
    {
        public TerminalNode InfoNode { get; }
        public TerminalKeyword RegistryKeyword { get; }
        public CompatibleNoun GetPair() => Create(RegistryKeyword, InfoNode);
    }

    public interface ITerminalPurchasableEntry : ITerminalEntry
    {
        public int PurchasePrice { get; }
        public void SetPurchasePrice(int purchasePrice);
        public TerminalNode PurchasePromptNode { get; }
        public TerminalNode PurchaseConfirmNode { get; }
        public TerminalKeyword RegistryKeyword { get; }
        public CompatibleNoun GetPair() => Create(RegistryKeyword, PurchasePromptNode);
    }
}
