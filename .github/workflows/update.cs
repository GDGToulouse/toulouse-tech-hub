#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property PublishTrimmed=false
#:property PublishAot=false
#:property EnableTrimAnalyzer=false

#:package Microsoft.Playwright@1.50.0
#:package System.Linq.Async@6.0.1

using Microsoft.Playwright;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

// gestion des emoticons
Console.OutputEncoding = Encoding.UTF8;

// enable reflection-based System.Text.Json for Playwright
AppContext.SetSwitch("System.Text.Json.Serialization.EnableReflectionSerializer", true);

// execution options
var loadEvents = args.All(a => a != "--no-load"); // load next events
var loadPastEvents = args.Any(a => a == "--load-past"); // load past events
var groupFilter = args.FirstOrDefault(a => !a.StartsWith("--"));
MeetupGroup.loadTimeoutMs = 5 * 1000; // timeout waiting for network idling when loading meetup pages
MeetupGroup.loadStateType = LoadState.NetworkIdle; // type of timeout, unreliable on meetup
MeetupGroup.loadTimeoutScreenshots = false; // take screenshot in case of timeouts

// sets to french culture
var french = CultureInfo.GetCultureInfo("fr-FR");
Thread.CurrentThread.CurrentCulture = french;
Thread.CurrentThread.CurrentUICulture = french;

// specific encoder to allow emojis in json
var utf8CustomEncoder = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback());

// liste des groupes connus
var groups = new IGroup[] {
    new ToulouseGameDevGroup(),
    new MeetupGroup("gdg-toulouse", "gdg"),
    new MeetupGroup("toulouse-java-user-group", "jug"),
    new MeetupGroup("toulouse-ruby-friends", "ruby"),
    new MeetupGroup("python-toulouse", "python"),
    new MeetupGroup("tlse-data-science", "tds"),
    new MeetupGroup("agile-toulouse", "agile"),
    new MeetupGroup("mtg-toulouse", "mtg"),
    new MeetupGroup("meetup-visualisation-des-donnees-toulouse", "dataviz"),
    new MeetupGroup("ateliers-cpp-toulouse", "cpp"),
    new MeetupGroup("toulouse-amazon-web-services", "aws"),
    new MeetupGroup("swift-toulouse", "swift"),
    new MeetupGroup("tech-a-break", "tech-a-break"),
    new MeetupGroup("toulouse-devops", "devops"),
    new MeetupGroup("devops-cloud-toulouse", "devops-cloud"),
    new MeetupGroup("javascript-and-co", "js-and-co"),
    new MeetupGroup("latoulboxducloudnatif", "toulbox"),
    new MeetupGroup("artilect-fablab", "artilect"),
    new MeetupGroup("postgres-toulouse", "postgres"),
    new MeetupGroup("rust-community-toulouse", "rust")
};

/****************************/
/*   load events-job.json   */
/****************************/
static string ResolveRepoRoot(string startDir)
{
    var current = startDir;
    while (!string.IsNullOrEmpty(current))
    {
        if (File.Exists(Path.Combine(current, "events-job.json")))
            return current;

        current = Directory.GetParent(current)?.FullName;
    }

    return startDir;
}

var repoRoot = ResolveRepoRoot(Directory.GetCurrentDirectory());
var jsonPath = Path.Combine(repoRoot, "events-job.json");
List<Event> knownEvts = [];
{
    if (File.Exists(jsonPath))
    {
        knownEvts = JsonSerializer.Deserialize<List<Event>>(File.ReadAllText(jsonPath)) ?? [];
        Console.WriteLine($"⚗️ {knownEvts.Count} événements lus depuis fichier json");

        // gestion migration des évènements
        foreach (var evt in knownEvts)
        {
            // on essaie de renseigner le groupId en recherchant un autre évènement où le même groupe a cette info pour la recopier
            // il suffirait de renseigner cette info sur qq évènements de chaque groupe manquant pour que l'info s'ajoute toute seule partout
            if (string.IsNullOrEmpty(evt.GroupId))
            {
                var other = knownEvts.FirstOrDefault(e => !string.IsNullOrEmpty(e.GroupId) && e.Group == evt.Group);
                if (other != null)
                    evt.GroupId = other.GroupId;
            }

            // on essaie de renseigner les dates de publication manquantes en supposant qu'elles ont été publiées 1 mois avant leur déclenchement
            if (evt.PublishedOn == DateTimeOffset.MinValue)
            {
                evt.PublishedOn = evt.Start.AddMonths(-1);
            }
        }

        // Re-calculate LocalImgSrc for all events to use the filename prefix
        // This makes it easier to connect images with their corresponding event files
        foreach (var evt in knownEvts)
        {
            if (!string.IsNullOrEmpty(evt.ImgSrc) || !string.IsNullOrEmpty(evt.FullImgSrc))
            {
                var fileNamePrefix = $"{evt.Start:yyyy-MM-dd}-{evt.GroupId}-{evt.Id}";
                evt.SetLocalImgSrcWithNamePrefix(fileNamePrefix);
            }
        }
    }
}

/***********************/
/*        Load         */
/***********************/
List<Event> evts = [];
{
    if (loadEvents)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var ctx = await browser.NewContextAsync();
        var defaultPage = await ctx.NewPageAsync();
        var http = new HttpClient();

        foreach (var group in groups.Where(g => string.IsNullOrEmpty(groupFilter) || g.Id == groupFilter))
        {
            await foreach (var evt in group.Scan(ctx, defaultPage, loadPastEvents))
            {
                Console.WriteLine($"💥 [{evt.Start:dd/MM/yyyy HH:mm}] {evt.Group} - {evt.Title}");

                evts.Add(evt);

                // téléchargement de l'image => event-imgs/meetup-xxx.webp
                // Mise à jour basée sur Last-Modified: télécharge si source plus récente que le fichier local
                if (evt.LocalImgSrc != null)
                {
                    var imgPath = Path.Combine(repoRoot, evt.LocalImgSrc);
                    var imgSource = evt.FullImgSrc ?? evt.ImgSrc;

                    if (imgSource != null)
                    {
                        bool shouldDownload = false;

                        if (!File.Exists(imgPath))
                        {
                            shouldDownload = true;
                        }
                        else
                        {
                            // Check if source image has been updated by comparing Last-Modified headers
                            try
                            {
                                var headRequest = new HttpRequestMessage(HttpMethod.Head, imgSource);
                                var response = await http.SendAsync(headRequest);

                                // Try to get Last-Modified from response headers (generic headers, not content-specific)
                                if (response.Headers.TryGetValues("Last-Modified", out var lastModifiedValues) && lastModifiedValues.FirstOrDefault() is string lastModStr)
                                {
                                    if (DateTimeOffset.TryParse(lastModStr, out var remoteFileTime))
                                    {
                                        var localFileTime = File.GetLastWriteTimeUtc(imgPath);
                                        shouldDownload = remoteFileTime.UtcDateTime > localFileTime;
                                    }
                                }
                            }
                            catch
                            {
                                // If we can't determine modification time, don't update to avoid unnecessary downloads
                                shouldDownload = false;
                            }
                        }

                        if (shouldDownload)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(imgPath)!);
                            await File.WriteAllBytesAsync(imgPath, await http.GetByteArrayAsync(imgSource));
                        }
                    }
                }
            }
        }
    }
}

/****************************/
/*   save events-job.json   */
/****************************/
{
    // update events when asked for loading
    if (loadEvents)
    {
        var loadedEvts = evts.OrderBy(x => x.Start).ThenBy(x => x.Id);
        if (loadedEvts.Any())
        {
            Console.WriteLine(loadPastEvents ? "📅 Événements détectés :" : "📅 Prochains événements détectés :");
            foreach (var evt in loadedEvts)
            {
                Console.WriteLine($"📅 [{evt.Start:dd/MM/yyyy HH:mm}] {evt.Group} - {evt.Title}");

                var known = knownEvts.FirstOrDefault(e => e.Id == evt.Id);
                if (known == null)
                {
                    // événement inconnu on l'ajoute
                    knownEvts.Add(evt);
                }
                else if (!evt.IsPast)
                {
                    // tant que l'évènement n'est pas passé on le remplace
                    knownEvts.Remove(known);
                    knownEvts.Add(evt);

                    // on conserve la date de publication d'origine de l'évènement
                    evt.PublishedOn = known.PublishedOn;
                }
                else
                {
                    // on ne touche plus aux évènements passés
                }
            }
        }

        knownEvts.Sort();

        var missingEvts = knownEvts.Where(ke => ke.IsPast == false && !evts.Any(e => e.Id == ke.Id));
        if (missingEvts.Any())
        {
            Console.WriteLine("❌ Événements à venir introuvables :");
            foreach (var evt in missingEvts)
            {
                Console.WriteLine($"❌ [{evt.Start:dd/MM/yyyy HH:mm}] {evt.Group} - {evt.Title}");
            }
        }

        // 🗑️ Purge events older than 1 year
        var oneYearAgo = FrenchLocales.ParisNow.AddYears(-1);
        var oldEvents = knownEvts.Where(e => e.Start < oneYearAgo).ToList();
        if (oldEvents.Any())
        {
            Console.WriteLine($"🗑️ Suppression de {oldEvents.Count} événements passés depuis plus d'un an :");
            foreach (var evt in oldEvents)
            {
                Console.WriteLine($"🗑️ [{evt.Start:dd/MM/yyyy HH:mm}] {evt.Group} - {evt.Title}");
                knownEvts.Remove(evt);
            }
        }

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        // Note: This block executes once per workflow run. JsonSerializerOptions allocation is acceptable here.
        var json = JsonSerializer.Serialize(knownEvts, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
#pragma warning restore CA1869

        json = utf8CustomEncoder.GetString(utf8CustomEncoder.GetBytes(json));

        var dir = Path.GetDirectoryName(jsonPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(jsonPath, json);
    }
}

/*******************************/
/* Archive old _events/*.html */
/*******************************/
{
    var eventsDir = Path.Combine(repoRoot, "_events");
    var archiveDir = Path.Combine(eventsDir, "archives");
    var oneYearAgo = FrenchLocales.ParisNow.AddYears(-1);

    if (Directory.Exists(eventsDir))
    {
        // Find all .html and .html.skip files
        var allHtmlFiles = Directory.GetFiles(eventsDir, "*.html")
            .Concat(Directory.GetFiles(eventsDir, "*.skip"))
            .ToList();

        var filesToArchive = new List<string>();

        foreach (var file in allHtmlFiles)
        {
            var filename = Path.GetFileName(file);
            
            // Parse date from filename (YYYY-MM-DD-...)
            if (filename.Length >= 10 && DateOnly.TryParse(filename.Substring(0, 10), out var fileDate))
            {
                // Convert DateOnly to DateTimeOffset for comparison
                var fileDateOffset = new DateTimeOffset(fileDate.ToDateTime(TimeOnly.MinValue), FrenchLocales.ParisTimeZone.GetUtcOffset(DateTime.Now));
                
                if (fileDateOffset < oneYearAgo)
                {
                    filesToArchive.Add(file);
                }
            }
        }

        if (filesToArchive.Any())
        {
            Directory.CreateDirectory(archiveDir);
            Console.WriteLine($"📦 Archivage de {filesToArchive.Count} fichiers HTML anciens :");
            
            var eventImgsDir = Path.Combine(repoRoot, "event-imgs");
            
            foreach (var file in filesToArchive)
            {
                var filename = Path.GetFileName(file);
                var destPath = Path.Combine(archiveDir, filename);
                
                // If file already exists in archive, delete it first
                if (File.Exists(destPath))
                    File.Delete(destPath);
                
                File.Move(file, destPath);
                Console.WriteLine($"📦 Archivé: {filename}");
                
                // Extract image filename prefix from HTML filename (remove .html/.skip extension)
                // Image files now use the same naming convention: YYYY-MM-DD-groupId-eventId.*
                var imageFilePrefix = filename.Replace(".html", "").Replace(".skip", "");
                    
                // Delete associated images in event-imgs/
                if (Directory.Exists(eventImgsDir))
                {
                    var imagesToDelete = Directory.GetFiles(eventImgsDir, imageFilePrefix + ".*");
                    foreach (var imgFile in imagesToDelete)
                    {
                        File.Delete(imgFile);
                        Console.WriteLine($"🗑️ Image supprimée: {Path.GetFileName(imgFile)}");
                    }
                }
            }
        }
    }
}

/*******************************/
/* Generate _events/*.html */
/*******************************/
{
    var eventsDir = Path.Combine(repoRoot, "_events");
    Directory.CreateDirectory(eventsDir);
    foreach (var evt in knownEvts.Where(e => e.Start >= DateTime.Today).OrderBy(e => e.Start).ThenBy(e => e.Id))
    {
        var fileNamePrefix = $"{evt.Start:yyyy-MM-dd}-{evt.GroupId}-{evt.Id}";
        var fileName = Path.Combine(eventsDir, $"{fileNamePrefix}.html");

        // on ignore tous les évènements marqués comme "skip"
        if (File.Exists(fileName + ".skip"))
            continue;

        // Ensure LocalImgSrc uses the same naming convention as the HTML file
        evt.SetLocalImgSrcWithNamePrefix(fileNamePrefix);

        using var writer = new StreamWriter(fileName);
        // Write YAML front matter
        writer.WriteLine("---");
        writer.WriteLine($"id: {evt.Id}");
        writer.WriteLine($"title: '{evt.Title.Replace("'", "''")}'");
        writer.WriteLine($"community: {evt.Group}");
        writer.WriteLine($"datePublished: {evt.PublishedOn:yyyy-MM-dd HH:mm}");
        writer.WriteLine($"dateIso: {evt.Start:yyyy-MM-dd HH:mm}");
        writer.WriteLine($"dateFr: {evt.Start:dddd dd MMMM}");
        writer.WriteLine($"timeFr: '{evt.Start:HH:mm}'");
        writer.WriteLine($"place: {evt.VenueName}");
        writer.WriteLine($"placeAddr: {evt.VenueAddr}");
        writer.WriteLine($"link: {evt.Href}");
        writer.WriteLine($"img: {evt.FullImgSrc}");
        writer.WriteLine($"localImg: {evt.LocalImgSrc}");
        writer.WriteLine("---");
        // Write HTML content
        if (evt.HtmlDescription != null)
        {
            writer.Write(evt.HtmlDescription);
        }
    }
}

/***********************/
/*        Code         */
/***********************/

interface IGroup
{
    /// <summary>
    /// Identifiant unique court du groupe.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Nom d'affichage du groupe, disponible uniquement après le scan.
    /// </summary>
    public string? Name { get; }

    IAsyncEnumerable<Event> Scan(IBrowserContext browserContext, IPage page, bool loadPast);
}

partial class MeetupGroup : IGroup
{
    public static int loadTimeoutMs = 30 * 1000;

    public static LoadState loadStateType = LoadState.NetworkIdle;

    public static bool loadTimeoutScreenshots = false;

    private static int screenIndex = 0;


    private static readonly CultureInfo s_frenchLocale = CultureInfo.GetCultureInfo("fr-FR");
    private readonly string _slug;
    private readonly string _url;

    public MeetupGroup(string slug, string id)
    {
        _slug = slug;
        Id = id;
        _url = $"https://www.meetup.com/fr-FR/{slug}";
    }

    public string Id { get; }

    public string? Name { get; private set; }

    public override string ToString() => $"Meetup {_slug}";

    public async IAsyncEnumerable<Event> Scan(IBrowserContext browserContext, IPage page, bool loadPast)
    {
        // on commence par inclure les évènements à venir
        await foreach (var evt in ScanEventPage(browserContext, page))
        {
            yield return evt;
        }

        if (loadPast)
        {
            // on ajoute les évènements passés
            await foreach (var evt in ScanEventPage(browserContext, page, past: true))
            {
                yield return evt;
            }
        }
    }

    /// <summary>
    /// Scanne la page "évènements".
    /// </summary>
    public async IAsyncEnumerable<Event> ScanEventPage(IBrowserContext ctx, IPage page, bool past = false)
    {
        Console.Write($"⏳ Chargement page évènements Meetup {_slug} {(past ? "précédents" : "à venir")}...");
        page.Request += Page_Request;
        page.RequestFinished += Page_RequestFinished;
        await page.GotoAsync($"{_url}/events/?type={(past ? "past" : "upcoming")}");
        try
        {
            await page.WaitForLoadStateAsync(loadStateType, new PageWaitForLoadStateOptions { Timeout = loadTimeoutMs });
            Console.WriteLine($" ✅ terminé");
        }
        catch (TimeoutException)
        {
            if (loadTimeoutScreenshots)
            {
                var bytes = await page.ScreenshotAsync();
                File.WriteAllBytes($"screen{++screenIndex:000}.jpg", bytes);
                Console.WriteLine($" ⚠️ timeout : screen{screenIndex:000}.jpg");
            }
            else
            {
                Console.WriteLine($" 🟧 terminé (timeout)");
            }
        }

        page.Request -= Page_Request;
        page.RequestFinished -= Page_RequestFinished;

        // on récupère le nom du groupe
        Name = (await page.TitleAsync()).Replace(" Événements", "");

        var evtPage = await ctx.NewPageAsync();
        try
        {
            // on parcourt tous les évènements de la liste
            await foreach (var evt in ScanEventPageItems(page, past))
            {
                // on scanne la page de chaque évènement pour obtenir des infos complémentaires
                // UNIQUEMENT sur les évènements à venir : meetup bloque l'affichage des évènements passés pour les non membres
                if (!past)
                {
                    await ScanDetailPage(evtPage, evt);
                }

                yield return evt;
            }
        }
        finally
        {
            await evtPage.CloseAsync();
        }

        void Page_Request(object? sender, IRequest e)
        {
            //Console.WriteLine($"⏳ Requête {e.Url}...");
        }

        void Page_RequestFinished(object? sender, IRequest e)
        {
            //Console.WriteLine($"⏳ Requête {e.Url} terminée...");
        }
    }

    private async IAsyncEnumerable<Event> ScanEventPageItems(IPage page, bool past)
    {
        var items = (await page.Locator("div[data-eventref]").AllAsync()).ToList();
        foreach (var item in items)
        {
            var testId = await item.GetAttributeAsync("data-testid");
            if (testId == "similar-events-card")
                continue;

            Event? eventItem = null;

            try
            {
                eventItem = await ScanEventPageItem(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur de lecture de l'évènement, abandon...");
                Console.WriteLine(ex.ToString());
            }

            if (eventItem != null)
                yield return eventItem;
        }
    }

    private async Task<Event?> ScanEventPageItem(ILocator item)
    {
        Console.WriteLine($"🤖 +1 évènement détecté...");

        var link = item.Locator("a[data-event-label]");

        // ex: https://www.meetup.com/javascript-and-co/events/305479083/?eventOrigin=group_events_list
        var url = await link.GetAttributeAsync("href");
        if (url == null)
        {
            Console.WriteLine($"⚠️ Impossible de détecter l'url, abandon...");
            return null;
        }

        // normalization
        url = url.Replace("?eventOrigin=group_events_list", "");
        if (!url.Contains("/fr-FR/")) url = url.Replace("meetup.com/", "meetup.com/fr-FR/");

        // https://www.meetup.com/fr-FR/javascript-and-co/events/305479083/ => 305479083
        var id = new Uri(url).Segments.Last().TrimEnd('/');

        // Titre
        var title = await item.Locator("h3").InnerTextAsync();
        Console.WriteLine($"🤖 Titre = {title}");

        // 2025-10-07T18:30:00+02:00[Europe/Paris]
        // 2025-11-10T19:00:00+01:00
        // 2026-01-28T17:45:00+01:00[Europe/Paris]
        // TODO Scan standardized attribute
        var timeElt = item.Locator("time");
        var timeAttr = await timeElt.GetAttributeAsync("datetime") ?? string.Empty;
        var regexMatch = false;

        // Extract datetime before optional [Europe/Paris] suffix
        var bracketIndex = timeAttr.IndexOf('[');
        var coreTimeAttr = bracketIndex >= 0 ? timeAttr[..bracketIndex].Trim() : timeAttr;

        if (coreTimeAttr.Length > 0 && DateTimeOffset.TryParse(coreTimeAttr, out var time))
        {
            // success
            if (timeAttr.EndsWith("[Europe/Paris]") && !FrenchLocales.ParisTimeZone.IsDaylightSavingTime(time))
            {
                // Meetup renvoie la mauvaise timezone pour les dates hors DST, on corrige en ajoutant une heure
                // ex: 2026-01-28T17:45:00+01:00[Europe/Paris] pour un évènement à 18h45 heure locale paris
                // à priori cela ne bug que pour l'hiver (heure standard)
                time = time.AddHours(1);
            }

            regexMatch = true;
        }
        else
        {
            // ex: JEU. 6 FÉVR. 2025, 18:30 CET
            // ex: JEU. 6 FÉVR. 2025, 18:30 CEST
            // ex: JEU. 6 FÉVR. 2025, 18:30 UTC+1
            // ex: JEU. 6 FÉVR. 2025, 18:30 UTC+2
            Console.WriteLine($"⚠️ Impossible de détecter la date/heure dans l'attribut {timeAttr}, parsing alternatif...");
            var dateTimeText = await timeElt.InnerTextAsync();
            var match = TimeZoneRegex().Match(dateTimeText);
            int offset;
            if (match.Success)
            {
                // extract timezone offset
                offset = int.Parse(match.Groups[1].Value);

                // remove timezone info from time text
                dateTimeText = dateTimeText[..^match.Length];
            }
            else if (dateTimeText.EndsWith(" CET"))
            {
                // Central European Time => UTC+1
                offset = 1;
                dateTimeText = dateTimeText[..^4];
            }
            else if (dateTimeText.EndsWith(" CEST"))
            {
                // Central European Summer Time => UTC+2
                offset = 2;
                dateTimeText = dateTimeText[..^5];
            }
            else if (dateTimeText.EndsWith(" UTC"))
            {
                offset = 0;
                dateTimeText = dateTimeText[..^4];
            }
            else
            {
                Console.WriteLine($"⚠️ Impossible de parser la date/heure {dateTimeText}, abandon...");
                return null;
            }

            // reconstruct date + time
            time = new(DateTime.Parse(dateTimeText, s_frenchLocale), TimeSpan.FromHours(offset));

            // ajust to locale timezone
            time = time.ToOffset(FrenchLocales.ParisTimeZone.GetUtcOffset(time.UtcDateTime));
        }

        Console.WriteLine($"🤖 Date/heure = {time} (raw {timeAttr}, regex? {regexMatch})");

        // try find image
        var imgs = item.Locator($"img[fetchpriority='high']");
        string? img = null;
        if (await imgs.CountAsync() != 0)
        {
            img = await imgs.GetAttributeAsync("src");
        }

        return new()
        {
            Id = $"meetup-{id}",
            PublishedOn = DateTimeOffset.UtcNow,
            Href = url,
            Title = title,
            GroupId = Id,
            Group = Name!,
            Start = time,
            ImgSrc = img
        };
    }

    private static async Task ScanDetailPage(IPage evtPage, Event evt)
    {
        Console.Write($"⏳ Chargement page détail évènement {evt.Title}...");
        await evtPage.GotoAsync(evt.Href);
        try
        {
            await evtPage.WaitForLoadStateAsync(loadStateType, new PageWaitForLoadStateOptions { Timeout = loadTimeoutMs });
            Console.WriteLine($" ✅ terminé");
        }
        catch (TimeoutException)
        {
            if (loadTimeoutScreenshots)
            {
                var bytes = await evtPage.ScreenshotAsync();
                File.WriteAllBytes($"screen{++screenIndex:000}.jpg", bytes);
                Console.WriteLine($" ⚠️ timeout : screen{screenIndex:000}.jpg");
            }
            else
            {
                Console.WriteLine($" 🟧 terminé (timeout)");
            }
        }

        // full image, optionnel
        var imgs = evtPage.Locator("main aside img[fetchpriority='high'].object-center");
        if (await imgs.CountAsync() != 0)
        {
            evt.FullImgSrc = await imgs.First.GetAttributeAsync("src");
        }

        var loc = evtPage.Locator("main section div.flex.flex-col.items-start.gap-ds2-16");
        evt.HtmlDescription = await loc.InnerHTMLAsync();
        evt.TextDescription = await loc.TextContentAsync();

        var venueLocator = evtPage.Locator("main aside img[alt='pin icon'] + div > p");
        if (await venueLocator.CountAsync() != 0)
        {
            //var venueLink = await venueLocator.Locator().GetAttributeAsync("href");
            //if (venueLink != null && venueLink.StartsWith("https://www.google.com/maps"))
            //{
            //    var match = GoogleMapsCoordsRegex().Match(venueLink);
            //    if (match.Success)
            //    {
            //        evt.Latitude = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            //        evt.Longitude = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            //    }
            //}

            var items = (await venueLocator.AllAsync()) ?? [];

            evt.VenueName = "";
            evt.VenueAddr = "";

            if (items.Count > 0)
            {
                evt.VenueName = await items[0].InnerTextAsync();
            }

            if (items.Count > 1)
            {
                evt.VenueAddr = await items[1].InnerTextAsync();
                evt.VenueAddr = evt.VenueAddr.Replace(" · ", " ");
            }
        }
    }

    [GeneratedRegex(@"query=(-?[\d\.]+)%2C%20(-?[\d\.]+)$", RegexOptions.IgnoreCase, "fr-FR")]
    private static partial Regex GoogleMapsCoordsRegex();

    [GeneratedRegex(@" UTC([+-]\d{1,2})$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex TimeZoneRegex();
}

partial class ToulouseGameDevGroup : IGroup
{
    private const string Url = "https://toulousegamedev.fr/evenements/";

    public string Id => "tgd";

    public string Name => "Toulouse Game Dev";

    public async IAsyncEnumerable<Event> Scan(IBrowserContext browserContext, IPage _, bool loadPast)
    {
        Console.WriteLine($"🤖 Début scan page Toulouse Game Dev");

        var page = await browserContext.NewPageAsync();

        try
        {
            await page.GotoAsync(Url);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15 * 1000 });

            // siblings blocks after the first h2
            var loc = page.Locator("css=h2.wp-block-heading:nth-of-type(1) + div");

            foreach (var item in await loc.AllAsync())
            {
                // figure > img
                var img = await item.Locator("css=figure > img").GetAttributeAsync("src");

                // title
                var title = await item.Locator("css=div > p.has-large-font-size").InnerTextAsync();

                // description + date
                List<string> descHtml = [];
                List<string> destText = [];
                DateTimeOffset? date = null;
                TimeSpan? duration = null;

                foreach (var paragraph in await item.Locator("css=div > p:not(.has-large-font-size)").AllAsync())
                {
                    var text = await paragraph.InnerTextAsync();

                    // Jeudi 17 Avril 2025: 18h30-22h30 => OK [ "Jeudi", "17", "Avril", "2025" "18h30-22h30" ]
                    // Mardi 21 Janvier 2024 : 18h30-22h30 => OK [ "Mardi", "21", "Janvier", "2024", ":", "18h30-22h30" ]
                    // Jeudi 12 Décembre 2024 : 18h30-22h30 => OK
                    // Les 20 et 21 Septembre 2024 => KO
                    var words = text.Trim().Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);

                    // [ "Mardi", "21", "Janvier", "2024" ]
                    var datePart = string.Join(' ', words.Take(4));

                    if (DateTimeOffset.TryParseExact(datePart.ToLowerInvariant(), "dddd dd MMMM yyyy", FrenchLocales.FrenchCultureInfo, DateTimeStyles.AssumeLocal, out var dateOnly))
                    {
                        // ex: 18h30-22h30 => [ "18", "30", "22", "30" ]
                        var timeWord = words.Skip(5).FirstOrDefault() ?? "";
                        var timeMatch = DurationRegex().Match(timeWord);
                        if (timeMatch.Success)
                        {
                            TimeSpan startTime = new(int.Parse(timeMatch.Groups[1].Value), int.Parse(timeMatch.Groups[2].Value), 0); // 18h30m00s
                            TimeSpan endTime = new(int.Parse(timeMatch.Groups[3].Value), int.Parse(timeMatch.Groups[4].Value), 0); // 22h30m00s
                            duration = endTime - startTime; // 4h
                            date = new(dateOnly.Year, dateOnly.Month, dateOnly.Day, startTime.Hours, startTime.Minutes, 0, FrenchLocales.ParisTimeZone.GetUtcOffset(dateOnly));
                        }
                    }
                    else
                    {
                        destText.Add(text);
                        var html = await paragraph.InnerHTMLAsync();
                        descHtml.Add($"<p>{html}</p>");
                    }
                }

                if (date != null)
                {
                    yield return new()
                    {
                        Id = $"tgd-{date:yyyy-MM-dd}",
                        Href = @"https://toulousegamedev.fr/evenements/",
                        PublishedOn = DateTimeOffset.UtcNow,
                        GroupId = Id,
                        Group = Name,
                        Title = title,
                        ImgSrc = img,
                        Start = date.Value,
                        Duration = duration,
                        HtmlDescription = string.Join('\n', descHtml)
                    };
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [GeneratedRegex(@"(\d+)h(\d+)-(\d+)h(\d+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();
}

sealed class Event : IComparable<Event>
{
    private string? _smallImgSrc;

    private string? _largeImgSrc;

    /// <summary>Id unique interne.</summary>
    public required string Id { get; set; }

    /// <summary>Date et heure à laquelle l'évènement a été publié pour la première fois.</summary>
    public DateTimeOffset PublishedOn { get; set; }

    public required string Href { get; set; }

    public required string Title { get; set; }

    /// <summary>Date et heure de début de l'évènement.</summary>
    public required DateTimeOffset Start { get; set; }

    /// <summary>URL de l'image par défaut de l'évènement.</summary>
    /// <remarks>LocalImgSrc will be computed using SetLocalImgSrcWithNamePrefix() with the filename prefix</remarks>
    public required string? ImgSrc
    {
        get => _smallImgSrc;
        set
        {
            if (_smallImgSrc != value)
            {
                _smallImgSrc = value;
                // Note: LocalImgSrc will be set properly using SetLocalImgSrcWithNamePrefix() with the filename prefix
            }
        }
    }

    /// <summary>Nom public du groupe.</summary>
    public required string Group { get; set; }

    /// <summary>Id du groupe.</summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Indique si l'évènement a été chargé depuis les évènements passés.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsPast => Start < FrenchLocales.ParisNow;

    /// <summary>Durée de l'évènement si dispo.</summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>URL de l'image de grande taille associée, si dispo en plus de l'image par défaut <see cref="ImgSrc"/>.</summary>
    /// <remarks>LocalImgSrc will be computed using SetLocalImgSrcWithNamePrefix() with the filename prefix</remarks>
    public string? FullImgSrc
    {
        get => _largeImgSrc;
        set
        {
            if (_largeImgSrc != value)
            {
                _largeImgSrc = value;
                // Note: LocalImgSrc will be set properly using SetLocalImgSrcWithNamePrefix() with the filename prefix
            }
        }
    }

    /// <summary>Description HTML de l'évènement.</summary>
    public string? HtmlDescription { get; set; }

    public string? TextDescription { get; set; }

    /// <summary>Nom du lieu</summary>
    public string? VenueName { get; set; }

    /// <summary>Adresse du lieu</summary>
    public string? VenueAddr { get; set; }

    /// <summary>Coordonnées GPS Latitude</summary>
    public float? Latitude { get; set; }

    /// <summary>Coordonnées GPS Longitude</summary>
    public float? Longitude { get; set; }

    /// <summary>URL du fichier local contenant l'image de l'évènement.</summary>
    public string? LocalImgSrc { get; set; }

    /// <summary>Set LocalImgSrc using a filename prefix (typically YYYY-MM-DD-groupId-eventId)</summary>
    public void SetLocalImgSrcWithNamePrefix(string prefix)
    {
        if (!string.IsNullOrEmpty(_largeImgSrc))
        {
            LocalImgSrc = $"event-imgs/{prefix}{Path.GetExtension(new Uri(_largeImgSrc).AbsolutePath)}";
        }
        else if (!string.IsNullOrEmpty(_smallImgSrc))
        {
            LocalImgSrc = $"event-imgs/{prefix}{Path.GetExtension(new Uri(_smallImgSrc).AbsolutePath)}";
        }
    }

    public int CompareTo(Event? other)
    {
        var x = Start.CompareTo(other?.Start ?? DateTimeOffset.MinValue);
        if (x != 0)
            return x;

        return Id.CompareTo(other?.Id ?? string.Empty);
    }

    public override string ToString() => $"[{Group}] - {Start:dd/MM/yyyy à HH:mm} - {Title}";
}

static class FrenchLocales
{
    public static readonly CultureInfo FrenchCultureInfo = CultureInfo.GetCultureInfo("fr-FR");

    public static readonly TimeZoneInfo ParisTimeZone = TimeZoneInfo.FromSerializedString("Romance Standard Time;60;(UTC+01:00) Brussels, Copenhagen, Madrid, Paris;Romance Standard Time;Romance Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");

    public static DateTimeOffset ParisNow => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ParisTimeZone);
}

static class Extensions
{
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> @this, Func<bool> predicate, T obj)
    {
        if (!predicate())
        {
            return @this;
        }

        return @this.Append(obj);
    }

    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> @this, Func<bool> predicate, Func<T> obj)
    {
        if (!predicate())
        {
            return @this;
        }

        return @this.Append(obj());
    }
}
