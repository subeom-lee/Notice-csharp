using System.ComponentModel.DataAnnotations;

namespace Notice.Models
{
    public class AttachfileDto
    {
        public int? File_id { get; set; }

        public string? File_name { get; set; }
        public string? File_path { get; set; }
    }
}
