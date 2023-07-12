using System;
namespace ChtGPTHubBuilder.Objects
{
	public class ArtisticConceptResponse
	{
        public string Concept_Name { get; set; }
        public ArtConcept ArtConcept { get; set; }
        public Entity Entity { get; set; }
        public string summary { get; set; }
        public string? Id { get; set; }
    }
}

