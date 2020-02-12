#pragma warning disable CS1591
namespace OnXap.Journaling.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("JournalName")]
    public class JournalNameDAO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdJournal { get; set; }

        public int IdJournalType { get; set; }

        [Column("JournalName")]
        public string Name { get; set; }

        public string UniqueKey { get; set; }
    }
}
