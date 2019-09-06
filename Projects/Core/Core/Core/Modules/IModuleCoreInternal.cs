﻿using System;
using System.Collections.Generic;

namespace OnXap.Core.Modules
{
    using Items;

    interface IModuleCoreInternal
    {
        IReadOnlyDictionary<ItemBase, Uri> GenerateLinks(IEnumerable<ItemBase> items);

        int IdModule { get; }
    }
}
