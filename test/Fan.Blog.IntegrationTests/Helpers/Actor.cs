﻿using Fan.Membership;

namespace Fan.Blog.IntegrationTests.Helpers
{
    /// <summary>
    /// Different users that interact with the system. Used for tests.
    /// </summary>
    public class Actor
    {
        public const int ADMIN_ID = 1;
        public const string ADMIN_USERNAME = "user";
        public static User User = new User { Id = 1, UserName = "user", DisplayName = "My Name", Email = "user@email.com" };
    }
}