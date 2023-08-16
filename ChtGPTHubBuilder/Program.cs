// See https://aka.ms/new-console-template for more information
using ChtGPTHubBuilder;
using GraphHub.Database.Database.GitManager;
using GraphHub.Server.GitUploader;
using GraphHub.Shared;

Console.WriteLine("Hello, World!");

//var content = File.ReadAllText("/Users/danielyeheskel-hai/Projects/Checkpoints/GraphHub Database EF/GraphHub/Shared/ConceptsData/tesla.json");
//var graphdata = JsonSerializer.Deserialize<GraphData>(content);
//var dic = new Dictionary<string, GraphData>();
//dic.Add("tesla", graphdata);
//var upload = new GitJsonSaver("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");
//await upload.SaveData(dic);


Extractor ext = new Extractor();
ext.readfromfil = true;
ext.conversatoinID = "cll27zuqj6gogmn0ogxitnz5q";
await ext.WriteConcepts();

HubBuilder hubbuilder = new HubBuilder();
var graphData = await hubbuilder.Run();

//var gotIDOptimized = OptimizeIDs.RunThis();
GitJsonSaver saver = new GitJsonSaver("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", "main");
GhPullRequest? pullrequest = new()
{
    title = "testing",
    body = "testing",
    branch = "testingbranch",
    Execute = false   
};


await saver.UpdateGraph(graphData);
return;


Console.WriteLine("Close");