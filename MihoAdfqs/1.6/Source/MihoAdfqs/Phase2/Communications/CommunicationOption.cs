using System;

namespace MihoAdfqs.Phase2.Communications
{
    //职责：保存通讯选项的可见文本及选择行为。
    public class CommunicationOption
    {
        public string label;
        public Action action;

        //职责：构造一个可以排版和延迟执行的选项。
        public CommunicationOption(string label, Action action) { this.label = label; this.action = action; }
    }
}
