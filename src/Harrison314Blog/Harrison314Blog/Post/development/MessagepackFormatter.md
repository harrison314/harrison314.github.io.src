Published: 13.11.2025
Title: Vlastny formát v Minimal API
Menu: Vlastny formát  v Minimal API
Cathegory: Dev
OgImage: images/InterpolatedStringAsHtmlTemplate/preview.jpg
Description: Ako doplniť 
---

Minimal API prinieslo jednoduché a rýchle riešenie pre JSON REST API. Ale voči kontrolerom majú tú nevýhodu, že sú priamo naviazané na JSON.
Dostal som sa do pozície, kde som potreboval pracovať s messagepack requestom a responsom.

Výsledok vyzerá takto:


```cs
app.MapPost("/messagepack", (MessagePackObj<Person> reuqest) =>
{
    Person person = reuqest.Value;
    return Results.Extensions.Messagepack(person);
});

[MessagePackObject]
public class Person
{
    [Key(0)]
    public int Age { get; set; }

    [Key(1)]
    public string FirstName { get; set; }

    [Key(2)]
    public string LastName { get; set; }
}
```

V Minimal API to ide, ale treba požiť nasledujúci trik.
Pre parsovanie odpovede sa použije statická metóda `BindAsync`, ktorá prečíta a deserializuje stream z HTTP requestu.

Jedna z noviniek .NET 10 je to, že pre túto metódu definuje rozhranie `IBindableFromHttpContext<T>`, ktoré definuje metódu `BindAsync` z nullable parametrom,
ktorá  keď vráti `null` hodnotu, tak sa okamžite odpovie chybou.
Doteraz bola metóda `BindAsync` urćená len konvenciou.

Výsledný typ pre prečítanie messagepack správy z requestu vyzerá nasledovne.

```cs
public class MessagePackObj<T> : IBindableFromHttpContext<MessagePackObj<T>>
{
    public T Value
    {
        get;
    }

    public MessagePackObj(T value)
    {
        this.Value = value;
    }

    public static async ValueTask<MessagePackObj<T>?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        string? contentType = context.Request.Headers.ContentType.SingleOrDefault();
        if (contentType != "application/x-msgpack")
        {
            return null;
        }

        try
        {
            T item = await MessagePackSerializer.DeserializeAsync<T>(context.Request.Body,
                null,
                context.RequestAborted);

            return new MessagePackObj<T>(item);
        }
        catch (Exception ex)
        {
            context.RequestServices.GetRequiredService<ILogger<MessagePackObj<T>>>()
                .LogError(ex, "Error during deserialize messagpeack request.");
            return null;
        }
    }
}
```

Pre odpoveď je to o niečo ľahšie, stačí napísať vlastný `IResult`.

```cs
public static class MessagepackResultExtensions
{
    public static IResult Messagepack<T>(this IResultExtensions _, T? obj, int statusCode = 200)
    {
        return new MessagepackResult<T>(obj, statusCode);
    }

    private class MessagepackResult<T> : IResult
    {
        private readonly T? item;
        private readonly int statusCode;

        public MessagepackResult(T? item, int statusCode)
        {
            this.item = item;
            this.statusCode = statusCode;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/x-msgpack";
            httpContext.Response.StatusCode = this.statusCode;

            await MessagePackSerializer.SerializeAsync<T>(httpContext.Response.Body,
               this.item!,
               null,
               httpContext.RequestAborted);
        }
    }
}
```

## Zdroje
1. <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-10.0>
1. <https://blog.vyvojari.dev/asp-net-core-minimal-api-custom-output-formatter/>
1. <https://blog.vyvojari.dev/asp-net-core-minimal-api-content-negotiation/>
