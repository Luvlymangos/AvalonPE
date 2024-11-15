using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.AgentBehaviours
{
    public class GreetingsBehaviour : AgentComponent
    {
        public List<Agent> AcknowledgedAgents = new List<Agent>();

        public GreetingsBehaviour(Agent agent) : base(agent)
        {
        }

    }
}
