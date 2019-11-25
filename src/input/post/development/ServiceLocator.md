Published: 26.4.2016
Title: Stupne zla Service lokátoru
Menu: Stupne zla Service lokátoru
Cathegory: Dev
Description: Myšlienky okolo Service lokátoru.
---
O [servise lokátore](http://www.martinfowler.com/articles/injection.html) ako antipatterne bolo toho už popísane veľa,
ale v skratke, čo to je.

Service lokátor vracia inštanciu služby podľa nejakého
parametru, no na rozdiel od [Factory method](http://voho.cz/wiki/factory-method/), tá istá metóda vracia rôzne typy služieb.

Medzi hlavné námietky voči nemu patrí poučovanie princípu [Dependency injection (DI)](http://www.martinfowler.com/articles/injection.html),
teda to, že trieda sa navonok neprizná k svojim závislostiam a má prístup ku všetkému, 
k čomu chce. To má niekoľko praktických dôsledkov:

* znemožníte testovanie vášho kódu, hlavne mockovanie závislostí,
* vaša trieda môže hodiť neočakávané výnimky, teda spadnúť lebo v servise lokátore chýba závislosť,
* ak sa jedná o extrenú závislosť, človek je nútený hľadať dekompilátorom, čo vlastne treba pridať do referencií a ako to nakonfigurovať, 
* nemôžete si byť istý, či sa požadovaná služba v servise lokátore bola už registrovaná,
* miešanie infraštruktúrneho kódu s aplikačným

Najmenej zlý je inštančný servise lokátor, ktorý má rozhranie a resolvuje inštancie 
len podľa typu. V malých aplikáciách to nemusí byť problém, hlavne pri jedinom 
vývojárovi, no aplikácie sa zvyknú rozrastať.  

```cs
    public interface IServiceLocator
    {
        T Resolve<T>();
    }

    public class Locator : IServiceLocator
    {
        private readonly Dictionary<Type, Func<object>> services;
     
        public Locator()
        {
            this.services = new Dictionary<Type, Func<object>>();
        }
         
        public void Register<T>(Func<T> resolver)
        {
           this.services[typeof(T)] = () => resolver();
        }
         
        public T Resolve<T>()
        {
            return (T)this.services[typeof(T)].Invoke();
        }
    }
```

O dosť horší je servise lokátor, ktorý je súčasne Singlton.
Ten prerastie kódom ako rakovina a už nikdy sa ho nezbavíte (pamätáte si ešte,
že globálne premenné sú zlo). Testovateľnosť sa tým znížila takmer na nulu,
a znovu-použiteľnosť tiež, pretože ľubovoľná trieda môže byť závislá na vašom servise lokátore.

Najhoršie je, keď servise lokátor resolvuje inštancie podľa stringu (textového parametru).
 Tu sa k tomu všetkému pridávajú preklepy v textových kľúčoch, nesprávne pretypovanie služieb 
a nemožnosť aspoň akej takej typovej kontroly počas kompilácie alebo v kompozitnom roote.


## Ako sa s tým vysporiadať?
Najlepšie je sa rozumne držať [SOLID princípov](https://www.zdrojak.cz/clanky/navrhove-principy-solid/), neskrývať závislosti.

Keď už treba vytvárať inštancie podľa nejakých parametrov, treba použiť [factory](http://voho.cz/wiki/factory-method/), alebo factory na factory, no aj samotnú factory treba injektovať.

Zatiaľ najlepším riešením správy závislostí a inštancií je použiť kvalitný IoC kontajner.
Kvalitný pre mňa znamená, že okrem poskladania objektového grafu podporuje nenúti programátora meniť aplikačný kód
(teda žiadne atribúty a anotácie, aplikačný kód by ani nemal vedieť, že používate IoC kontajner)
a mal by vedieť zverifikovať a zvalidovať objektový graf a jeho nastavenia. Tým si človek ušetrí veľa nepríjemných prekvapení.

Veci ako podpora dekorátorov, dynamických proxy sú len bonus k tomu (napr. [Castle Windsor](https://github.com/castleproject/Windsor/blob/master/docs/README.md), a [Simple inejctor](https://simpleinjector.org/index.html)).

## IoC v JavaScripre?
V poslednej dobe sa na fórach stretávam s tým, že v JavaScripte chcú vývojári používať IoC kontajner, dokonca jestvuje plno implementácií.

Až na to, že JavaScript ako taký nemá typy parametrov (duck typing), nemá rozhrania, a objekty môžu meniť metódy, môžu ich pridávať aj prepisovať.

Jediné čoho sa môže „IoC“ kontajner v čistom JavaScripte chytiť sú názvy parametrov konštruktora. No ale to už nie je IoC kontajner,
ale najhoršia forma servise lokátora. Dokonca nejde ani rozumne validovať objektový graf.
Preto to podľa mňa prináša viac problémov ako úžitku. 