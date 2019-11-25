Published: 10.4.2016
Title: Ako používať var a dynamic v C#
Menu: var a dynamic v C#
Cathegory: Dev
Description: Kľúčové slová var a dynamic, ako to vidím ja.
---
V poslednej dobe som si všimol, že vývojári píšu *var* úplne všade a potom C\# vyzerá ako JavaScript.

## Kedy *var* použiť:
Anonymné triedy, tam ani iná možnosť nie je.

Ak máte šialený generický typ 
`Dictionary<string, List<Func<Nullable<int>, string>>>`,
no tu je asi na mieste podotknúť, že ľudom , ktorý niečo takéto napíšu treba zlámať prsty.
Pretože daný typ je absolútne nesémantický (hlavne `Func<Nullable<int>, string>`) a bolo by ho lepšie nahradiť implementáciou rozumnej triedy alebo tried.

## Takže kedy a prečo nepoužívať var:
Znesprehľadnenie kódu, pretože na mnohých miestach nie je človeku jasné čo v tej premennej vlastne je.

```cs
var lastItem = this.FindLastItem();
```

Je tam číslo, reťazec, objekt? aký?... To nie je na prvý pohľad jasné.

Znižuje sémantiku kódu. Pretože je rozdiel v tom, čo chce vývojár povedať týmito dvoma riadkami.

```cs
FooStruct foo = new FooStruct();
IFoo foo = new FooStruct();
```

Na *var* nefunguje vo Visual Studiu  *„Find all references“* a *„Find usages“*.

## Kedy používať dynamic
Všade tam, kde mapujeme dynamicky svet na statický, to znamená interakcia z JSON-om, JavaScriptom, databázami, NoSql databázami... Poprípade interakcia s COM objektami, kde nechceme pevne zviazať vytváraný softvér s konkrétnou verziou COM knižnice (tu má dynamic výhodu oproti reflexii v tom, že interne používa cache).

Perfektný príklad je volanie JavaScriptovských funkcií v 
[SignalR](http://www.asp.net/signalr/overview/guide-to-the-api/hubs-api-guide-net-client).

Ďalším častým prípadom je mapovanie JSON resultu v ASP.NET MVC binderoch.

```cs
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Harrison314.ExamplexAsp.Binders
{
    public class DocumentModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {

            NameValueCollection query = controllerContext.HttpContext.Request.Params;
            string jsonData = query.Get("data");

            DocumentViewModel model = new DocumentViewModel ();

            dynamic data = JsonConvert.DeserializeObject(jsonData);
            model.ProfileId = data.profileId;
            model.DocumentId = data.documentId;
            model.GroupRoleId = data.groupRoleId;
            model.FormDocumentData = new Dictionary<string, string>();

            foreach (dynamic constructionData in data.formData)
            {
                model. FormDocumentData.Add((string)constructionData.id, (string)constructionData.value);
            }

            return model;
        }
    }
}
```
Takéto použitie je čisté, pretože dynamic sa používa len interne a vracia sa typový objekt.
