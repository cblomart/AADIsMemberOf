using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AADIsMemberOf
{
    abstract class MembershipChecker
    {
        public string TenantId;

        public string Token;

        public abstract Task<bool> IsMemberOf(string username, List<string> groups); 

        public MembershipChecker(IConfiguration configuration, string resourceurl)
        {
            TenantId = configuration["TenantId"];
            Token = Helpers.GetToken(configuration,resourceurl);
        }
    }

    class Program
    {
        static IConfiguration Configuration { get; set; }

        static void Main(string[] args)
        {
            if (args.Length < 3) {
                Console.WriteLine("Please indicate a user and a group");
                return;
            }
            // get the configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            // get the group
            var group = args[2];
            if (String.IsNullOrWhiteSpace(group))
            {
                Console.WriteLine("Please indicated a group to check");
                return;
            }
            var groups = new List<string>();
            groups.Add(group);
            // get the user
            var user = args[1];
            if (String.IsNullOrWhiteSpace(user))
            {
                Console.WriteLine("Please indicate a user to check");
                return;
            }
            // check type
            var type = args[0];
            if (String.IsNullOrWhiteSpace(type))
            {
                Console.WriteLine("Please indicate a check type: fnet, aad, graph");
                return;
            }
            // output some informations about the test
            Console.WriteLine(String.Format("Check group membership: is '{0}' in '{1}'",user, group));
            MembershipChecker checker;
            switch(type){
                case "fnet":
                    checker = new fnet.MemberhipChecker(Configuration,Constants.AADResourceUrl);
                    break;
                case "aad":
                    checker = new aad.MemberhipChecker(Configuration,Constants.AADResourceUrl);
                    break;
                case "graph":
                    checker = new graph.MemberhipChecker(Configuration,Constants.GraphresourceUrl);
                    break;
                default:
                    Console.WriteLine(String.Format("Membership checker not implemented: '{0}'",type));
                    return;
            }
            var isMemberOfTask = checker.IsMemberOf(user, groups);
            isMemberOfTask.Wait();
            if (isMemberOfTask.Result)
            {
                Console.WriteLine(String.Format("'{0}' is member of '{1}'", user, group));
            }
            else
            {
                Console.WriteLine(String.Format("'{0}' is not member of '{1}'", user, group));
            }
        }
    }
}
