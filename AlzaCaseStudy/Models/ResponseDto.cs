using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlzaCaseStudy.Models
{
    public class ResponseDto<T>
    {
        public T? Response { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
