using System;
using GraphHub.Shared;

namespace ChtGPTHubBuilder.Builder
{
	public static class DataValidation
	{
        public static bool ValidateGraphData(GraphData graphData)
        {
            var concepts = graphData.Concepts.Select(x => x.Id).ToHashSet();
            var lists = graphData.Lists.Select(x => x.Id).ToHashSet();

            //validate relationships
            foreach (var membership in graphData.Memberships)
            {
                var containsConcept = concepts.Contains(membership.ConceptId);
                var containsList = lists.Contains(membership.ListId);
                if (!containsConcept || !containsList)
                {
                    LogMembershipError(membership.ConceptId, membership.ListId, graphData);
                    return false;
                }
            }

            foreach (var list in graphData.Lists)
            {
                if (list.ParentConceptId != null)
                {
                    var containsParentConcept = concepts.Contains(list.ParentConceptId);
                    if (!containsParentConcept)
                    {
                        Console.WriteLine($"Missing parent concept {list.ParentConceptId} in list {list.Title} ({list.Id})");
                        return false;
                    }
                }
                if (list.PullFromListsIds != null)
                {
                    foreach (var pullfromlist in list.PullFromListsIds)
                    {
                        var containsPullList = lists.Contains(pullfromlist);
                        if (!containsPullList)
                        {
                            LogPullListError(list.Id, pullfromlist, graphData);
                            return false;
                        }

                    }
                }
            }

            return true;
        }

        public static void LogMembershipError(string coneptId, string listId, GraphData graphData)
        {
            var concept = graphData.Concepts.FirstOrDefault(x => x.Id.Equals(coneptId));
            var list = graphData.Lists.FirstOrDefault(x => x.Id.Equals(listId));
            Console.WriteLine($"ERROR: Could not find member {concept?.Title}({coneptId}) in list {list?.Title}({listId}) ");
        }

        public static void LogPullListError(string pullerId, string pulledId, GraphData graphData)
        {
            var pullerList = graphData.Lists.FirstOrDefault(x => x.Id.Equals(pullerId));
            var pulledList = graphData.Lists.FirstOrDefault(x => x.Id.Equals(pulledId));
            Console.WriteLine($"ERROR: Could not find pulled list {pulledList?.Title}({pulledList?.Id}) in LISTS! Puller is {pullerList?.Title}({pullerList?.Id}) ");
        }

    }
}

