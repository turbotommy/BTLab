/* @(#) MQMBID sn=p750-005-150424 su=_cGGLkOp9EeSJoq1UhPFS6Q pn=samples/dotnet/cs/base/SimplePut/SimplePut.cs                                               */
/*********************************************************************/
/*    <copyright                                                   */
/*    notice="oco-source"                                          */
/*    pids="5724-H72,5724-M21,"                                    */
/*    years="2003,2012"                                            */
/*    crc="1378262472" >                                           */
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
/* Notes: The application can be used to put messages to queue       */
/*                                                                   */
/* Usage: SimplePut -q queueName [-h host -p port                    */
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
using IBM.WMQ;

namespace SimplePut
{

    /// <summary>
    /// Summary description for SimplePut.
    /// </summary>
    class SimplePut
    {
        /// <summary>
        /// Name of the host on which Queue manager is running 
        /// </summary>
        private String hostName = "localhost";
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
        private String queueManagerName = null;
        /// <summary>
        /// Queue name.
        /// </summary>
        private String queueName = null;
        /// <summary>
        /// Sample message to send
        /// </summary>
        private const String messageString = "test message";
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
            Console.WriteLine("Start of SimplePut Application\n");
            try
            {
                SimplePut simplePut = new SimplePut();
                if (simplePut.ParseCommandline(args))
                    simplePut.PutMessages();
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

            Console.WriteLine("\nEnd of SimplePut Application\n");
        }

        /// <summary>
        /// Parse commandline parameters
        /// Usage: SimplePut -q queueName [-h host -p port -l channel -n numberOfMsgs]
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
                Console.WriteLine("Exception caught in parsing command line arguments: " + e.Message);
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
            Console.WriteLine("Usage: SimplePut -q queueName [-h host -p port -l channel -n numberOfMsgs]");
            Console.WriteLine("- queueName    : a queue name");
            Console.WriteLine("- host         : hostname like 192.122.178.78. Default hostname is localhost");
            Console.WriteLine("- port         : port number like 3555. Default port is 1414");
            Console.WriteLine("- channel      : connection channel. Default is SYSTEM.DEF.SVRCONN");
            Console.WriteLine("- numberOfMsgs : The number of messages. Default is 1");
            Console.WriteLine("Ex: SimplePut -q QA");
            Console.WriteLine("    SimplePut -q B -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN");
            Console.WriteLine();
        }

        /// <summary>
        /// Put messages
        /// </summary>
        void PutMessages()
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
                queue = queueManager.AccessQueue(queueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                Console.WriteLine("done");

                // creating a message object
                message = new MQMessage();
                message.WriteString(messageString);
                
                // putting messages continuously
                for (int i = 1; i <= numberOfMsgs; i++)
                {
                    Console.Write("Message " + i + " <" + messageString + ">.. ");
                    queue.Put(message);
                    Console.WriteLine("put");
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
