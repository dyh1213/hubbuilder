﻿using System.Globalization;
using System.Text.Json;
using ChtGPTHubBuilder.Objects;
using GraphHub.Shared;

namespace ChtGPTHubBuilder
{
    public class HubBuilder
    {
        // Static GraphData object that will be populated with JSON data
        public static GraphData graphData = new GraphData();

        public const string HubConceptId = "10000000-024a-44e5-8844-998342022971";
        public const string TbdParentId = "10000000-0000-0000-0000-000000000000";

        private const string ArtStylesId = "1";
        private const string ArtPropertiesId = "2";
        private const string ArtEntitiesId = "3";

        private const string knollingConcept = "185";
        private const string steampunkConcept = "271";
        private const string banksyConcept = "272";
        // Static dictionary mapping ListName to its ID

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

            else Intitilize();

            var isValidInit = ValidateGraphData();
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

                ProcessTitleCase(file, conceptResponse);

                if (conceptResponse.Id == null)
                {
                    conceptResponse.Id = currentCount.ToString();
                    string updatedJson = JsonSerializer.Serialize(conceptResponse, new JsonSerializerOptions() { WriteIndented = true });
                    File.WriteAllText(file.FullName, updatedJson);
                }

                // Process each ArtisticConceptResponse
                HandleEntityConcept(conceptResponse);

                var isValid = ValidateGraphData();
                if (!isValid)
                {
                    Console.WriteLine("Graph is no longer valid");
                    throw new Exception("Graph is no longer valid");
                }


                currentCount++;
            }

            return graphData;
        }

        private static void ProcessTitleCase(FileInfo file, ArtisticConceptResponse conceptResponse)
        {
            string original = JsonSerializer.Serialize(conceptResponse, new JsonSerializerOptions() { WriteIndented = true });

            conceptResponse.Concept_Name = ToCustomTitleCase(conceptResponse.Concept_Name);
            conceptResponse.ArtConcept.Color = ToCustomTitleCase(conceptResponse.ArtConcept.Color);
            conceptResponse.ArtConcept.Composition = ToCustomTitleCase(conceptResponse.ArtConcept.Composition);
            conceptResponse.ArtConcept.Environment = ToCustomTitleCase(conceptResponse.ArtConcept.Environment);
            conceptResponse.ArtConcept.Medium = ToCustomTitleCase(conceptResponse.ArtConcept.Medium);
            conceptResponse.ArtConcept.Mood = ToCustomTitleCase(conceptResponse.ArtConcept.Mood);
            conceptResponse.ArtConcept.Lighting = ToCustomTitleCase(conceptResponse.ArtConcept.Lighting);

            for (int i = 0; i < (conceptResponse.ArtConcept.Art_Styles?.Count() ?? 0); i++)
            {
                conceptResponse.ArtConcept.Art_Styles[i] = ToCustomTitleCase(conceptResponse.ArtConcept.Art_Styles[i]);
            }

            for (int i = 0; i < (conceptResponse.ArtConcept.relevant_artists?.Count() ?? 0); i++)
            {
                conceptResponse.ArtConcept.relevant_artists[i] = ToCustomTitleCase(conceptResponse.ArtConcept.relevant_artists[i]);
            }

            if (conceptResponse.Entity != null) {
                conceptResponse.Entity.Entity_Category = ToCustomTitleCase(conceptResponse.Entity.Entity_Category);
                conceptResponse.Entity.Entity_Class = ToCustomTitleCase(conceptResponse.Entity.Entity_Class);
            }


            string updatedString = JsonSerializer.Serialize(conceptResponse, new JsonSerializerOptions() { WriteIndented = true });
            if (!updatedString.Equals(original))
            {
                File.WriteAllText(file.FullName, updatedString);
            }

        }

        private static string ToCustomTitleCase(string conceptName)
        {
            if (string.IsNullOrEmpty(conceptName)) return conceptName;

            //Clean Up Data
            var titleCase = ToCustomTitleCase_internal(conceptName);
            if (!conceptName.Equals(titleCase))
            {
                Console.WriteLine($"Change FROM: {conceptName} TO: {titleCase}");
                return titleCase;
            }
            return conceptName;
        }

        public static string ToCustomTitleCase_internal(string str)
        {
            var lowerCaseWords = new HashSet<string>
            {
                "a", "an", "the", "and", "but", "or", "for", "nor", "on", "at", "to", "from", "by", "of"
            };

            var split = str.Split(' ');
            for (var i = 0; i < split.Length; i++)
            {
                // Always capitalize the first and last word
                if (i == 0 || i == split.Length - 1 || !lowerCaseWords.Contains(split[i].ToLower()))
                {
                    // Capitalize the first character only
                    if (split[i].Length > 0)
                    {
                        split[i] = char.ToUpper(split[i][0]) + split[i].Substring(1);
                    }
                }
            }
            return string.Join(' ', split);
        }

        public bool ValidateGraphData()
        {
            var concepts = graphData.Concepts.Select(x=>x.Id).ToHashSet();
            var lists = graphData.Lists.Select(x => x.Id).ToHashSet();

            //validate relationships
            foreach (var membership in graphData.Memberships) {
                var containsConcept = concepts.Contains(membership.ConceptId);
                var containsList = lists.Contains(membership.ListId);
                if (!containsConcept || !containsList)
                {
                    LogMembershipError(membership.ConceptId, membership.ListId);
                    return false;
                }
            }

            foreach(var list in graphData.Lists) {
                if (list.ParentConceptId != null) {
                    var containsParentConcept = concepts.Contains(list.ParentConceptId); 
                    if (!containsParentConcept)
                    {
                        Console.WriteLine($"Missing parent concept {list.ParentConceptId} in list {list.Title} ({list.Id})");
                        return false;
                    }
                }
                if (list.PullFromListsIds != null) {
                    foreach(var pullfromlist in list.PullFromListsIds) {
                        var containsPullList = lists.Contains(pullfromlist);
                        if (!containsPullList) {
                            LogPullListError(list.Id, pullfromlist);
                            return false;
                        }
                        
                    }
                }
            }

            return true;
        }

        public void LogMembershipError(string coneptId, string listId) {
            var concept = graphData.Concepts.FirstOrDefault(x=>x.Id.Equals(coneptId));
            var list = graphData.Lists.FirstOrDefault(x => x.Id.Equals(listId));
            Console.WriteLine($"ERROR: Could not find member {concept?.Title}({coneptId}) in list {list?.Title}({listId}) ");
        }

        public void LogPullListError(string pullerId, string pulledId)
        {
            var pullerList = graphData.Lists.FirstOrDefault(x => x.Id.Equals(pullerId));
            var pulledList = graphData.Lists.FirstOrDefault(x => x.Id.Equals(pulledId));
            Console.WriteLine($"ERROR: Could not find pulled list {pulledList?.Title}({pulledList?.Id}) in LISTS! Puller is {pullerList?.Title}({pullerList?.Id}) ");
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
                ConceptData concept = FindOrCreateConcept(conceptName, conceptResponse.summary, ListIds[ListName.ArtStyles], "MAIN_ARTSTYLE", conceptResponse.Id);
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


        public static Dictionary<ListName, string> ListIds = new Dictionary<ListName, string>()
        {
            //Major Items
            { ListName.ArtStyles, ArtStylesId },
            { ListName.Properties, ArtPropertiesId },
            { ListName.Entities, ArtEntitiesId },

            //Part of entities
            { ListName.UnmappedEntities, "999" },

            //Properties
            { ListName.ArtMediums, "4" },
            { ListName.Environments, "5" },
            { ListName.Lightings, "6" },
            { ListName.Colors, "7" },
            { ListName.Moods, "8" },
            { ListName.Compositions, "9" },
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
                            list.Title = "Artistic Elements";
                            list.Description = "List of all elements and techniques that offer a structured way to analyze an image. It includes items such as medium which identifies the tool canvas or material used, environment indicates the setting, lighting style illuminates the subject, color defines the palette used, mood captures the emotional tone, while composition refers to how the image elements are arranged. Each of these attributes can help to understand and describe an image's unique artistic qualities and the intentions behind it.";
                            break;
                        case ListName.Entities:
                            list.Title = "Art Entities";
                            list.Description = "A collection of entities that have a definitive art style, this could be artists from a spefici field or other entities like tv-shows, games, studios etc.";
                            break;
                    }
                }
                else if (properties.Contains(item.Key))
                {
                    /*
                    var parentConcept = new ConceptData()
                    {
                        Id = GenerateGuid(true),
                        Title = item.Key.ToString().Remove(item.Key.ToString().Length - 1, 1),
                    };

                    graphData.Concepts.Add(parentConcept);
                    */

                    switch (item.Key)
                    {
                        case ListName.ArtMediums:
                            list.Title = "Art Mediums";
                            list.Description = "List of artistic mediums used by creators across the world. It spans traditional classics like oil, acrylic, watercolor, and charcoal, alongside contemporary innovations such as digital art, mixed media, and installations. Embrace the diverse array of mediums employed by artists to express their creativity and vision, representing a rich tapestry of human ingenuity in the world of art.";
                            break;
                        case ListName.Environments:
                            list.Title = "Environments";
                            list.Description = "List of art environments, from urban cityscapes to fantastical realms, post-apocalyptic wastelands to serene nature scenes. Delve into historical eras, futuristic worlds, and underwater wonders, as artists depict captivating backdrops that enrich their artworks with imaginative contexts and emotions. Whether it's the charm of Victorian elegance or the allure of cybernetic futurism, this collection showcases the boundless creativity of human expression through various captivating landscapes.";
                            break;
                        case ListName.Lightings:
                            list.Title = "Lighting Styles";
                            list.Description = "List of the diverse lighting styles that breathe life into artworks. From soft morning light to dramatic chiaroscuro, each style plays a crucial role in setting the ambiance and focus. Explore the enchanting glow of natural, artificial, and ultraviolet lights, as well as vibrant and high-contrast illuminations. Delight in surrealistic and electro-illuminated effects, as artists masterfully wield lighting to evoke emotions and accentuate their artistic creations.";
                            break;
                        case ListName.Colors:
                            list.Title = "Colors";
                            list.Description = "This list is a vibrant preview of the expansive world of color. It offers a glimpse into the variety of color schemes, from bold and high-contrast, to subdued and muted, painting a picture of possibilities. It explores the broad spectrum from monochrome to multi-color, and from vintage to modern palettes. You'll encounter warm, cool, natural, and period-specific tones, setting the stage for a more in-depth exploration.";
                            break;
                        case ListName.Moods:
                            list.Title = "Moods";
                            list.Description = "List of the diverse landscape of moods and themes in art. It illustrates a vast range from the playful and whimsical, to the introspective and contemplative, from the joyful and vibrant, to the mysterious and surreal. The list hints at the potential for art to be political, satirical, rebellious, or fantastical, reflecting the complexities of the human experience. Whether you're seeking the calm and tranquil, the edgy and dystopian, or the humorous and quirky, this list provides a glimpse into the powerful moods that art can evoke.";
                            break;
                        case ListName.Compositions:
                            list.Title = "Compositions";
                            list.Description = "List of the diverse tapestry of artistic compositions. From the bold, dynamic and geometric designs to the serene, harmonious and naturalistic forms. It includes abstract and surrealistic patterns, fluid and symmetrical structures, and extends into the detail-oriented world of anatomical, mechanical, and text-based compositions. Discover the artistry in three-dimensional space, layered imagery, or in the simplicity of a monospace canvas. Get specific like an organized grid, the or generic like an expansive landscape.";
                            break;
                    }

                    //list.ParentConceptId = parentConcept.Id;
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
                    RootConcept = "10000000-024a-44e5-8844-998342022971",
                    Description = "The 'text-to-image' graph is a the definitive guide for users of tools such as Midjourney, DALLE-2, Stable Diffusion, or similar AIs. This chart presents a mapping of art styles, lighting, colors, moods, and renowned entities, assisting in crafting prompts that yield predictable and consistent results.",
                    TopFeaturedLists = new List<GraphHub.Database.Dto.FeaturedItemImage>() {
                        new GraphHub.Database.Dto.FeaturedItemImage() {
                            Id = ArtStylesId,
                            Base64Image = "text-to-image-artstyles.png"
                        },
                        new GraphHub.Database.Dto.FeaturedItemImage() {
                            Id = ArtPropertiesId,
                            Base64Image = "text-to-image-colors.png"
                        },
                        new GraphHub.Database.Dto.FeaturedItemImage() {
                            Id = ArtEntitiesId,
                            Base64Image = "frida-kahlo-portrait.png"
                        },
                    },
                    FeaturedLists = ListIds.Where(x => properties.Contains(x.Key)).Select(x => x.Value).ToList(),
                    TopFeaturedConcepts = new List<GraphHub.Database.Dto.FeaturedItemImage>() {
                        new GraphHub.Database.Dto.FeaturedItemImage() {
                            Id = knollingConcept,
                            Base64Image = "knolling-art-style.png"
                        },
                        new GraphHub.Database.Dto.FeaturedItemImage() {
                            Id = steampunkConcept,
                            Base64Image = "steampunk-art-style.png"
                        },
                        new GraphHub.Database.Dto.FeaturedItemImage() {
                            Id = banksyConcept,
                            Base64Image = "banksy-art-style.png"
                        }
                    },
                };

                graphData.GraphInfo = metadata;
            }

            var propertyList = graphData.Lists.First(x => x.Id.Equals(ListIds[ListName.Properties]));
            foreach (var item in properties)
            {
                var itemAddedToProperties = ListIds[item];
                propertyList.PullFromListsIds.Add(itemAddedToProperties);
                Console.WriteLine($"Added property {item}({itemAddedToProperties}) to {propertyList.Id}");
                /*
                var propertyConcept = graphData.Lists.First(x => x.Id.Equals(ListIds[item]));
                graphData.Memberships.Add(new MembershipData()
                {
                    ConceptId = propertyConcept.ParentConceptId,
                    ListId = propertyList.Id
                });
                */
            }

            var entitiesList = graphData.Lists.First(x => x.Id.Equals(ListIds[ListName.Entities]));

            entitiesList.PullFromListsIds.Add(ListIds[ListName.UnmappedEntities]);

            graphData.ConceptMarkdown.Add(new ConceptMarkdown()
            {
                ConceptId = steampunkConcept,
                Markdown = "#### Origins and Influences of Steampunk\r\n\r\nSteampunk originated in the 1980s, but its foundations can be traced back to nineteenth-century speculative fiction, particularly the works of pioneering authors like Jules Verne, H.G. Wells, and Mary Shelley. Initially, it was a subgenre of science fiction that merged historical elements with anachronistic technological features inspired by science fiction. The term \"steampunk\" was coined by author K.W. Jeter as a playful variation of \"cyberpunk,\" another popular genre at the time.\r\n\r\nSteampunk's influence extended beyond literature and quickly found its way into various forms of artistic expression. It drew inspiration from the Industrial Revolution, Victorian-era Britain, the American Wild West, and the rise of steam power. Today, Steampunk is a vibrant art style that permeates movies, fashion, music, visual arts, and even technology.\r\n\r\n#### Key Elements of Steampunk Art Style\r\n\r\n##### Machinery and Technology\r\nSteampunk art prominently features machinery and technology, especially those powered by steam. Artists often incorporate gears, cogs, gauges, and analog devices in their works. This extends to flying machines, fantastical submarines, and mechanical prosthetics, emphasizing the interaction between humans and technology.\r\n\r\n##### Victorian Aesthetics\r\nDespite its focus on technology, steampunk incorporates Victorian fashion and design elements, evoking a nostalgic longing for an era of elegance and craftsmanship. Corsets, top hats, tailcoats, and goggles are common, as are intricate patterns, lace, and embroidery. Brass, copper, glass, and wood are prominent materials in both steampunk fashion and architecture.\r\n\r\n##### Retro-futurism\r\nSteampunk art presents an alternate history or parallel universe where modern technologies exist in the past, typically during the 19th century. This retro-futuristic vision showcases inventions and devices that are far ahead of their time, created with the materials and aesthetics of the Victorian era.\r\n\r\n##### Adventure and Exploration\r\nSteampunk's literary roots in the speculative fiction of Verne and Wells contribute to a spirit of adventure and exploration within the style. This may be portrayed through grand airships, elaborate submarines, or mysterious and exotic locations.\r\n\r\n##### Ornate Detailing\r\nElaborate detailing is a significant characteristic of steampunk design, reflecting the Victorian love for decoration and craftsmanship. Artists pay attention to even the minutest details, incorporating ornate gears, cogs, and filigree on clothing and buildings.\r\n\r\n##### Industrial Influence\r\nThe Industrial Revolution plays a crucial role in shaping the steampunk aesthetic, emphasizing manufacturing, mechanical engineering, and raw materials like iron and steel. This can be seen in art featuring imposing factories, industrial landscapes, or heavy machinery.\r\n\r\n##### The Color Palette\r\nSteampunk art employs a specific color palette, favoring sepia, brown, black, brass, and copper hues. These colors contribute to a sense of antiquity and a gritty, soot-covered industrial atmosphere.\r\n\r\n##### Dichotomy\r\nA core element of steampunk is the contrast between the mechanical and the organic, the historical and the futuristic, and the industrial and the elegant. This tension adds depth and complexity to steampunk art, creating a world that is simultaneously familiar and foreign.\r\n\r\n### Example Propmpts\r\n\r\n#### Steampunk Goggles\r\n![Steampunk Goggles Midjourney Example](https://cdn.discordapp.com/attachments/1057967671188135936/1126410897586405417/haidi_steampunk_goggles_burning_man_stylish_in_the_desert_Black_3efebb2e-e09b-4f33-856d-6ef4c1afd106.png \"Steampunk Goggles\")\r\n> AI: **MidJourney**\r\n> Prompt: `steampunk goggles, burning man, stylish, in the desert, Black rock city, high detail`\r\n\r\n#### Steampunk Engineer\r\n![Steampunk Engineer Midjourney Example](https://cdn.discordapp.com/attachments/1057967671188135936/1126410837368770611/haidi_steampunk_female_engineer_boarding_airship_with_gear_very_dcb64415-5668-4896-9673-bb3b680c7a71.png \"Steampunk Engineer\")\r\n> AI: **MidJourney**\r\n> Prompt: `steampunk goggles, burning man, stylish, in the desert, Black rock city, high detail`\r\n\r\n#### Steampunk Machine\r\n![Steampunk Machine Midjourney Example](https://cdn.discordapp.com/attachments/1057967671188135936/1126410940020183121/haidi_Steampunk_perpetuum_mobile_light_bulbs_bronze_gears_smoke_d80a548f-b53a-4550-b57d-53ed0edc2bb0.png \"Steampunk Machine\")\r\n> AI: **MidJourney**\r\n> Prompt: `steampunk goggles, burning man, stylish, in the desert, Black rock city, high detail`"
            });
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



