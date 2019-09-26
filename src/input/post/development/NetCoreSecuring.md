Published: 29.8.2019
Title: Dodatočné zabezpečenie .Net Core
Menu: Dodatočné zabezpečenie .Net Core
Cathegory: Dev
---
# Dodatočné zabezpečenie .Net Core aplikácie na CentOs
Každá aplikácia môže obsahovať bezpečnostné chyby, preto sa na serveroch zavádzajú dodatočné opatrenia,
aby sa minimalizovali škody, ktoré môže útočník spôsobiť cez napadnutú aplikáciu.

Tento návod je rozšírením oficiálnej dokumentácie k deploymentu ASP.NET Core webovej aplikácie na _CentOs_ 
<https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-2.2>.

V prípade serverových aplikácií odporúčam používať LTS verziu _.NET Core_ a nepoužívať self&nbsp;contained build,
aby bolo možné aktualizovať framework a runtime.

## Rozšírenie návodu
Oproti pôvodnému návodu sa pre službu vytvára používateľ, umiestňuje sa do iného adresára (v ktorom sa upravujú práva)
a pridávajú sa nastavenia  _unit súboru_ pre _systemd_.

### Vytvorenie používateľa
Pre službu sa vytvorí samostatný používateľ a skupina (pre každú službu je vhodné mať samostatného používateľa):
```txt
sudo groupadd helloappuser
sudo adduser --system -g helloappuser --no-create-home helloappuser
```

Používateľovi je možné zakázať shell, ale tento krok komplikuje prípadné neskoršie hľadanie problémov:
```txt
sudo usermod -s /usr/sbin/nologin helloappuser
```

### Nasadenie aplikácie
V [pôvodnom návode](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-2.2>) sa aplikácia umiestňuje do `/var/www` ale spolu z logmi (loguje sa aj do súborov) ju umiestnime do priečinka `/opt`, kam patria komerčné aplikácie.

Vytvoríme priečinky pre službu:
```txt
mkdir /opt/HelloApp
mkdir /opt/HelloApp/bin
mkdir /opt/HelloApp/log
```

Skopírujeme a rozbalíme _HelloApp_ aplikáciu do _/opt/HelloApp/bin_.
Nastavíme súborom vlastníka a prístupné práva pre nami vytvoreného používateľa:
```txt
chown -R helloappuser:helloappuser /opt/HelloApp/bin
chown -R helloappuser:helloappuser /opt/HelloApp/log

find /opt/HelloApp/bin -type f -exec chmod u=r,g=r {} \;
find /opt/HelloApp/bin -type d -exec chmod u=rwx,g=rx {} \;
```

### Konfigurácia služby
_Unit_ súbor `/etc/systemd/system/kestrel-helloapp.service` doplníme o nastavenie používateľa a obmedzíme prístup na súborový systém.

```txt
[Unit]
Description=Example .NET Web API App running on CentOS 7

[Service]
Type=simple
WorkingDirectory=/opt/HelloApp/bin
ExecStart=/usr/bin/dotnet /opt/HelloApp/bin/HelloApp.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-HelloApp
Environment=ASPNETCORE_ENVIRONMENT=Production 
Environment=ASPNETCORE_URLS=http://localhost:5000/

User=helloappuser
Group=helloappuser

ProtectSystem=full
PrivateDevices=yes
PrivateTmp=yes
NoNewPrivileges=true
CapabilityBoundingSet=~CAP_SYS_ADMIN

[Install]
WantedBy=multi-user.target
```

## Zdroje
1. [Deploy ASP.NET Core aplikácie](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-2.2)
1. [Unit súbor](https://www.freedesktop.org/software/systemd/man/systemd.exec.html)
