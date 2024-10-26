using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_NamedItem : UsableMissionObject
    {
        public string Name = "";
        public string Description = "";

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject(Name);
            base.DescriptionMessage = new TextObject(Description);

        }
        public override void OnUse(Agent userAgent)
        {
            base.OnUse(userAgent);
            userAgent.StopUsingGameObjectMT(true);
            return;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return Name;
        }
    }
}
