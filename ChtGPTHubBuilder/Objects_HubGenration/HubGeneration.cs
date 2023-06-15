using System;
namespace ChtGPTHubBuilder.Objects_HubGenration
{
    public class GraphData
    {
        public List<ConceptData> Concepts { get; set; }
        public List<ConceptListData> Lists { get; set; }
        public List<MembershipData> Memberships { get; set; }
        public List<ConceptMarkdown> ConceptMarkdown { get; set; }
    }

    public class ConceptData
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? PluralTitle { get; set; }
        public string? Description { get; set; }
    }

    public class ConceptListData
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? PluralTitle { get; set; }
        public string? Description { get; set; }
        public string? ParentConceptId { get; set; }
        public List<string>? PullFromListsIds { get; set; }
        public bool DisableDirectMembers { get; set; }
    }

    public class MembershipData
    {
        public string? ConceptId { get; set; }
        public string? ListId { get; set; }
    }

    public class ConceptMarkdown
    {
        public string? ConceptId { get; set; }
        public string? Markdown { get; set; }
    }
}

