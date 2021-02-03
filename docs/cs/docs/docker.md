# Docker

GrillBot je možné spustit jako docker kontejner. **Součástí image není databáze.** Databázový MSSQL server je možné také spustit jako kontejner, nicméně to, jak databázový server není podmínka.

**Doporučení**:
Sestavení docker image trvá přibližně 1 minutu, zatím, co překlad aplikace trvá pár vteřin. Doporučuje se tedy vytvářet až výsledný image a nepoužívat jej k vývojovým účelům.

## Vytvoření obrazu

Pro sestavení obrazu je zapotřebí být v kořenovém adresáři repozitáře. Poté staří zavolat příkaz

```sh
docker build -t misha12/grillbot .
```

## Vytvoření kontejneru a spuštění

Pro vytvoření spustitelného kontejneru z image již není třeba být v kořenovém adresáři projektu. Nicméně bude zapotřebí naplnit požadované vlastnosti.

### Příklad vytvoření kontejneru

```sh
docker run --name GrillBot -e 'APP_TOKEN=<your_token>' -e 'DB_CONN=<YOUR_MSSQL_CONNECTION_STRING>' -e 'ASPNETCORE_ENVIRONMENT=Production' -e 'ASPNETCORE_URLS=http://+:5000' misha12/grillbot
```

kde (kromě názvu obrazu) je definováno:

- `--name` značí pojmenování kontejneru. Není povinný. Pokud není uveden, tak docker engine přiřadí náhodný řetězec.
- Konfigurační hodnoty proměnného prostředí:
    - `-e 'APP_TOKEN=<your_token>'`: Nastavení tokenu, pod kterým se aplikace (bot) ověřuje oproti Discord rozhraní. Povinné. Bez ní aplikace zahlásí chybu a nespustí se.
    - `-e 'DB_CONN=<YOUR_MSSQL_CONNECTION_STRING>'`: Nastavení připojovacího řetězce, pomocí kterého se bude aplikace připojovat k databázi. Povinné. Bez ní aplikace zahlásí chybu a nespustí se.
    - `-e 'ASPNETCORE_ENVIRONMENT=Production'`: Nastavení pracovního prostředí aplikace. Na výběr je `Development`, `Staging` a `Production`. Nepovinné. Pokud není zadáno, tak se implicitně bere jako `Production`.

## DockerHub

Veškeré vydání aplikace GrillBot jsou k dispozici na [DockerHub](https://hub.docker.com/r/misha12/grillbot).

Mohou se zde občas objevit vydání, které nejsou doplněny v [GitHub Releases](https://github.com/Misha12/GrillBot/releases). Většinou se jedná o testovací vydání, nebo budou doplnění v Releases později.

### Příkaz na stažení obrazu GrillBot z DockerHub

```sh
docker pull misha12/grillbot
```
