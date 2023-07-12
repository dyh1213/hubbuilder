using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using ChtGPTHubBuilder.Objects;

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
        private int bulksize = 2;
        string apiKey = "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";  // Replace with your OpenAI API key
        string systemPrompt = "You are an AI assistant tasked with dissecting artistic concepts into their key characteristics. Your responses should be formatted in a simplified JSON (minified) format.Every term is either a \"string\" or an \"array\".For optional properties, simply leave them out if they are not applicable or cannot be filled with a relevant value.For arrays, provide a range of values; however, exclude the property entirely if no relevant values can be found.Provide as specific values as possible, closely associated with the art concept.Don’t combine 2 values with “and” in a single response.Each JSON response is an array of objects structured as:1. \"Concept_Name\": This is the name of the input concept.2. \"ArtConcept\": An object with the following properties:    * \"ArtConcept-Name\": The full name of the concept, followed by the word “style”.    * \"Art-Styles\": An array of up to five art styles related to the concept.    * \"Medium\": A single medium of the art (optional), like painting, sculpture, etc.    * \"Environment\":  A single  environment in which the art is set (optional).    * \"Lighting\":  A single lighting condition in the art (optional).    * \"Color\":  A single color scheme of the art (optional).    * \"Mood\":  A single mood that the art portrays (optional).    * \"Composition\":  A single composition or the arrangement of the art (optional).    * \"relevant_artists\": An array of up to five artists associated with the concept. Exclude if the concept is an actual person.3. \"Entity\": An object, only to be included if relevant, with the following properties:    * \"Entity-Class\": The class of the entity like a person, animal, game, etc.    * \"Entity-Category\": A more specific category of the entity, like animator, cartoon, movie, etc.4. \"summary\": A concise summary that should give a Wikipedia-like overview of the artistic interpretation of the concept, not exceeding 450 characters. Dissect the visual art style and focus on that.";
        //string apiKey = "";  // Replace with your OpenAI API key

        public bool readfromfil = true;
        public string filename = "UploadFile.json";

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
                completionText = await GetFromAPI(userInput);
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
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

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
}