Published: 10.5.2025
Title: Ako spraviť z interpolated strings v C# HTML šablóny
Menu: Interpolated strings ako HTML šablóna
Cathegory: Dev
OgImage: images/InterpolatedStringAsHtmlTemplate/preview.jpg
Description: Ako spraviť z interpolated strings v C# HTML šablóny
---
Pri pokusoch s HTMX, Minimal API a AOT kompiláciou som mal problém nájsť šablóny, ktpré by fungovali. Našiel som source generátor [Weave](https://github.com/otac0n/Weave/), ale ten vyzerá neudržiavaný.

Tak som si povedal, že interpolated strings sú súčasťou C#, tak ich skúsim ohnúť aby slúžili ako HTML šablónovací systém pre Minimal API a o svoje snaženie sa podelím. Potreboval som iteráciu a podmienky. No výsledok nie je dokončený, stále v ňom existuje možnosť XSS zraniteľnosti cez nejaký objekt, takisto lepšieho výkonu by šlo dosiahnuť vlastnou implementáciou HTML enkóderu, ktorý by akceptoval `Span<>` a `ISpanFormatable`.

Potreboval som:
- defaultný HTML escaping,
- iteráciu/foreach,
- podmienky,
- možnosť renderovať "raw" HTML.

Ku koncu som objavil source generátor [RazorSlices]( https://github.com/DamianEdwards/RazorSlices), ktorý umožňuje používať Razor šablóny (aj CSS izoláciu atď.) v Minimal API a AOT kompilácii.

Tu je ukážka použitia mojej šablóny:

```cs
var sampleTodos = new Todo[] {
     new(1, "Walk the dog"),
     new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
     new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
     new(4, "Clean the bathroom"),
     new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
 };

app.MapGet("/", () =>
{
    string text = "&nbsp;<span>xss";
    int value = Random.Shared.Next();
    return Html.Content($"""
    <h1>Hello world! {DateTime.UtcNow}</h1>
    <p>
        Lorm ipsum dolor sit amet, consectetur adipiscing elit.
    </p>
    <p>{text}</p>
    <h2>Table 1</h2>
    <table>
      <tbody>
      {Html.Each(sampleTodos, t => $"""
        <tr>
          <td>{t.Id}</td> 
          <td>{t.Title}</td>
          <td>{t.IsComplete}</td>
        </tr>
        """)}
      </tbody>
    </table>
    <h2>Random value</h2>
    <p>Next random value: {value}</p>
    {Html.If(value % 2 == 0)}
        <p>Condition met</p>
    {Html.Else()}
        <p><strong>Condition not met</strong></p>
        {Html.If(value % 3 == 0)}
            <p>The number is divisible by 3.</p>
        {Html.EndIf()}
    {Html.EndIf()}
    """);
});
```

A samotný kód šablónovacieho systému:

```cs
public class Html
{
    public static IResult Content([StringSyntax("html")] ref HtmlSafeStringHandler handler)
    {
        return Results.Content(handler.ToStringAndClear(), "text/html");
    }

    public static async Task Stream<T>(HttpResponse response, 
        IAsyncEnumerable<T> stream,
        Func<T, HtmlSafeStringHandler> renderFunction,
        CancellationToken cancellationToken = default)
    {
        response.ContentType = "text/html";
        await foreach (T item in stream.WithCancellation(cancellationToken))
        {
            string html = renderFunction(item).ToStringAndClear();
            await response.WriteAsync(html, cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }
    }

    public static Task Write(HttpResponse response, ref HtmlSafeStringHandler handler, CancellationToken cancellationToken = default)
    {
        return Write(response, handler.ToStringAndClear(), cancellationToken);
    }

    private static async Task Write(HttpResponse response, string html, CancellationToken cancellationToken = default)
    {
        await response.WriteAsync(html, cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    public static IEnumerable<RawHtml> Each<T>(IEnumerable<T> items, Func<T, HtmlSafeStringHandler> renderFunction)
    {
        foreach (T item in items)
        {
            yield return new RawHtml(renderFunction(item).ToStringAndClear());
        }
    }

    public static IEnumerable<RawHtml> Each<T>(IEnumerable<T> items, Func<T, int, HtmlSafeStringHandler> func)
    {
        int index = 0;
        foreach (T item in items)
        {
            yield return new RawHtml(func(item, index).ToStringAndClear());
            index++;
        }
    }

    public static IEnumerable<RawHtml> For(int start, int end, Func<int, HtmlSafeStringHandler> func)
    {
        for (int i = start; i < end; i++)
        {
            yield return new RawHtml(func(i).ToStringAndClear());
        }
    }

    public static LiteralIf If(bool condition)
    {
        return new LiteralIf(condition);
    }

    public static LiteralElse Else()
    {
        return new LiteralElse();
    }

    public static LiteralEndIf EndIf()
    {
        return new LiteralEndIf();
    }
}

internal enum IfState
{
    Nop,
    True,
    False
}

public record struct RawHtml(string Value)
{
    public static explicit operator RawHtml(string value) => new RawHtml(value);
}

public struct LiteralElse
{
}

public struct LiteralEndIf
{
}

public record struct LiteralIf(bool Condition);

[InterpolatedStringHandler]
public ref struct HtmlSafeStringHandler
{
    private DefaultInterpolatedStringHandler innerHandler;

    private Stack<IfState>? conditionStack;

    public HtmlSafeStringHandler(int literalLength, int formattedCount)
    {
        this.innerHandler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
    }

    public void AppendLiteral(string value)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendLiteral(value);
        }
    }

    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        if (this.CanRender() && value.Length > 0)
        {
            this.innerHandler.AppendFormatted(HttpUtility.HtmlEncode(value.ToString()));
        }
    }

    public void AppendFormatted(string? value)
    {
        if (this.CanRender() && !string.IsNullOrEmpty(value))
        {
            this.innerHandler.AppendFormatted(HttpUtility.HtmlEncode(value));
        }
    }

    public void AppendFormatted(string? value, string? format)
    {
        if (this.CanRender() && !string.IsNullOrEmpty(value))
        {
            if (format == "raw")
            {
                this.innerHandler.AppendFormatted(value);
            }
            else if (format == "attr")
            {
                this.innerHandler.AppendFormatted(HttpUtility.HtmlAttributeEncode(value));
            }
            else
            {
                this.innerHandler.AppendFormatted(HttpUtility.HtmlEncode(value));
            }
        }
    }

    public void AppendFormatted(RawHtml value)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value.Value);
        }
    }
    #region Flow controll
    public void AppendFormatted(IEnumerable<RawHtml> values)
    {
        if (this.CanRender())
        {
            foreach (RawHtml value in values)
            {
                this.innerHandler.AppendFormatted(value.Value);
            }
        }
    }

    public void AppendFormatted(IEnumerable<string> values)
    {
        if (this.CanRender())
        {
            foreach (string value in values)
            {
                this.innerHandler.AppendFormatted(HttpUtility.HtmlEncode(value));
            }
        }
    }

    public void AppendFormatted(LiteralIf value)
    {
        if (this.conditionStack == null)
        {
            this.conditionStack = new Stack<IfState>();
        }

        if (this.CanRender())
        {
            this.conditionStack.Push(value.Condition ? IfState.True : IfState.False);
        }
        else
        {
            this.conditionStack.Push(IfState.Nop);
        }
    }

    public void AppendFormatted(LiteralElse value)
    {
        System.Diagnostics.Debug.Assert(this.conditionStack != null);
        System.Diagnostics.Debug.Assert(this.conditionStack.Count > 0);

        this.conditionStack.Push(this.conditionStack.Pop() switch
        {
            IfState.Nop => IfState.Nop,
            IfState.True => IfState.False,
            IfState.False => IfState.True,
            _ => throw new UnreachableException("Invalid state")
        });
    }

    public void AppendFormatted(LiteralEndIf value)
    {
        System.Diagnostics.Debug.Assert(this.conditionStack != null);
        System.Diagnostics.Debug.Assert(this.conditionStack.Count > 0);
        this.conditionStack.Pop();
    }

    #endregion

    public void AppendFormatted<T>(T value, string? format)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, format);
        }
    }

    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, alignment, format);
        }
    }

    public void AppendFormatted<T>(T value, int alignment)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, alignment);
        }
    }

    public void AppendFormatted<T>(T value)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(HttpUtility.HtmlEncode(value));
        }
    }

    #region Primite types
    public void AppendFormatted(int value, string? format)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, format);
        }
    }

    public void AppendFormatted(int value, int alignment, string? format)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, alignment, format);
        }
    }

    public void AppendFormatted(int value, int alignment)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, alignment);
        }
    }

    public void AppendFormatted(int value)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value);
        }
    }

    public void AppendFormatted(long value, string? format)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, format);
        }
    }

    public void AppendFormatted(long value, int alignment, string? format)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, alignment, format);
        }
    }

    public void AppendFormatted(long value, int alignment)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value, alignment);
        }
    }

    public void AppendFormatted(long value)
    {
        if (this.CanRender())
        {
            this.innerHandler.AppendFormatted(value);
        }
    }
    #endregion

    public readonly override string ToString()
    {
        return this.innerHandler.ToString();
    }

    // Forward to the inner handler
    public string ToStringAndClear()
    {
        System.Diagnostics.Debug.Assert(this.conditionStack == null || this.conditionStack.Count == 0);

        return this.innerHandler.ToStringAndClear();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanRender()
    {
        return this.conditionStack == null
            || this.conditionStack.Count == 0
            || this.conditionStack.Peek() == IfState.True;
    }
}
```