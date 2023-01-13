﻿using System.ComponentModel.DataAnnotations;

namespace Notice.Models
{
    public class Post
    {
        [Key]
        public int post_id { get; set; }
        public string title { get; set; }
        public string contents { get; set; }
        public DateTime CreatedDatetime { get; set; } = DateTime.Now;
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
    }
}