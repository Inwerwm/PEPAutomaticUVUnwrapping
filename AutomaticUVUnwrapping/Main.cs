using PEPlugin;
using PEPlugin.Pmx;
using System;
using System.Linq;
using System.Windows.Forms;

namespace AutomaticUVUnwrapping
{
    public class AutomaticUVUnwrapping : PEPluginClass
    {
        public AutomaticUVUnwrapping() : base()
        {
        }

        public override string Name
        {
            get
            {
                return "自動UV展開";
            }
        }

        public override string Version
        {
            get
            {
                return "0.0";
            }
        }

        public override string Description
        {
            get
            {
                return "自動UV展開";
            }
        }

        public override IPEPluginOption Option
        {
            get
            {
                // boot時実行, プラグインメニューへの登録, メニュー登録名
                return new PEPluginOption(false, true, "自動UV展開");
            }
        }

        public override void Run(IPERunArgs args)
        {
            try
            {
                if (ctrlForm == null)
                {
                    ctrlForm = new CtrlForm(args);
                    ctrlForm.Visible = true;
                }
                else
                {
                    ctrlForm.Format();
                    ctrlForm.Visible = !ctrlForm.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public override void Dispose()
        {
            if (ctrlForm != null)
            {
                ctrlForm.Close();
                ctrlForm = null;
            }
        }

        CtrlForm ctrlForm;
    }
}