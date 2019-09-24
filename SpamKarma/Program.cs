using System;
using System.IO;
using Newtonsoft.Json;
using SpamKarmaBase;

namespace SpamKarma
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            var vm = new Victim();
            BlackList bl;

            try
            {
                string json = File.ReadAllText("BlackList.json");
                bl = JsonConvert.DeserializeObject<BlackList>(json);
                Actions.CheckMail(bl, vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }

       


    }
}
