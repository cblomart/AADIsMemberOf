using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADIsMemberOf.aad
{
    class MemberhipChecker : MembershipChecker
    {
        public override async Task<bool> IsMemberOf(string username, List<string> groups)
        {
            // get the client
            var client = Helpers.GetActiveDirectoryClient(Token, TenantId);

            // get the user
            var user = await client.Users.GetByObjectId(username).ExecuteAsync();

            // check group memberships (!! max 20 groups)
            //
            // The list of group ids to check could be cached globaly as this most probably doesn't 
            // change from user to user in a complete application
            //
            var groupmember = await user.CheckMemberGroupsAsync(GetGroupIds(groups));

            return groupmember.Count() > 0;
        }

        public List<string> GetGroupIds(List<string> groups)
        {  
            // get the client
            var client = Helpers.GetActiveDirectoryClient(Token, TenantId);

            // get a list of the group ids
            List<Task<IGroup>> getGroupTasks = new List<Task<IGroup>>();
            foreach (string group in groups)
            {
                getGroupTasks.Add(
                    client.Groups.Where(g => group.Equals(g.DisplayName, StringComparison.InvariantCultureIgnoreCase)
                ).ExecuteSingleAsync());
            }
            Task.WaitAll(getGroupTasks.ToArray());
            return getGroupTasks.Select(t => t.Result.ObjectId).ToList();
        }

        public MemberhipChecker(IConfiguration configuration, string resourceurl) : base(configuration, resourceurl)
        {
            // nothing to do
        }
    }
}
