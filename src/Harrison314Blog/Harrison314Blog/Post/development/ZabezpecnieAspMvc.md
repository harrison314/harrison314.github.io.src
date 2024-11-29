Published: 15.9.2017
Title: Zabezpečenie vstupov ASP.NET MVC aplikácie
Menu: Zabezpečenie ASP.NET MVC
Cathegory: Dev
Description: Ako zabezpečiť HTTP vstupy do ASP.NET MVC aplikácie.
---
Medzi základné pravidlá zabezpečenia webových aplikácií (na aplikačnej úrovni) patrí kontrola 
všetkých vstupov a to na validnosť aj dĺžku.

Táto úloha je v ASP.NET MVC (Core) o niečo jednoduchšia, lebo v modeloch a parametroch akcií 
ide používať typy ako _int_, _Guid_, _long_, _bool_ ktoré sú pri bindingu validované.

Trochu iná situácia je pri menej štandardných vstupoch ako  návratová adresa (relatívna),
hexadecimálny kód, dáta vo forme base64, či špecifické kódy (kód produktu, diagnózy, stomatologického výkonu)
alebo špecifických identifikátorov či tokenov. V týchto prípadoch sa často spolieha na to,
že aplikačná vrstva odmietne daný vstup ako invalidný, no nie vždy sa tak musí stať.
Horší prípad, ak tento vstup prebuble do HTML, alebo vygenerovaného JavaScrip kódu.

Ak si bude tieto vstupy validovať človek sám v akciách, je jednak otrava a ľahko sa nejaký vstup prehliadne.

Ďalšia možnosť je vytvoriť si [ActionFilter](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
a validovať vstupy podľa mennej konvencie, napríklad _retUrl_ sa vždy pokladá za návratovú adresu, _documentId_ za identifikátor dokumentu,...
 a podľa toho sa globálne validujú.
Toto je celkom schodná cesta, lebo jediným kusom kódu dokáže ošetriť celú aplikáciu globálne,
no je dosť situácií, kedy nestačí.

Moje preferované riešenie je používať na tieto druhy vstupov vlastné [value objekty](https://martinfowler.com/bliki/ValueObject.html).
Vytvorí sa jednoduchý typ pre vstup, ktorý sa validuje v čase bindingu.

Tento spôsob je vhodné na vstupy, ktoré nezadáva používateľ.

## Ukážka kódu
Vytvorí sa abstraktný [binder](https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.Core/ModelBinding/Binders/SimpleTypeModelBinder.cs):

```cs
public abstract class ArmoredBinder<T> : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            // no entry
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        if (bindingContext.ModelType != typeof(T))
        {
            return Task.CompletedTask;
        }

        try
        {
            if (this.TryExtractValue(valueProviderResult.FirstValue, out T value))
            {
                bindingContext.Result = ModelBindingResult.Success(value);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
        catch (Exception exception)
        {
            bool isFormatException = exception is FormatException;
            if (!isFormatException &amp;&amp; exception.InnerException != null)
            {
                exception = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                exception,
                bindingContext.ModelMetadata);
        }

        return Task.CompletedTask;
    }

    protected abstract bool TryExtractValue(string stringValue, out T value);

}
```

Ako value objekt som zvolil návratovú relatívnu URL adresu. Odvodíme pre ňu binder, ktorý kontroluje povolené znaky a URL enkóding a relatívnosť.

```cs
public class ReturnUrlTypeBinder : ArmoredBinder<ReturnUrl>;
{
    public ReturnUrlTypeBinder()
    {

    }

    protected override bool TryExtractValue(string stringValue, out ReturnUrl value)
    {
        bool isMatch = Regex.IsMatch(stringValue, @"^(/([-a-zA-Z0-9_\(\)_~:?#\]\]@!\.\$&\*\+,;=&#39;]|%[a-zA-Z0-9]{2})+)+$", RegexOptions.Singleline);
        value = (isMatch && stringValue.Length < 600) ? new ReturnUrl(stringValue) : null;

        return isValid;
    }
}
```

Value objekt _ReturnUrl_:

```cs
[ModelBinder(BinderType = typeof(ReturnUrlTypeBinder))]
public class ReturnUrl
{
    public string Value
    {
        get;
        protected set;
    }

    public ReturnUrl(string url)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        this.Value = url;
    }

    public override string ToString()
    {
        return this.Value;
    }

    public static implicit operator string(ReturnUrl returnUrl)
    {
        return returnUrl.Value;
    }
}
```

Konečne ukážka použitia. _ReturnUrl_ v akcii kontroleru. V prípade, keď bude _retUrl_ obsahovať nedovolené znaky (je nevalidná)
nebude nabindovaná, jej hodnota bude _null_ a _ViewState_ sa dostane do nevalidného stavu.

```cs
public class TestController : Controller
{
    public IActionResult ConfirmRedirect(ReturnUrl retUrl)
    {
       if (!this.ModelState.Isvalid)
       {
         //TODO: osetrenie chyboveho stavu
       }
        //TODO: spracovanie akcie
        return this.Redirect(retUrl.Value);
    }
}
```

Pre zjednodušenie kontroly _ViewState_ v metódach reagujúcich na HTTP GET požiadavku používam nasledujúci atribút.

```cs
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RqueireValidModelStateFilterAttribute : Attribute, IActionFilter
{
    public RqueireValidModelStateFilterAttribute()
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {

    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.BadRequestResult();
        }
    }
}
```
Akcia kontroleru môže vyzerať nasledovne:

```cs
public class TestController : Controller
{
     [RqueireValidModelStateFilter]
     public IActionResult ConfirmRedirect(ReturnUrl retUrl, bool isTest)
     {
         //TODO: spracovanie akcie
         return this.Redirect(retUrl.Value);
     }
}
```

Po zavolaní adresy 
_/Test/ConfirmRedirect?retUrl=%2FTest%2FIndex%3Faa%3D47&amp;isTest=false_ sa akcia vykoná.
No pri 
_/Test/ConfirmRedirect?retUrl=%2FTest%2FIndex%3Faa%3D47&amp;isTest=iamattacker_
alebo 
_/Test/ConfirmRedirect?retUrl=%3Cscript%3Ealert%281%29%3B%3C%2Fscript%3E&amp;isTest=false_
už nie a používateľ dostane odpoveď Bad Request, pretože v prvom prípade sa nepodarilo ASP.NET MVC sparsovať bool
a v druhom boli u URL-ke nepovolené znaky a pokus o [XSS](https://en.wikipedia.org/wiki/Cross-site_scripting).

V ASP.NET Core je samozrejme možné použiť dependency injection v bindroch aj atribútoch,
čo sa veľmi hodí pri logovaní toho, čo sa na serveri udialo.