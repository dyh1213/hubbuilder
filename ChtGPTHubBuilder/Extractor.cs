using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using ChtGPTHubBuilder.Objects;
using Octokit;
using Azure.Core;
using Octokit.Internal;
using System.Reflection.PortableExecutable;
using System.Net.Http.Headers;

namespace ChtGPTHubBuilder
{
    public class Extractor
    {
        private static readonly HttpClient client = new HttpClient();
        private const string GptApiUrl = "https://api.openai.com/v1/chat/completions";
        private string source_filePath = "/Users/danielyeheskel-hai/Documents/ChatGPTResponses/";
        private string source_filename = "concepts.csv";
        private string destPath = "/Users/danielyeheskel-hai/Documents/ChatGPTResponses/DEST/";
        private string backupPath = "/Users/danielyeheskel-hai/Documents/ChatGPTResponses/BACKUP/";
        private int bulksize = 1;
        //string apiKey = "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";  // Replace with your OpenAI API key
        string systemPrompt = "You are an AI assistant tasked with dissecting artistic concepts into their key characteristics. Your responses should be formatted in a simplified JSON (minified) format.\nThe input is 1-20 artistic terms separated by a comma, each one should be a separate json object in the response.\u2028Respond only with a json array of each artistic concept, and nothing else.\nEvery term is either a \"string\" or an \"array\". Terms should be short and concise.\nFor arrays, provide a range of values; however, exclude the property entirely if no relevant values can be found.\nProvide as specific values as possible, closely associated with the art concept.\nDon’t combine 2 values with “and” or “or” in a single response.\nEach JSON response is an array of objects structured as:\n1. \"Concept_Name\": This is the name of the input concept (mandatory).\n2. \"ArtConcept\": An object with the following properties:\n    * \"ArtConcept_Name\": The full name of the concept, followed by the word “style” (mandatory).\n    * \"Art_Styles\": An array of up to five art styles related to the concept (at least 1).\n    * \"Medium\": A single example of a medium of the art (optional), like painting, sculpture, etc (optional).\n    * \"Environment\":  A single example of a environment in which the art is set (optional).\n    * \"Lighting\":  A single example of a lighting condition in the art (optional).\n    * \"Color\":  A single example of a color scheme of the art (optional).\n    * \"Mood\":  A single example of a mood that the art portrays (optional).\n    * \"Composition\":  A single example of a composition or the arrangement of the art (optional).\n    * \"relevant_artists\": An array of up to five artists associated with the art concept. (At least 1)\n3. \"Entity\": An object, only to be included if the concept is an entity. People are always considered entities.\n    * \"Entity_Class\": The class of the entity like a person, animal, game, movie, object etc.\n    * \"Entity_Category\": A more specific category of the entity, like animator, cartoon, movie, etc.\n4. \"summary\": A concise summary that should give a Wikipedia-like overview of the artistic interpretation of the concept, not exceeding 450 characters. Dissect the visual art style and focus on that.";
        //string apiKey = "";  // Replace with your OpenAI API key

        public bool readfromfil = true;
        public string filename = "UploadFile.json";

        public string? conversatoinID { get; set; }

        public async Task WriteConcepts()
        {
            List<string[]> data = LoadCsvFile(source_filePath + source_filename);

            Console.WriteLine("Loaded Concepts: " + data.Count());

            List<CSV_File> objects = data.Select(x => new CSV_File()
            {
                conceptname = x[0],
                fileName = x[1],
                backupFile = x[2]
            }).ToList();

            List<string> reaminingConcepts = objects
                .Where(x => string.IsNullOrEmpty(x.backupFile))
                .Select(x => x.conceptname)
                .ToList();
                
            Console.WriteLine("Reamining Concepts: " + reaminingConcepts.Count());

            List<string> selectedConcepts = reaminingConcepts
                        .Take(bulksize)
                        .ToList();        


            (List<ArtisticConceptResponse> responses, string backupfile) = await GetArtisticConcepts(selectedConcepts);

            foreach (ArtisticConceptResponse response in responses)
            {
                Console.WriteLine($"Writing concept file: {response.Concept_Name}");
                string conceptName = response.Concept_Name;
                string fileName = GetFileNameFromConceptName(conceptName);
                string json = JsonSerializer.Serialize(response, new JsonSerializerOptions() { WriteIndented = true });
                await WriteToFile(destPath, fileName, json);
                var concept = objects.FirstOrDefault(x => x.conceptname.Equals(response.Concept_Name));
                if (concept == null)
                {
                    objects.Add(new CSV_File()
                    {
                        conceptname = response.Concept_Name,
                        fileName = fileName,
                        backupFile = backupfile
                    });
                }
                else
                {
                    concept.fileName = fileName;
                    concept.backupFile = backupfile;
                }

            }

            Console.WriteLine($"Update CSV files");
            UpdateCsvFile(objects);
            
            
        }

        private async Task<(List<ArtisticConceptResponse>, string)> GetArtisticConcepts(List<string> concepts)
        {
            string userInput = string.Join(",", concepts);

            Console.WriteLine("Processing: " + userInput);

            (string responseJson, string backup) = await GetGptResponse(userInput);

            List<ArtisticConceptResponse> response = JsonSerializer.Deserialize<List<ArtisticConceptResponse>>(responseJson);

            return (response, backup);
        }

        private async Task<(string, string)> GetGptResponse(string userInput)
        {
            string completionText = "";
            if (!readfromfil)
            {
                completionText = await GetFromRapid_API(userInput);
            }
            else
            {
                completionText = System.IO.File.ReadAllText(filename);
            }


            string fileName = "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            Console.WriteLine($"Writing backup: {fileName}");

            await WriteToFile(backupPath, fileName, completionText);

            Console.WriteLine($"Writing backup successful: {fileName}");

            return (completionText, fileName);
        }

        private async Task<string> GetFromAPI(string userInput)
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userInput }
                },
                max_tokens = 800,
                model = "gpt-4"
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Clear();
            //client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", $"4aa497a065msh85f5c857541867ep1db601jsn936c3ee01924");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", $"chatgpt-gpt4-ai-chatbot.p.rapidapi.com");

            HttpResponseMessage response = await client.PostAsync(GptApiUrl, requestContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {

                Console.WriteLine($"GPT API request failed: {responseContent}");

                throw new Exception($"GPT API request failed: {responseContent}");
            }

            dynamic jsonResponse = JsonSerializer.Deserialize<dynamic>(responseContent);
            string completionText = jsonResponse.choices[0].message.content;
            return completionText;
        }

        private async Task<string> GetFromRapid_API(string userInput)
        {
            /*
            if (conversatoinID == null)
            {
                await ResolveConversationID();
            }
            */

            var setup = new RapidAPIQuery()
            {
                query = "What version of the ChatGPT model is this based on? Can you please provide details about the underlying architecture and its version number?",
                //conversationId = conversatoinID
            };

            var query = JsonSerializer.Serialize(setup);

            if (conversatoinID != null)
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://chatgpt-gpt4-ai-chatbot.p.rapidapi.com/ask"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", "e113d18ae5msh1fec6ef69638c83p13ad33jsn504e4c3677b4" },
                        { "X-RapidAPI-Host", "chatgpt-gpt4-ai-chatbot.p.rapidapi.com" },
                    },
                    Content = new StringContent(query)

                    {
                        Headers =

                        {
                             ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    }
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var rapidApiResponse = JsonSerializer.Deserialize<RapidAPIReponse>(body);
                    var responseObjectString = rapidApiResponse.response;

                    // Deserialize the nested JSON string into an array of objects
                    var validation = JsonSerializer.Deserialize<List<ArtisticConceptResponse>>(responseObjectString);

                    // Check if the array is not empty
                    if (validation != null && validation.Count > 0)
                    {
                        // Serialize only the first item with proper indentation
                        var indentedJsonString = JsonSerializer.Serialize(validation, new JsonSerializerOptions { WriteIndented = true });

                        Console.WriteLine("Validation JSON:");
                        Console.WriteLine(indentedJsonString); // This will print the indented JSON
                        return indentedJsonString;
                    }
                    else
                    {
                        // Handle the case when the array is empty or null
                        Console.WriteLine("No items in the array.");
                        return null;
                    }
                }
            }

            return null;
        }

        private async Task ResolveConversationID()
        {
            var setup = new RapidAPIQuery()
            {
                query = systemPrompt
            };

            var query = JsonSerializer.Serialize(setup);

            if (conversatoinID == null)
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://chatgpt-gpt4-ai-chatbot.p.rapidapi.com/ask"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", "cd2d9a958amshca80e3993f43f19p17944fjsnced55307573c" },
                        { "X-RapidAPI-Host", "chatgpt-gpt4-ai-chatbot.p.rapidapi.com" },
                    },
                    Content = new StringContent(query)

                    {
                        Headers =

                        {
                                        ContentType = new MediaTypeHeaderValue("application/json")

                        }
                    }
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var resoinsess = JsonSerializer.Deserialize<RapidAPIReponse>(body);
                    conversatoinID = resoinsess.conversationId;
                    Console.WriteLine(body);
                }
            }
        }

        private string GetFileNameFromConceptName(string conceptName)
        {
            return conceptName.Replace(" ", "_");
        }

        private async Task WriteToFile(string path, string name, string content)
        {
            string filePath = $"{path}{name}.json";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the header line
                writer.WriteLine(content);
            }
        }

        public static List<string[]> LoadCsvFile(string filePath)
        {
            List<string[]> data = new List<string[]>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    data.Add(fields);
                }
            }

            return data;
        }

        private void UpdateCsvFile(List<CSV_File> objects)
        {
            StringBuilder csvContent = new StringBuilder();

            foreach (CSV_File csvFile in objects)
            {
                string[] fields = { csvFile.conceptname, csvFile.fileName, csvFile.backupFile };
                string line = string.Join(",", fields);
                csvContent.AppendLine(line);
            }

            string csvFilePath = Path.Combine(source_filePath, source_filename);
            string backupFilePath = csvFilePath + ".bak"; // Create a backup file path

            // Write the backup file
            File.Copy(csvFilePath, backupFilePath, true);

            // Write the new content to the CSV file
            File.WriteAllText(csvFilePath, csvContent.ToString());
        }
    }

    public class RapidAPIReponse
    {
        public string conversationId { get; set; }
        public string response { get; set; }
    }

    public class RapidAPIQuery
    {
        public string query { get; set; }
        public string? conversationId { get; set; }
    }
}