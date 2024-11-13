using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    public class ChangeSkill : PEAdminButtonVM
    {
        public string SkillName { get; set; }
        public int Value { get; set; }

        public ChangeSkill(string skillName, int value)
        {
            SkillName = skillName;
            Value = value;
        }
        public override string GetCaption()
        {
            return $"{SkillName}";
        }

        public void SetValue(int value)
        {
            Value = value;
        }
        public override void Execute()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestUpdateSkills(SelectedPlayer.GetPeer(), SkillName, Value));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}