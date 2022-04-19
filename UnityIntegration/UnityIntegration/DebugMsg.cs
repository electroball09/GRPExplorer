using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration
{
    public class DebugMsg
    {
        string text = "";
        Func<string> getText;
        public float StartTime { get; private set; } = 0f;
        public float Duration { get; set; } = 5f;

        public string Text { get { return getText(); } set { text = value; } }

        public DebugMsg(Func<string> getText)
        {
            this.getText = getText != null ? getText : myGetText;
            StartTime = Time.time;
        }

        private string myGetText()
        {
            return text;
        }
    }

    public class DebugMsgMgr : SingletonBehaviour<DebugMsgMgr>
    {
        List<DebugMsg> msgs = new List<DebugMsg>();

        public DebugMsg NewMessage(string initialText = "", float msgTime = 5f, Func<string> getTextFunc = null)
        {
            DebugMsg msg = new DebugMsg(getTextFunc)
            {
                Text = initialText,
                Duration = msgTime
            };

            msgs.Add(msg);

            return msg;
        }

        public DebugMsg NewPermMessage(string initialText = "", Func<string> getTextFunc = null)
        {
            return NewMessage(initialText, -1f, getTextFunc);
        }

        public void AddMsg(DebugMsg msg)
        {
            msgs.Add(msg);
        }

        public void RemoveMsg(DebugMsg msg)
        {
            msgs.Remove(msg);
        }

        void Update()
        {
            foreach (DebugMsg msg in msgs)
            {
                if (msg.Duration == -1f)
                    continue;

                if (Time.time - msg.StartTime > msg.Duration)
                {
                    msgs[msgs.IndexOf(msg)] = null;
                }
            }

            msgs.RemoveAll((msg) => msg == null);
        }

        void OnGUI()
        {
            Rect rect = new Rect(250f, 250f, 9999f, 25f);
            foreach (DebugMsg msg in msgs)
            {
                GUI.Label(rect, msg.Text);
                rect.y += rect.height;
            }
        }
    }
}
