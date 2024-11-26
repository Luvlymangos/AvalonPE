using PersistentEmpiresLib;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons
{
    internal class Debug : PEAdminButtonVM
    {
        public override string GetCaption()
        {
            return "Debug Mode";
        }

        public override void Execute()
        {
            PersistentEmpireRepresentative rerp = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            rerp.DebugMode = !rerp.DebugMode;
        }
    }
}