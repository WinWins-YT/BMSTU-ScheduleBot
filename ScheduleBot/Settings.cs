using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleBot
{
    internal class Settings
    {
        public string Token { get; set; }
        public string GroupUrl { get; set; }
        public long?[] AdminUsers { get; set; }
    }
}
