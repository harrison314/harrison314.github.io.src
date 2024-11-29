Published: 22.6.2021
Title: Meranie performace pre dotnetistov
Menu: Meranie performace pre dotnetistov
Cathegory: Dev
Description: Benchmarking, load testing a hľadanie úzkeho hrdla v dotnet aplikáciách.
---
Ak chce programátor zlepšiť výkonnosť a priepustnosť svojej aplikácie, musí ju vedieť odmerať a na základe nich zistiť,
či jeho zmena priniesla alebo nepriniesla zlepšenie výkonu.
V tomto blogu zhrniem nástroje a techniky, ktoré používam na mikrobenchmarking,
záťažové testovanie a hľadanie úzkeho hrdla v aplikáciách.

## Benchmarking
Pre benchmarking (zistiť koľko daná metóda trvá) spomeniem len jeden nástroj (naozaj nie je dobrý nápad si na to písať niečo vlastné).

### BenchmarkDotNet
_BenchmarkDotNet_ je knižnica/nástroj na mikrobenchmarking kódu v pre dotnet.
Slúži na porovnanie rýchlosti niekoľkých kódov (napríklad rôznych implementácii metódy alebo algoritmu).
Táto knižnica nás odtieni od garbage collectora, aktuálneho zaťaženia počítača na ktorom sa testuje
a využíva štatistické metódy na vyradenie anomálií z výsledkov.

_BenchmarkDotNet_ dokáže byť tak presný, že dokáže rozlíšiť medzi inkrementáciu property v triede a štruktúre.

Ako príklad použitia uvádzam známu ukážku, ktorá ukazuje vplyv [branch predikcie](https://en.wikipedia.org/wiki/Branch_predictor) v procesora na rýchlosť vykonávaného kódu – získanie súčtu nepárnych čísiel z poľa.

Ako bonus uvádzam aj implementáciu pomocou [SIMD inštrukcií](https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/).

```cs
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarking
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SumOddBenchmark>();
        }
    }

    public class SumOddBenchmark
    {
        private int[] numbers;

        [Params(10, 150, 500, 3000)]
        public int Size;

        [GlobalSetup]
        public void Setup()
        {
            Random rand = new Random(42);

            this.numbers = Enumerable.Range(0, this.Size)
                .Select(_ => rand.Next(0, 1024))
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public int SumOdd()
        {
            int sum = 0;
            for (int i = 0; i < this.numbers.Length; i++)
            {
                if (this.numbers[i] % 2 == 0)
                {
                    sum += this.numbers[i];
                }
            }

            return sum;
        }

        [Benchmark]
        public int SumOdd_EliminateBranching()
        {
            int sum = 0;
            for (int i = 0; i < this.numbers.Length; i++)
            {
                sum += (this.numbers[i] & 1) * this.numbers[i];
            }

            return sum;
        }

        [Benchmark]
        public int SumOdd_Simd()
        {
            ReadOnlySpan<Vector<int>> numbersVector = MemoryMarshal.Cast<int, Vector<int>>(this.numbers);
            Vector<int> accumulator = Vector<int>.Zero;

            for (int i = 0; i < numbersVector.Length; i++)
            {
                accumulator += (numbersVector[i] & Vector<int>.One) * numbersVector[i];
            }

            int sum = Vector.Dot(accumulator, Vector<int>.One);
            for(int i= numbersVector.Length * Vector<int>.Count;i<this.numbers.Length;i++)
            {
                sum += (this.numbers[i] & 1) * this.numbers[i];
            }

            return sum;
        }
    }
}
```

![Výsledky pre SumOddBenchmark.](images/PerformaceInNet/BenchmarkDotnet.png){.img-center}

Z výsledkov je vidieť jasný rozdiel medzi jednotlivými spôsobmi výpočtu sumy. A aj to, čo dokážu _SIMD_ inštrukcie.

_BenchmarkDotNet_ vie okrem merania času potrebného na vykonanie kódu aj iné zaujímavé veci. Najčastejšie stačí pridať nejaký atribút:
 * `[MemoryDiagnoser]` - Do výsledkov zahrnie informácie o behu garbage collectora a celkovej alkovovanej pamäte. Oboje sú veľmi užitočné informácie pri ladení priepustnosti.
 * `[NativeMemoryProfiler]` - Do výsledkov zahrnie memory leaky.
 * `[GcServer]` - Použije serverovy garbage collector.
 * `[HardwareCounters(...)]` - Rôzne HW counters, napríklad pre branch missprediction...
 * `[DisassemblyDiagnoser]` - Do výstupu pridá JIT-om vygenerovaný assembler.

 **Zdroje:**
 - <https://benchmarkdotnet.org/articles/overview.html>
 - <https://github.com/dotnet/BenchmarkDotNet>
 - <https://wug.cz/zaznamy/559-WUG-Days-2019-Jak-merit-vykon-NET-kodu-spravne>

 ## Load testing
 Záťažové testovanie som používal hlavne na testovanie priepustnosti a maximálneho času odpovede.
 Je dobré vedieť limity tvorenej aplikácie a aj jej hardvérové požiadavky.

### Netling
_Netling_ je jednoduchý GUI nástroj pre záťažové testovanie webových aplikácií.
Občas ho používam kvôli jednoduchosti, stačí zadať adresu, mieru paralelizmu a dĺžku trvania testu.
Výsledky zobrazuje prehľadne aj s jednoduchými grafmi.

No dokáže vytvárať len GET požiadavky.

Celkovo je Netling na jednoduché záťažové testovanie veľmi pohodlný nástroj.

![Okno nástroja Netling.](images/PerformaceInNet/InMemorytable.png){.img-center}

**Zdroje:**
- <https://github.com/hallatore/Netling>

### Vegeta
_Vegeta_ je nástroj na performace testing HTTP endpointov. Má plno nastavení v súvislosti s protokolom HTTP a HTTPS.
Je pomocou nej možné definovať si kompletne vlastné requesty (v súbore _targets.txt_), ktoré sa budú dookola odosielať.

Ukážka použitia vegety:

```
vegeta.exe attack -duration=600s -rate=15000 -targets=targets.txt \
  -output=results.bin -max-workers 32
```

```
vegeta.exe report results.bin
```

_Vegeta_ je multiplaformová konzolová aplikácia, takže ju stačí stiahnuť a použiť.

Nevýhodou je, že stále ide iba o veľmi syntetické testy, veľmi mi chýbala možnosť použiť v requestoch nejaké premenné (napríklad náhodné ID, aby som eliminoval cache, atď.).

**Zdroje:**
 - <https://github.com/tsenart/vegeta>
 - <https://kimsereyblog.blogspot.com/2018/12/load-test-your-api-with-vegeta.html>

### NBomber
_NBomber_ je load testing framework pre C# a F#. Umožňuje vytváranie vlastných testovacích scenárov pomocou jednotlivých krokov.
Má samostatný balíček špeciálne na HTTP requesty,
ale je ním možné testovať performace prakticky čohokoľvek (zápisu do databázy, volanie API, WebSocketov,...).

Veľmi sa hodí na scenáre, kde kroky nadväzujú, napríklad: vytvorenie používateľa, použitie API s jeho tokenom,... Výsledky každého kroku idú odovzdať nasledujúcim v rámci scenára.

Základné výsledky zobrazí v konzole, ale podrobné výsledky aj s kompletnými grafmi uloží ako HTML súbor na disk.

Príklad použitia:

```cs
IFeed<string> feed = Feed.CreateRandom("name", new string[] { "John", "Harold", "Jocelyn", "Lionel", "Sameen","Samantha", "Bear" });

IClientFactory<HttpClient> httpFactory = HttpClientFactory.Create();
IStep step1 = Step.Create("Find person", clientFactory: httpFactory, feed: feed, execute: async context =>
{
    NBomber.Plugins.Http.HttpRequest request = Http.CreateRequest("GET", $"https://local.test/Person?serach={context.FeedItem}");
    Response response = await Http.Send(request, context);

    return response;
});

IStep step2 = Step.Create("Delete person", clientFactory: httpFactory, execute: async context =>
{
    HttpResponseMessage step1Response = context.GetPreviousStepResponse<HttpResponseMessage>();

    using System.IO.Stream stream = await step1Response.Content.ReadAsStreamAsync();
    Person[] peoples = await System.Text.Json.JsonSerializer.DeserializeAsync<Person[]>(stream);

    NBomber.Plugins.Http.HttpRequest request = Http.CreateRequest("DELETE", $"https://local.test/Person/{peoples.First().Id}");
    Response response = await Http.Send(request, context);

    return response;
});

Scenario scenario = ScenarioBuilder.CreateScenario("FindAndDeleteScenario", step1, step2)
    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
    .WithLoadSimulations(Simulation.InjectPerSec(100, TimeSpan.FromSeconds(30)));

PingPluginConfig pingPluginConfig = PingPluginConfig.CreateDefault(new string[] { "local.test" });
PingPlugin pingPlugin = new PingPlugin(pingPluginConfig);

NBomberRunner.RegisterScenarios(scenario)
    .WithWorkerPlugins(pingPlugin)
    .Run();
```

**Zdroje:**
- <https://nbomber.com/>
- <https://github.com/PragmaticFlow/NBomber>

## Hľadanie úzkeho hrdla
Niekoľkokrát som sa stretol s výkonnostným problémom, alebo naopak tým, že som potreboval maximalizovať priepustnosť.

No nepoužil som komerčne tvorené nástroje, ale riešenie som si poskladal sám.

### Event counters
_Event counters_ sa hodia na hľadanie vážneho problému a vedia poskytnúť aspoň prehľad o tom, čo sa v aplikácii deje.

_Event counters_ je vstavané API v dotnete, ktoré umožňuje zbierať pomerne veľké množstvo metrík z bežiacej aplikácie takmer v reálnom čase.

Zoznám základných metrík, ktoré idú sledovať je možné nájsť tu: <https://docs.microsoft.com/en-us/dotnet/core/diagnostics/available-counters>.

Veľká výhoda je, že v aplikácii netreba nič zapínať, ani ju reštartovať, stačí prečítať hodnoty.

_Event counters_ by sa mi hodili v jednom prípade, kde aplikácii po týždni behu na produkcii začala výrazne spomaľovať,
no na testovacom serveri sa to neprejavilo, nepomohol ani memory dump,
lebo okamžite po memory dumpu sa tento stav opravil.

**Zdroje:**
- <https://devblogs.microsoft.com/dotnet/introducing-diagnostics-improvements-in-net-core-3-0/>
- <https://docs.microsoft.com/en-us/dotnet/core/diagnostics/event-counters>
- <https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters>

### Vlastný profiling 
V pár prípadoch som potreboval diagnostikovať výkonnostné problémy v špecifických podmienkach. Tak som si profiler vyrobil tak povediac na kolene.

V oboch prípadoch šlo o smojenie [MiniProfileru](https://miniprofiler.com/dotnet/) s nejakou formou _Aspektovo orientovaného programovania_.

V prvom príde som potreboval profiling ako plugin do webovej aplikácie, 
tak som použil [MiniProfiler](https://miniprofiler.com/dotnet/) a [MassiveDynamicProxyGenerator](https://github.com/harrison314/MassiveDynamicProxyGenerator). 

Pomocou dependecny injection šlo obaliť všetky metódy služieb z vlastného assebly.
Celé to prinieslo veľa muziky za málo riadkov kódu.

V inom prípade som potreboval spraviť perfomace tuning vlastnej aplikácie (niečo podobné ako API gateway) v _ASP.NET Core 3.1_. Potreboval som dosiahnuť priepustnosť v tisíckach requestov za sekundu, pričom trebalo každý sprasovať 
a podľa jeho vnútorností sa rozhodnúť, kam request preposlať a či odpoveď uložiť do cache.

Znovu som použil MiniProfiler ale teraz z [MrAdvice](https://github.com/ArxOne/MrAdvice) (ide o AOP IL weaver, ktorý vtká svoj kód do už existujúceho kódu po kompilácii).
Stačilo implementovať profilovací aspekt (advice - jedna krátka trieda), použiť jeden attribút a mal som profiling na každej metóde v projekte.

V obcoh prípadoch už len stačilo simluovať bežnú záťaž a cez webové rozhranie si pozrieť výsledky.

**Zdroje:**
- <https://cs.wikipedia.org/wiki/Aspektov%C4%9B_orientovan%C3%A9_programov%C3%A1n%C3%AD>
- [https://github.com/harrison314/MassiveDynamicProxyGenerator](https://github.com/harrison314/MassiveDynamicProxyGenerator#massivedynamicproxygeneratormicrosoftdependencyinjection)
- <https://miniprofiler.com/dotnet/AspDotNetCore>
- <https://github.com/ArxOne/MrAdvice>
