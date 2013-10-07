using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PulseServer.Headers;
using PulseServer.Game;
using System.IO;
using PulseServer.Handlers;

namespace PulseServer
{
    class PacketWriter
    {
        public static void sendSpectateRecord(ClientHandler h)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_RECORD);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendUserInfo(ClientHandler ch, User u, string usr)
        {
            try
            {
                BinaryWriter Bw = ch.Bw;
                if (ch != null && Bw != null)
                {
                    Bw.Write((short)SendHeaders.USER_REQUEST_INFO);
                    //Console.WriteLine(u.Name);
                    //online bool, name,realname,,avatar,playcount,totalscore,mode,currentsong,currentchart, accuracy, level
                    if (u == null)
                    {
                        Bw.Write(false);
                        Bw.Write(usr);
                    }
                    else
                    {
                        Bw.Write(true);
                        Bw.Write(u.Name);
                        Bw.Write(u.RealName);
                        Bw.Write(u.Avatar);
                        Bw.Write(u.Playcount);
                        Bw.Write(u.TotalScore);
                        Bw.Write((int)u.Mode);
                        Bw.Write(u.CurrentSong);
                        Bw.Write(u.CurrentChart);
                        Bw.Write(u.Accuracy);
                        Bw.Write(u.Level);
                    }
                }
                else
                {
                    ch.abort();
                }
            }
            catch (Exception e)
            {
                ch.abort(e);
            }
        }
        public static void endSpectateRecord(ClientHandler h)
        {
            try
            {
                if (h != null & h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_END);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateStart(ClientHandler h, string user, string md5, int mods, double scroll)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_START);
                    h.Bw.Write(user);
                    h.Bw.Write(md5);
                    h.Bw.Write(mods);
                    h.Bw.Write(scroll);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateHit(ClientHandler h, int off, int diff, int lane, int type)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_HIT);
                    h.Bw.Write(off);
                    h.Bw.Write(diff);
                    h.Bw.Write(lane);
                    h.Bw.Write(type);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateRelease(ClientHandler h, int off, int lane)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_RELEASE);
                    h.Bw.Write(off);
                    h.Bw.Write(lane);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectatePress(ClientHandler h, int off, int lane)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_PRESS);
                    h.Bw.Write(off);
                    h.Bw.Write(lane);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateHeartbeat(ClientHandler h, int off, int score, int combo, int hp, double acc)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_HEARTBEAT);
                    h.Bw.Write(off);
                    h.Bw.Write(score);
                    h.Bw.Write(combo);
                    h.Bw.Write(hp);
                    h.Bw.Write(acc);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateFinish(ClientHandler h, int score, int combo, int per, int gr, int ok, int miss, int flags, double acc)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_FINISH);
                    h.Bw.Write(score);
                    h.Bw.Write(combo);
                    h.Bw.Write(per);
                    h.Bw.Write(gr);
                    h.Bw.Write(ok);
                    h.Bw.Write(miss);
                    h.Bw.Write(flags);
                    h.Bw.Write(acc);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateCancel(ClientHandler h)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_CANCEL);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateUsers(ClientHandler h, string list)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_USERS);
                    h.Bw.Write(list);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateUsersMe(ClientHandler h, string list)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_USERS_ME);
                    h.Bw.Write(list);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
        public static void sendSpectateFail(ClientHandler h)
        {
            try
            {
                if (h != null && h.Bw != null)
                {
                    h.Bw.Write((short)SendHeaders.SPECTATE_FAIL);
                }
                else
                {
                    h.abort();
                }
            }
            catch (Exception e)
            {
                h.abort(e);
            }
        }
    }
}
