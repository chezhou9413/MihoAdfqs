using MihoAdfqs.Phase2.Orbital;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //从通讯主菜单进入独立支援面板。
    public partial class Dialog_ImperialCommunications
    {
        //关闭联络对话后打开目录，支援操作由专用面板处理。
        private void ShowExchange()
        {
            Close();
            Find.WindowStack.Add(new Dialog_ImperialSupport(map));
        }
    }
}
