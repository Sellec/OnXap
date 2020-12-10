using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnXap.Journaling
{
    /// <summary>
    /// Хранит параметры фильтрации записей журнала.
    /// </summary>
    public class DataFilterOptions
    {
        /// <summary>
        /// Идентификатор журнала для поиска записей. Имеет более низкий приоритет, чем <see cref="JournalComponentType"/>.
        /// </summary>
        public int? IdJournal { get; set; }

        /// <summary>
        /// Тип компонента для поиска журнала на основе типа (<see cref="JournalingManager.GetJournalTyped(Type)"/>). Имеет более высокий приоритет, чем <see cref="IdJournal"/>.
        /// </summary>
        public Type JournalComponentType { get; set; }

        /// <summary>
        /// Код события для поиска записей в журнале/журналах.
        /// </summary>
        public int? EventCode { get; set; }

        /// <summary>
        /// Ограничивает количество получаемых записей журнала/журналов, которые будут выбраны.
        /// </summary>
        /// <remarks>В данный момент записи сортируются по уменьшению времени события (<see cref="Model.JournalData.DateEvent"/>).</remarks>
        public int? Limit { get; set; }

        /// <summary>
        /// Определяет минимальную дату выбираемых событий.
        /// </summary>
        public DateTime? DateMin { get; set; }

        /// <summary>
        /// Определяет максимальную дату выбираемых событий.
        /// </summary>
        public DateTime? DateMax { get; set; }
    }
}
