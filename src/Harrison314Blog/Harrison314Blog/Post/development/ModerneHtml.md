Published: 15.5.2025
Updated: 24.5.2025
Title: Renderovať HTML na serveri nie je hanba
Menu: Renderovať HTML na serveri nie je hanba
Cathegory: Dev
OgImage: images/ModerneHtml/fullstack-md.jpg
Description: Renderovať HTML na serveri nie je v roku 2025 hanba. Moderné možnosti HTML, Streaming HTML, SSE, HTTP/2 s použitím knižnice HTMX – pre tvorbu serverom riadených klientskych aplikácií s minimom javascriptu.
---
V tomto blogu chcem zhrnúť moderné<sup>*</sup> možnosti použitia HTTP a HTML v prístupoch tvorby interaktívnych webových aplikácii s veľmi malým množstvom javascriptu. 

_*_ moderne tu znamená, že tieto princípy fungovali už keď som začínal s PHP v roku 2008, ale vďaka HTTP/2, novým CSS možnostiam, HTML tagom a knižniciam sa začali používať.

## HTMX
[HTMX](https://htmx.org/) je jednosúborová knižnica, ktorá v roku 2024 zobrala frontend frameworky útokom,
umožňuje tvorbu interaktívnych webových aplikácii (teda toho, čo je teraz doménou SPA).
A to celé pomocou server-renderingu a HTML atribútov, pričom od serveru sa chce len to, aby dokázal vrátiť kúsky HTML.
Sám autor HTMX dúfa v to, že HTMX zanikne a atribúty, ktoré HTMX pridáva sa dostanú do štandardného HTML.

HTMX je na prvý pohľad knižnica, ktorá pridáva do HTML niekoľko atribútov, ktoré umožňujú spraviť HTTP request z akéhokoľvek tagu na akúkoľvek udalosť a výsledok umiestniť niekam na stránku.
To dokáže aj _jQuery_, tak kde je rozdiel? Rozdiel je v tom, že HTMX je deklaratívne ale hlavne so sebou prináša inú paradigmu v tvorbe UI.
Je to nástroj, ktorý ponúkla dosť, aby sa v ňom dal spraviť e-shop, biznis aplikáciu, manažment portál,... proste 80% vecí na ktoré sa teraz používa React/Angular/Vue.

Povedzme, že máme SPA aplikáciu, kde klikneme na linku pre načítanie dát do tabuľky podľa zvolených filtrov:
* V reaktej SPA, sa cez store odpáli javascript klient pre REST API, ten zavolá fetch na dáta na serveri, na serveri sa dáta vytiahnu z databázy, z dát sa vytvorí JSON, ktorý sa vráti browseru, ten ho pomocou klienta rozparsuje, vytvorí sa akcia, zavolá sa reducer, updatne sa store, vyrenderujú sa všetky komponenty do virtuálneho DOM-u, ten sa porovná s predchádzajúcim virtuálnym DOM-om, a updatne sa HTML na stránke. 
* V HTML stránke s HTMX sa odošle request na server, server sa pozrie do databázy, vyrenderuje HTML, odošle ho na klienta a HTMX zmení HTML v stránke.

![HTMX UI example](images/ModerneHtml/before-after-htmx-md.jpg){.img-center}

Treba podotknúť, že HTMX dokáže nahradzovať viaceré časti stránky na jeden response a tiež, že v načítaných fragmentoch HTML fungujú jeho atribúty automaticky. 

HTMX má vyriešené napríklad lazy-loading, debouncing, loading indocator, vie použiť history API v prehliadača, v príkladoch má ukážky na inline-editing, tranzitions, morphing a [mnohé ďalšie](https://htmx.org/examples/).

Na to prečo a ako použiť HTMX sú tu iné články (linkujem ich nižšie), skôr chcem pridať svoje postrehy z troch hoby projektov, ktoré som pomocou HTMX robil:
* HTMX je jednoduchý, nie však primitívny, myslím, že si ho proste treba vyskúšať. 
* Počet endpointov voči SPA aplikácii zostane približne rovnaký, len namiesto JSON-u budú vracať kúsky HTML. 
* Pri použití HTMX som nemal pocit, že ide o mágiu, lebo mám pod kontrolou aj request aj response a nič sa nedeje samo. 
* Vďaka moderným CSS selektorom netreba targetovať ciele HTML len pomocou id-čka, ale ide použiť direktívy ako `next table`. 
* Osvedčila sa mi kombinácia [ASP.NET Core Minimal API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-10.0&tabs=visual-studio), HTMX a [RazorSlices](https://github.com/DamianEdwards/RazorSlices). Takže endpoint, model, (prípadne [mediátor handler](https://github.com/martinothamar/Mediator)), a šablónu ide mať v rovnakom adresári, poprípade mať v adresári všetko potrebne ku komponente. Samozrejme ide použiť aj izolácia CSS a AOT kompiláciu!
* Pre zobrazenie chýb je vhodné použiť `hx-swap-oob`, alebo [HX-Redirect HTTP hlavičku](https://htmx.org/headers/hx-redirect/). 
* Pri mojich hoby projektoch som javascript nepotreboval. No HTMX neotvorí dialógové okno (aj to dialógové okno ide dnes otvoriť pomocou HTML), na takúto klientsku interakciu ide použiť knižnicu [Alpine.js](https://alpinejs.dev/), ktorá má podobne deklaratívnu filozofiu.  
* Ak bude potrebné využiť nejaké komponenty, ktoré interagujú na strane klienta, tak je možné použiť _web componenty_ s čistým javascritom pre jednoduché veci, pre zložitejšsie použiť knižnicu [Lit](https://lit.dev/).

Samozrejme HTMX nie je strieborná guľka, ale vďaka nemu viem tvoriť rovnaké webové aplikácie bez NPM, oveľa rýchlejšie, jednoduchšie a z menším počtom riadkov kódov. 

### Praktický príklad
Nasleduje príklad, v ktorom sa zobrazujú logy, je v nich možné vyhľadávať, zoraďovať a zobraziť podľa kategórie.

Pri použití HTMX stačí z modrej oblasti spraviť formulár (označený modrou farbou) zo štandardnými HTML elementami a pridať mu tieto atribúty:

```html 
<form hx-get="/logs" 
      hx-target="next tbody" 
      hx-trigger="input changed delay:500ms, keyup[key=='Enter'], select changed, submit, load" 
      hx-indicator="#spinner"> 
 ... 
``` 

`hx-get` hovorí kam sa má poslať GET požiadavka, na vyhľadanie logov s parametrami (tie sa získajú s inputov),  
`hx-target` hovorí, že obsah vrátený zo serveru sa umiestni do nasledujúceho tagu `tbody`,   
`hx-trigger` hovorí, na aká udalosti reagovať, je tam zahrnutá zmena formuláru, debouncing, aj úvodné načítanie,   
`hx-indicator` hovorí, kde sa nachdza _loading inidicator_.

![HTMX UI example](images/ModerneHtml/HtmxExample.png){.img-center}

Šablóna pre zobrazenie logov (na obrázku oblasti označené zelenou farbou) vyzerá takto:

```html 
@foreach (LogEntity log in Model.Logs) 
{ 
    <tr> 
        <td>@log.Time.ToString("dd.MM.yyyy HH:mm:ss")</td> 
        <td>@log.Ip</td> 
        <td>@log.Host</td> 
        <td><span class="@CreateBage(log.Type)">@log.Type</span></td> 
        <td>@log.Content</td> 
    </tr> 
} 

<div id="LoadOther" hx-swap-oob="true"> 
    @if (Model.RenderNext) 
    { 
        <button type="button" class="btn btn-outline-secondary" 
           hx-get="@Model.NextUrl" 
           hx-target="previous tbody" 
           hx-trigger="click" 
           hx-indicator="#spinner" 
           hx-swap="beforeend"> 
            Load more 
        </button> 
    } 
</div> 
``` 

Server vráti obsah pre `tbody` a obsah pre tlačidlo _"Load more"_, ten je oanotovaný atribútom `hx-swap-oob="true"`,
pretože sa umiestňuje mimo tabuľky a má na sebe rovnaké štyri HTMX tribúty (ale s URL vrátane parametrov) a `hx-swap="beforeend"`,
aby sa donačítané logy umiestnili na koniec tabuľky. 

A to je všetko. 

### Zdroje ku HTMX
Tu sú nejaké zdroje o HTMX a kedy ho použiť a nepoužiť:
1. https://htmx.org/ - hlavná stránka projektu,
1. https://hexshift.medium.com/10-underused-htmx-hooks-that-can-take-your-frontend-from-good-to-great-64812d9da905 - triky s udalosťami,
1. https://medium.com/@simsketch/understanding-htmx-building-modern-web-applications-with-html-4792b9ce1083 - zhrnutie HTMX s príkladmi použitia,
1. https://medium.com/@alexander.heerens/htmx-patterns-01-how-to-build-a-multi-step-form-in-htmx-554d4c2a3f36 - _Multi-Step Form in HTMX_,
1. https://medium.com/@alexander.heerens/micro-frontends-with-htmx-266b457490b9 - _Micro Frontends with HTMX_,
1. https://medium.com/pragmatic-programmers/server-driven-web-apps-with-htmx-2ecb93e60f09 - _Server-Driven Web Apps with htmx_,
1. https://www.youtube.com/watch?v=hA6DQZLFwi0 
1. https://htmx.org/essays/a-real-world-react-to-htmx-port/ - ukážka konverzie reálnej aplikácie z reactu,
1. https://htmx.org/essays/another-real-world-react-to-htmx-port/ - ukážka konverzie reálnej aplikácie z reactu,
1. https://github.com/khalidabuhakmeh/Htmx.Net - Htmx.Net - HTMX tag helpre pre ASP.NET Core,
1. https://github.com/DamianEdwards/RazorSlices - _RazorSlices_ - AOT kompatibilná knižnica pre Razor šblóny a Minimal API,
1. Obrázky sú priamo zo stránky <https://htmx.org/>.

## SSE
_Server-Sent Events_ je HTTP mechanizmus, ktorým môže server informovať klienta o udalostiach na serveri formou zaslanej správy. Ide o bežný HTTP request,
no odpoveď má nastavené `TransferEncoding` na `chunked`.
Nerozdivel od Web-Socketov ide o jednosmerný kanál, ktorý dokáže naplno využiť možnosti protokolu HTTP/2,
ale netreba naň ďalší port ani špeciálne pravidlá na firewally.

Pomocou Minimal API v .NET 10 idú jednoducho implementovať:

```cs 
async IAsyncEnumerable<SseItem<string>> GetStockValues(CancellationToken cancellationToken) 
{
    double lastValue = 0.0; 
    while(!cancellationToken.IsCancellationRequested) 
    { 
        double value = Random.Shared.NextDouble() * 100.0; 
        string mark = value > lastValue ? "⇑" : "⇓"; 
        string html = $"<p>SSE coin - {value:.00} EUR <span>{mark}</span></p>"; 

        yield return new SseItem<string>(html); 

        lastValue = value; 
        await Task.Delay(2000 + Random.Shared.Next(-100, 300), cancellationToken); 
    } 
}

app.MapGet("/stockPrice", (CancellationToken cancellationToken) 
{ 
   return TypedResults.ServerSentEvents(GetStockValues(cancellationToken)); 
});
```

A jednoducho konzumovať pomocou javascriptu: 

```js 
const evtSource = new EventSource("/stockPrice"); 
evtSource.onmessage = (event) => { 
  document.getElementById('stockPrice').innerHTML = JSON.parse(event.data); 
}; 
```
Vďaka HTMX ale nemusíme posielať cez SSE len JSON, ale aj kusy HTML, ktoré je vďaka atribútu `hx-swap-oob` umiestniť kamkoľvek na stránku.
Takže jedným SSE pripojením je možné riešiť viac udalostí - napríklad aktualizovať ceny _SSE coinu_ a súčasne zobrazovať notifikácie. 

Pre HTMX je potrebné použiť [oficíalne rozšírenie pre SSE](https://htmx.org/extensions/sse/).

```html
<div hx-ext="sse" sse-connect="/stockPrice" sse-swap="message"></div> 
```

### Zdroje pre SSE
1. https://kotlin-htmx.fly.dev/demo/htmx/checkboxes - _100 000_ relatime checkboxov pomouu HTMX a SSE,
1. https://www.youtube.com/watch?v=x0725PDUho8 - SSE v .Net 10.

## Streaming HTML
Streaming HTML je ako názov napovedá posielanie posielanie kúskov HTML otvoreným spojením. Podobne ako SSE ťaží z HTTP/2. 

Predstavte si príklad sociálnej siete, kde pri prvom načítaní stránky, sa vám zobrazí layout s nejakými základnými údajmi, ale zoznam kontaktov,
nástenka a notifikácie sa vám načítajú neskôr asynchrónne. Vďaka streamingu HTML a modernému HTML to ide spraviť v jednom requeste,
bez toho aby používateľ čakal.
Proste sa najskôr pošle celý layout a následne sa server pozrie do svojho dátového úložiska,
čo nejaký čas trvá a následne ich pošle ako ďalšie kusy HTML, ktoré prehliadač umiestni na správne miesto.

Ide na to použiť tagy `template` a `slot`. Ako v nasledujúcej ukážke (pozor na uzatváracie tagy):

```html 
<html lang="en">
<head>
  <meta charset="utf-8" />  
</head>
<body>
  <template shadowrootmode="open">  
    <header>
    <h1>Streaming HTML - best food</h1>
  </header>
  <main>
    <slot name="content">Loading content...</slot>
  </main>
  <footer>Footer (c) 2025</a></footer>
</template>
```

Prvým načítaním sa načítal jednoduchý layout, kde sa zobrazí hlavička a pätička stránky.
Na serveri sa zistí, koľko obľúbených jedál sa bude zobrazovať a pošle sa ďalšia časť HTML.

```html 
<div slot="content">
  <template shadowrootmode="open">
    <p>Food list with poularity:</p>
    <ul>
      <li><slot name="slot-1">Loading...</slot></li>
      <li><slot name="slot-2">Loading...</slot></li>
      <li><slot name="slot-3">Loading...</slot></li>
    </ul>
  </template>
```

A následne sa postupne donačítajú jednotlivé jedlá (tu si môžeme predstaviť, že sa načítajú z nejakého extra pomalého API po jednom). 

```html
  <span slot="slot-1">Pizza (<em>95%</em>)</span>
```

```html
  <span slot="slot-3">Burger (<em>82%</em>)</span>
```

```html
  <span slot="slot-2">Sushi (<em>84%</em>)</span>  
</div>
</body>
</html>
```

Výsledok vyzerá nasledovne:

![Streaming HTML example](images/ModerneHtml/StreamingExample.gif){.img-center}

Tento spôsob žiaľ funguje iba pri prvotnom načítaní stránky, takže nejde využiť pri ajaxových volaniach. 

Streaming HTML ide pomocou HTMX využiť napríklad na postupné donačítanie tabuľky už na vyrendrovanej stránke, alebo na aktualizáciu rôznych miest cez `hx-swap-oob`. 

Ukážka použitia v HTMX pomocou rozšírenia [chunked-transfer](https://github.com/douglasduteil/htmx.ext...chunked-transfer):
```html
<button type="button"
        hx-ext="chunked-transfer"
        hx-get="/streaming" 
        hx-target="next div"
        hx-trigger="click">
         Load content
</button>
<div></div>
```

Prípadne daného loading efektu dosiahnuť pomocou CSS, kde sa na klienta posiela vždy aktuálne HTML
a CSS-kom sa skryjú pochádzajúce verzie (`display:none;` na elementy v sekcii a `display:block;` na posledný element).

### Zdroje ku streamingu HTML
1. https://dev.to/tigt/the-weirdly-obscure-art-of-streamed-html-4gc2 - _The weirdly obscure art of Streamed HTML_,
1. https://hypermedia.systems/tricks-of-the-htmx-masters/ - _Tricks Of The Htmx Masters_,
1. https://github.com/douglasduteil/htmx.ext...chunked-transfer - _HTMX extension pre streaming HTML_.

## Záver 

HTMX je zaujímavý nástroj, ktorý umožňuje vytvárať serverom riadené klientske aplikácie. Sám by som ho prirovnal ku nožíku švšvajčiarskej armády pre HTML.

Medzi výčitky voči nemu je, že komplexnosť sa presunula na server, čo je pravda len čiastočne, lebo pri mnohých typoch aplikácii prináša odstránenie niekoľkých vrstiev (napríklad taký router už prehliadač implementuje sám), navyše na serveri sa zvyčajne používa o dosť lepší a výkonnejší jazyk ako v prehliadači.

Za mňa HTMX, SSE a streaming HTML predstavujú odpoveď na komplexnosť forntendu pre bakcned programátorov.
