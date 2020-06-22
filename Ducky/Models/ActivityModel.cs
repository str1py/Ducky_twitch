using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ducky.Models
{
    public class ActivityModel
    {
        [Key]
        public int id { get; set; }
        public int vievers { get; set; }
        public int activevievers { get; set; }
        public decimal activity { get; set; }
        public DateTime created_at { get; set; }
    }
}
