using System.Collections.Generic;

namespace OnXap.Modules.ItemsCustomize.Model
{
    using Core.Db;
    using Field;
    using Scheme;

    public class Fields
    {
        /// <summary>
        /// Идентификатор модуля, для которого редактируется схема.
        /// </summary>
        public int IdModule { get; set; }

        public Dictionary<uint, string> Schemes { get; set; }

        public Dictionary<ItemType, Dictionary<SchemeItem, string>> SchemeItems { get; set; }

        public Dictionary<int, IField> FieldsList { get; set; }

        public bool AllowSchemesManage { get; set; }
    }
}
