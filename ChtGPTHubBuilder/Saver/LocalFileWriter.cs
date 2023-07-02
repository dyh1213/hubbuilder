using System;
using System.Text.Json;
using GraphHub.Shared;

namespace ChtGPTHubBuilder
{
	public class LocalFileWriter
	{
		public LocalFileWriter()
		{
            const string? pathToGraphDataDest = "/Users/danielyeheskel-hai/Projects/GraphHub/GraphHub/Shared/ConceptsData/midjourney.json";

			static void WriteData(GraphData graphData, string dest = pathToGraphDataDest)
			{
                // Optionally, write the modified GraphData back to the file
                string modifiedGraphDataJson = JsonSerializer.Serialize(graphData, new JsonSerializerOptions() { WriteIndented = true });

                if (pathToGraphDataDest == null)
                {
                    File.WriteAllText("newgraph.json", modifiedGraphDataJson);
                }
                else
                {
                    File.WriteAllText(pathToGraphDataDest, modifiedGraphDataJson);
                }
                
            }
        }
	}
}

