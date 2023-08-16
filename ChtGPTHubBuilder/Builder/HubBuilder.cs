using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using ChtGPTHubBuilder.Objects;
using GraphHub.Server.GitUploader;
using GraphHub.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octokit;
using static ChtGPTHubBuilder.Builder.Constants;

namespace ChtGPTHubBuilder
{
    public class HubBuilder
    {
        public GraphData graphData = new GraphData();

        public async Task<GraphData> Run()
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

                //Create the appropriate lists for properties styles and add them under the relevant list.
                //Using weight 3 to target those items
                foreach(var concept in graphData.Concepts.Where(x=>x.Weight == 3))
                {
                    var conceptProcessed = CreateOrUpdateConcept(concept.Title, null, false, ArtMediumsListName);
                }
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

            //LOAD AND CLEAN UP DATA
            var fileData = new List<ArtisticConceptResponse>();

            foreach (FileInfo file in Files.OrderBy(x => x.CreationTime))
            {
                var text = File.ReadAllText(file.FullName);
                var data = JsonSerializer.Deserialize<ArtisticConceptResponse>(text);

                if (data.Id == null)
                {
                    data.Id = Guid.NewGuid().ToString();
                    string updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });
                    File.WriteAllText(file.FullName, updatedJson);
                }

                data = Builder.DataCleaning.ProcessTitleCase(file, data);

                fileData.Add(data);
            }

            //todo GET THE DATA FROM GIT AND MERGE IMAGES IN
            var githubDownloader = new GitJsonLoader("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", "main");
            githubDownloader.limitGraphs = new List<string>() {"text-to-image"};
            var githubgraph = await githubDownloader.LoadData();
            var tti = githubgraph.First().Value;

            var initialConcepts = fileData.Select(x =>
            {
                var matchingItem = tti.Concepts.FirstOrDefault(p => p.Title.Equals(x.Concept_Name));

                return new ConceptData()
                {
                    Id = x.Id,
                    Title = x.Concept_Name,
                    Image = matchingItem?.Image,
                    Weight = matchingItem?.Weight
                };
            }).ToList();

            graphData.Concepts.AddRange(initialConcepts);

            foreach (var artisticConcpt in fileData)
            {
                Console.WriteLine($"Processing item {currentCount}/{totalCount}");
                Console.WriteLine($"Item Name {artisticConcpt.Concept_Name}");
                Console.WriteLine($"Item ID {artisticConcpt.Id}");

                if (artisticConcpt.Id.Equals("272"))
                {
                    var i = 5; ;
                }

                // Process each ArtisticConceptResponse
                HandleEntityConcept(artisticConcpt);

                var item = graphData.Concepts.FirstOrDefault(x => x.Id == "272");

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
                var entityClassConcept = CreateOrUpdateConcept(entityClass, null, false, ArtEntitiesListName);
                var entityCategoryConcept = CreateOrUpdateConcept(entityCategory, null, false, StylesTitle(entityClassConcept.Title));
                var concept = CreateOrUpdateConcept(conceptName, conceptResponse.summary, true, memberOfList: StylesTitle(entityCategoryConcept.Title));
            }
            else
            {
                var concept = CreateOrUpdateConcept(conceptName, conceptResponse.summary, false, ArtStylesListName);

                if (artConcept.relevant_artists != null)
                {
                    foreach (var artist in artConcept.relevant_artists)
                    {
                        var artistConcept = CreateOrUpdateConcept(artist, null, isEntity: true, memberOfList: ArtistTitle(concept.Title));
                    }
                }
            }

            foreach (var style in artConcept.Art_Styles)
            {
                var concept = CreateOrUpdateConcept(style, null, false, ArtStylesListName);
            }

            var ArtMediums = CreateOrUpdateConcept(artConcept.Medium, null, true, memberOfList: StylesTitle(ArtPropertyName_Medium));
            var Environments = CreateOrUpdateConcept(artConcept.Environment, null, true, memberOfList: StylesTitle(ArtPropertyName_Environment));
            var Lightings = CreateOrUpdateConcept(artConcept.Lighting, null, true, memberOfList: StylesTitle(ArtPropertyName_Lighting));
            var Colors = CreateOrUpdateConcept(artConcept.Color, null, true, memberOfList: StylesTitle(ArtPropertyName_Color));
            var Moods = CreateOrUpdateConcept(artConcept.Mood, null, true, memberOfList: StylesTitle(ArtPropertyName_Mood));
            var Compositions = CreateOrUpdateConcept(artConcept.Composition, null, true, memberOfList: StylesTitle(ArtPropertyName_Composition));
        }

        private ConceptData? CreateOrUpdateConcept(string? title, string? summary, bool isEntity, string? stylesParentName = null, string? memberOfList = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }

            ConceptData concept = CreateOrUpdateConceptInternal(title, summary, memberOfListName: memberOfList);

            var propts = CreateOrUpdateListInternal(concept.Id, PromptsTitle(title), $"List of prompts using the concept '{title}'. {concept.Description}", overrideID: concept.Id+"-P");

            if (!isEntity)
            {
                var usage = CreateOrUpdateListInternal(concept.Id, ArtistTitle(title), $"List of artists or entities related to the concept '{title}'. {concept.Description}", overrideID: concept.Id + "-A");
                var styles = CreateOrUpdateListInternal(concept.Id, StylesTitle(title), $"List of '{title}' art styles. {concept.Description}", stylesParentName, overrideID: concept.Id + "-S");
            }

            return concept;
        }

        private ConceptListData CreateOrUpdateListInternal(string conceptId, string listTitle, string? Description, string? parentListId = null, string? ownerListId = null, string? overrideID = null)
        {
            ConceptListData list = graphData.Lists.FirstOrDefault(c => c.Title == listTitle);
            bool newList = list == null;

            if (!newList)
            {
                list.Id = list.Id ?? overrideID ?? GenerateGuid(true);
                list.Title = list.Title ?? listTitle;
                list.Description = (list.Description == null || list.Description.Length < (Description ?? "").Length) ? Description : list.Description;
                list.ParentConceptId = conceptId;
                list.ImageConceptId = conceptId;
                list.Type = GraphHub.Database.Dto.Enums.ItemTypeEnum.Gallery;
            }
            else
            {
                list = new ConceptListData
                {
                    Id = overrideID ?? GenerateGuid(true),
                    Title = listTitle,
                    Description = Description,
                    ParentConceptId = conceptId,
                    ImageConceptId = conceptId,
                    Type = GraphHub.Database.Dto.Enums.ItemTypeEnum.Gallery
                };
                graphData.Lists.Add(list);
                Console.WriteLine($"Create a new list: {list.Title}({list.Id})");
            }

            //Add to the parent list if its not already there as a pull list
            if (parentListId != null)
            {
                CreateOrUpdateListInListMembership(parentListId, list.Id);
            }

            return list;
        }

        private void CreateOrUpdateListInListMembership(string? addToParentListId, string listId)
        {
            ConceptListData parentList = graphData.Lists.FirstOrDefault(c => c.Title == addToParentListId);

            if (parentList != null)
            {
                var membership = parentList.PullFromListsIds?.Contains(listId) ?? false;

                if (!membership)
                {
                    if (parentList.PullFromListsIds == null)
                    {
                        parentList.PullFromListsIds = new List<string>();
                    }

                    parentList.PullFromListsIds.Add(listId);
                }
            }
            else
            {
                throw new ArgumentException($"Tried to add list {listId} to a list that does not exist! {addToParentListId}");
            }
        }

        private ConceptData CreateOrUpdateConceptInternal(string title, string? summary, string? memberOfListName = null)
        {
            if (title == "Banksy")
            {
                var i = 5;
            }

            //Try and find the concept
            ConceptData concept = graphData.Concepts.FirstOrDefault(c => c.Title == title);

            bool newConcept = concept == null;

            if (!newConcept)
            {
                concept.Id = concept.Id ?? GenerateGuid(true);
                concept.Title = concept.Title ?? title;
                concept.Description = concept.Description ?? summary;
            }
            else
            {
                concept = new ConceptData
                {
                    Id = GenerateGuid(true),
                    Title = title,
                    Description = summary,
                };

                graphData.Concepts.Add(concept);

                Console.WriteLine($"Create a new concept: {concept.Title}({concept.Id})");
            }

            if (memberOfListName != null)
            {
                ConceptListData parentList = graphData.Lists.FirstOrDefault(c => c.Title == memberOfListName);

                if (parentList == null)
                {
                    throw new ArgumentException($"Cant create membership for {concept.Title} to list that does not exist! {memberOfListName}");
                }

                var membership = graphData.Memberships.FirstOrDefault(c => c.ConceptId == concept.Id && c.ListId == parentList.Id);

                if (membership == null)
                {
                    membership = new MembershipData()
                    {
                        ConceptId = concept.Id,
                        ListId = parentList.Id,
                    };

                    graphData.Memberships.Add(membership);
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



