#:package Microsoft.Playwright@1.63.0
#:property PublishAot=false
#:property PublishTrimmed=false
#:property EnableTrimAnalyzer=false

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Playwright;

var assembly = Assembly.LoadFrom(Path.GetFullPath(args.Single()));
var groupType = assembly.GetType("MeetupGroup", throwOnError: true)!;
var eventType = assembly.GetType("Event", throwOnError: true)!;
groupType.GetField("loadTimeoutScreenshots")!.SetValue(null, false);

var checks = 0;
var success = await Scan(new() { Cards = [new("1")], Details = { ["1"] = new(true) } });
Check(success.Events.Count == 1 && Get<string?>(success.Events[0], "HtmlDescription") == "<p>detail 1</p>",
    "Successful list and detail navigation parses event content");
Check(success.ListPage.GotoCalls == 1 && success.ListPage.LoadCalls == 1 &&
    success.DetailPage.GotoCalls == 1 && success.DetailPage.LoadCalls == 1,
    "Successful navigation and load-state checks both run");

var listGotoTimeout = await Scan(new()
{
    Cards = [new("2")],
    Details = { ["2"] = new(true) },
    GotoTimeoutOnList = true
});
Check(listGotoTimeout.Events.Count == 1, "A list navigation timeout still parses available event cards");

var listLoadTimeout = await Scan(new()
{
    Cards = [new("3")],
    Details = { ["3"] = new(true) },
    WaitTimeoutOnList = true
});
Check(listLoadTimeout.Events.Count == 1, "A list load-state timeout still parses available event cards");

var detailGotoTimeout = await Scan(new()
{
    Cards = [new("4")],
    Details = { ["4"] = new(true) },
    GotoTimeoutDetails = ["4"],
    AddQueryToDetailUrl = true
});
Check(Get<string?>(detailGotoTimeout.Events[0], "HtmlDescription") == "<p>detail 4</p>",
    "A committed detail navigation timeout still parses content with redirected query parameters");

var detailLoadTimeout = await Scan(new()
{
    Cards = [new("5")],
    Details = { ["5"] = new(true) },
    WaitTimeoutDetails = ["5"]
});
Check(Get<string?>(detailLoadTimeout.Events[0], "HtmlDescription") == "<p>detail 5</p>",
    "A detail load-state timeout still parses available detail content");

var missingDetail = await Scan(new()
{
    Cards = [new("6"), new("7")],
    Details = { ["6"] = new(false), ["7"] = new(true) },
    GotoTimeoutDetails = ["6"]
});
Check(missingDetail.Events.Count == 1 && Get<string?>(missingDetail.Events[0], "HtmlDescription") == "<p>detail 7</p>",
    "An unusable timed-out detail page is skipped without stopping later events");

var staleDetail = await Scan(new()
{
    Cards = [new("8"), new("9"), new("10")],
    Details = { ["8"] = new(true), ["9"] = new(true), ["10"] = new(true) },
    GotoTimeoutDetails = ["9"],
    PreCommitGotoTimeoutDetails = ["9"]
});
Check(staleDetail.Events.Count == 2 &&
    Get<string?>(staleDetail.Events[0], "HtmlDescription") == "<p>detail 8</p>" &&
    Get<string?>(staleDetail.Events[1], "HtmlDescription") == "<p>detail 10</p>",
    "A pre-commit detail timeout skips the failed event and later events still parse");

var staleList = await Scan(new()
{
    Cards = [new("11")],
    Details = { ["11"] = new(true) },
    GotoTimeoutOnList = true,
    PreCommitGotoTimeoutOnList = true,
    ListInitialUrl = "https://www.meetup.com/fr-FR/test-group/events/?type=past"
});
Check(staleList.Events.Count == 0,
    "A pre-commit list timeout cannot parse cards left over from a different list page");

Console.WriteLine($"{checks} vérifications réussies.");

async Task<(List<object> Events, PageState ListPage, PageState DetailPage)> Scan(Scenario scenario)
{
    var root = new ScenarioState(scenario);
    var listPageState = new PageState(root, true);
    listPageState.CurrentUrl = scenario.ListInitialUrl;
    var listPage = CreatePage(listPageState);
    var detailPage = new PageState(root, false);
    var context = Mock<IBrowserContext>((method, _) =>
        method.Name == nameof(IBrowserContext.NewPageAsync)
            ? Task.FromResult(CreatePage(detailPage))
            : throw new NotSupportedException(method.Name));
    var group = Activator.CreateInstance(groupType, ["test-group", "test"])!;
    var sequence = groupType.GetMethod("ScanEventPage")!.Invoke(group, [context, listPage, false])!;
    var events = await Drain(sequence);
    return (events, listPageState, detailPage);
}

IPage CreatePage(PageState page) => Mock<IPage>((method, args) =>
{
    switch (method.Name)
    {
        case "add_Request":
        case "remove_Request":
        case "add_RequestFinished":
        case "remove_RequestFinished":
            return null;
        case "get_Url":
            return page.CurrentUrl ?? "";
        case nameof(IPage.GotoAsync):
        {
            var url = (string)args![0]!;
            page.GotoCalls++;
            var timeout = page.IsList
                ? page.Root.Scenario.GotoTimeoutOnList
                : page.Root.Scenario.GotoTimeoutDetails.Contains(EventNumber(url));
            var preCommitTimeout = page.IsList
                ? page.Root.Scenario.PreCommitGotoTimeoutOnList
                : page.Root.Scenario.PreCommitGotoTimeoutDetails.Contains(EventNumber(url));
            if (!preCommitTimeout)
                page.CurrentUrl = !page.IsList && page.Root.Scenario.AddQueryToDetailUrl
                    ? $"{url}?utm_source=test"
                    : url;
            return timeout
                ? Task.FromException<IResponse?>(new TimeoutException("navigation timeout"))
                : Task.FromResult<IResponse?>(null);
        }
        case nameof(IPage.WaitForLoadStateAsync):
            page.LoadCalls++;
            var waitTimeout = page.IsList
                ? page.Root.Scenario.WaitTimeoutOnList
                : page.Root.Scenario.WaitTimeoutDetails.Contains(EventNumber(page.CurrentUrl ?? ""));
            return waitTimeout
                ? Task.FromException(new TimeoutException("load-state timeout"))
                : Task.CompletedTask;
        case nameof(IPage.TitleAsync):
            return Task.FromResult("Test Événements");
        case nameof(IPage.Locator):
            return PageLocator(page, (string)args![0]!);
        case nameof(IPage.CloseAsync):
            return Task.CompletedTask;
        default:
            throw new NotSupportedException(method.Name);
    }
});

ILocator PageLocator(PageState page, string selector)
{
    if (page.IsList && selector == "div[data-eventref]")
        return MakeLocator(all: page.Root.Scenario.Cards.Select(card => CardLocator(card)).ToList());

    if (page.IsList)
        throw new NotSupportedException(selector);

    var id = EventNumber(page.CurrentUrl ?? "");
    var detail = page.Root.Scenario.Details.GetValueOrDefault(id) ?? new(false);
    return selector switch
    {
        "main aside img[fetchpriority='high'].object-center" => MakeLocator(),
        "main section div.flex.flex-col.items-start.gap-ds2-16" =>
            MakeLocator(detail.DescriptionAvailable ? 1 : 0, html: $"<p>detail {id}</p>", text: $"detail {id}"),
        "main aside img[alt='pin icon'] + div > p" => MakeLocator(),
        _ => throw new NotSupportedException(selector)
    };
}

ILocator CardLocator(EventCard card)
{
    var eventHref = $"https://www.meetup.com/fr-FR/test-group/events/{card.Id}/";
    return Mock<ILocator>((method, args) =>
    {
        if (method.Name == nameof(ILocator.GetAttributeAsync))
            return Task.FromResult<string?>(null);
        if (method.Name == nameof(ILocator.Locator))
        {
            var selector = (string)args![0]!;
            return selector switch
            {
                "a[data-event-label]" => MakeLocator(attribute: eventHref),
                "h3" => MakeLocator(text: $"Event {card.Id}"),
                "time" => MakeLocator(attribute: "2099-06-02T18:30:00+02:00"),
                "img.size-full.object-cover.object-center" => MakeLocator(),
                _ => throw new NotSupportedException(selector)
            };
        }
        throw new NotSupportedException(method.Name);
    });
}

ILocator MakeLocator(int count = 1, string? attribute = null, string? text = null,
    string? html = null, IReadOnlyList<ILocator>? all = null)
{
    ILocator? locator = null;
    locator = Mock<ILocator>((method, args) => method.Name switch
    {
        "get_First" => locator,
        nameof(ILocator.CountAsync) => Task.FromResult(count),
        nameof(ILocator.AllAsync) => Task.FromResult<IReadOnlyList<ILocator>>(all ?? []),
        nameof(ILocator.GetAttributeAsync) => Task.FromResult(attribute),
        nameof(ILocator.InnerTextAsync) => Task.FromResult(text ?? ""),
        nameof(ILocator.TextContentAsync) => Task.FromResult(text ?? ""),
        nameof(ILocator.InnerHTMLAsync) => count == 0
            ? Task.FromException<string>(new InvalidOperationException("locator is empty"))
            : Task.FromResult(html ?? ""),
        _ => throw new NotSupportedException(method.Name)
    });
    return locator;
}

T Mock<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
{
    var proxy = DispatchProxy.Create<T, MockProxy>();
    ((MockProxy)(object)proxy).Handler = handler;
    return proxy;
}

async Task<List<object>> Drain(object sequence)
{
    var enumerableType = sequence.GetType().GetInterfaces().Single(type =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));
    var elementType = enumerableType.GetGenericArguments()[0];
    var enumeratorType = typeof(IAsyncEnumerator<>).MakeGenericType(elementType);
    var enumerator = enumerableType.GetMethod(nameof(IAsyncEnumerable<object>.GetAsyncEnumerator))!
        .Invoke(sequence, [CancellationToken.None])!;
    var moveNext = enumeratorType.GetMethod(nameof(IAsyncEnumerator<object>.MoveNextAsync))!;
    var current = enumeratorType.GetProperty(nameof(IAsyncEnumerator<object>.Current))!;
    var results = new List<object>();
    try
    {
        while (await (ValueTask<bool>)moveNext.Invoke(enumerator, null)!)
            results.Add(current.GetValue(enumerator)!);
    }
    finally
    {
        await (ValueTask)typeof(IAsyncDisposable).GetMethod(nameof(IAsyncDisposable.DisposeAsync))!
            .Invoke(enumerator, null)!;
    }
    return results;
}

void Check(bool condition, string name)
{
    if (!condition)
        throw new InvalidOperationException(name);
    checks++;
    Console.WriteLine($"✓ {name}");
}

T Get<T>(object instance, string name) =>
    (T)eventType.GetProperty(name)!.GetValue(instance)!;

static string EventNumber(string url) => new Uri(url).AbsolutePath.TrimEnd('/').Split('/').Last();

sealed record EventCard(string Id);
sealed record Detail(bool DescriptionAvailable);
sealed class Scenario
{
    public string? ListInitialUrl { get; init; }
    public List<EventCard> Cards { get; init; } = [];
    public Dictionary<string, Detail> Details { get; init; } = [];
    public bool GotoTimeoutOnList { get; init; }
    public bool PreCommitGotoTimeoutOnList { get; init; }
    public bool WaitTimeoutOnList { get; init; }
    public HashSet<string> GotoTimeoutDetails { get; init; } = [];
    public HashSet<string> PreCommitGotoTimeoutDetails { get; init; } = [];
    public HashSet<string> WaitTimeoutDetails { get; init; } = [];
    public bool AddQueryToDetailUrl { get; init; }
}

sealed class ScenarioState(Scenario scenario)
{
    public Scenario Scenario { get; } = scenario;
}

sealed class PageState(ScenarioState root, bool isList)
{
    public ScenarioState Root { get; } = root;
    public bool IsList { get; } = isList;
    public string? CurrentUrl { get; set; }
    public int GotoCalls { get; set; }
    public int LoadCalls { get; set; }
}

public class MockProxy : DispatchProxy
{
    public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        Handler(targetMethod ?? throw new InvalidOperationException("Missing method"), args);
}
