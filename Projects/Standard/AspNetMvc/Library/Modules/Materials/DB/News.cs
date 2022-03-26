using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.Materials.DB
{
    using Core.Items;
    using Modules.Routing;

    [Table("News")]
    public class News : ItemBase, IItemRouted
    {
        public override int IdBase
        {
            get => id;
        }

        public override string CaptionBase
        {
            get => name;
        }

        public override DateTime DateChangeBase
        {
            get => date;
            set => date = value;
        }

        public int id { get; set; }

        public int category { get; set; }

        public bool status { get; set; }

        [Required]
        [MaxLength(300)]
        public string name { get; set; }

        public string short_text { get; set; }

        public string text { get; set; }

        public DateTime date { get; set; }

        public int comments_count { get; set; }

        public int user { get; set; }

        public bool Block { get; set; } = false;

        public Uri Url => UrlBase;

        public UrlSourceType UrlSourceType => UrlSourceTypeBase;
    }
}
