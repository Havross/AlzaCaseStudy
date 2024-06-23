using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlzaCaseStudy.Models
{
    public class PositionItem
    {
        public string? Text { get; set; }
        public string? Label { get; set; }
        public string? Content { get; set; }
        public List<string>? SubContent { get; set; }
        public int? Type { get; set; }
    }
}
