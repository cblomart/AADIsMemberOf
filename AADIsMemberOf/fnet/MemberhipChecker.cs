using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADIsMemberOf.fnet
{
    class MemberhipChecker : MembershipChecker
    {

        override public async Task<bool> IsMemberOf(string username, List<string> groups)
        {
            // get the client
            var client = Helpers.GetActiveDirectoryClient(Token, TenantId);
            bool AADResult = false;

            List<IUser> usersList = null;
            IPagedCollection<IUser> searchResults = null;
            try
            {
                IUserCollection userCollection = client.Users;
                searchResults = await userCollection.Where(user => user.UserPrincipalName == username).Take(10).ExecuteAsync();
                usersList = searchResults.CurrentPage.ToList();

                if (usersList.Count == 1)
                {
                    User user = (User)usersList[0];

                    IUserFetcher signedInUserFetcher = user;
                    try
                    {
                        IPagedCollection<IDirectoryObject> pagedCollection = await signedInUserFetcher.MemberOf.ExecuteAsync();
                        do
                        {
                            List<IDirectoryObject> directoryObjects = pagedCollection.CurrentPage.ToList();
                            if (directoryObjects.Count == 0)
                            {
                                Console.WriteLine("User is not member of a group");
                                break;
                            }
                            foreach (IDirectoryObject directoryObject in directoryObjects)
                            {
                                if (directoryObject is Group)
                                {
                                    Group group = directoryObject as Group;

                                    for (int i = 0; i < groups.Count; i++)
                                    {
                                        if (groups[i].ToLower() == group.DisplayName.ToLower())
                                        {
                                            AADResult = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            pagedCollection = await pagedCollection.GetNextPageAsync();

                        } while (pagedCollection != null);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else
                {
                    Console.WriteLine(String.Format("No user found: {0}", username));
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return AADResult;
        }

        public MemberhipChecker(IConfiguration configuration, string resourceurl) : base(configuration, resourceurl)
        {
            // nothing to do
        }
    }
}
