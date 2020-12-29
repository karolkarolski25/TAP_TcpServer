using System;

namespace Login.Enums
{
    [Flags]
    public enum UserLoginSettings
    {
        LoggedIn,
        BadPassword,
        UserNotExists
    }
}
