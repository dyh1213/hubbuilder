// See https://aka.ms/new-console-template for more information
using ChtGPTHubBuilder;
using GraphHub.Server.GitUploader;
using GraphHub.Shared;
using Microsoft.VisualBasic;
using System.Text.Json;

Console.WriteLine("Hello, World!");

//var content = File.ReadAllText("/Users/danielyeheskel-hai/Projects/Checkpoints/GraphHub Database EF/GraphHub/Shared/ConceptsData/tesla.json");
//var graphdata = JsonSerializer.Deserialize<GraphData>(content);
//var dic = new Dictionary<string, GraphData>();
//dic.Add("tesla", graphdata);
//var upload = new GitJsonSaver("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");
//await upload.SaveData(dic);


Extractor ext = new Extractor();
await ext.WriteConcepts();

HubBuilder hubbuilder = new HubBuilder();
hubbuilder.Run();
return;


Console.WriteLine("Close");