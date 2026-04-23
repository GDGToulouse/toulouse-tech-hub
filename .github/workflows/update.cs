#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property PublishTrimmed=false
#:property PublishAot=false
#:property EnableTrimAnalyzer=false

#:package Microsoft.Playwright@1.50.0

using Microsoft.Playwright;
using System.Globalization;
using System.Text;
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
    new MeetupGroup("afup-toulouse", "afup-toulouse"),
    new MeetupGroup("agile-toulouse", "agile"),
    new MeetupGroup("artilect-fablab", "artilect"),
    new MeetupGroup("toulouse-amazon-web-services", "aws"),
    new MeetupGroup("ateliers-cpp-toulouse", "cpp"),
    new MeetupGroup("meetup-visualisation-des-donnees-toulouse", "dataviz"),
    new MeetupGroup("toulouse-devops", "devops"),
    new MeetupGroup("devops-cloud-toulouse", "devops-cloud"),
    new MeetupGroup("gdg-toulouse", "gdg"),
    new MeetupGroup("javascript-and-co", "js-and-co"),
    new MeetupGroup("toulouse-java-user-group", "jug"),
    new MeetupGroup("mtg-toulouse", "mtg"),
    new MeetupGroup("mug-toulouse-mobile-user-group", "mug-toulouse"),
    new MeetupGroup("postgres-toulouse", "postgres"),
    new MeetupGroup("python-toulouse", "python"),
    new MeetupGroup("rust-community-toulouse", "rust"),
    new MeetupGroup("swift-toulouse", "swift"),
    new MeetupGroup("tlse-data-science", "tds"),
    new MeetupGroup("tech-a-break", "tech-a-break"),
    new MeetupGroup("latoulboxducloudnatif", "toulbox"),
    new MeetupGroup("wptoulouse", "wordpress-toulouse")
    //new LinkedInGroup("embedded-meetup-toulouse", "embedded") non fonctionnel car pages non visibles sans compte
};

/************************/
/* find root directory  */
/************************/
var repoRoot = Directory.GetCurrentDirectory();
{
    var current = repoRoot;
    while (!string.IsNullOrEmpty(current))
    {
        if (Directory.Exists(Path.Combine(current, "_events")))
            break;

        current = Directory.GetParent(current)?.FullName;
    }

    repoRoot = current ?? repoRoot;
}

/****************************/
/*   load events from YAML  */
/****************************/
List<Event> knownEvts = [];
{
    var eventsDir = Path.Combine(repoRoot, "_events");
    if (Directory.Exists(eventsDir))
    {
        var eventFiles = Directory.GetFiles(eventsDir, "*.html");
        foreach (var file in eventFiles)
        {
            var evt = ParseEventFromYaml(file);
            if (evt != null)
                knownEvts.Add(evt);
        }
        Console.WriteLine($"⚗️ {knownEvts.Count} événements lus depuis fichiers YAML");
    }

    static Event? ParseEventFromYaml(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 3 || lines[0] != "---")
                return null;

            var yamlEnd = Array.IndexOf(lines, "---", 1);
            if (yamlEnd == -1)
                return null;

            var yaml = new Dictionary<string, string>();
            for (int i = 1; i < yamlEnd; i++)
            {
                var line = lines[i];
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');
                    yaml[key] = value;
                }
            }

            // Extract HTML content after second ---
            var htmlContent = string.Join("\n", lines.Skip(yamlEnd + 1));

            return new Event
            {
                Id = yaml.GetValueOrDefault("eventId", ""),
                GroupId = yaml.GetValueOrDefault("groupId", ""),
                Title = yaml.GetValueOrDefault("title", ""),
                Group = yaml.GetValueOrDefault("community", ""),
                Href = yaml.GetValueOrDefault("link", ""),
                ImgSrc = yaml.GetValueOrDefault("img", ""),
                VenueName = yaml.GetValueOrDefault("place", ""),
                VenueAddr = yaml.GetValueOrDefault("placeAddr", ""),
                Start = DateTimeOffset.TryParse(yaml.GetValueOrDefault("dateIso", ""), out var start) ? start : DateTimeOffset.MinValue,
                PublishedOn = DateTimeOffset.TryParse(yaml.GetValueOrDefault("datePublished", ""), out var pub) ? pub : DateTimeOffset.MinValue,
                HtmlDescription = htmlContent
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erreur lors du parsing de {Path.GetFileName(filePath)}: {ex.Message}");
            return null;
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
                Console.WriteLine($"🔹💥 [{evt.Start:dd/MM/yyyy HH:mm}] {evt.Group} - {evt.Title}");

                evts.Add(evt);

                // téléchargement de l'image => event-imgs/YYYY-MM-DD-groupId-eventId.webp
                // Mise à jour basée sur Last-Modified: télécharge si source plus récente que le fichier local
                if (!string.IsNullOrEmpty(evt.ImgSrc))
                {
                    var fileNamePrefix = $"{evt.Start:yyyy-MM-dd}-{evt.GroupId}-{evt.Id}";
                    var localPath = Path.Combine(repoRoot, $"event-imgs/{fileNamePrefix}.webp");
                    var imgUrl = evt.ImgSrc;

                    if (imgUrl != null)
                    {
                        var shouldDownload = false;
                        if (!File.Exists(localPath))
                        {
                            shouldDownload = true;
                        }
                        else
                        {
                            // Check if source image has been updated by comparing Last-Modified headers
                            try
                            {
                                var headRequest = new HttpRequestMessage(HttpMethod.Head, imgUrl);
                                var response = await http.SendAsync(headRequest);

                                // Try to get Last-Modified from response headers (generic headers, not content-specific)
                                if (response.Headers.TryGetValues("Last-Modified", out var lastModifiedValues) && lastModifiedValues.FirstOrDefault() is string lastModStr)
                                {
                                    if (DateTimeOffset.TryParse(lastModStr, out var remoteFileTime))
                                    {
                                        var localFileTime = File.GetLastWriteTimeUtc(localPath);
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
                            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
                            await File.WriteAllBytesAsync(localPath, await http.GetByteArrayAsync(imgUrl));
                        }
                    }
                }
            }
        }
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
        // Find all .html files and .html.skip files
        var allHtmlFiles = Directory.GetFiles(eventsDir, "*.html")
            .Concat(Directory.GetFiles(eventsDir, "*.html.skip"))
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
                Console.WriteLine($"🔹📦 Archivé: {filename}");

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
                        Console.WriteLine($"🔹🗑️ Image supprimée: {Path.GetFileName(imgFile)}");
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
    foreach (var evt in evts.Where(e => e.Start >= DateTime.Today).OrderBy(e => e.Start).ThenBy(e => e.Id))
    {
        var fileNamePrefix = $"{evt.Start:yyyy-MM-dd}-{evt.GroupId}-{evt.Id}";
        var fileName = Path.Combine(eventsDir, $"{fileNamePrefix}.html");

        // on ignore tous les évènements marqués comme "skip"
        if (File.Exists(fileName + ".skip"))
            continue;

        var knownEvt = knownEvts.FirstOrDefault(e => e.Id == evt.Id);
        if (knownEvt != null)
        {
            var knownFileNamePrefix = $"{knownEvt.Start:yyyy-MM-dd}-{knownEvt.GroupId}-{knownEvt.Id}";

            if (knownFileNamePrefix != fileNamePrefix)
            {
                // on ignore tous les évènements marqués comme "skip"
                if (File.Exists(knownFileNamePrefix + ".skip"))
                    continue;

                // le nom du fichier à changé (ex: date modifiée), on renomme l'ancien fichier pour éviter les doublons
                var knownFileName = Path.Combine(eventsDir, $"{knownFileNamePrefix}.html");
                File.Move(knownFileName, fileName);
                Console.WriteLine($"🔹🔄 Renommé: {Path.GetFileName(knownFileName)} -> {Path.GetFileName(fileName)}");
            }

            evt.PublishedOn = knownEvt.PublishedOn; // on conserve la date de publication initiale pour éviter les changements inutiles dans le YAML
        }

        using var writer = new StreamWriter(fileName);
        // Write YAML front matter
        writer.WriteLine("---");
        writer.WriteLine($"eventId: {evt.Id}");
        writer.WriteLine($"groupId: {evt.GroupId}");
        writer.WriteLine($"title: \"{evt.Title.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
        writer.WriteLine($"community: \"{evt.Group.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
        writer.WriteLine($"datePublished: {evt.PublishedOn:yyyy-MM-dd HH:mm}");
        writer.WriteLine($"dateIso: {evt.Start:yyyy-MM-dd HH:mm}");
        writer.WriteLine($"dateFr: {evt.Start:dddd d MMMM}");
        writer.WriteLine($"timeFr: '{evt.Start:HH:mm}'");
        writer.WriteLine($"place: \"{(evt.VenueName ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
        writer.WriteLine($"placeAddr: \"{(evt.VenueAddr ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
        writer.WriteLine($"link: {evt.Href}");
        writer.WriteLine($"img: {evt.ImgSrc}");
        writer.WriteLine("---");
        // Write HTML content
        if (evt.HtmlDescription != null)
        {
            writer.Write(evt.HtmlDescription);
        }

        Console.WriteLine($"🛒 Généré: {fileName}");
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
            Console.WriteLine($"🔹⚠️ Impossible de détecter l'url, abandon...");
            return null;
        }

        // normalization
        url = url.Replace("?eventOrigin=group_events_list", "");
        if (!url.Contains("/fr-FR/")) url = url.Replace("meetup.com/", "meetup.com/fr-FR/");

        // https://www.meetup.com/fr-FR/javascript-and-co/events/305479083/ => 305479083
        var id = new Uri(url).Segments.Last().TrimEnd('/');

        // Titre
        var title = await item.Locator("h3").InnerTextAsync();
        Console.WriteLine($"🔹🤖 Titre = {title}");

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
                // affichage du fuseau courant
                Console.WriteLine($"🔹⌚ Fuseau courant : {TimeZoneInfo.Local.BaseUtcOffset} / {TimeZoneInfo.Local.Id}");
                if (TimeZoneInfo.Local.Id == "Etc/UTC")
                {
                    // Meetup renvoie la mauvaise timezone pour les dates hors DST, on corrige en ajoutant une heure
                    // ex: 2026-01-28T17:45:00+01:00[Europe/Paris] pour un évènement à 18h45 heure locale paris
                    // (à priori cela ne bug que pour l'hiver (heure standard) ?)
                    time = time.AddHours(1);
                }
            }

            regexMatch = true;
        }
        else
        {
            // ex: JEU. 6 FÉVR. 2025, 18:30 CET
            // ex: JEU. 6 FÉVR. 2025, 18:30 CEST
            // ex: JEU. 6 FÉVR. 2025, 18:30 UTC+1
            // ex: JEU. 6 FÉVR. 2025, 18:30 UTC+2
            Console.WriteLine($"🔹⚠️ Impossible de détecter la date/heure dans l'attribut {timeAttr}, parsing alternatif...");
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
                Console.WriteLine($"🔹⚠️ Impossible de parser la date/heure {dateTimeText}, abandon...");
                return null;
            }

            // reconstruct date + time
            time = new(DateTime.Parse(dateTimeText, s_frenchLocale), TimeSpan.FromHours(offset));

            // ajust to locale timezone
            time = time.ToOffset(FrenchLocales.ParisTimeZone.GetUtcOffset(time.UtcDateTime));
        }

        Console.WriteLine($"🔹🤖 Date/heure = {time} (raw {timeAttr}, regex? {regexMatch})");

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
        Console.Write($"🔹⏳ Chargement page détail évènement {evt.Title}...");
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
            evt.ImgSrc = await imgs.First.GetAttributeAsync("src");
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

partial class LinkedInGroup : IGroup
{
    private readonly string _slug;

    private readonly string _url;

    public LinkedInGroup(string slug, string id)
    {
        _slug = slug;
        Name = slug;
        Id = id;
        _url = $"https://www.linkedin.com/company/{slug}/events/";
    }

    public string Id { get; }

    public string Name { get; private set; }

    public async IAsyncEnumerable<Event> Scan(IBrowserContext browserContext, IPage page, bool loadPast)
    {
        Console.WriteLine($"🤖 Début scan page LinkedIn {_slug}");
        await page.GotoAsync(_url);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15 * 1000 });

        // ☠️ ne fonctionne pas car LinkedIn nécessite d'être connecté pour visualiser les pages
        yield break;

        // on récupère le nom du groupe
        Name = await page.TitleAsync();
        Console.WriteLine("🤖 Titre de la page = " + Name);

        foreach (var section in (await page.Locator("section.artdeco-card").AllAsync()).ToList())
        {
            var heading = await section.Locator("h4").InnerTextAsync();
            if (heading == "Événements à venir")
            {
                Console.WriteLine($"🤖 Section évènements à venir détectée, scan en cours...");
                foreach (var li in (await section.Locator("li").AllAsync()).ToList())
                {
                    var link = await li.Locator("a").First.GetAttributeAsync("href"); // ex: /events/toulouseembeddedmeetup-31mars207431788648037199872/
                    var img = await li.Locator("img").First.GetAttributeAsync("src"); // ex: https://media.licdn.com/dms/image/v2/D4E1EAQH4QTd5VjFTpQ/event-background-image-crop_270_480/B4EZyMEX3THoAQ-/0/1771876489167?e=1772697600&v=beta&t=CkDSaTI1pxDRSxp8ppZgmQsd4D7nYUE7fIjo_PI04r0
                    var title = await li.Locator("span.events-components-shared-event-card__event-title").First.InnerTextAsync(); // ex: Toulouse Embedded Meetup: 31 mars 2026
                    var timeText = await li.Locator("span.events-components-shared-event-card__text-single-line").First.InnerTextAsync(); // ex: mar., 31 mars 2026, 19:00

                    Console.WriteLine("🤖 Évènement détecté :");
                    Console.WriteLine($"🔹 Titre : {title}");
                    Console.WriteLine($"🔹 Date : {timeText}");
                    Console.WriteLine($"🔹 Lien : {link}");
                    Console.WriteLine($"🔹 Image : {img}");

                }
            }
            else
            {
                Console.WriteLine($"🤖 Section avec heading '{heading}' ignorée");
            }
        }

        yield break;
    }
}

partial class ToulouseGameDevGroup : IGroup
{
    private const string Url = "https://toulousegamedev.fr/evenements/";

    /// <summary>Lieu habituel des meet-ups TGD, utilisé par défaut si aucun autre lieu n'est mentionné.</summary>
    private const string DefaultVenueName = "La Mêlée";
    private const string DefaultVenueAddr = "27 rue d'Aubuisson, 31000 Toulouse";

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

            // Scan all media-text blocks to capture any number of upcoming event months
            var loc = page.Locator("css=div.wp-block-media-text");

            foreach (var item in await loc.AllAsync())
            {
                // figure > img
                var imgLocator = item.Locator("css=figure img");
                var img = await imgLocator.CountAsync() > 0 ? await imgLocator.First.GetAttributeAsync("src") : null;

                // title
                var titleLocator = item.Locator("css=.wp-block-media-text__content p.has-large-font-size");
                if (await titleLocator.CountAsync() == 0) continue;
                var title = await titleLocator.First.InnerTextAsync();

                // description + date
                List<string> descHtml = [];
                DateTimeOffset? date = null;
                TimeSpan? duration = null;
                string? venueName = null;
                string? venueAddr = null;

                foreach (var paragraph in await item.Locator("css=.wp-block-media-text__content p:not(.has-large-font-size)").AllAsync())
                {
                    var text = await paragraph.InnerTextAsync();

                    // Formats attendus (avec ou sans année) :
                    // "Jeudi 17 Avril 2025: 18h30-22h30"  => OK [ "Jeudi", "17", "Avril", "2025", "18h30-22h30" ]
                    // "Mardi 21 Janvier 2024 : 18h30-22h30" => OK [ "Mardi", "21", "Janvier", "2024", "18h30-22h30" ]
                    // "Mardi 17 Mars : 18h30-22h30" (sans année) => OK [ "Mardi", "17", "Mars", "18h30-22h30" ]
                    // "Les 20 et 21 Septembre 2024" => KO
                    var words = text.Trim().Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);

                    bool parsedDate = false;

                    // Essai avec année : "Mardi 17 Avril 2025" (4 mots)
                    var datePart4 = string.Join(' ', words.Take(4));
                    if (DateTimeOffset.TryParseExact(datePart4.ToLowerInvariant(), "dddd dd MMMM yyyy", FrenchLocales.FrenchCultureInfo, DateTimeStyles.AssumeLocal, out var dateWith4))
                    {
                        // le mot suivant (index 4) contient l'horaire ex: 18h30-22h30
                        var timeWord = words.Skip(4).FirstOrDefault() ?? "";
                        var timeMatch = DurationRegex().Match(timeWord);
                        if (timeMatch.Success)
                        {
                            TimeSpan startTime = new(int.Parse(timeMatch.Groups[1].Value), int.Parse(timeMatch.Groups[2].Value), 0);
                            TimeSpan endTime = new(int.Parse(timeMatch.Groups[3].Value), int.Parse(timeMatch.Groups[4].Value), 0);
                            duration = endTime - startTime;
                            date = new(dateWith4.Year, dateWith4.Month, dateWith4.Day, startTime.Hours, startTime.Minutes, 0, FrenchLocales.ParisTimeZone.GetUtcOffset(dateWith4));
                        }
                        parsedDate = true;
                    }

                    // Essai sans année : "Mardi 17 Mars" (3 mots) — inférer l'année courante
                    if (!parsedDate)
                    {
                        var datePart3 = string.Join(' ', words.Take(3));
                        if (DateTimeOffset.TryParseExact(datePart3.ToLowerInvariant(), "dddd dd MMMM", FrenchLocales.FrenchCultureInfo, DateTimeStyles.AssumeLocal, out var dateWith3))
                        {
                            // Inférer l'année : si la date est plus de 6 mois dans le passé, supposer l'année suivante.
                            // Seuil de 6 mois pour tolérer les évènements récemment passés (ex : page TGD qui liste
                            // encore les derniers mois) sans basculer trop tôt vers l'année suivante (ex : un évènement
                            // de novembre/décembre scanné en janvier de l'année suivante reste correctement daté).
                            int year = FrenchLocales.ParisNow.Year;
                            int month = dateWith3.Month;
                            int day = dateWith3.Day;

                            // Comparaison pour l'heuristique d'année en date locale (sans dépendre du décalage de "now").
                            var tentativeLocalDate = new DateTime(year, month, day);
                            if (tentativeLocalDate < FrenchLocales.ParisNow.AddMonths(-6).Date)
                                year++;

                            // Maintenant que l'année est inférée, calculer le décalage UTC sur la date de l'évènement.
                            var eventDateForOffset = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
                            var offset = FrenchLocales.ParisTimeZone.GetUtcOffset(eventDateForOffset);
                            var tentativeDate = new DateTimeOffset(year, month, day, 0, 0, 0, offset);

                            // le mot suivant (index 3) contient l'horaire ex: 18h30-22h30
                            var timeWord = words.Skip(3).FirstOrDefault() ?? "";
                            var timeMatch = DurationRegex().Match(timeWord);
                            if (timeMatch.Success)
                            {
                                TimeSpan startTime = new(int.Parse(timeMatch.Groups[1].Value), int.Parse(timeMatch.Groups[2].Value), 0);
                                TimeSpan endTime = new(int.Parse(timeMatch.Groups[3].Value), int.Parse(timeMatch.Groups[4].Value), 0);
                                duration = endTime - startTime;
                                date = new(year, month, day, startTime.Hours, startTime.Minutes, 0, offset);
                            }
                            parsedDate = true;
                        }
                    }

                    if (!parsedDate)
                    {
                        var trimmed = text.Trim();
                        // Détection d'une ligne de lieu : ex. "Au LEVEL UP, 96 Bd Pierre et Marie Curie, Toulouse"
                        // Indicateurs : début par "Au "/"À "/"Chez " ou présence d'un numéro de voirie
                        if (venueName == null && VenueLineRegex().IsMatch(trimmed))
                        {
                            var commaIdx = trimmed.IndexOf(',');
                            if (commaIdx > 0)
                            {
                                venueName = trimmed[..commaIdx].Trim();
                                venueAddr = trimmed[(commaIdx + 1)..].Trim();
                            }
                            else
                            {
                                venueName = trimmed;
                            }
                            // La ligne de lieu fait aussi partie de la description
                            var html = await paragraph.InnerHTMLAsync();
                            descHtml.Add($"<p>{html}</p>");
                        }
                        else
                        {
                            var html = await paragraph.InnerHTMLAsync();
                            descHtml.Add($"<p>{html}</p>");
                        }
                    }
                }

                if (date != null && (loadPast || date.Value > FrenchLocales.ParisNow))
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
                        VenueName = venueName ?? DefaultVenueName,
                        VenueAddr = venueAddr ?? DefaultVenueAddr,
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

    /// <summary>
    /// Détecte une ligne de lieu/adresse dans le texte de l'évènement.
    /// Exemples reconnus : "Au LEVEL UP, 96 Bd Pierre et Marie Curie, Toulouse"
    ///                     "27 rue d'Aubuisson, 31000 Toulouse"
    /// Les deux alternatives sont ancrées en début de ligne pour éviter les faux positifs.
    /// </summary>
    [GeneratedRegex(@"^\s*(?:(Au |À |Chez )|\d+[\s,]+(rue|bd|boulevard|allée|av\.?|avenue|chemin|impasse|place|square)\b)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VenueLineRegex();
}

sealed class Event : IComparable<Event>
{
    /// <summary>Id unique interne.</summary>
    public required string Id { get; set; }

    /// <summary>Date et heure à laquelle l'évènement a été publié pour la première fois.</summary>
    public DateTimeOffset PublishedOn { get; set; }

    public required string Href { get; set; }

    public required string Title { get; set; }

    /// <summary>Date et heure de début de l'évènement.</summary>
    public required DateTimeOffset Start { get; set; }

    /// <summary>URL de l'image par défaut de l'évènement.</summary>
    public required string? ImgSrc { get; set; }

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
