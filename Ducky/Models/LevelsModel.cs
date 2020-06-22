using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ducky.Models
{
    public class LevelsModel
    {
        [Key]
        public int lvl { get; set; }
        public double exp_total { get; set; }
        public double exp_to_lvl { get; set; }
    }
}
