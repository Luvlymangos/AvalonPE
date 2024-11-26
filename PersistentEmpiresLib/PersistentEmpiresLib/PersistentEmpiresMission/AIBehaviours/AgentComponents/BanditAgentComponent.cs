using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using TaleWorlds.Engine;
using TaleWorlds.Core;

namespace PersistentEmpiresMission.AIBehaviours.Data
{
    public class BanditAgentComponent : AgentComponent
    {
        //PerceptionSystem
        public float SightDistance = 30;
        public float HearingDistance = 10;
        public float SightAngle = 120;
        public List<Agent> PerceivedAgents = new List<Agent>();

        //SETUP
        public Agent myagent;
        public double LastCheck = 0;


        //Sound
        public bool IsTalking = false;
        public double LastTalked = 0;
        public BanditAgentComponent(Agent agent) : base(agent)
        {
            this.myagent = agent;
        }
        public override void OnTickAsAI(float dt)
        {
            base.OnTickAsAI(dt);
            this.PerceptionSystem();
            this.SoundControlSystem();
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastCheck > 1)
            {
                OnTickAsSecond(dt);
            }
        }

        public void OnTickAsSecond(float dt)
        {
            this.LookAround();
            
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public bool CheckVisual(Agent myagent, Agent target)
        {
            // Calculate the direction vector from myagent to the target
            Vec3 directionToTarget = (target.Position - myagent.Position).NormalizedCopy();

            // Get the forward direction of myagent
            Vec3 myAgentForward = myagent.LookDirection;

            // Calculate the angle between the two vectors
            float dotProduct = Vec3.DotProduct(myAgentForward, directionToTarget);
            float angleBetween = MathF.Acos(dotProduct) * (180 / MathF.PI); // Convert radians to degrees

            // Check if the target is within the 120-degree FOV
            if (angleBetween <= (this.SightAngle / 2)) // 120 degrees FOV means 60 degrees to either side of the forward direction
            {
                // Perform the raycast to check for obstructions
                if (Mission.Current.Scene.RayCastForClosestEntityOrTerrain(myagent.Position, target.Position, out float hitpoint, out Vec3 CollisionPoint, out GameEntity collidedPoint, (float)0.01))
                {
                    if (CollisionPoint == null || target.Position == CollisionPoint && target.Position.Distance(myagent.Position) < SightDistance) // No obstruction
                    {
                        return true;
                    }
                }
            }

            return false; // Out of FOV or obstructed
        }

        public bool CheckSound(Agent myagent, Agent target)
        {
            if (target.MovementVelocity.x > 1 || target.MovementVelocity.y > 1)
            {
                return true;
            }
            return false;
        }

        public bool CheckDistance(Agent myagent, Agent target)
        {
            if (myagent.Position.Distance(target.Position) < 50)
            {
                return true;
            }
            return false;
        }

        public void PerceptionSystem()
        {
            try
            {
                foreach (Agent agent in Mission.Current.Agents)
                {
                    if (!CheckDistance(myagent, agent)) continue;
                    bool CanSee = CheckVisual(myagent, agent);
                    bool CanHear = CheckSound(myagent, agent);
                    if (CanSee || CanHear)
                    {
                        if (!PerceivedAgents.Contains(agent))
                        {
                            PerceivedAgents.Add(agent);
                        }
                    }
                    else
                    {
                        if (PerceivedAgents.Contains(agent))
                        {
                            PerceivedAgents.Remove(agent);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("Error in PerceptionSystem: " + e.Message);
            }
        }

        public void LookAround()
        {
            if (myagent.GetTargetAgent() == null)
            {
                myagent.LookDirection = new Vec3(myagent.LookDirection.x, myagent.LookDirection.y, myagent.LookDirection.z + MBRandom.RandomFloatRanged(-1, 1));
            }
        }

        public void SoundControlSystem()
        {
            if (IsTalking) return;
        }
    }
}
