using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SusSuite.Plugins.TeamSpeedRun.Models
{
    public class SpeedRunData
    {
        public SpeedRunTeam RedTeam { get; set; } = new();
        public SpeedRunTeam BlueTeam { get; set; } = new();
    }

    public class SpeedRunTeam
    {
        public List<int> TeamPlayers { get; set; } = new();
        public int TotalTasks { get; set; }
        public int TasksDone { get; set; }
    }
}
