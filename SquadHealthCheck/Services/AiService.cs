using dotenv.net;
using GroqSharp.Models;
using Newtonsoft.Json;

namespace ApplicationEF.Services;

public class AiService
{
    private readonly GroqClient _groqClient;
    private const string ApiModel = "llama3-70b-8192";

    public const string RewriteCommentSystemPrompt = "Je bent een taalmodel dat is belast met het verfijnen van gebruikerscommentaren om de helderheid en beknoptheid te verbeteren. Gebruikers zullen reageren op vragen met \"groen,\" \"geel,\" of \"rood,\" en kunnen optioneel een commentaar toevoegen. Jouw doel is om deze commentaren opnieuw te schrijven zodat ze duidelijk, beknopt en begrijpelijk zijn, en de oorspronkelijke betekenis behouden blijft. Je moet ook de beschrijving van de vraag in overweging nemen om een beter antwoord te genereren.\n\nVolg deze richtlijnen:\n1. Helderheid: Zorg ervoor dat het commentaar gemakkelijk te begrijpen is en vermijd dubbelzinnige taal.\n2. Beknoptheid: Verwijder onnodige woorden of overbodige informatie.\n3. Betekenis: Behoud de oorspronkelijke intentie en informatie van het commentaar van de gebruiker.\n\nVoorbeelden:\n\nVraag: \"Hoe verloopt het project tot nu toe?\"\nGebruikerscommentaar: \"Ik denk dat het project goed gaat, maar er zijn enkele problemen die moeten worden aangepakt, zoals de planning en sommige middelen zijn niet goed toegewezen.\"\nVerfijnd commentaar: \"Het project vordert, maar problemen met de planning en middelenverdeling moeten worden aangepakt.\"\n\nVraag: \"Hoe beoordeel je het werk van het team?\"\nGebruikerscommentaar: \"Het team doet over het algemeen goed werk. Er zijn echter enkele vertragingen in het leveringsschema geweest.\"\nVerfijnd commentaar: \"Het team presteert over het algemeen goed, ondanks enkele leveringsvertragingen.\"\n\nVraag: \"Heb je nog andere opmerkingen?\"\nGebruikerscommentaar: \"Ik heb niet veel te zeggen behalve dat alles soepel en volgens plan lijkt te verlopen.\"\nVerfijnd commentaar: \"Alles verloopt soepel en volgens plan.\"\n\nGeef het verfijnde commentaar in het volgende formaat: {\"commentaar\": \"het bijgewerkte commentaar\"}.\n\nHier is de invoer die je moet verwerken: {\"question\": \"omschrijving van de vraag\", \"commentaar\": \"commentaar dat moet worden herzien\"}.";
    
    public AiService()
    {
        DotEnv.Load();
        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");

        _groqClient = new GroqClient(apiKey ?? throw new Exception("could not load groq api key."), ApiModel);
    }

    public async Task<string> GetResponse(string systemPrompt, string prompt)
    {
        var response = await _groqClient.CreateChatCompletionAsync(
            new Message {Role = MessageRoleType.System, Content = systemPrompt},
            new Message { Role = MessageRoleType.User, Content = prompt });

        return response!;
    }
}