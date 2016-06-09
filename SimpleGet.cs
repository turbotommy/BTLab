/* @(#) samples/dotnet/cs/MQSamples/SimpleGet/SimpleGet.cs, dotnet, p000 1.8 11/07/13 06:33:24                                               */
/*********************************************************************/
/*    <copyright                                                   */
/*    notice="oco-source"                                          */
/*    pids="5724-H72,5724-M21,"                                    */
/*    years="2003,2012"                                            */
/*    crc="1161593733" >                                           */
/*   IBM Confidential                                              */
/*                                                                 */
/*   OCO Source Materials                                          */
/*                                                                 */
/*   5724-H72,5724-M21,                                            */
/*                                                                 */
/*   (C) Copyright IBM Corp. 2003, 2012                            */
/*                                                                 */
/*   The source code for the program is not published              */
/*   or otherwise divested of its trade secrets,                   */
/*   irrespective of what has been deposited with the              */
/*   U.S. Copyright Office.                                        */
/*    </copyright>                                                   */
/*                                                                   */
/*********************************************************************/
/*                                                                   */
/* A simple application to demonstrate putting of messages.          */
/*                                                                   */
/* Notes: The application can be used to get messages from queue     */
/*                                                                   */
/* Usage: SimpleGet -q queueName [-h host -p port                    */
/*                          -l channel -n numberOfMsgs]              */
/*                                                                   */
/* - queueName      : name of a queue                                */
/* - host           : hostname                                       */
/* - port           : port number                                    */
/* - channel        : connection channel                             */
/* - numberOfMsgs   : number of messages                             */
/*                                                                   */
/* Provider type: WebSphere MQ                                       */
/*                                                                   */
/*                                                                   */
/*********************************************************************/

using System;
using System.Collections;
using System.Threading;
using IBM.WMQ;
using System.IO;

namespace SimpleGet
{

    /// <summary>
    /// Summary description for SimpleGet.
    /// </summary>
    class SimpleGet
    {
        /// <summary>
        /// Name of the host on which Queue manager is running 
        /// </summary>
        private String hostName = "xelpg-s-btsqi1d.xe.abb.com";
        /// <summary>
        /// Port number on which Queue manager is listening
        /// </summary>
        private int port = 1414;
        /// <summary>
        /// Name of the channel
        /// </summary>
        private String channelName = "SYSTEM.DEF.SVRCONN";
        /// <summary>
        /// Name of the Queue manager to connect to
        /// </summary>
        private String queueManagerName = "XEABB024T";
        /// <summary>
        /// Queue name.
        /// </summary>
        private String queueName = "QA.CCP.FROM.BIZTALK";
        /// <summary>
        /// Number of messages to be put
        /// </summary>
        private int numberOfMsgs = 1;

        /// <summary>
        /// Variables
        ///</summary>
        private MQQueueManager queueManager;
        private MQQueue queue;
        private Hashtable properties;
        private MQMessage message;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Console.WriteLine("Start of SimpleGet Application\n");
            try
            {
                SimpleGet simpleGet = new SimpleGet();
                //if (simpleGet.ParseCommandline(args))
                    simpleGet.GetMessages();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Invalid arguments!\n{0}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex);
                Console.WriteLine("Sample execution FAILED!");
            }

            Console.WriteLine("\nEnd of SimpleGet Application\n");
        }

        /// <summary>
        /// Parse commandline parameters
        /// Usage: SimpleGet -q queueName [-h host -p port -l channel -n numberOfMsgs]
        /// </summary>
        /// <param name="args"></param>
        bool ParseCommandline(string[] args)
        {
            String token;
            int flag = 0;

            try
            {
                if (args.Length < 2 || args.Length % 2 == 1)
                {
                    DisplayHelp();
                    return false;
                }

                for (int argIndex = 0; argIndex < args.Length; argIndex++)
                {
                    // Get the token
                    token = (String)args.GetValue(argIndex);

                    switch (token)
                    {
                        case "-q":
                            // queue name
                            queueName = (String)args.GetValue(argIndex + 1);
                            argIndex++;
                            flag = 1;
                            break;

                        case "-h":
                            // host name
                            hostName = (String)args.GetValue(argIndex + 1);
                            argIndex++;
                            continue;

                        case "-p":
                            // port name
                            port = Convert.ToInt32((String)args.GetValue(argIndex + 1));
                            argIndex++;
                            continue;

                        case "-l":
                            // channel name
                            channelName = (String)args.GetValue(argIndex + 1);
                            argIndex++;
                            break;

                        case "-n":
                            // number of messages
                            numberOfMsgs = Convert.ToInt32((String)args.GetValue(argIndex + 1));
                            argIndex++;
                            break;

                        default:
                            flag = -1;
                            break;
                    }
                }

                if (flag != 1)
                {
                    DisplayHelp();
                    return false;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exeption caught in parsing command line arguments: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Display Help
        /// </summary>
        void DisplayHelp()
        {
            Console.WriteLine("Usage: SimpleGet -q queueName [-h host -p port -l channel -n numberOfMsgs]");
            Console.WriteLine("- queueName    : a queue name");
            Console.WriteLine("- host         : hostname like 192.122.178.78. Default hostname is localhost");
            Console.WriteLine("- port         : port number like 3555. Default port is 1414");
            Console.WriteLine("- channel      : connection channel. Default is SYSTEM.DEF.SVRCONN");
            Console.WriteLine("- numberOfMsgs : The number of messages. Default is 1");
            Console.WriteLine("Ex: SimpleGet -q QA");
            Console.WriteLine("    SimpleGet -q B -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN");
            Console.WriteLine();
        }

        /// <summary>
        /// Get messages
        /// </summary>
        void GetMessages()
        {
            try
            {
                // mq properties
                properties = new Hashtable();
                properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                properties.Add(MQC.HOST_NAME_PROPERTY, hostName);
                properties.Add(MQC.PORT_PROPERTY, port);
                properties.Add(MQC.CHANNEL_PROPERTY, channelName);

                // display all details
                Console.WriteLine("MQ Parameters");
                Console.WriteLine("1) queueName = " + queueName);
                Console.WriteLine("2) host = " + hostName);
                Console.WriteLine("3) port = " + port);
                Console.WriteLine("4) channel = " + channelName);
                Console.WriteLine("5) numberOfMsgs = " + numberOfMsgs);
                Console.WriteLine("");

                // create connection
                Console.Write("Connecting to queue manager.. ");
                queueManager = new MQQueueManager(queueManagerName, properties);
                Console.WriteLine("done");

                // accessing queue
                Console.Write("Accessing queue " + queueName + ".. ");
                queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING);
                Console.WriteLine("done");

                // getting messages continuously
                for (int i = 1; i <= numberOfMsgs; i++)
                {
                    // creating a message object
                    message = new MQMessage();

                    try
                    {
                        queue.Get(message);
                        string sMsg=message.ReadString(message.MessageLength);
                        Console.WriteLine("Message " + i + " got = " + sMsg);
                        File.WriteAllText(@"C:\Temp\" + message.MessageId + ".msg", sMsg);
                        message.ClearMessage();
                    }
                    catch (MQException mqe)
                    {
                        if (mqe.ReasonCode == 2033)
                        {
                            Console.WriteLine("No message available");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message);
                            break;
                        }
                    }
                }

                // closing queue
                Console.Write("Closing queue.. ");
                queue.Close();
                Console.WriteLine("done");

                // disconnecting queue manager
                Console.Write("Disconnecting queue manager.. ");
                queueManager.Disconnect();
                Console.WriteLine("done");
            }

            catch (MQException mqe)
            {
                Console.WriteLine("");
                Console.WriteLine("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message);
                Console.WriteLine(mqe.StackTrace);
            }
        }
    }
}
