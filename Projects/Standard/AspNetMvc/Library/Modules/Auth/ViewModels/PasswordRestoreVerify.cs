using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Auth.ViewModels
{
    public class PasswordRestoreVerify : Auth.Model.PasswordRestoreSave
    {
        public string CodeType { get; set; }
    }
}