using System.ComponentModel.DataAnnotations;

namespace Notice.Models
{
    public class Attachfile
    {
        [Key]
        public int File_id { get; set; }
        public string File_name { get; set; }
        public byte[] File_data { get; set; }
        public string File_path { get; set; }
        public int post_id { get; set; }
    }
}
