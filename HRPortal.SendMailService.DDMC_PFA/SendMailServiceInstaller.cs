using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace HRPortal.SendMailService.DDMC_PFA
{
    [RunInstaller(true)]
    public partial class SendMailServiceInstaller : System.Configuration.Install.Installer
    {
        public SendMailServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
