using System;
using System.Windows.Forms;
using NServiceBus;
using Grid;
using NServiceBus.Grid.MessageHandlers;
using NServiceBus.Grid.Messages;
using NServiceBus.Config;
using ObjectBuilder;
using System.Reflection;
using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
using Common.Logging;
using System.Collections.Generic;

namespace UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ObjectBuilder.SpringFramework.Builder builder = new ObjectBuilder.SpringFramework.Builder();

            string nameSpace = System.Configuration.ConfigurationManager.AppSettings["NameSpace"];
            string serialization = System.Configuration.ConfigurationManager.AppSettings["Serialization"];

            switch (serialization)
            {
                case "xml":
                    builder.ConfigureComponent<MessageMapper>(ComponentCallModelEnum.Singleton);
                    builder.ConfigureComponent<NServiceBus.Serializers.XML.MessageSerializer>(ComponentCallModelEnum.Singleton)
                        .Namespace = nameSpace;
                    
                    List<Type> additionalTypes = new List<Type>{
                        typeof(ChangeNumberOfWorkerThreadsMessage),
                        typeof(GetNumberOfWorkerThreadsMessage),
                        typeof(GotNumberOfWorkerThreadsMessage)
                    };

                    builder.ConfigureComponent<NServiceBus.Serializers.XML.MessageSerializer>(ComponentCallModelEnum.Singleton)
                        .AdditionalTypes = additionalTypes;
                    break;
                case "binary":
                    builder.ConfigureComponent<NServiceBus.Serializers.Binary.MessageSerializer>(ComponentCallModelEnum.Singleton);
                    break;
                default:
                    throw new ConfigurationException("Serialization can only be either 'xml', or 'binary'.");
            }

            NServiceBus.Config.Configure.With(builder)
                .MsmqTransport()
                    .IsTransactional(false)
                    .PurgeOnStartup(false)
                .UnicastBus()
                    .ImpersonateSender(false);

            IBus bClient = builder.Build<IBus>();


            bClient.Start();

            Manager.SetBus(bClient);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 f = new Form1();

            f.Closed += delegate { bClient.Dispose(); };

            Application.Run(f);
        }
    }
}