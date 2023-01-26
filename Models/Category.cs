using System.ComponentModel.DataAnnotations;

namespace Notice.Models
{
    public class Category
    {
        [Key]
        public int Category_id { get; set; }
        [StringLength(50)]
        public string CategoryValue { get; set; }
    }
}
