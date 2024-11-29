Published: 19.7.2018
Title: Inštalácia .Net Core cez Ansible
Menu: Ansible a .Net Core
Cathegory: Dev
Description: Návod na inštláciu .Net Core 2.1 pomocou Ansible.
---
Na testovacích serveroch  používame distribúcie založená na Ubuntu a ako človek,
čo ma rád Windows, administrácia týchto prostredí mi prišla ako monotónna a otravná činnosť.

Pri snahe automatizovať činnosti pre vytváranie testovacieho prostredia pre ASP.NET Core aplikácie
som narážal hlavne na odkazy na [Docker](https://www.docker.com/) a [Kubernetes](https://kubernetes.io/),
no nemohol som ich použiť.
Našiel som veľa riešení hlavne zo shell skriptami ale po skúsenostiach z [Cake](https://cakebuild.net/) a [ARM](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview) 
mi to prišla príliš piplačka a chýbala mi tam indepotencia a deklaratívnosť.

Nakoniec som narazil na [Ansible](https://docs.ansible.com/ansible/2.3/index.html), čo je _autmation tool_ hlavne pre unixový svet.
Ansible nie je úplne deklaratívne ani indepotentné,
je niekde medzi, ale na inštaláciu prerequizít a počiatočné nastavenie servera bohate stačí. 

## Inštalácia .Net Core 2.1
Pre inštaláciu .Net Core sa dá použiť nasledujúci Ansible playbook (je v Yaml formáte).

```
# Ensure ASP.NET Core 2.1
---
- hosts: local
  tasks:
    - name: Register Microsoft key and feed
      get_url:
        url: https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
        dest: /tmp/packages-microsoft-prod.deb

    - name: Add feed using packages-microsoft-prod.deb
      apt:
        deb: /tmp/packages-microsoft-prod.deb
      notify:
        - Apt get update

    - name: Add dotnetdev source for Ubuntu
      copy: 
        content: 'deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-bionic-prod bionic main'
        dest: /etc/apt/sources.list.d/dotnetdev.list
      notify:
        - Apt get update

    - name: Ensure apt-transport-https
      apt:
          name: apt-transport-https
          state: present
      notify:
        - Apt get update

    - meta: flush_handlers
    
    - name: Ensure Net Core 2.1
      apt:
          name: "{{item}}"
          state: latest
      with_items:
       - dotnet-runtime-2.1
       - aspnetcore-runtime-2.1
  
  handlers:
    - name: Apt get update
      apt:
          update_cache: yes
```

V sekcii _hosts_ treba nasatviť ciel na ktorý bude playbook uplatnený.

Spúšťa sa pomocou `ansible-playbook aspnetcore.yml`.

Rovnakým spôsobom ide nainštalovať aj _powershell_, stačí zmeniť posledný krok _"Ensure Net Core 2.1"_ na:

```
    - name: Ensure Powershell
      apt:
          name: powershell
          state: latest
```

## Záver
Hlavná výhoda použitia Ansible playbooku je indepotencia, to znamená, že aj po viacnásobnom spustení bude výsledný stav serveru rovnaký.
Takisto je ním možné prevádzať operácie na viacerých serveroch súčasne.
