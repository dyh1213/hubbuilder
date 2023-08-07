using System;
using ChtGPTHubBuilder.Objects;
using System.Text.Json;

namespace ChtGPTHubBuilder.Builder
{
	public static class DataCleaning
	{
        public static ArtisticConceptResponse ProcessTitleCase(FileInfo file, ArtisticConceptResponse conceptResponse)
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

            if (conceptResponse.Entity != null)
            {
                conceptResponse.Entity.Entity_Category = ToCustomTitleCase(conceptResponse.Entity.Entity_Category);
                conceptResponse.Entity.Entity_Class = ToCustomTitleCase(conceptResponse.Entity.Entity_Class);
            }


            string updatedString = JsonSerializer.Serialize(conceptResponse, new JsonSerializerOptions() { WriteIndented = true });

            if (!updatedString.Equals(original))
            {
                File.WriteAllText(file.FullName, updatedString);
            }

            return conceptResponse;
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

        private static string ToCustomTitleCase_internal(string str)
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


    }
}

