Published: 20.1.2018
Title: Optimalizácia výpočtu Levenstainovej vzdialenosti
Menu: Optimalizácia výpočtu Levenstainovej vzdialenosti
Cathegory: Dev
Description: Mikrooptimalizácia výpočtu Levenstainovej vzdialenosti v .Net-e.
---
V projekte [PerDia2012](PerDia2012.html) som sa rozhodol pridať do ku mužnostiam 
fulltext vyhľadávania aj možnosť približného (fuzzy) vyhľadávania, ktoré by bolo odolné 
aj voči mojim preklepom.

Samotné vyhľadávanie funguje tak, že ku vyhľadávaným zadaným slovám nájdu pomocou 
stored procedúry v invertovanom indexe trmy,
ktoré majú menšiu editačnú vzdialenosť ako je prahová hodnota.
S týchto termov sa vytvorí klasický fulltext dopyt a ten už vráti požadované výsledky.

Výpočet editačnej  vzdialenosti sa musí zavolať pre každý term v invertovanom indexe
krát každé slovo v dopyte. Invertovaný index je v mojom prípade tvorený cca. 70 000 termami.

Pre prvé experimenty som použil základnú implementáciu výpočtu Leventainovej vzdialnosti pomocou 
dynamického programovania, no odozvy boli neuveriteľné pomalé – rádovo sekundy až 
desiatky sekúnd pre vyhľadávaný dopyt tvorený dvomi až tromi slovami. Použitý kód sa nachádza nižšie ([zdroj](https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance)).

```cs
public int LevenshteinDistance(string source, string target)
{
    if (string.IsNullOrEmpty(source))
    {
        if (string.IsNullOrEmpty(target))
        {
            return 0;
        }
        else
        {
            return target.Length;
        }
    }

    if (string.IsNullOrEmpty(target))
    {
        return source.Length;
    }

    if (source.Length > target.Length)
    {
        var temp = target;
        target = source;
        source = temp;
    }

    int m = target.Length;
    int n = source.Length;
    int[] distance = new int[2, m + 1];

    for (int j = 1; j <= m; j++)
    {
        distance[0, j] = j;
    }

    int currentRow = 0;
    for (int i = 1; i <= n; ++i)
    {
        currentRow = i &amp; 1;
        distance[currentRow, 0] = i;
        int previousRow = currentRow ^ 1;
        for (int j = 1; j <= m; j++)
        {
            int cost = (target[j - 1] == source[i - 1] ? 0 : 1);
            distance[currentRow, j] = Math.Min(Math.Min(
                        distance[previousRow, j] + 1,
                        distance[currentRow, j - 1] + 1),
                        distance[previousRow, j - 1] + cost);
        }
    }

    return distance[currentRow, m];
}
```

Pre to som sa rozhodol siahnuť po rôznych optimalizáciách na úrovni aplikácie (cache). No výsledky boli stále nedostačujúce, tak som sa rozhodol optimalizovať surový výkon. Výsledný kód sa nachádza nižšie.

```cs
public unsafe bool IsLevenshteinDistanceLessMaxDistUnsafe(string source, string target, int maxDistance)
{
    if (maxDistance < 1)
    {
        throw new ArgumentOutOfRangeException("Parameter maxDistance is more then 1.", "maxDistance");
    }

    if (string.IsNullOrEmpty(source))
    {
        if (string.IsNullOrEmpty(target))
        {
            return true;
        }

        return target.Length < maxDistance;
    }

    if (source.Length > target.Length)
    {
        string temp = target;
        target = source;
        source = temp;
    }

    int m = target.Length;
    int n = source.Length;

    int* current = stackalloc int[m + 1];
    int* previous = stackalloc int[m + 1];

    for (int j = 0; j <= m; j++)
    {
        previous[j] = j;
    }

    fixed (char* sourcePtr = source)
    {
        fixed (char* targetPtr = target)
        {
            for (int i = 1; i <= n; ++i)
            {
                current[0] = i;
                int minDistance = int.MaxValue;
                for (int j = 1; j <= m; j++)
                {
                    int cost = (targetPtr[j - 1] == sourcePtr[i - 1] ? 0 : 1);
                    int distanceLocal = Min(previous[j] + 1, current[j - 1] + 1, previous[j - 1] + cost);
                    current[j] = distanceLocal;
                    if (minDistance > distanceLocal)
                    {
                        minDistance = distanceLocal;
                    }
                }

                if (minDistance >= maxDistance)
                {
                    return false;
                }

                int* tmp = current;
                current = previous;
                previous = tmp;
            }
        }
    }

    return previous[m] < maxDistance;
}
```

Prepísaním do _unsafe_ kódu som beh zrýchlil približne trojnásobne.
To hlavne kvôli odstráneniu alokovania nových polí a priami prístup k nim.
Na každý prístup to je len pár inštrukcií ale ak kus kódu robí 99% času to,
že pristupuje k poliam, tak to rozdiel urobí.

V nasledujúcej tabuĺke sú uvedené časy behu (čas behu je súčet časov potrebbných na vyhodnotenie šeistich porovnaní termov rôznej dĺžky a rôzneho prefixu) 
jednotlivých implementácií  merané pomocou [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) 
na Windows 10, .Net Framework 4.6.2 a procesore Intel Xeon CPU E3-1246 v3 3.50GHz.

:::{.table-responsive}
|Metóda|Mean|Error|StdDev|
|-|-|-|-|
|Klasická implementácia|2.754 &micro;s|0.0562 &micro;s|0.0601 &micro;s|
|Unsafe implementácia|1.074  &micro;s|0.0205 &micro;s|0.0227 &micro;s|
:::

## Záver
Optimalizácia pomocou _unsafe_ kódu v C# môže byť veľmi výhodná, v prípade,
že sa optimalizujú algoritmy/metódy, ktoré často pristupujú k poliam a tento kód sa volá často.
No na klasickú aplikačnú logiku je to veľmi nevhodné.
