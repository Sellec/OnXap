﻿using System.Collections.Generic;

namespace OnXap.Modules.Customer.Design.Model
{
    using Core.DB;

    public class AdminUsersManage
    {
        public UserState? RequestedState { get; set; }
        public List<User> DataList { get; set; }
        public int DataCountAll { get; set; }
    }
}