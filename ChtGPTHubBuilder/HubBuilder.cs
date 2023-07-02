using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using ChtGPTHubBuilder.Objects;
using GraphHub.Server.GitUploader;
using GraphHub.Shared;
using static System.Net.Mime.MediaTypeNames;

namespace ChtGPTHubBuilder
{
    public class HubBuilder
    {
        // Static GraphData object that will be populated with JSON data
        public static GraphData graphData = new GraphData();

        public const string HubConceptId = "10000000-024a-44e5-8844-998342022971";
        public const string TbdParentId = "10000000-0000-0000-0000-000000000000";
        // Static dictionary mapping ListName to its ID

        public void Run()
        {
            string? pathToGraphDataSource = null;
            //string? pathToGraphData = null;
            string pathToArtConcepts = "/Users/danielyeheskel-hai/Documents/ChatGPTResponses/DEST";


            if (pathToGraphDataSource != null)
            {
                // Load GraphData from JSON file
                string graphDataJson = File.ReadAllText(pathToGraphDataSource);
                graphData = JsonSerializer.Deserialize<GraphData>(graphDataJson);

                foreach (var item in graphData.Lists)
                {
                    if (item.PullFromListsIds == null)
                    {
                        item.PullFromListsIds = new List<string>();
                    }
                }
            }

            else Intitilize();

            // Load all ArtisticConceptResponse JSON files in the directory
            DirectoryInfo d = new DirectoryInfo(pathToArtConcepts);
            FileInfo[] Files = d.GetFiles("*.json");

            foreach (FileInfo file in Files.OrderBy(x=>x.CreationTime))
            {
                string artConceptJson = File.ReadAllText(file.FullName);
                ArtisticConceptResponse conceptResponse = JsonSerializer.Deserialize<ArtisticConceptResponse>(artConceptJson);

                // Process each ArtisticConceptResponse
                HandleEntityConcept(conceptResponse);
            }

            GitJsonSaver saver = new GitJsonSaver("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");
            saver.UpdateGraph(graphData).GetAwaiter().GetResult();
        }

        private static void Intitilize()
        {
            graphData = new GraphData()
            {
                Concepts = new List<ConceptData>(),
                Lists = new List<ConceptListData>(),
                Memberships = new List<MembershipData>(),
                ConceptMarkdown = new List<ConceptMarkdown>(),
            };

            graphData.Concepts.Add(new ConceptData()
            {
                Id = HubConceptId,
                Title = "Art Styles Knowledge Graph",
                Description = "This is a large community built graph of all different types of art styles. This map helps you get better at using AI image generators like Midjourney, Doll-E, Stable Diffusion, and more. It also shows you famous artists and characters, and the special art styles they're known for"
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
                    switch (item.Key)
                    {
                        case ListName.ArtStyles:
                            list.Title = "Art Styles";
                            list.Description = "A list that contains all art styles pulled from other lists related to specific domains like Painting, Architercture, Graphic Design etc.";
                            break;
                        case ListName.Properties:
                            list.Title = "Art Properties";
                            list.Description = "A list that contains artisitc concepts like color, lighting, moods and more that can be used as additional information for a prompt.";
                            break;
                        case ListName.Entities:
                            list.Title = "Art Entities";
                            list.Description = "A collection of entities that have a definitive art style, this could be artists from a spefici field or other entities like tv-shows, games, studios etc.";
                            break;
                    }
                }
                else if (properties.Contains(item.Key))
                {
                    var parentConcept = new ConceptData()
                    {
                        Id = GenerateGuid(true),
                        Title = item.Key.ToString().Remove(item.Key.ToString().Length - 1, 1),
                    };
                    graphData.Concepts.Add(parentConcept);

                    switch (item.Key)
                    {
                        case ListName.ArtMediums:
                            list.Title = "Art Mediums";
                            list.Description = "eces. This can range from conventional mediums like oil, acrylic, watercolor, charcoal, to more contemporary ones like digital, mixed media, or installations.";
                            break;
                        case ListName.Environments:
                            list.Title = "Environments";
                            list.Description = "AThis is a catalogue of different environmental settings within art. This could include forests, cityscapes, seascapes, deserts, or abstract backgrounds, depicting the context and backdrop of the artwork.";
                            break;
                        case ListName.Lightings:
                            list.Title = "Lighting Styles";
                            list.Description = "The lightings list explores different types of illumination used in an artwork. This could be soft morning light, harsh midday light, artificial light, or magical surrealistic light, playing crucial roles in the art's ambiance and focus.";
                            break;
                        case ListName.Colors:
                            list.Title = "Colors";
                            list.Description = "This list describes the myriad of colors artists employ in their work. From primary, secondary, and tertiary hues, to monochrome or vibrant palettes, colors breathe life and emotion into the artwork.";
                            break;
                        case ListName.Moods:
                            list.Title = "Moods";
                            list.Description = "The moods list represents the range of emotions or feelings an artwork can evoke, like happiness, sadness, tranquility, mystery, anger or nostalgia. It's about the emotional resonance of an artwork.";
                            break;
                        case ListName.Compositions:
                            list.Title = "Compositions";
                            list.Description = "This list details different compositional techniques used in art. These may include rules like the Golden Ratio, Rule of Thirds, Symmetry or Asymmetry, leading lines, or framing, influencing the viewer's focus and the overall balance of the piece.";
                            break;
                    }

                    list.ParentConceptId = parentConcept.Id;
                }
                else
                {
                    list.ParentConceptId = TbdParentId;
                }

                graphData.Lists.Add(list);

                var metadata = new GraphHub.Database.Dto.GraphInfo()
                {
                    GraphId = "100",
                    GraphDisplayName = "Text To Image",
                    GraphUrlName = "text-to-image",
                    GraphGitHubDatabaseUrl = "https://github.com/dyh1213/graphhub.data/tree/main/graphs_current/text-to-image",
                    RootConcept = "10000000-024a-44e5-8844-998342022971"
                };

                graphData.GraphInfo = metadata;
            }

            var propertyList = graphData.Lists.First(x => x.Id.Equals(ListIds[ListName.Properties]));
            foreach (var item in properties)
            {
                propertyList.PullFromListsIds.Add(ListIds[item]);
                var propertyConcept = graphData.Lists.First(x => x.Id.Equals(ListIds[item]));
                graphData.Memberships.Add(new MembershipData()
                {
                    ConceptId = propertyConcept.ParentConceptId,
                    ListId = propertyList.Id
                });
            }

            var entitiesList = graphData.Lists.First(x => x.Id.Equals(ListIds[ListName.Entities]));

            entitiesList.PullFromListsIds.Add(ListIds[ListName.UnmappedEntities]);
        }

        private static void HandleEntityConcept(ArtisticConceptResponse conceptResponse)
        {


            string conceptName = conceptResponse.Concept_Name;
            if (conceptName.Equals("Syd Mead"))
            {
                int i = 5;
            }

            var artConcept = conceptResponse.ArtConcept;

            string? resultingID = null;
            var isEntity = conceptResponse.Entity != null;
            if (isEntity)
            {
                string entityClass = conceptResponse.Entity.Entity_Class;
                string entityCategory = conceptResponse.Entity.Entity_Category;
                ConceptListData entityClassList = FindOrCreateList(entityClass, null, ListIds[ListName.Entities]);
                ConceptListData entityCategoryList = FindOrCreateList(entityCategory, null, entityClassList.Id);
                ConceptData concept = FindOrCreateConcept(conceptName, conceptResponse.summary, entityCategoryList.Id);
                //Check if it was previously created as a stub and remove that stub
                resultingID = concept.Id;
            }
            if (conceptResponse.Entity == null)
            {
                ConceptData concept = FindOrCreateConcept(conceptName, conceptResponse.summary, ListIds[ListName.ArtStyles]);
                //ConceptListData list = FindOrCreateList(conceptName + " Styles", conceptResponse.summary, ListIds[ListName.ArtStyles], concept.Id);
                resultingID = concept.Id;
                if (artConcept.relevant_artists != null)
                {
                    foreach (var artist in artConcept.relevant_artists)
                    {
                        ConceptData artistConcept = FindOrCreateConcept(artist, null, ListIds[ListName.UnmappedEntities]);
                    }
                }
            }

            foreach (var style in artConcept.Art_Styles)
            {
                ConceptData concept = FindOrCreateConcept(style, null, ListIds[ListName.ArtStyles]);
                //ConceptListData list = FindOrCreateList(style + " Styles", null, ListIds[ListName.ArtStyles], concept.Id);
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
                ConceptData concept = FindOrCreateConcept(attributeFieldValue, null, ListIds[attributeField]);
                //ConceptListData list = FindOrCreateList(attributeFieldValue + "Styles", null, ListIds[attributeField], concept.Id);

                if (isEntity)
                {
                    //graphData.Memberships.Add(new MembershipData { ConceptId = resultingID, ListId = list.Id });
                }
                else
                {
                    //if (!list.PullFromListsIds.Contains(resultingID))
                    //{
                    //    list.PullFromListsIds.Add(resultingID);
                    //}
                }
            }
        }

        private static ConceptListData FindOrCreateList(string title, string? summary, string parentListId, string parentConceptId = TbdParentId)
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

                //Remove Stubs
                var checkIsStubExists = graphData.Memberships.FirstOrDefault(m => m.ConceptId == concept.Id && m.ListId == ListIds[ListName.UnmappedEntities]);
                if (checkIsStubExists != null)
                {
                    graphData.Memberships.Remove(checkIsStubExists);
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


        public static Dictionary<string, string> Markdown = new Dictionary<string, string>()
        {
            //Major Items
            {HubConceptId, "20000000-024a-44e5-8844-998342022971" },
        };

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



