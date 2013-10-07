using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Client
{
    enum SendHeaders : short
    {
        ERROR = 5,
        VER = 6,
        LOGIN = 7,
        SONG_START = 8,
        GET_USERS = 9,
        SPECTATE_HEARTBEAT = 10,
        SPECTATE_HIT = 11,
        SPECTATE_CANCEL = 12,
        SPECTATE_HOOK = 13,
        SPECTATE_PRESS = 14,
        SPECTATE_RELEASE = 15,
        SPECTATE_FINISH = 16,
        USER_REQUEST = 17,
        UPDATE_USER_DATA=18,
        SPECTATE_FAIL = 19,
        CLIENT_HEARTBEAT = 20,
        SPECTATE_GOT_CHART = 21
    }
}
