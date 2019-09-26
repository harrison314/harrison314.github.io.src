Published: 28.3.2019
Title: Slovenská typografia pre wyam
Menu: Slovenská typografia pre wyam
Cathegory: Dev
---
# Slovenská typografia pre Wyam
[Wyam](https://wyam.io/) je primárne generátor statických webových stránok napísaných v dotnete (v súčasnosti .Net Core a dostupný ako dotnet global tool), no dá sa použiť aj na mnoho iných vecí ako generovanie e-kníh, generovanie HTML dokumentácie ku knižniciam...

Pomocou wyamu je generovaný aj tento blog. Je vhodný pre .Net vývojárov, lebo podporuje razor syntax, markdawn, rôzne zdroje metadát (YAML, Json, XML,...), vie používať nugety, doplnky a konfigurácia sa preň píše v C#.

Pre jednoduchšie písanie blogu som si preň vytvoril modul, ktorý sa stará o typografiu v texte - dopĺňa nedeliteľné medzery medzi predložky a slová a nahrádza tri bodky za trojbodku.
Tieto detaily sú síce malé ale na výslednej čitateľnosti textu sa prejavia.

Nasledujúci kód je pre modul slovenskej typografie (je umiestnený priamo vo _config.wyam_ súbore).

```cs
public class Typography : Wyam.Common.Modules.IModule
{
    private readonly Regex regex;

    public Typography()
    {
        this.regex = new Regex(@"(\s|^)(z|zo|bez|na|po|od|do|pri|pre|so|miesto|o|v|s|za|a|i|ani|aj|najprv|potom|ešte|ale|no|lež|jednako|alebo|buď|či|že|aby|čo|aký|ktorý|kde|keď|kým|kde|čo|akoby|lebo|pretože|predsa)(\s+)([^\p{Cc}\p{Cf}\p{Z}]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
    {
       foreach(IDocument inputDocument in inputs)
       {
         string text = this.UseTypography(inputDocument.Content);
         yield return context.GetDocument(inputDocument, context.GetContentStream(text), null, true);
       }
    }

    private string UseTypography(string html)
    {
      string text = this.regex.Replace(html, "$1$2&#160;$4");
      text = text.Replace("...", "&#8230;");

      return text;
    }
}
```

A jeho ukážkové použitie vo wyam pipeline.

```cs
Pipelines.Add("Posts",
    ReadFiles("post/*.md"),
    FrontMatter(Yaml()),
    new Typography(),
    Meta("Post", @doc.Content),
    Meta("PostFile", string.Format(@"{0}.html", @doc["SourceFileBase"])),
    Merge(ReadFiles("post/postTemplate.cshtml")),
    Razor(),
    WriteFiles((string)@doc["PostFile"])
);
```

## Zdroje
1. [Wyam.io](https://wyam.io/)
1. [Wyam github](https://github.com/Wyamio/Wyam)
1. [JAM Stack](https://jamstack.org/)