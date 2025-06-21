using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public interface IExtendedContent<E,C,M> where E : ExtendedContent<E,C,M>, IExtendedContent<E,C,M> where M : ExtendedContentManager, IExtendedManager<E,C,M>
    {
        public C Content { get; }

    }
}
