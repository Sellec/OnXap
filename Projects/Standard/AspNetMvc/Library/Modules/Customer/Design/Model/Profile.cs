﻿namespace OnXap.Modules.Customer.Design.Model
{
    using Core.Db;

    public class Profile
    {
        public User User { get; set; }

        public Customer.Model.ProfileEdit Edit { get; set; }
    }
}