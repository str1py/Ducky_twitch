using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ducky.Models
{
    public class TwitchUserModel
    {
        [Key]
        public int userId { get; set; }
        public string userName { get; set; }
        public string userType { get; set; }
        public int userActivity { get; set; }
        [Column(TypeName = "bit")]
        public bool isSub { get; set; }
        public int subMounthsCount { get; set; }
        public string SubPlan { get; set; }
        [Column(TypeName = "bit")]
        public bool isModer { get; set; }
        [Column(TypeName = "bit")]
        public bool isVip { get; set; }
        public int giftsCount { get; set; }
        public int messagescount { get; set; }
        public int messagestostreamer { get; set; }
        public int watchtime { get; set; }
        public int exp { get; set; }
        public int userlevel { get; set; }
        public DateTime created_at { get; set; }
    }
}
