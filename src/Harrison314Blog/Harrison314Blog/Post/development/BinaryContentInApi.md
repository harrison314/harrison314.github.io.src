Published: 16.8.2021
Title: ASP.NET Core API a binárny obsah
Menu: ASP.NET Core API a binárny obsah
Cathegory: Dev
Description: Nahrávanie a sťahovanie binárnych dát alebo súborov v ASP.NET Core API s podporou OpenAPI špecifikácie.
OgImage: images/BinaryContentInApi/UploadButton.jpg
---

Občas je potrebné cez REST-ové API poslať súbor (napríklad PDF, obrázok, ZIP archív,...), alebo iné binárne dáta.
Ak sú malé (desiatky bajtov max. jednotky kB) je stále vhodné použiť štandardnú JSON serializáciu, no pri väčších objemoch dát je vhodné ich posielať ako binárne dáta
a nie ako _base64_ string v JSON-e, lebo sa tým šetrí pamäť, buffering a umožňuje na strane serveru použiť streamové spracovanie.

Tento návod funguje pre _ASP.NET Core 3.1_ a vyšší s použitím _NSwag_ knižnice.

Na takýto prenos dát myslí aj špecifikácia OpenAPI (Swagger 3) - <https://swagger.io/specification/>, 
ktorá hovorí, že sa majú prenášať s content-type `application/octet-stream` ako binárny string.  
(**Pozor:** Swagger verzie 2 to neumožňoval, podporoval len upload ako form data s content-type `multipart/form-data` - <https://swagger.io/docs/specification/2-0/file-upload/>).

## Nahratie binárneho obsahu
Pre nahranie (upload) binárnych dát s klienta na server je potrebné určiť konzumovaný content-type a doplniť _InputFormater_ do _ASP.NET Core_.

Stačí oanotovať akciu API kontroleru nasledovne:

```cs
[HttpPost]
[Consumes("application/octet-stream")]
[ProducesResponseType(typeof(void), 200)]
public IActionResult Upload([FromBody] Stream content)
{
    // ...
    return this.Ok();
}
```

No to pre správne fungovanie je potrebné pridať vlastný _InputFormater_ pre kontroleri:

```cs
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.IO;

public class RawRequestBodyFormatter : InputFormatter
{
    public RawRequestBodyFormatter()
    {
        this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
    }

    public override bool CanRead(InputFormatterContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return context.ModelType == typeof(Stream);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        Microsoft.AspNetCore.Http.HttpRequest request = context.HttpContext.Request;

        if (context.ModelType == typeof(Stream))
        {
            return await InputFormatterResult.SuccessAsync(request.Body);
        }

        return await InputFormatterResult.FailureAsync();
    }
}
```

A v triede `Startup.cs` do inicializácie kontrolerov (`AddControllers`, `AddControllersWithViews`,...) zaradiť tento _InputFormater_:

```cs
services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, new RawRequestBodyFormatter());
});
```

Následne aj Sagger UI zobrazuje upload tlačidlo pre túto akciu:

![Upload v Swagger UI.](images/BinaryContentInApi/UploadButton.jpg){.img-center}

## Stiahnutie binárneho obsahu
Samotné stiahnutie (download) binárnych dát v _ASP.NET Core_ funguje. No treba pedant vlastný atribút aby to bolo jasné OpenAPI a _Swagger UI_.

```cs
[HttpGet("{id}")]
[ProducesBinaryString(200)]
[Produces("application/octet-stream")]
public async ValueTask<IActionResult> GetDocumentContent(int id)
{
    Stream stream = await this.GetContentStreamById(id);
    return this.Ok(stream);
}
```

Je potrebné upraviť response body v OpenAPI špecifikácii, na to sa použije nasledujúci atribút:

```cs
[AttributeUsage(AttributeTargets.Method)]
internal class ProducesBinaryStringAttribute : OpenApiOperationProcessorAttribute
{
    public ProducesBinaryStringAttribute(int statusCode)
        :base(typeof(ProducesBinaryStringOperationProcessor), statusCode)
    {

    }
}

internal class ProducesBinaryStringOperationProcessor : IOperationProcessor
{
    private readonly int statusCode;

    public ProducesBinaryStringOperationProcessor(int statusCode)
    {
        this.statusCode = statusCode;
    }

    public bool Process(OperationProcessorContext context)
    {
        string defaultMimeType = context.OperationDescription.Operation.Produces.FirstOrDefault() ?? "application/octet-stream";
        NSwag.OpenApiResponse response = new NSwag.OpenApiResponse();
        NSwag.OpenApiMediaType mediaType = new NSwag.OpenApiMediaType()
        {
            Schema = new NJsonSchema.JsonSchema()
            {
                Type = NJsonSchema.JsonObjectType.String,
                Format = "binary"
            }
        };

        response.Content.Add(defaultMimeType, mediaType);
        context.OperationDescription.Operation.Responses.Add(this.statusCode.ToString(), response);

        return true;
    }
}
```

## Záver
Pri posielaní binárnych dát na API kontroler sa dá podobne prestupovať aj ku bajtovému poľu alebo stringu, záleží ako sa budú dáta ďalej spracovávať.

No treba myslieť na to, že hoci tento postup dodržuje OpenAPI špecifikáciu,
tak nie každý generátor dokáže vygenerovať správneho klienta (stávalo sa mi, že niektoré pre _Rust_ sa pokúšali binárny obsah najskôr serializovať a až potom poslať).
V takomto prípade je ale veľmi jednoduché chybnú metódu dopísať ručne. 

## Zdroje
1. [NSwag](https://github.com/RicoSuter/NSwag)
1. [NSwag - Consumes attribute is ignored for OpenApi3](https://github.com/RicoSuter/NSwag/issues/2508#issuecomment-720927415)
1. [NSwag - OpenAPI 3.0.1 upload/download file example](https://github.com/RicoSuter/NSwag/issues/2495)
1. [Swagger UI](https://swagger.io/tools/swagger-ui/)
