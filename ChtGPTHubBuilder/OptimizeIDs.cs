using System;
using GraphHub.Server.GitUploader;
using GraphHub.Shared;
using Octokit;

namespace ChtGPTHubBuilder
{
	public class OptimizeIDs
	{
        const string API_kEY = "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";
        const string pullBranch = "development";

        public static GraphData RunThis()
        {
            var downloader = new GitJsonLoader(API_kEY, pullBranch);
            var OriginalData = downloader.LoadData().GetAwaiter().GetResult();
            var gotGraph = OriginalData["game-of-thrones"];
            
            int currentConcptId = 1;
            foreach (var concept in gotGraph.Concepts) {
                var oldId = concept.Id;
                var newId = currentConcptId.ToString();
                concept.Id = newId;

                foreach (var list in gotGraph.Lists)
                {
                    if (list.ParentConceptId.Equals(oldId)) {
                        list.ParentConceptId = newId;
                    }
                }
                foreach (var memberships in gotGraph.Memberships)
                {
                    if (memberships.ConceptId.Equals(oldId))
                    {
                        memberships.ConceptId = newId;
                    }
                }
                foreach (var markdown in gotGraph.ConceptMarkdown)
                {
                    if (markdown.ConceptId.Equals(oldId))
                    {
                        markdown.ConceptId = newId;
                    }
                }
                for (int i = 0; i < gotGraph.GraphInfo.FeaturedConcepts.Count; i++)
                {
                    if (gotGraph.GraphInfo.FeaturedConcepts[i].Equals(oldId))
                    {
                        gotGraph.GraphInfo.FeaturedConcepts[i] = newId;
                    }
                }
                foreach (var featuredConcept in gotGraph.GraphInfo.TopFeaturedConcepts)
                {
                    if (featuredConcept.Id.Equals(oldId))
                    {
                        featuredConcept.Id = newId;
                    }
                }
                currentConcptId++;
            }

            int currentListId = 1;
            foreach (var list in gotGraph.Lists)
            {
                var oldId = list.Id;
                var newId = currentListId.ToString();
                list.Id = newId;

                foreach (var listItem in gotGraph.Lists)
                {
                    for (int j = 0; j < (listItem.PullFromListsIds?.Count ?? 0); j++)
                    {
                        if (listItem.PullFromListsIds[j].Equals(oldId))
                        {
                            listItem.PullFromListsIds[j] = newId;
                        }
                    }
                }
                foreach (var memberships in gotGraph.Memberships)
                {
                    if (memberships.ListId.Equals(oldId))
                    {
                        memberships.ListId = newId;
                    }
                }
                foreach (var markdown in gotGraph.ConceptMarkdown)
                {
                    if (markdown.ConceptId.Equals(oldId))
                    {
                        markdown.ConceptId = newId;
                    }
                }
                for (int i = 0; i < gotGraph.GraphInfo.FeaturedLists.Count; i++)
                {
                    if (gotGraph.GraphInfo.FeaturedLists[i].Equals(oldId))
                    {
                        gotGraph.GraphInfo.FeaturedLists[i] = newId;
                    }
                }
                foreach (var featuredList in gotGraph.GraphInfo.TopFeaturedLists)
                {
                    if (featuredList.Id.Equals(oldId))
                    {
                        featuredList.Id = newId;
                    }
                }
                currentListId++;
            }

            return gotGraph;
        }
	}
}
