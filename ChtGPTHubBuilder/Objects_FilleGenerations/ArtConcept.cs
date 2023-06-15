using System;
namespace ChtGPTHubBuilder.Objects
{
	public class ArtConcept
	{
        public string ArtConcept_Name { get; set; }
        public string[] Art_Styles { get; set; }
        public string Medium { get; set; }
        public string Environment { get; set; }
        public string Lighting { get; set; }
        public string Color { get; set; }
        public string Mood { get; set; }
        public string Composition { get; set; }
        public string[] relevant_artists { get; set; }
    }
}

