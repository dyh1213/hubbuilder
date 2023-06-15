using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using ChtGPTHubBuilder.Objects;
using ChtGPTHubBuilder.Objects_HubGenration;
using static System.Net.Mime.MediaTypeNames;

namespace ChtGPTHubBuilder
{
    public class HubBuilder
    {
        // Static GraphData object that will be populated with JSON data
        public static GraphData graphData = new GraphData();

        public static string HubConceptId = "10000000-024a-44e5-8844-998342022971";
        public static string TbdParentId = "10000000-0000-0000-0000-000000000000";
        // Static dictionary mapping ListName to its ID
        public static Dictionary<ListName, string> ListIds = new Dictionary<ListName, string>()
        {
            //Major Items
            { ListName.ArtStyles, "20000000-024a-44e5-8844-998342022971" },
            { ListName.Properties, "20000000-b814-41a4-a2bd-9d346cc5ed0a" },
            { ListName.Entities, "20000000-1e9e-4e93-be44-e4b69a2c3590" },

            //Part of entities
            { ListName.UnmappedEntities, "20000000-e3f3-4b7e-bad6-c8d1255168f2" },

            //Properties
            { ListName.ArtMediums, "20000000-3467-4e75-bed2-fa727f9e2707" },
            { ListName.Environments, "20000000-70bb-4928-ac4a-6e99b9b6441f" },
            { ListName.Lightings, "20000000-069a-4b2a-849d-e1e3fddb99a0" },
            { ListName.Colors, "20000000-0327-46d9-b7d2-e80e60d56e0a" },
            { ListName.Moods, "20000000-8dca-46f4-99a4-2f1457396504" },
            { ListName.Compositions, "20000000-14b6-4367-b302-0cd4e748aea4" },
        };

        public static List<ListName> coreItems = new List<ListName>()
        {
            ListName.ArtStyles,
            ListName.Properties,
            ListName.Entities,
        };

        public static List<ListName> properties = new List<ListName>()
        {
            ListName.ArtMediums,
            ListName.Environments,
            ListName.Lightings,
            ListName.Colors,
            ListName.Moods,
            ListName.Compositions
        };

        public void Run()
        {
            string? pathToGraphData = null;
            string pathToArtConcepts = "/Users/danielyeheskel-hai/Documents/ChatGPTResponses/DEST";


            if (pathToGraphData != null)
            {
                // Load GraphData from JSON file
                string graphDataJson = File.ReadAllText(pathToGraphData);
                graphData = JsonSerializer.Deserialize<GraphData>(graphDataJson);

                foreach (var item in graphData.Lists)
                {
                    if (item.PullFromListsIds == null)
                    {
                        item.PullFromListsIds = new List<string>();
                    }
                }
            }
            else
            {
                graphData = new GraphData()
                {
                    Concepts = new List<ConceptData>(),
                    Lists = new List<ConceptListData>(),
                    Memberships = new List<MembershipData>(),
                };

                graphData.Concepts.Add(new ConceptData()
                {
                    Id = HubConceptId,
                    Title = "Midjourney Core",
                });

                graphData.Concepts.Add(new ConceptData()
                {
                    Id = TbdParentId,
                    Title = "TBD",
                });

                foreach (var item in ListIds)
                {
                    var list = new ConceptListData()
                    {
                        Id = item.Value,
                        Title = item.Key.ToString(),
                        PullFromListsIds = new List<string>()
                    };

                    if (coreItems.Contains(item.Key))
                    {
                        list.ParentConceptId = HubConceptId;
                    }
                    else
                    {
                        list.ParentConceptId = TbdParentId;
                    }

                    graphData.Lists.Add(list);
                }

                var propertyList = graphData.Lists.First(x => x.Id.Equals(ListIds[ListName.Properties]));
                foreach (var item in properties)
                {
                    propertyList.PullFromListsIds.Add(ListIds[item]);
                }

                var entitiesList = graphData.Lists.First(x => x.Id.Equals(ListIds[ListName.Entities]));

                entitiesList.PullFromListsIds.Add(ListIds[ListName.UnmappedEntities]);
            }



            // Load all ArtisticConceptResponse JSON files in the directory
            DirectoryInfo d = new DirectoryInfo(pathToArtConcepts);
            FileInfo[] Files = d.GetFiles("*.json");

            foreach (FileInfo file in Files)
            {
                string artConceptJson = File.ReadAllText(file.FullName);
                ArtisticConceptResponse conceptResponse = JsonSerializer.Deserialize<ArtisticConceptResponse>(artConceptJson);

                // Process each ArtisticConceptResponse
                HandleEntityConcept(conceptResponse);
            }

            // Optionally, write the modified GraphData back to the file
            string modifiedGraphDataJson = JsonSerializer.Serialize(graphData, new JsonSerializerOptions() { WriteIndented = true });

            if (pathToGraphData == null)
            {
                pathToGraphData = "newgraph.json";
            }

            File.WriteAllText(pathToGraphData, modifiedGraphDataJson);
        }

        private static void HandleEntityConcept(ArtisticConceptResponse conceptResponse)
        {


            string conceptName = conceptResponse.Concept_Name;
            var artConcept = conceptResponse.ArtConcept;

            string? resultingID = null;
            var isEntity = conceptResponse.Entity != null;
            if (isEntity)
            {
                string entityClass = conceptResponse.Entity.Entity_Class;
                string entityCategory = conceptResponse.Entity.Entity_Category;
                ConceptListData entityClassList = FindOrCreateList(entityClass, null, ListIds[ListName.Entities]); ; ;
                ConceptListData entityCategoryList = FindOrCreateList(entityCategory, null, entityClassList.Id);
                ConceptData concept = FindOrCreateConcept(conceptName, conceptResponse.summary, entityCategoryList.Id);
                resultingID = concept.Id;
            }
            if (conceptResponse.Entity == null)
            {
                ConceptListData list = FindOrCreateList(conceptName, conceptResponse.summary, ListIds[ListName.ArtStyles]);
                resultingID = list.Id;
                foreach (var artist in artConcept.relevant_artists)
                {
                    ConceptData concept = FindOrCreateConcept(artist, null, resultingID);
                    graphData.Memberships.Add(new MembershipData()
                    {
                        ConceptId = concept.Id,
                        ListId = ListIds[ListName.UnmappedEntities]
                    });
                }
            }
            foreach (var style in artConcept.Art_Styles)
            {
                ConceptListData list = FindOrCreateList(style, null, ListIds[ListName.ArtStyles]);
            }

            ProcessAttribiteField(isEntity, resultingID, artConcept.Medium, ListName.ArtMediums);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Environment, ListName.Environments);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Lighting, ListName.Lightings);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Color, ListName.Colors);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Mood, ListName.Moods);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Composition, ListName.Compositions);
        }

        private static void ProcessAttribiteField(bool isEntity, string? resultingID, string attributeFieldValue, ListName attributeField)
        {
            if (attributeFieldValue != null)
            {
                ConceptListData list = FindOrCreateList(attributeFieldValue, null, ListIds[attributeField]);
                if (isEntity)
                {
                    graphData.Memberships.Add(new MembershipData { ConceptId = resultingID, ListId = list.Id });
                }
                else
                {
                    if (!list.PullFromListsIds.Contains(resultingID))
                    {
                        list.PullFromListsIds.Add(resultingID);
                    }
                }
            }
        }

        private static ConceptListData FindOrCreateList(string title, string? summary, string parentListId)
        {
            ConceptListData list = graphData.Lists.FirstOrDefault(l => l.Title == title);

            if (list == null)
            {
                list = new ConceptListData
                {
                    Id = GenerateGuid(false),
                    Title = title,
                    Description = summary,
                    PullFromListsIds = new List<string>(),
                    ParentConceptId = TbdParentId
                };

                var parentListDataa = graphData.Lists.Find(l => l.Id == parentListId);
                parentListDataa.PullFromListsIds.Add(list.Id);

                graphData.Lists.Add(list);
            }
            else
            {
                if (summary != null)
                {
                    list.Description = summary;
                }
            }

            ConceptListData parentListData = graphData.Lists.Find(l => l.Id == parentListId);
            if (!parentListData.PullFromListsIds.Contains(list.Id))
            {
                parentListData.PullFromListsIds.Add(list.Id);
            }

            return list;
        }

        private static ConceptData FindOrCreateConcept(string title, string? summary, string listId)
        {
            ConceptData concept = graphData.Concepts.FirstOrDefault(c => c.Title == title);

            if (concept == null)
            {
                concept = new ConceptData
                {
                    Id = GenerateGuid(true),
                    Title = title,
                    Description = summary
                };

                graphData.Concepts.Add(concept);
            }
            else
            {
                if (summary != null)
                {
                    concept.Description = summary;
                }
            }

            if (!graphData.Memberships.Any(m => m.ConceptId == concept.Id && m.ListId == listId))
            {
                graphData.Memberships.Add(new MembershipData { ConceptId = concept.Id, ListId = listId });
            }

            return concept;
        }

        private static string GenerateGuid(bool IsConcept)
        {
            var guid = Guid.NewGuid().ToString();
            var start = (IsConcept) ? "10000000" : "20000000";
            var value = start + guid.Substring(8);

            return value;
        }
    }

    public enum ListName
    {
        ArtStyles,
        ArtMediums,
        Environments,
        Lightings,
        Colors,
        Moods,
        Compositions,
        Entities,
        UnmappedEntities,
        Properties
    }
}



