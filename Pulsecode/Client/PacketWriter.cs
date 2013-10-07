using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
namespace Pulse.Client
{
    class PacketWriter
    {
        /*  Connection con;
          BinaryWriter bw;
          public PacketWriter(Connection c)
          {
              con = c;
              bw = c.Bw;
          }*/

        //Cannot do any reading while sending; all listening should be done on the listening thread because any listening on sending would result in blocking the main game thread
        public static void sendUserRequest(BinaryWriter bw, string users)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.USER_REQUEST);
                bw.Write(users);
            }
        }
        public static void sendClientHeartbeat(BinaryWriter bw)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.CLIENT_HEARTBEAT);
            }
        }
        public static void sendLogin(BinaryWriter bw, string user, string hashedpass)
        {
            if (bw != null)
            {
                Console.WriteLine("{0} {1}", user, hashedpass);
                bw.Write((short)SendHeaders.LOGIN);
                bw.Write(user);
                bw.Write(hashedpass);
            }
        }
        public static void sendCheckVersion(BinaryWriter bw)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.VER);
                bw.Write((double)Config.Version);
            }
        }
        public static void sendError(BinaryWriter bw, string error)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.ERROR);
                bw.Write(error);
            }
        }
        public static void sendUserUpdateSelf(BinaryWriter bw)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.UPDATE_USER_DATA);
            }
        }

        /// <summary>
        /// tells the server what the client is doing, send "" for md5/name and idle mode if no song
        /// </summary>
        /// <param name="bw">connection writer</param>
        /// <param name="user">username for some validation, still easy to fake</param>
        /// <param name="md5">md5 of current chart used for multiplayer/spectate</param>
        /// <param name="name">song name as "arist - songname", just for display</param>
        /// <param name="mode">use mode, full mode numbers can be found in servers user.cs</param>
        /// <param name="modflags">flags used in play, needed for if someone specs. could show on userlist too i guess make it easier to tell if to spec</param>
        /// <param name="scroll">scroll speed for spectate</param>
        /// <param name="speed">play speed for spectate</param>
        public static void sendSongStart(BinaryWriter bw, string user, string md5, string name, short mode, uint modflags, double scroll, double speed)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SONG_START);
                bw.Write(user);
                bw.Write(md5);
                bw.Write(name);
                bw.Write(mode);
                bw.Write(modflags);
                bw.Write(scroll);
                bw.Write(speed);
            }
        }
        /// <summary>
        /// returns a string of a list of users the length of number, with the query LIKE start%. so 5, "a" would return 5 users with names beginning with a
        /// </summary>
        /// <param name="bw">connection writer</param>
        /// <param name="number">total users to retrieve</param>
        /// <param name="start">search term</param>
        public static void getUsers(BinaryWriter bw, int number, string start)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.GET_USERS);
                bw.Write(number);
                bw.Write(start);
            }
        }
        /// <summary>
        /// sends a hit to the server to display to users spectating 
        /// </summary>
        /// <param name="bw">connection writer</param>
        /// <param name="offset">offset of the note that was hit</param>
        /// <param name="diff">users difference in offset</param>
        /// <param name="lane">lane the note was on</param>
        /// <param name="hitType">type of hit, such as great miss etc, use enum in hittype in replay</param>
        public static void sendSpectateHit(BinaryWriter bw, int offset, int diff, int lane, int hitType)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_HIT);
                bw.Write(offset);
                bw.Write(diff);
                bw.Write(lane);
                bw.Write(hitType);
            }
        }
        public static void sendSpectatePress(BinaryWriter bw, int offset, int lane)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_PRESS);
                bw.Write(offset);
                bw.Write(lane);
            }
        }
        public static void sendSpectateRelease(BinaryWriter bw, int offset, int lane)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_RELEASE);
                bw.Write(offset);
                bw.Write(lane);
            }
        }
        public static void sendSpectateHook(BinaryWriter bw, string name)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_HOOK);
                bw.Write(name);
            }
        }
        public static void sendSpectateCancel(BinaryWriter bw, string name)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_CANCEL);
                bw.Write(name);
            }
        }
        public static void sendSpectateFail(BinaryWriter bw)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_FAIL);
            }
        }
        public static void sendSpectateHeartbeat(BinaryWriter bw, int offset, int score, int combo, int health, double acc)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_HEARTBEAT);
                bw.Write(offset);
                bw.Write(score);
                bw.Write(combo);
                bw.Write(health);
                bw.Write(acc);
            }
        }
        public static void sendSpectateFinish(BinaryWriter bw, int score, int combo, int per, int gr, int ok, int miss, int flags, double acc)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_FINISH);
                bw.Write(score);
                bw.Write(combo);
                bw.Write(per);
                bw.Write(gr);
                bw.Write(ok);
                bw.Write(miss);
                bw.Write(flags);
                bw.Write(acc);
            }
        }
        public static void sendSpectateGotChart(BinaryWriter bw)
        {
            if (bw != null)
            {
                bw.Write((short)SendHeaders.SPECTATE_GOT_CHART);
            }
        }
    }
}
