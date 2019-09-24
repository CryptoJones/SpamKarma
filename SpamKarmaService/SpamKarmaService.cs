using SpamKarmaBase;
using System;
using System.IO;
using System.ServiceProcess;
using Newtonsoft.Json;

namespace SpamKarmaService
{
    public partial class SpamKarmaService : ServiceBase
    {
        public SpamKarmaService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var vm = new Victim();
            BlackList bl;

            try
            {
                string json = File.ReadAllText("SpamKarmaBlackList.json");
                bl = JsonConvert.DeserializeObject<BlackList>(json);
                Actions.CheckMail(bl, vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }

        protected override void OnStop()
        {
            Console.WriteLine("SpamKarma Service Stopped");
        }
    }
}
