using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public interface ITerminalContent
    {
        public TerminalKeyword TerminalKeyword { get; }
    }

    public interface ITerminalEntry : ITerminalContent
    {
        public TerminalNode TerminalEntryNode { get; }
        public TerminalKeyword GetTerminalEntryRegistryKeyword();
    }

    public interface ITerminalPurchasableEntry : ITerminalEntry
    {
        public TerminalNode PurchasePromptNode { get; }
        public TerminalNode PurchaseConfirmNode { get; }
        public TerminalKeyword GetTerminalPurchasableRegistryKeyword();
    }
}
