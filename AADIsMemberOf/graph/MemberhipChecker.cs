using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADIsMemberOf.graph
{
    class MemberhipChecker : MembershipChecker
    {

        override public async Task<bool> IsMemberOf(string username, List<string> groups)
        {
            // get the client
            var client = Helpers.GetGraphClient(Token);

            // check group memberships (!! max 20 groups)
            //
            // The list of group ids to check could be cached globaly as this most probably doesn't 
            // change from user to user in a complete application
            //
            var checkgroups = await client.Users[username].CheckMemberGroups(await GetGroupIdsAsync(groups)).Request().PostAsync();

            return checkgroups.Count > 0;
        }

        public async Task<List<string>> GetGroupIdsAsync(List<string> groups)
        {
            // get the client
            var client = Helpers.GetGraphClient(Token);

            // build filter
            List<string> filters = new List<string>();
            foreach(string group in groups)
            {
                filters.Add(String.Format("DisplayName eq '{0}'", group.ToLower()));
            }

            // get groups
            var aadgroups = await client.Groups.Request().Filter(String.Join(" or ", filters)).Select("Id").GetAsync();

            // get all groups into list
            List<string> groupIds = new List<string>();
            groupIds.AddRange(aadgroups.Select(g => g.Id).ToList());
            while (aadgroups.NextPageRequest != null)
            {
                aadgroups = await aadgroups.NextPageRequest.GetAsync();
                groupIds.AddRange(aadgroups.Select(g => g.Id).ToList());
            }

            // return group ids
            return groupIds;
        }

        public MemberhipChecker(IConfiguration configuration, string resourceurl) : base(configuration, resourceurl)
        {
            // nothing to do
        }
    }
}
