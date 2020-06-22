using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ducky.Models
{
    public class ActivityCountModel
    {
        [Key]
        public int id { get; set; }
        public int count { get; set; }
    }
}
