using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresMission.HungerGames
{
    public class KillZone : MissionLogic
    {
        public GameEntity KillZoneEntity;
        public double LastCheck = 0;
        public float StartSize = 600;
        public MatrixFrame StartFrame;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Debug.Print("KillZone initialized");
            LastCheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override void AfterStart()
        {
            base.AfterStart();
            
            if (GameNetwork.IsServer)
            {
                KillZoneEntity = Mission.Scene.FindEntityWithTag("KillZone");
                if (KillZoneEntity != null)
                {
                    KillZoneEntity.SetVisibilityExcludeParents(false);
                    StartFrame = KillZoneEntity.GetGlobalFrame();
                }
                else
                {
                    Debug.Print("KillZone entity not found");
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (GameNetwork.IsServer)
            {
                try { 
                GameController gc = Mission.Current.GetMissionBehavior<GameController>();
                if (!gc.GameStarted) return;
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastCheck > 1)
                {
                    if (KillZoneEntity == null)
                    {
                        return;
                    }
                    Debug.Print(KillZoneEntity.GetGlobalFrame().GetScale().X.ToString() + " " + KillZoneEntity.GetGlobalFrame().GetScale().Y.ToString() + " " + KillZoneEntity.GetGlobalFrame().GetScale().Z.ToString());
                    LastCheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (KillZoneEntity.GetGlobalFrame().GetScale().X >= 20)
                    {
                        Vec3 oldScale = KillZoneEntity.GetGlobalFrame().GetScale();
                        MatrixFrame killZoneFrame = KillZoneEntity.GetGlobalFrame();

                        // Decrease X and Y scales by 1 while keeping Z unchanged
                        float newXScale = (float)Math.Max(1, oldScale.X - 2); // Ensure scale doesn't go below 1
                        float newYScale = (float)Math.Max(1, oldScale.Y - 2); // Ensure scale doesn't go below 1
                        float zScale = oldScale.Z; // Keep Z scale the same

                        // Apply the new scale
                        killZoneFrame.Scale(new Vec3(newXScale / oldScale.X, newYScale / oldScale.Y, 1.0f));
                        KillZoneEntity.SetGlobalFrame(killZoneFrame);

                        // Broadcast the updated scale
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new UpdateKillzone((int)newXScale));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

                        Debug.Print($"Updated Scale: X={newXScale}, Y={newYScale}, Z={zScale}");
                    }


                    foreach (Agent agent in Mission.Agents)
                    {
                        if (agent.Position.Distance(KillZoneEntity.GetGlobalFrame().origin) > KillZoneEntity.GetGlobalFrame().GetScale().X / 2)
                        {
                            Blow blow = new Blow(agent.Index);
                            blow.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
                            blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
                            blow.GlobalPosition = agent.Position;
                            blow.GlobalPosition.z = blow.GlobalPosition.z + agent.GetEyeGlobalHeight();
                            blow.BaseMagnitude = 50;
                            blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                            blow.InflictedDamage = 50;
                            blow.SwingDirection = agent.LookDirection;
                            MatrixFrame frame = agent.Frame;
                            blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
                            blow.SwingDirection.Normalize();
                            blow.Direction = blow.SwingDirection;
                            blow.DamageCalculated = true;
                            sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
                            AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
                            agent.RegisterBlow(blow, attackCollisionDataForDebugPurpose);
                        }
                    }
                }
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                }
            }
        }

            

        public void NewRound()
        {
            KillZoneEntity.SetGlobalFrame(StartFrame);
        }
    }


}
