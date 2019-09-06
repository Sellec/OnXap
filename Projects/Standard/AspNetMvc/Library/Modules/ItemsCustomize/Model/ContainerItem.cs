using System.Collections.Generic;

namespace OnXap.Modules.ItemsCustomize.Model
{
    using Core.Modules.ItemsCustomize.Field;
    using Core.Modules.ItemsCustomize.Scheme;

#pragma warning disable CS1591 // todo внести комментарии.
    public class SchemeContainerItem
    {
        public class Scheme
        {
            public string Name;
            public Dictionary<int, IField> Fields;
        }

        public int IdModule { get; set; }

        public SchemeItem SchemeItem;

        public Dictionary<uint, Scheme> Schemes = new Dictionary<uint, Scheme>();
    }

}
