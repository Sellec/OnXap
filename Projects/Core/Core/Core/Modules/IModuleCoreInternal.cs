using System;
using System.Collections.Generic;

namespace OnXap.Core.Modules
{
    using Items;

    interface IModuleCoreInternal
    {
        IReadOnlyList<KeyValuePair<ItemBase, Uri>> GenerateLinks(IEnumerable<ItemBase> items);

        int IdModule { get; }
    }
}
