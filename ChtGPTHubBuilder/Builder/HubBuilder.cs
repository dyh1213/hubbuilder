using System.Globalization;
using System.Text.Json;
using ChtGPTHubBuilder.Objects;
using GraphHub.Shared;
using static ChtGPTHubBuilder.Builder.Constants;

namespace ChtGPTHubBuilder
{
    public class HubBuilder
    {
        public GraphData graphData = new GraphData();

        public GraphData Run()
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
            else
            {
                graphData = ChtGPTHubBuilder.Builder.Constants.Intitilize();
            }

            var isValidInit = ChtGPTHubBuilder.Builder.DataValidation.ValidateGraphData(graphData);
            if (!isValidInit)
            {
                Console.WriteLine("Graph is not valid init");
                throw new Exception("Graph is not valid init");
            }

            // Load all ArtisticConceptResponse JSON files in the directory
            DirectoryInfo d = new DirectoryInfo(pathToArtConcepts);
            FileInfo[] Files = d.GetFiles("*.json");
            
            var totalCount = Files.Count();
            var currentCount = 1;
            foreach (FileInfo file in Files.OrderBy(x=>x.CreationTime))
            {
                string artConceptJson = File.ReadAllText(file.FullName);
                ArtisticConceptResponse conceptResponse = JsonSerializer.Deserialize<ArtisticConceptResponse>(artConceptJson);

                Console.WriteLine($"Processing item {currentCount}/{totalCount}");
                Console.WriteLine($"Item Name {conceptResponse.Concept_Name}");
                Console.WriteLine($"Item ID {conceptResponse.Id}");

                ChtGPTHubBuilder.Builder.DataCleaning.ProcessTitleCase(file, conceptResponse);

                if (conceptResponse.Id == null)
                {
                    conceptResponse.Id = currentCount.ToString();
                    string updatedJson = JsonSerializer.Serialize(conceptResponse, new JsonSerializerOptions() { WriteIndented = true });
                    File.WriteAllText(file.FullName, updatedJson);
                }

                // Process each ArtisticConceptResponse
                HandleEntityConcept(conceptResponse);

                var isValid = ChtGPTHubBuilder.Builder.DataValidation.ValidateGraphData(graphData);
                if (!isValid)
                {
                    Console.WriteLine("Graph is no longer valid");
                    throw new Exception("Graph is no longer valid");
                }


                currentCount++;
            }

            return graphData;
        }

        private void HandleEntityConcept(ArtisticConceptResponse conceptResponse)
        {
            string conceptName = conceptResponse?.Concept_Name;

            var artConcept = conceptResponse.ArtConcept;

            string? resultingID = null;
            var isEntity = conceptResponse.Entity != null;
            if (isEntity)
            {
                Console.WriteLine("Processing as Entity");
                string entityClass = conceptResponse.Entity.Entity_Class;
                string entityCategory = conceptResponse.Entity.Entity_Category;
                ConceptListData entityClassList = FindOrCreateList(entityClass, null, ListIds[ListName.Entities]);
                ConceptListData entityCategoryList = FindOrCreateList(entityCategory, null, entityClassList.Id);
                ConceptData concept = FindOrCreateConcept(conceptName, conceptResponse.summary, entityCategoryList.Id, "MAIN_ENTITY",conceptResponse.Id);
                //Check if it was previously created as a stub and remove that stub
                resultingID = concept.Id;
            }
            else
            {
                Console.WriteLine("Processing as Art Concept");
                ConceptData concept = FindOrCreateConcept(conceptName, conceptResponse.summary, ChtGPTHubBuilder.Builder.Constants.ListIds[ListName.ArtStyles], "MAIN_ARTSTYLE", conceptResponse.Id);
                //ConceptListData list = FindOrCreateList(conceptName + " Styles", conceptResponse.summary, ListIds[ListName.ArtStyles], concept.Id);
                resultingID = concept.Id;
                if (artConcept.relevant_artists != null)
                {
                    foreach (var artist in artConcept.relevant_artists)
                    {
                        ConceptData artistConcept = FindOrCreateConcept(artist, null, ListIds[ListName.UnmappedEntities], "ARTIST");
                    }
                }
            }

            foreach (var style in artConcept.Art_Styles)
            {
                ConceptData concept = FindOrCreateConcept(style, null, ListIds[ListName.ArtStyles], "STYLE");
                //ConceptListData list = FindOrCreateList(style + " Styles", null, ListIds[ListName.ArtStyles], concept.Id);
            }

            ProcessAttribiteField(isEntity, resultingID, artConcept.Medium, ListName.ArtMediums);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Environment, ListName.Environments);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Lighting, ListName.Lightings);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Color, ListName.Colors);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Mood, ListName.Moods);
            ProcessAttribiteField(isEntity, resultingID, artConcept.Composition, ListName.Compositions);
        }

        private void ProcessAttribiteField(bool isEntity, string? resultingID, string attributeFieldValue, ListName attributeField)
        {
            if (attributeFieldValue != null)
            {
                ConceptData concept = FindOrCreateConcept(attributeFieldValue, null, ListIds[attributeField], attributeField.ToString());
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

        private ConceptListData FindOrCreateList(string title, string? summary, string parentListId, string parentConceptId = TbdParentId)
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
                Console.WriteLine($"Created a new list {list.Title} ({list.Id}): Added it as a pull to {parentListDataa.Title} ({parentListDataa.Id})");
            }
            else
            {
                if (summary != null)
                {
                    list.Description = summary;
                }
                Console.WriteLine($"UPDATED LIST SUMMARY");
            }

            ConceptListData parentListData = graphData.Lists.Find(l => l.Id == parentListId);
            if (!parentListData.PullFromListsIds.Contains(list.Id))
            {
                parentListData.PullFromListsIds.Add(list.Id);
                Console.WriteLine($"CREATED A NEW PULL FROM LIST");
            }

            return list;
        }

        private ConceptData FindOrCreateConcept(string title, string? summary, string listId, string type, string conceptId = null)
        {
            ConceptData concept = graphData.Concepts.FirstOrDefault(c => c.Title == title);

            if (concept == null)
            {
                concept = new ConceptData
                {
                    Id = conceptId == null ? GenerateGuid(true) : conceptId,
                    Title = title,
                    Description = summary
                };

                graphData.Concepts.Add(concept);

                Console.WriteLine($"Create a new {type} concept: {concept.Title}({concept.Id})");

                graphData.Memberships.Add(new MembershipData { ConceptId = concept.Id, ListId = listId });

                Console.WriteLine($"Created new membership {type}: concept={concept.Id}, list={listId}");
            }
            else
            {
                Console.WriteLine($"Found existing {type} concept: {concept.Title}({concept.Id})");
                //Remove Stubs
                var checkIsStubExists = graphData.Memberships.FirstOrDefault(m => m.ConceptId == concept.Id && m.ListId == ListIds[ListName.UnmappedEntities]);
                if (checkIsStubExists != null)
                {
                    graphData.Memberships.Remove(checkIsStubExists);
                    Console.WriteLine($"Removed stub membership for {type}: {concept.Title}({concept.Id})");
                }

                //update memberships
                if (conceptId != null) {
                    var membershipsa = graphData.Memberships.Where(m => m.ConceptId == concept.Id);
                    foreach(var mem in membershipsa) {
                        Console.WriteLine($"Updated membership ID {type}: {mem.ConceptId}");
                        mem.ConceptId = conceptId;  
                    }
                    concept.Id = conceptId;
                }

                if (summary != null)
                {
                    concept.Description = summary;
                }

                if (!graphData.Memberships.Any(m => m.ConceptId == concept.Id && m.ListId == listId))
                {
                    graphData.Memberships.Add(new MembershipData { ConceptId = concept.Id, ListId = listId });
                    Console.WriteLine($"Created new membership {type}: concept={concept.Id}, list={listId}");
                }
            }

            return concept;
        }

        private string GenerateGuid(bool IsConcept)
        {
            //var guid = Guid.NewGuid().ToString();
            var start = (IsConcept) ? graphData.Concepts.Count() + 1000 : graphData.Lists.Count() + 1000;
            //var value = start + guid.Substring(8);

            return start.ToString();
        }


        
    }




}



