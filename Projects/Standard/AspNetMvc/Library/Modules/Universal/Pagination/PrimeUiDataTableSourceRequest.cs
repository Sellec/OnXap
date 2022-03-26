using System.ComponentModel.DataAnnotations;

namespace OnXap.Modules.Universal.Pagination
{
    public enum PrimeUiDataTableFieldFilterMatchMode : int
    {
        None = 0,
        StartsWith = 1,
        Contains = 2,
    }

    public enum PrimeUiDataTableFieldFilterConstraintGroupOperator : int
    {
        None = 0,
        And = 1,
        Or = 2,
    }

    public class PrimeUiDataTableFieldConstraint
    {
        [Required]
        public PrimeUiDataTableFieldFilterMatchMode MatchType { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Value { get; set; }
    }

    public class PrimeUiDataTableFieldFilter
    {
        [Required]
        [MaxLength(200)]
        public string FieldName { get; set; }

        [Required]
        public PrimeUiDataTableFieldFilterConstraintGroupOperator Operator { get; set; }

        public PrimeUiDataTableFieldConstraint[] Constraints { get; set; }
    }

    public class PrimeUiDataTableSourceRequest
    {
        public uint FirstRow { get; set; }

        public uint RowsLimit { get; set; }

        [MaxLength(200)]
        public string SortByFieldName { get; set; }

        public bool SortByAcsending { get; set; }

        public PrimeUiDataTableFieldFilter[] FilterFields { get; set; }
    }
}