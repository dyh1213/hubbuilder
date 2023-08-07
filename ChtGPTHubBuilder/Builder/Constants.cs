using System;
using GraphHub.Shared;

namespace ChtGPTHubBuilder.Builder
{
	public static class Constants
	{
        public const string HubConceptId = "10000000-024a-44e5-8844-998342022971";
        public const string TbdParentId = "10000000-0000-0000-0000-000000000000";

        private const string ArtStylesId = "1";
        private const string ArtPropertiesId = "2";
        private const string ArtEntitiesId = "3";

        private const string knollingConcept = "185";
        private const string steampunkConcept = "271";
        private const string banksyConcept = "272";

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

        public static GraphData Intitilize()
        {
            var graphData = new GraphData()
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

            return graphData;
        }
    }


}

