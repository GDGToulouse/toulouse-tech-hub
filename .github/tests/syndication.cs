#:package Microsoft.Playwright@1.63.0
#:property PublishAot=false
#:property PublishTrimmed=false
#:property EnableTrimAnalyzer=false

using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Playwright;

AppContext.SetSwitch("System.Text.Json.Serialization.EnableReflectionSerializer", true);

// Charger l'assembly compilé permet de tester le script sans exécuter son archivage ni son écriture YAML.
var assembly = Assembly.LoadFrom(Path.GetFullPath(args.Single()));
var groupType = assembly.GetType("SyndicationGroup", throwOnError: true)!;
var eventType = assembly.GetType("Event", throwOnError: true)!;
var parseTgd = assembly.GetType("ToulouseGameDevGroup")!.GetMethod("ParseEventStart")!
    .CreateDelegate<Func<string, string, DateTimeOffset?>>();

using var portProbe = new TcpListener(IPAddress.Loopback, 0);
portProbe.Start();
var port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
portProbe.Stop();
var origin = $"http://127.0.0.1:{port}";
using var listener = new HttpListener();
listener.Prefixes.Add(origin + "/");
listener.Start();
var feed = "";
var articles = new Dictionary<string, string>();
var server = Task.Run(async () =>
{
    try
    {
        while (listener.IsListening)
        {
            var request = await listener.GetContextAsync();
            var path = request.Request.Url!.AbsolutePath;
            var body = path == "/feed" ? feed : articles.GetValueOrDefault(path, "<time datetime='2024-01-01'>Publication</time>");
            using var writer = new StreamWriter(request.Response.OutputStream);
            await writer.WriteAsync(body);
        }
    }
    catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException) { }
});

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new()
{
    Headless = true,
    ExecutablePath = Environment.GetEnvironmentVariable("PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH")
});
await using var context = await browser.NewContextAsync();
var page = await context.NewPageAsync();
var checks = 0;
try
{
    var first = Item("one", "Meetup de Juin", "2099-06-02T18:30:00+02:00");
    var second = Item("two", "Autre rencontre", "2099-06-02T20:30:00+02:00");
    var events = await Scan(Rss(first, second));
    Check(events.Count == 2, "Les annonces déjà publiées des rencontres futures sont importées");
    Check(Get<DateTimeOffset>(events[0], "Start") == DateTimeOffset.Parse("2099-06-02T18:30:00+02:00"), "La date de début n'est pas pubDate");
    Check(Get<DateTimeOffset>(events[0], "PublishedOn").Year == 2024, "La publication reste distincte");
    var firstId = Get<string>(events[0], "Id");
    var secondId = Get<string>(events[1], "Id");
    Check(firstId != secondId, "Deux rencontres le même jour ont des IDs distincts");
    var reordered = await Scan(Rss(second, first));
    Check(Get<string>(reordered[0], "Id") == secondId && Get<string>(reordered[1], "Id") == firstId, "Les IDs résistent au réordonnancement");
    Check(Get<string>((await Scan(Rss(second)))[0], "Id") == secondId, "Les IDs résistent au retrait d'une entrée");
    Check((await Scan(Rss(first, first))).Count == 1, "Un guid répété ne crée pas de doublon");

    var old = Item("past", "Rencontre passée", "2024-06-02T18:30:00+02:00");
    Check((await Scan(Rss(old))).Count == 0, "Les rencontres passées sont filtrées");
    Check((await Scan(Rss(old), loadPast: true)).Count == 1, "--load-past accepte les rencontres passées");
    Check(Get<string>((await Scan(Rss(old, second)))[0], "Id") == secondId, "Le filtrage ne modifie pas les IDs");

    var noDate = Item("no-date", "Sans date", null);
    Check((await Scan(Rss(noDate))).Count == 0, "Une publication seule ne fournit jamais la date de rencontre");
    var noLink = new XElement(first);
    noLink.Element("link")!.Remove();
    Check((await Scan(Rss(noLink))).Count == 0, "Une entrée sans lien est ignorée");

    articles["/article"] = """
        <script type="application/ld+json">
        {"@graph":[{"@type":"BlogPosting","datePublished":"2024-01-01"},
        {"@type":["Event"],"startDate":"2099-06-02T18:30:00+02:00"}]}
        </script>
        """;
    var linked = Item("article", "Date dans l'article", null);
    Check(Get<DateTimeOffset>((await Scan(Rss(linked)))[0], "Start").Year == 2099, "La page liée fournit Event.startDate");
    var contentDate = Item("content", "Date dans le contenu", null);
    contentDate.Add(new XElement("description", new XCData("<meta itemprop='startDate' content='2099-06-02T18:30:00'>")));
    var summer = Get<DateTimeOffset>((await Scan(Rss(contentDate)))[0], "Start");
    Check(summer.Hour == 18 && summer.Offset == TimeSpan.FromHours(2), "Une date locale utilise Paris en été");
    var winterItem = Item("winter", "Horaire d'hiver", "2099-01-02T18:30:00");
    Check(Get<DateTimeOffset>((await Scan(Rss(winterItem)))[0], "Start").Offset == TimeSpan.FromHours(1), "Paris en hiver");

    var atom = """
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:event="https://schema.org/">
          <entry><id>urn:event:atom</id><title>Atom</title>
            <link rel="self" href="/ignored"/><link rel="alternate" href="/atom"/>
            <published>2024-01-01T10:00:00Z</published><updated>2025-01-01T10:00:00Z</updated>
            <event:startDate>2099-06-02T16:30:00Z</event:startDate>
            <content type="html">&lt;b&gt;Texte&lt;/b&gt;&lt;img src=x onerror=alert(1)&gt;</content>
          </entry>
        </feed>
        """;
    var atomEvent = (await Scan(atom))[0];
    Check(Get<string>(atomEvent, "Href") == origin + "/atom", "Le lien alternatif Atom est résolu");
    Check(Get<DateTimeOffset>(atomEvent, "Start").Hour == 18, "Le début Atom est converti en heure de Paris");
    Check(Get<DateTimeOffset>(atomEvent, "PublishedOn").Year == 2024, "Atom utilise published avant updated");
    Check(!Get<string>(atomEvent, "HtmlDescription").Contains("<img"), "Le HTML importé est neutralisé");
    Check(Get<string>((await Scan(atom))[0], "Id") == Get<string>(atomEvent, "Id"), "L'id Atom est stable");

    var tgdDate = parseTgd("Rendez-vous le mardi 2 Juin à partir de 18h30 !", origin + "/2099/article/");
    Check(tgdDate == DateTimeOffset.Parse("2099-06-02T18:30:00+02:00"), "La date TGD est extraite du texte avec l'année de l'URL");
    Check(parseTgd("Rendez-vous le 2 janvier 2099 à partir de 18h30 !", origin + "/2098/article/")?.Year == 2099, "L'année explicite TGD est prioritaire");
    Check(parseTgd("Rendez-vous le 31 février 2099 à partir de 18h30 !", origin + "/2099/article/") == null, "Une date TGD invalide est ignorée");
    articles["/2099/tgd/"] = "<p>Rendez-vous le mardi 2 Juin à partir de 18h30 !</p>";
    var tgdItem = Item("tgd", "Meetup de Juin", null);
    tgdItem.Element("link")!.Value = origin + "/2099/tgd/";
    Check(Get<DateTimeOffset>((await Scan(Rss(tgdItem), parser: parseTgd))[0], "Start") == tgdDate, "L'extracteur TGD est utilisé sur l'article lié");

    var legacy = Activator.CreateInstance(eventType)!;
    Set(legacy, "Id", "tgd-2099-06-02");
    Set(legacy, "GroupId", "tgd");
    Set(legacy, "Title", "Meet-up de Juin");
    Set(legacy, "Href", "https://toulousegamedev.fr/evenements/");
    Set(legacy, "Start", DateTimeOffset.Parse("2099-06-02T18:30:00+02:00"));
    var migrated = await Scan(Rss(first, second), [legacy]);
    Check(Get<string>(migrated[0], "Id") == "tgd-2099-06-02", "L'identifiant historique TGD est préservé");
    Check(Get<string>(migrated[1], "Id") == secondId, "L'autre rencontre du même jour conserve son ID source");
    var nextRun = await Scan(Rss(second, first), migrated);
    Check(Get<string>(nextRun[1], "Id") == "tgd-2099-06-02", "L'ID historique reste stable après migration et réordonnancement");
    var sameTitle = Item("same-title", "Meetup de Juin", "2099-06-02T20:30:00+02:00");
    var ambiguousDay = await Scan(Rss(sameTitle, first), [legacy]);
    Check(Get<string>(ambiguousDay[1], "Id") == "tgd-2099-06-02" &&
        Get<string>(ambiguousDay[0], "Id") != "tgd-2099-06-02", "L'heure distingue les anciens événements de même titre le même jour");
    Set(migrated[1], "Id", "tgd-2099-06-02-2");
    Check(Get<string>((await Scan(Rss(second), migrated))[0], "Id") == "tgd-2099-06-02-2", "Les IDs suffixés du précédent lecteur sont préservés par le lien");
    first.Element("startDate")!.Value = "2099-06-03T18:30:00+02:00";
    Check(Get<string>((await Scan(Rss(first), migrated))[0], "Id") == "tgd-2099-06-02", "Un report conserve l'ID existant reconnu par son lien");

    var withoutGuid = Item("no-guid", "Sans guid", "2099-06-02T18:30:00+02:00");
    withoutGuid.Element("guid")!.Remove();
    var noGuidId = Get<string>((await Scan(Rss(withoutGuid)))[0], "Id");
    Check(Get<string>((await Scan(Rss(second, withoutGuid)))[1], "Id") == noGuidId, "Sans guid, le lien fournit un ID stable");
    var futurePublication = Item("future-publication", "Publication future", "2099-06-02T18:30:00+02:00");
    futurePublication.Element("pubDate")!.Value = "2199-01-01T10:00:00Z";
    var importedPublication = Get<DateTimeOffset>((await Scan(Rss(futurePublication)))[0], "PublishedOn");
    Check(importedPublication <= DateTimeOffset.UtcNow && importedPublication.Year == DateTimeOffset.UtcNow.Year,
        "Une publication future est remplacée par la date d'import");
    var rejectedDtd = false;
    try
    {
        await Scan("<!DOCTYPE rss [<!ENTITY xxe SYSTEM 'file:///unavailable'>]><rss><channel>&xxe;</channel></rss>");
    }
    catch (System.Xml.XmlException)
    {
        rejectedDtd = true;
    }
    Check(rejectedDtd, "Les DTD et entités externes sont interdites");

    Console.WriteLine($"{checks} vérifications réussies.");
}
finally
{
    listener.Stop();
    await server;
}

XElement Item(string id, string title, string? start) => new("item",
    new XElement("guid", id), new XElement("title", title),
    new XElement("link", origin + "/" + id),
    new XElement("pubDate", "Mon, 01 Jan 2024 10:00:00 GMT"),
    start == null ? null : new XElement("startDate", start));

string Rss(params XElement[] items) => new XElement("rss", new XElement("channel", items)).ToString();

async Task<List<object>> Scan(string xml, IReadOnlyCollection<object>? known = null,
    bool loadPast = false, Func<string, string, DateTimeOffset?>? parser = null)
{
    feed = xml;
    var existing = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(eventType))!;
    foreach (var evt in known ?? [])
        existing.Add(evt);
    var group = Activator.CreateInstance(groupType, [origin + "/feed", "tgd", "Test", existing, parser])!;
    var results = (IAsyncEnumerable<object>)groupType.GetMethod("Scan")!.Invoke(group, [context, page, loadPast])!;
    var collected = new List<object>();
    await foreach (var evt in results)
        collected.Add(evt);
    return collected;
}

void Check(bool condition, string name)
{
    if (!condition)
        throw new InvalidOperationException(name);
    checks++;
    Console.WriteLine($"✓ {name}");
}

T Get<T>(object instance, string name) => (T)eventType.GetProperty(name)!.GetValue(instance)!;
void Set(object instance, string name, object value) => eventType.GetProperty(name)!.SetValue(instance, value);
