using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;

namespace NetXmpp
{
    public class Program
    {
        private static bool _wait = true;

        public static void Main(string[] args)
        {
            /*
             * Starting Jabber Console, setting the Display settings
             * 
             */
            Console.Title = "Jabber Client";
            Console.ForegroundColor = ConsoleColor.White;

            /*
             * Login
             * 
             */
            
            Console.WriteLine("Login");
            Console.WriteLine();
            Console.WriteLine("JID: ");
            //TODO jid server + user
            string JID_Sender = Console.ReadLine();
            Console.WriteLine("Password: ");
            string Password = Console.ReadLine();

            /*
             * Creating the Jid and the XmppClientConnection objects
             */
            Jid jidSender = new Jid(JID_Sender);
            XmppClientConnection xmpp = new XmppClientConnection(jidSender.Server);
            
            /*
             * Open the connection
             * and register the OnLogin event handler
             */
            try
            {
                xmpp.Open(jidSender.User, Password);
                xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
                xmpp.OnError += new ErrorHandler(xmpp_OnError);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            /*
             * workaround, jus waiting till the login 
             * and authentication is finished
             * 
             */
            Console.Write("Wait for Login ");
            int i = 0;
            _wait = true;
            do
            {
                Console.Write(".");
                i++;
                if (i == 10)
                    _wait = false;
                Thread.Sleep(500);
            } while (_wait);
            Console.WriteLine();

            /*
             * 
             * just reading a few information
             * 
             */
            Console.WriteLine("Login Status:");
            Console.WriteLine("xmpp Connection State {0}", xmpp.XmppConnectionState);
            Console.WriteLine("xmpp Authenticated? {0}", xmpp.Authenticated);
            Console.WriteLine();

            /*
             * 
             * tell the world we are online and in chat mode
             * 
             */
            Console.WriteLine("Sending Precence");
            Presence p = new Presence(ShowType.chat, "Online");
            p.Type = PresenceType.available;
            xmpp.Send(p);
            Console.WriteLine();

            /*
             * 
             * get the roster (see who's online)
             */
            xmpp.OnPresence += new PresenceHandler(xmpp_OnPresence);

            //wait until we received the list of available contacts            
            Console.WriteLine();
            Thread.Sleep(500);

            /*
             * now we catch the user entry, TODO: who is online
             */
            Console.WriteLine("Enter Chat Partner JID:");
            string JID_Receiver = Console.ReadLine();
            Console.WriteLine();

            /*
             * Chat starts here
             */
            Console.WriteLine("Start Chat");

            /*
             * Catching incoming messages in
             * the MessageCallBack
             */
            xmpp.MessageGrabber.Add(new Jid(JID_Receiver),
                                     new MessageCB(MessageCallBack),
                                     null);

            /*
             * Sending messages
             * 
             */
            string outMessage;
            bool halt = false;
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                outMessage = Console.ReadLine();
                if (outMessage == "q!")
                {
                    halt = true;
                }
                else
                {
                    xmpp.Send(new Message(new Jid(JID_Receiver),
                                  MessageType.chat,
                                  outMessage));
                }

            } while (!halt);
            Console.ForegroundColor = ConsoleColor.White;

            /*
             * finally we close the connection
             * 
             */
            xmpp.Close();
        }

        static void xmpp_OnError(object sender, Exception ex)
        {
            throw new NotImplementedException();
        }

        // Is called, if the precence of a roster contact changed        
        static void xmpp_OnPresence(object sender, Presence pres)
        {
            Console.WriteLine("Available Contacts: ");
            Console.WriteLine("{0}@{1}  {2}", pres.From.User, pres.From.Server, pres.Type);
            //Console.WriteLine(pres.From.User + "@" + pres.From.Server + "  " + pres.Type);
            Console.WriteLine();
        }

        // Is raised when login and authentication is finished 
        static void xmpp_OnLogin(object sender)
        {
            _wait = false;
            Console.WriteLine("Logged In");
        }

        //Handles incoming messages
        static void MessageCallBack(object sender,
                                    agsXMPP.protocol.client.Message msg,
                                    object data)
        {
            if (msg.Body != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0}>> {1}", msg.From.User, msg.Body);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
    }
}
