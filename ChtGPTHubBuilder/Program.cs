// See https://aka.ms/new-console-template for more information
using ChtGPTHubBuilder;

Console.WriteLine("Hello, World!");

HubBuilder hubbuilder = new HubBuilder();
hubbuilder.Run();
return;

Extractor ext = new Extractor();
await ext.WriteConcepts();

Console.WriteLine("Close");