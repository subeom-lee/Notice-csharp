using System.ComponentModel.DataAnnotations;

namespace Notice.Models
{
    public class Post
    {
        [Key]
        public int post_id { get; set; }
        [Required(ErrorMessage = "제목은 필수 입력 항목입니다.")]
        [StringLength(20, ErrorMessage = "제목은 20자를 초과할 수 없습니다.")]
        public string title { get; set; }
        [Required(ErrorMessage = "내용은 필수 입력 항목입니다.")]
        [StringLength(200, ErrorMessage = "내용은 200자를 초과할 수 없습니다.")]
        public string contents { get; set; }
        public DateTime CreatedDatetime { get; set; } = DateTime.Now;
        public DateTime UpdatedDatetime { get; set; } = DateTime.Now;
        public int ViewCount { get; set; }
    }
}