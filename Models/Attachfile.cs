using System.ComponentModel.DataAnnotations;

namespace Notice.Models
{
    public class Attachfile
    {
        [Key]
        public int File_id { get; set; }
        public byte[] File_data { get; set; }
    }
}
