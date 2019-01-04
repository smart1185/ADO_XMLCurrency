namespace HW_XMLCurrencyProject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Currency")]
    public partial class Currency
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Title { get; set; }

        public DateTime PubDate { get; set; }

        public double? CurrDescription { get; set; }
    }
}
