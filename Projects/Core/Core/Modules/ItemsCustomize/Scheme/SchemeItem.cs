namespace OnXap.Modules.ItemsCustomize.Scheme
{
    /// <summary>
    /// Контейнер схемы. Указывает идентификатор и тип объекта, к которому привязаны поля схемы.
    /// </summary>
    public class SchemeItem
    {
        public SchemeItem(int IdItem, int IdItemType)
        {
            this.IdItem = IdItem;
            this.IdItemType = IdItemType;
        }

        /// <summary>
        /// Идентификатор контейнера схемы.
        /// </summary>
        public int IdItem { get; set; }

        /// <summary>
        /// Тип контейнера схемы.
        /// </summary>
        public int IdItemType { get; set; }

        public override int GetHashCode()
        {
            return GetEqualityKey(this).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is SchemeItem) return (obj as SchemeItem).IdItem == IdItem && (obj as SchemeItem).IdItemType == IdItemType;
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return string.Format("IdItem={0}, IdItemType={1}", IdItem, IdItemType);
        }

        public static string GetEqualityKey(SchemeItem schemeItem)
        {
            return schemeItem == null ? null : schemeItem.IdItemType.ToString() + "_" + schemeItem.IdItem.ToString();
        }

        public static string GetEqualityKey(int idItemType, int idItem)
        {
            return idItemType.ToString() + "_" + idItem.ToString();
        }

    }


}
