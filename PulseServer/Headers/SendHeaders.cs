using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulseServer.Headers
{
    enum SendHeaders : short
    {
        LOGIN_AUTH = 0,
        VERSION_CHECK = 1,
        SPECTATE_RECORD = 2,
        SPECTATE_END = 3,
        SPECTATE_HIT = 4,
        SPECTATE_RELEASE = 5,
        SPECTATE_PRESS = 6,
        SPECTATE_HEARTBEAT = 7,
        SPECTATE_START = 8,
        SPECTATE_FINISH = 9,
        SPECTATE_CANCEL = 10,
        SPECTATE_USERS = 11,
        USER_REQUEST_INFO = 12,
        SPECTATE_FAIL = 13,
        SPECTATE_USERS_ME = 14
    }
}
